using System;
using NAudio.Wave;

namespace XANGELIX.Modules {
	class DCSampleProvider : ResettableSampleProvider {

		private double dcLevel;
		private double resetDCLevel;

		public DCSampleProvider(double dcLevel = 0d) {
			DCLevel = dcLevel;
		}

		public double DCLevel {
			get { return dcLevel; }
			set { dcLevel = value; }
		}
		
		public override int Read(float[] buffer, int offset, int count, uint frame) {
			ResetByFrame(frame);

			for (int i = 0; i < count; i++) {
				buffer[i + offset] = (float)DCLevel;
			}
			return count;
		}

		protected override void Reset() {
			dcLevel = resetDCLevel;
		}

		protected override void SaveResetData() {
			resetDCLevel = dcLevel;
		}
	}
}
