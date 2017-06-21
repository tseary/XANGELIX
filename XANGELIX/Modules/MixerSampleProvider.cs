using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XANGELIX.Modules {
	class MixerSampleProvider : ResettableSampleProvider {

		private List<ResettableSampleProvider> inputSampleProviders;
		private List<float> inputGains;

		private float[] inputBuffer;

		public MixerSampleProvider() {
			inputSampleProviders = new List<ResettableSampleProvider>(2);
			inputGains = new List<float>(2);

			inputBuffer = new float[0];
		}

		public void AddInput(ResettableSampleProvider input, float gain) {
			int index = inputSampleProviders.IndexOf(input);
			if (index >= 0) {
				inputGains[index] += gain;
			} else {
				inputSampleProviders.Add(input);
				inputGains.Add(gain);
			}
		}

		public void SetGain(ResettableSampleProvider input, float gain) {
			SetGain(inputSampleProviders.IndexOf(input), gain);
		}

		public void SetGain(int index, float gain) {
			if (index >= 0 && index < inputGains.Count) {
				inputGains[index] = gain;
			}
		}

		public override int Read(float[] buffer, int offset, int count, uint frame) {
			ResetByFrame(frame);

			// Grow input buffer if it is too small
			if (inputBuffer.Length < count) {
				inputBuffer = new float[count];
			}

			// Clear the input buffer (Array.Clear() doesn't work for some reason)
			for (int i = 0; i < count; i++) {
				buffer[offset + i] = 0f;
			}

			for (int j = 0; j < inputSampleProviders.Count; j++) {
				inputSampleProviders[j].Read(inputBuffer, 0, count, frame);
				for (int i = 0; i < count; i++) {
					buffer[offset + i] += inputGains[j] * inputBuffer[i];
				}
			}

			return count;
		}

		protected override void Reset() {
			//throw new NotImplementedException();
		}

		protected override void SaveResetData() {
			//throw new NotImplementedException();
		}
	}
}
