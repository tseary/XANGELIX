using System;

namespace XANGELIX.Modules {
	class EchoSampleProvider : ResettableSampleProvider {

		private int delaySamplesTarget;
		private int delaySamples;

		private const float AmplitudeSmoothing = 0.99f; // TODO do better
		private float amplitudeTarget = 1f;
		private float amplitude = 1f;

		private float[] inputBuffer;
		private CircleBuffer echoCircleBuffer;

		public EchoSampleProvider() {
			inputBuffer = new float[0];
			echoCircleBuffer = new CircleBuffer((uint)SampleRate);

			InputSampleProvider = new DCSampleProvider();

			Delay = 0.5f;
			Amplitude = 0.5f;
		}

		public ResettableSampleProvider InputSampleProvider { get; set; }

		public float Delay {
			get { return delaySamplesTarget / (float)SampleRate; }
			set {
				delaySamplesTarget = (int)Math.Round(value * SampleRate);
				if (delaySamplesTarget < 0) { delaySamplesTarget = 0; }
				echoCircleBuffer.EnsureCapacity(delaySamplesTarget);
			}
		}

		public float Amplitude {
			get { return amplitudeTarget; }
			set { amplitudeTarget = value; }
		}

		public override int Read(float[] buffer, int offset, int count, uint frame) {
			//ResetByFrame(frame);

			// Grow input buffer and get samples
			if (inputBuffer.Length < count) {
				inputBuffer = new float[count];
			}
			InputSampleProvider.Read(inputBuffer, 0, count, frame);

			// Generate echoes
			float outputSample;
			for (int i = 0; i < count; i++) {
				// Adjust echo volume
				amplitude = AmplitudeSmoothing * amplitude + (1f - AmplitudeSmoothing) * amplitudeTarget;

				// Adjust delay
				if (delaySamples < delaySamplesTarget) {
					delaySamples++;
				} else if (delaySamples > delaySamplesTarget) {
					delaySamples--;
				}

				// Read the oldest sample, mix it with the input and write it back to the circle buffer
				outputSample = amplitude * echoCircleBuffer.Read(delaySamples) + inputBuffer[i];
				echoCircleBuffer.Write(outputSample);

				buffer[i + offset] = outputSample;
			}

			return count;
		}

		protected override void Reset() {
			throw new NotImplementedException();
		}

		protected override void SaveResetData() {
			throw new NotImplementedException();
		}
	}
}
