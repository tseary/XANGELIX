using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XANGELIX.Modules {
	class NoiseSampleProvider : ResettableSampleProvider {

		private Random random;

		public NoiseSampleProvider() {
			random = new Random();
		}

		public override int Read(float[] buffer, int offset, int count, uint frame) {
			for (int i = 0; i < count; i++) {
				buffer[i + offset] = 2f * (float)random.NextDouble() - 1f;
			}
			return count;
		}

		protected override void Reset() { }

		protected override void SaveResetData() { }
	}
}
