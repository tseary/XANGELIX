using System;
using NAudio.Wave;
using System.Collections;

/* impulse response (sinc function)
 * h_d(n) = (w_c / pi) * sin(w_c * n) / (w_c * n)
 * 
 * Truncated sinc function produces ripples in pass and stop bands.
 * Higher order compresses ripples but does not change their amplitude.
 * 
 * 
 */

namespace XANGELIX.Modules {
	// TODO Make this an interface and implement with LPF, HPF, notch, bandpass etc.
	class FilterSampleProvider : ResettableSampleProvider {

		private double cornerFrequency;

		//private int filterLength;   // The number of input samples used to compute each output sample
		/// <summary>
		/// The last coefficient is the impulse response half-center. All
		/// coefficients are symmetrical, with a factor of 0.5 built in.
		/// e.g. An approximate all-pass filter would have filterCoefficients = {0.5}
		/// </summary>
		private float[] filterCoefficients;

		private float[] inputBuffer;
		private CircleBuffer inputCircleBuffer;

		public FilterSampleProvider(uint filterHalfLength) {
			InputSampleProvider = new DCSampleProvider();
			CornerFrequency = 440d;

			filterCoefficients = new float[filterHalfLength];

			inputBuffer = new float[0];
			inputCircleBuffer = new CircleBuffer(2 * filterHalfLength);

			// Generate filter coefficients
			//int center = filterLength / 2;
			for (int i = 0; i < filterCoefficients.Length; i++) {
				//filterCoefficients[i] = (float)Math.Pow(0.5d, i + 1d);  // Some BS

				// Approximate all-pass filter
				//filterCoefficients[i] = i == (filterCoefficients.Length - 1) ? 0.5f : 0f;

				// TODO Fix this half-remembered nonsense
				/*double phase = (i - center) / (double)SampleRate;
				double x = 2 * Math.PI * phase * cornerFrequency;
				filterCoefficients[i] = x != 0d ? (float)(Math.Sin(x) / x) : 1f;*/
				double phase = (i - (filterCoefficients.Length - 0.5d)) / 44100d;
				double x = 2 * Math.PI * phase * cornerFrequency;
				filterCoefficients[i] = 0.5f * (x != 0d ? (float)(Math.Sin(x) / x) : 0f);

				// TODO Apply (real) window function
				filterCoefficients[i] *= Math.Min(1f, i * 10f / filterCoefficients.Length);
			}
		}

		public ResettableSampleProvider InputSampleProvider { get; set; }

		// TODO Make this re-calculate the filter coefficients
		public double CornerFrequency {
			get { return cornerFrequency; }
			set { cornerFrequency = value; }
		}

		public override int Read(float[] buffer, int offset, int count, uint frame) {
			//ResetByFrame(frame);	// TODO

			// Get input samples
			if (inputBuffer.Length < count) {
				inputBuffer = new float[count];
			}
			count = InputSampleProvider.Read(inputBuffer, 0, count, frame);

			// Convolve each input sample with the filter coefficients
			float outputSample;
			for (int i = 0; i < count; i++) {
				// Load one sample into the circle buffer (overwriting the oldest sample)
				inputCircleBuffer.Write(inputBuffer[i]);

				outputSample = 0f;
				int oldestInputOffset = 2 * filterCoefficients.Length - 1;  // The offset of the last input sample
				for (int j = 0; j < filterCoefficients.Length; j++) {
					// Fold two input samples together and multiply by the filter coefficient
					outputSample += (inputCircleBuffer.Read(j) +
						inputCircleBuffer.Read(oldestInputOffset - j)) * filterCoefficients[j];
				}

				// Put the output sample in the output buffer
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
