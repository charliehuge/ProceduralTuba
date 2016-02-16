using System;
using UnityEngine;
using Random = System.Random;

namespace DerelictComputer
{
    /// <summary>
    /// Monophonic Tuba synthesizer
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class Tuba : MonoBehaviour
    {
        // DEBUG
        [SerializeField] private bool _testPlay;

        [SerializeField, Range(0.01f, 0.5f)] private double _filterAttackTime = 0.1;
        [SerializeField, Range(0.01f, 0.5f)] private double _filterReleaseTime = 0.1;
        [SerializeField, Range(0f, 1000f)] private double _filterNoiseAmount = 1000;
        [SerializeField, Range(200f, 10000f)] private double _filterCutoffMin = 200;
        [SerializeField, Range(200f, 20000f)] private double _filterCutoffMax = 4000;
        [SerializeField, Range(0f, 2f)] private double _filterResonance = 1;
        [SerializeField, Range(0.01f, 0.5f)] private double _volumeAttackTime = 0.1;
        [SerializeField, Range(0.01f, 0.5f)] private double _volumeReleaseTime = 0.1;

        private readonly Random _random = new Random();

        private int _currentNote;
        private double _currentNoteStartTime;
        private double _currentNoteReleaseTime;
        private double _currentNoteStopTime;
        private double _currentNotePhaseIncrement;
        private int _nextNote;
        private double _nextNoteStartTime;
        private double _nextNoteReleaseTime;
        private double _nextNoteStopTime;
        private double _nextNotePhaseIncrement;
        private double _sampleRate;
        private double _sampleDuration;
        private double _phase;
        private double _fIn1, _fIn2, _fIn3, _fIn4, _fOut1, _fOut2, _fOut3, _fOut4;

        public void Reset()
        {
            _currentNote = -1;
            _nextNote = -1;
            _phase = 0;
        }

        public void PlayNote(int midiNote, double startTime, double duration)
        {
            double dspTime = AudioSettings.dspTime;

            // guard against trying to start a note before now, because that's weird
            if (startTime < dspTime)
            {
                startTime = dspTime;
            }

            // if we have a current note, go ahead and start releasing
            // and put this note into the queue
            if (_currentNote >= 0)
            {
                _nextNote = midiNote;
                _nextNoteStartTime = startTime;
                _nextNoteReleaseTime = startTime + duration;
                _nextNoteStopTime = _nextNoteReleaseTime + _volumeReleaseTime;
                _nextNotePhaseIncrement = MusicMathUtils.MidiNoteToFrequency(midiNote)*_sampleDuration;

                // if the next note will start before we have a chance to do the
                // current note's release, squash the release down
                if (_nextNoteStartTime - _currentNoteStopTime < 0)
                {
                    _currentNoteReleaseTime = _nextNoteStartTime - _volumeReleaseTime;
                    _currentNoteStopTime = _nextNoteStartTime;
                }
            }
            // otherwise, just start playing this one when it's ready
            else
            {
                _currentNote = midiNote;
                _currentNoteStartTime = startTime;
                _currentNoteReleaseTime = startTime + duration;
                _currentNoteStopTime = _currentNoteReleaseTime + _volumeReleaseTime;
                _currentNotePhaseIncrement = MusicMathUtils.MidiNoteToFrequency(midiNote)*_sampleDuration;
                _fIn1 = _fIn2 = _fIn3 = _fIn4 = _fOut1 = _fOut2 = _fOut3 = _fOut4 = 0;
                _phase = 0;
            }
        }

        private void Awake()
        {
            Reset();

            _sampleRate = AudioSettings.outputSampleRate;
            _sampleDuration = 1.0 / AudioSettings.outputSampleRate;
            var dummyClip = AudioClip.Create("dummyclip", 1, 1, (int)_sampleRate, false);
            dummyClip.SetData(new float[] {1}, 0);
            var audioSource = GetComponent<AudioSource>();
            audioSource.clip = dummyClip;
            audioSource.loop = true;
            audioSource.Play();
        }

        private void Update()
        {
            if (_testPlay)
            {
                int note = UnityEngine.Random.Range(24, 48); // sane range for tuba
                PlayNote(note, AudioSettings.dspTime, .25);
                _testPlay = false;
            }
        }

        private void OnAudioFilterRead(float[] buffer, int channels)
        {
            // if we don't have a current note, don't do anything.
            if (_currentNote < 0)
            {
                for (int i = 0; i < buffer.Length; i++)
                {
                    buffer[i] = 0;
                }
                return;
            }

            double dspTime = AudioSettings.dspTime;

            for (int i = 0; i < buffer.Length; i += channels)
            {
                double sample = 0;

                if (_currentNote >= 0 && dspTime > _currentNoteStartTime)
                {
                    // just do a simple saw, we're filtering the signal anyway
                    sample = _phase * 2.0 - 1.0;

                    // apply volume envelope
                    double volumeEnvelopeAmount = 0;

                    if (dspTime < _currentNoteStartTime + _volumeAttackTime)
                    {
                        volumeEnvelopeAmount = Math.Pow((dspTime - _currentNoteStartTime)/_volumeAttackTime, 4);
                    }
                    else if (dspTime < _currentNoteReleaseTime)
                    {
                        volumeEnvelopeAmount = 1;
                    }
                    else if (dspTime < _currentNoteStopTime)
                    {
                        volumeEnvelopeAmount =
                            Math.Pow((_currentNoteStopTime - dspTime) /_volumeReleaseTime, 4);
                    }
                    // else attenuate fully

                    sample *= volumeEnvelopeAmount;

                    double filterCutoff = _filterCutoffMin;
                    
                    // apply filter envelope
                    // linear interpolation for now...
                    if (dspTime < _currentNoteStartTime + _filterAttackTime)
                    {
                        filterCutoff = ((dspTime - _currentNoteStartTime)/_filterAttackTime)*
                                       (_filterCutoffMax - _filterCutoffMin) + _filterCutoffMin;
                    }
                    else if (dspTime < _currentNoteReleaseTime)
                    {
                        filterCutoff = _filterCutoffMax;
                    }
                    else if (dspTime < _currentNoteStopTime)
                    {
                        filterCutoff = ((_currentNoteStopTime - dspTime) / _filterReleaseTime) *
                                       (_filterCutoffMax - _filterCutoffMin) + _filterCutoffMin;
                    }

                    // apply some noise to the filter
                    filterCutoff += _random.NextDouble()*_filterNoiseAmount;

                    // apply low pass filter
                    // pretty much this Moog-style VCF verbatim: http://musicdsp.org/showArchiveComment.php?ArchiveID=26
                    double f = (filterCutoff / _sampleRate) * 1.16f;
                    double fb = _filterResonance * (1f - 0.15f * f * f);
                    double tmpSamp = sample - _fOut4 * fb;
                    tmpSamp *= 0.35013f * (f * f) * (f * f);
                    _fOut1 = tmpSamp + 0.3f * _fIn1 + (1f - f) * _fOut1; // Pole 1
                    _fIn1 = tmpSamp;
                    _fOut2 = _fOut1 + 0.3f * _fIn2 + (1f - f) * _fOut2;  // Pole 2
                    _fIn2 = _fOut1;
                    _fOut3 = _fOut2 + 0.3f * _fIn3 + (1f - f) * _fOut3;  // Pole 3
                    _fIn3 = _fOut2;
                    _fOut4 = _fOut3 + 0.3f * _fIn4 + (1f - f) * _fOut4;  // Pole 4
                    _fIn4 = _fOut3;
                    sample = _fOut4;

                    // if it's time to end the note, switch to the next note or just stop
                    if (dspTime >= _currentNoteStopTime)
                    {
                        if (_nextNote >= 0)
                        {
                            _currentNote = _nextNote;
                            _nextNote = -1;
                            _currentNoteStartTime = _nextNoteStartTime;
                            _currentNoteReleaseTime = _nextNoteReleaseTime;
                            _currentNoteStopTime = _nextNoteStopTime;
                            _currentNotePhaseIncrement = _nextNotePhaseIncrement;
                            _fIn1 = _fIn2 = _fIn3 = _fIn4 = _fOut1 = _fOut2 = _fOut3 = _fOut4 = 0;
                        }
                        else
                        {
                            _currentNote = -1;
                        }
                    }

                    _phase += _currentNotePhaseIncrement;

                    if (_phase > 1.0)
                    {
                        _phase -= 1.0;
                    }
                }

                // put the sample in the buffer
                for (int j = 0; j < channels; j++)
                {
                    buffer[i + j] *= (float)sample;
                }

                // increment the current time by the sample duration
                dspTime += _sampleDuration;
            }
        }
    }
}