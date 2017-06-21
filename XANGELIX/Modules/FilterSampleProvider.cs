using System;
using NAudio.Wave;
using System.Collections;

namespace XANGELIX.Modules {
	// TODO Make this an interface and implement with LPF, HPF, notch, bandpass etc.
	class FilterSampleProvider : ResettableSampleProvider {

		private double cornerFrequency;

		//private int filterLength;   // The number of input samples used to compute each output sample
		private float[] filterCoefficients; // TODO Fold later

		private float[] inputBuffer;
		private CircleBuffer inputCircleBuffer;

		public FilterSampleProvider(uint filterHalfLength) {
			InputSampleProvider = new DCSampleProvider();
			CornerFrequency = 440d;

			// TODO take filterHalfLength as an arg
			filterCoefficients = new float[filterHalfLength];

			inputBuffer = new float[0];
			inputCircleBuffer = new CircleBuffer(2 * filterHalfLength);

			// Generate filter coefficients
			//int center = filterLength / 2;
			for (int i = 0; i < filterCoefficients.Length; i++) {
				//filterCoefficients[i] = (float)Math.Pow(0.5d, i + 1d);  // Some BS
				filterCoefficients[i] = i == 0 ? 1f : 0f;	// No filter

				// TODO Fix this half-remembered nonsense
				/*double phase = (i - center) / (double)SampleRate;
				double x = 2 * Math.PI * phase * cornerFrequency;
				filterCoefficients[i] = x != 0d ? (float)(Math.Sin(x) / x) : 1f;*/
			}
		}

		public ResettableSampleProvider InputSampleProvider { get; set; }

		// TODO Make this re-calculate the filter coefficients
		public double CornerFrequency {
			get { return cornerFrequency; }
			set { cornerFrequency = value; }
		}

		public override int Read(float[] buffer, int offset, int count, uint frame) {
			//ResetByFrame(frame);

			// Get input samples
			if (inputBuffer.Length < count) {
				inputBuffer = new float[count];
			}
			InputSampleProvider.Read(inputBuffer, 0, count, frame);

			// Convolve each input sample with the filter coefficients
			float outputSample;
			for (int i = 0; i < count; i++) {
				// Load one sample into the circle buffer (overwriting the oldest sample)
				inputCircleBuffer.Write(inputBuffer[i]);

				outputSample = 0f;
				int endInputOffset = 2 * filterCoefficients.Length - 1;	// The offset of the last input sample
				for (int j = 0; j < filterCoefficients.Length; j++) {
					// Fold two input samples together and multiply by the filter coefficient
					outputSample += (inputCircleBuffer.Read(j) +
						inputCircleBuffer.Read(endInputOffset - j)) * filterCoefficients[j];
				}

				buffer[i + offset] = outputSample;
			}

			return count;
		}

		protected override void Reset() {
			// TODO
		}

		protected override void SaveResetData() {
			// TODO
		}
	}
}
