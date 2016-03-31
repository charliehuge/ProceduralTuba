using System;
using UnityEngine;

namespace DerelictComputer
{
	public static class MusicMathUtils
	{
        public const int MidiNoteA440 = 69;

		public static string[] NoteNames = {"C", "C#(Db)", "D", "D#(Eb)", "E", "F", "F#(Gb)", "G", "G#/Ab", "A", "A#(Bb)", "B"};

		private static string[] _noteNamesWithOctave;

		/// <summary>
		/// A human readable list of MIDI note names, for use by the editor, mainly
		/// </summary>
		/// <value>The midi note names.</value>
		public static string[] NoteNamesWithOctave
		{
			get
			{
				if (_noteNamesWithOctave == null)
				{
					_noteNamesWithOctave = new string[128];

					var octave = 0;

					for (int noteNumber = 0; noteNumber < _noteNamesWithOctave.Length; noteNumber += NoteNames.Length)
					{
						for (int noteName = 0; noteName < NoteNames.Length; noteName++)
						{
							_noteNamesWithOctave[noteNumber + noteName] = NoteNames[noteName] + octave;
						}

						octave++;
					}
				}

				return _noteNamesWithOctave;
			}
		}

		/// <summary>
		/// Converts a semitone offset to a percentage pitch
		/// </summary>
		/// <param name="semitones">number of semitones from center</param>
		/// <returns>percentage-based pitch</returns>
		public static float SemitonesToPitch(float semitones)
		{
			return Mathf.Pow(2f, semitones/12f);
		}

		/// <summary>
		/// Converts a semitone offset to a percentage pitch
		/// </summary>
		/// <param name="semitones">number of semitones from center</param>
		/// <returns>percentage-based pitch</returns>
		public static double SemitonesToPitch(double semitones)
		{
			return Math.Pow(2.0, semitones/12.0);
		}

	    public static float PitchToSemitones(float pitch)
	    {
	        // pitch = 2^(semitones/12)
            // log2(pitch) = semitones/12
            // semitones = log2(pitch)*12
	        return Mathf.Log(pitch, 2f)*12;
	    }

		/// <summary>
		/// Converts a MIDI note number to a frequency, based on A 440
		/// </summary>
		/// <param name="midiNote">MIDI note number to convert</param>
		/// <returns></returns>
		public static float MidiNoteToFrequency(int midiNote)
		{
			return 440f*Mathf.Pow(2f, (midiNote - MidiNoteA440)/12f);
		}
	}
}