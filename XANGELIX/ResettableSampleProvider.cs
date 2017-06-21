using System;
using NAudio.Wave;

namespace XANGELIX {
	// TODO Make OutputSampleProvider class that calls Read and counts frames
	abstract class ResettableSampleProvider : ISampleProvider {

		protected uint lastFrame = 0;

		public ResettableSampleProvider(int sampleRate = 44100) {
			WaveFormat = WaveFormat.CreateIeeeFloatWaveFormat(sampleRate, 1);
		}

		public WaveFormat WaveFormat { get; private set; }
		public int SampleRate { get { return WaveFormat.SampleRate; } }

		/// <summary>
		/// Resets the sample provider or saves the state depending upon the frame number.
		/// Always call this at the beginning of Read(float[], int, int, uint).
		/// </summary>
		/// <param name="frame"></param>
		protected void ResetByFrame(uint frame) {
			if (frame <= lastFrame) {
				Reset();            // Reset to previous frame
			} else {
				SaveResetData();    // We are starting a new frame, so save the state
				lastFrame = frame;  // Save new frame number
			}
		}

		/// <summary>
		/// If frame is greater than it was at the last call, reads the next set of samples
		/// out of the provider as in Read(float[], int, int). Otherwise reads out the same
		/// samples as the previous call.
		/// Allows a sample provider to connect to multiple inputs.
		/// </summary>
		public abstract int Read(float[] buffer, int offset, int count, uint frame);

		/// <summary>
		/// Increments the frame number and calls Read(float[], int, int, uint)
		/// </summary>
		/// <param name="buffer"></param>
		/// <param name="offset"></param>
		/// <param name="count"></param>
		/// <returns></returns>
		public int Read(float[] buffer, int offset, int count) {
			return Read(buffer, offset, count, lastFrame + 1);
		}

		/// <summary>
		/// Resets the state of the sample provider to what it was at the beginning of the
		/// last call to Read().
		/// </summary>
		protected abstract void Reset();

		/// <summary>
		/// Saves the current state of the sample provider so that it can be recalled by Reset().
		/// </summary>
		protected abstract void SaveResetData();
	}
}
