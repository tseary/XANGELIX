using System;
using NAudio.Wave;

namespace XANGELIX.Modules {
	class ModulatedSineWaveProvider : ResettableSampleProvider {
		/// <summary>
		/// One cycle of sine wave values.
		/// </summary>
		private readonly float[] waveTable;

		private double frequency;       // Hz
		private double phase;           // Samples at 1 Hz

		private const float AmplitudeSmoothing = 0.99f; // TODO do better
		private float amplitudeTarget = 1f;
		private float amplitude = 1f;

		/// <summary>
		/// Buffer for reads from modulation sample provider. The data is not reused
		/// between calls to Read(), this is only to prevent frequent allocations.
		/// </summary>
		private float[] modulationBuffer;

		// Reset data
		private double resetFrequency;
		private double resetPhase;
		private float resetAmplitude;

		public ModulatedSineWaveProvider() {
			waveTable = new float[SampleRate];
			for (int i = 0; i < SampleRate; i++) {
				waveTable[i] = (float)Math.Sin(2 * Math.PI * i / SampleRate);
				//waveTable[i] = 2f * ((float)i / sampleRate) - 1f;   // Sawtooth
			}

			modulationBuffer = new float[0];

			Frequency = 440d;
			ModulationSampleProvider = new DCSampleProvider();

			SaveResetData();
		}

		public ResettableSampleProvider ModulationSampleProvider { get; set; }

		public double Frequency {
			get { return frequency; }
			set { frequency = value; }
		}

		public float Amplitude {
			get { return amplitudeTarget; }
			set { amplitudeTarget = value; }
		}

		public override int Read(float[] buffer, int offset, int count, uint frame) {
			// Reset or save the new frame number and state
			ResetByFrame(frame);

			// Grow buffer if it is too small, then get modulation samples
			if (modulationBuffer.Length < count) {
				modulationBuffer = new float[count];
			}
			ModulationSampleProvider.Read(modulationBuffer, 0, count, frame);

			// Load samples into buffer
			for (int i = 0; i < count; ++i) {
				// Adjust volume
				amplitude = AmplitudeSmoothing * amplitude + (1f - AmplitudeSmoothing) * amplitudeTarget;

				// Calculate instantaneous frequency
				double frequency = this.frequency + modulationBuffer[i];
				double phaseStep = waveTable.Length * (frequency / WaveFormat.SampleRate);

				// Increment phase
				phase += phaseStep;
				if (phase >= waveTable.Length) {
					phase -= waveTable.Length;
				}

				// Load value into output buffer
				int waveTableIndex = (int)phase % waveTable.Length;
				buffer[i + offset] = waveTable[waveTableIndex] * amplitude;

				// DEBUG Clicks, visible on scope
				//if (i == 0) {
				//	buffer[offset + i] = 2f * amplitude;
				//}
			}

			return count;
		}

		protected override void Reset() {
			frequency = resetFrequency;
			phase = resetPhase;
			amplitude = resetAmplitude;
		}

		protected override void SaveResetData() {
			resetFrequency = frequency;
			resetPhase = phase;
			resetAmplitude = amplitude;
		}
	}
}
