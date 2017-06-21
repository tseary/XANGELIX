using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XANGELIX {
	class CircleBuffer {

		private float[] buffer;

		/// <summary>
		/// The last-written index.
		/// </summary>
		private int headIndex = 0;

		public CircleBuffer(uint capacity) {
			buffer = new float[capacity];
		}

		/// <summary>
		/// Reads a sample with the specified age. e.g. Read(0) returns the most recent
		/// value written and Read(3) returns the value from three writes ago.
		/// </summary>
		/// <param name="offset"></param>
		/// <returns></returns>
		public float Read(int offset) {
			int index = headIndex - offset;
			if (index < 0) { index += buffer.Length; }
			return buffer[index];
		}

		public void Write(float value) {
			if (++headIndex >= buffer.Length) { headIndex = 0; }
			buffer[headIndex] = value;
		}

		public void EnsureCapacity(int capacity) {
			if (buffer.Length < capacity) {
				SetCapacity(capacity);
			}
		}

		public void SetCapacity(int capacity) {
			// Ignore if no change is needed
			if (capacity == buffer.Length) {
				return;
			}

			var newBuffer = new float[capacity];
			int smallerSize = Math.Min(capacity, buffer.Length);

			// Copy old data into new array
			// If shrinking, the oldest data is lost
			for (int i = 0; i < smallerSize; i++) {
				newBuffer[capacity - 1 - i] = Read(i);
			}
			headIndex = capacity - 1;   // Set head index to most recent data

			// Replace buffer
			buffer = newBuffer;
		}
	}
}
