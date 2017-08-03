using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SlimDX.XInput;

using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using XANGELIX.Modules;

namespace XANGELIX {
	class Program {
		static void Main(string[] args) {
			// Create audio source
			var baseOscillator = new ModulatedSineWaveProvider();
			baseOscillator.Amplitude = 0f;

			var modulationOscillator = new ModulatedSineWaveProvider();
			modulationOscillator.Frequency = 10d;
			modulationOscillator.Amplitude = 0f;

			baseOscillator.ModulationSampleProvider = modulationOscillator;

			//var noiseSource = new NoiseSampleProvider();

			var filter = new FilterSampleProvider(400);
			filter.InputSampleProvider = baseOscillator;

			var echo = new EchoSampleProvider();
			echo.InputSampleProvider = baseOscillator;

			var mixer = new MixerSampleProvider();
			//mixer.AddInput(baseOscillator, 0f);
			mixer.AddInput(echo, 0f);
			mixer.AddInput(filter, 1f);

			// Create audio player
			var waveOutEvent = new WaveOutEvent();
			waveOutEvent.NumberOfBuffers = 3;
			waveOutEvent.DesiredLatency = 100;
			IWavePlayer player = waveOutEvent;
			player.Init(new SampleToWaveProvider(mixer));   // Set the output module
			player.Play();

			// Make Xbox controller
			var controller = new Controller(UserIndex.One);

			if (!controller.IsConnected) {
				Console.WriteLine("Controller not connected");

				baseOscillator.Amplitude = 1f;

				Console.ReadKey();
				Environment.Exit(1);
			}

			// Synthesize!
			short lastRightThumbX = 0;
			short lastRightThumbY = 0;
			short lastLeftThumbX = 0;
			short lastLeftThumbY = 0;
			byte lastRightTrigger = 0;
			byte lastLeftTrigger = 0;

			while (true) {
				// Get controller state
				var state = controller.GetState();
				var gamepad = state.Gamepad;

				// Set base frequency (calculated as semitones above minimum frequency)
				if (gamepad.RightThumbX != lastRightThumbX) {
					double perUnit = shortToUnit(gamepad.RightThumbX);
					baseOscillator.Frequency = BaseFrequencyMinimum * Math.Pow(SemitoneRatio,
						Math.Round(perUnit * BaseFrequencySemitones));
					lastRightThumbX = gamepad.RightThumbX;
				}

				// Set volume
				if (gamepad.RightTrigger != lastRightTrigger) {
					baseOscillator.Amplitude = (float)byteToUnit(gamepad.RightTrigger);
					lastRightTrigger = gamepad.RightTrigger;
				}

				// Set modulation frequency
				if (gamepad.RightThumbY != lastRightThumbY) {
					double perUnit = shortToUnit(gamepad.RightThumbY);
					//modulationOscillator.Frequency = Math.Pow(10d, 2d * perUnit);	// Exponential
					modulationOscillator.Frequency = ModulationFrequencyMin +
						(ModulationFrequencyMax - ModulationFrequencyMin) * perUnit;    // Linear
					lastRightThumbY = gamepad.RightThumbY;
				}

				// Set modulation amplitude
				if (gamepad.LeftTrigger != lastLeftTrigger) {
					modulationOscillator.Amplitude = (float)(ModulationAmplitudeMax *
						byteToUnit(gamepad.LeftTrigger));
					lastLeftTrigger = gamepad.LeftTrigger;
				}

				// Set filter frequency
				/*if (gamepad.LeftThumbY != lastLeftThumbY) {
					double perUnit = shortToUnit(gamepad.LeftThumbY);
					filter.CornerFrequency = perUnit * (2d * BaseFrequencyMaximum -
						BaseFrequencyMinimum) + BaseFrequencyMinimum;
					lastLeftThumbY = gamepad.LeftThumbY;
				}*/

				// Set echo delay
				if (gamepad.LeftThumbX != lastLeftThumbX) {
					double perUnit = shortToUnit(gamepad.LeftThumbX);
					echo.Delay = (float)perUnit;
					lastLeftThumbX = gamepad.LeftThumbX;
				}

				// Set echo amplitude
				if (gamepad.LeftThumbY != lastLeftThumbY) {
					double perUnit = shortToUnit(gamepad.LeftThumbY);
					echo.Amplitude = (float)perUnit;
					lastLeftThumbY = gamepad.LeftThumbY;
				}

				// Set mix
				if (gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadRight)) {
					// Base oscillator
					mixer.SetGain(0, 1f);
					mixer.SetGain(1, 0f);
				} else if (gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadLeft)) {
					// Echo
					mixer.SetGain(0, 0f);
					mixer.SetGain(1, 1f);
				} else if(gamepad.Buttons.HasFlag(GamepadButtonFlags.DPadUp)) {
					// Base and echo
					mixer.SetGain(0, 0.5f);
					mixer.SetGain(1, 0.5f);
				}

				// Exit
				if (gamepad.Buttons.HasFlag(GamepadButtonFlags.Back)) {
					Environment.Exit(0);
				}
			}
		}

		static readonly double SemitoneRatio = Math.Pow(2d, 1 / 12d);

		const double BaseFrequencyOctaves = 4d;
		const double BaseFrequencySemitones = BaseFrequencyOctaves * 12d;
		const double BaseFrequencyMinimum = 110d;
		static readonly double BaseFrequencyMaximum = BaseFrequencyMinimum * Math.Pow(2d, BaseFrequencyOctaves);

		const double ModulationAmplitudeMax = 100d; // Hz +/-
		const double ModulationFrequencyMax = 100d; // Hz
		const double ModulationFrequencyMin = 1d;   // Hz

		static double byteToUnit(byte b) {
			return (b - byte.MinValue) / (double)(byte.MaxValue - byte.MinValue);
		}

		static double shortToUnit(short n) {
			return (n - short.MinValue) / (double)(short.MaxValue - short.MinValue);
		}
	}
}
