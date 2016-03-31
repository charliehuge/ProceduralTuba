using UnityEngine;

namespace DerelictComputer
{
    public class MelodyGenerator : MonoBehaviour
    {
        private const int RootNote = 24; // the root if our key is C
        private const double Latency = 0.1;

        public int Key;

        [SerializeField] private Tuba _tuba;

        private bool _playing;
        private int _currentTick;
        private double _interval; 
        private double _nextTickTime;
        private double _lastFootstepTime;
        private bool _isRunning;

        public void Reset()
        {
            _currentTick = 0;
            _lastFootstepTime = -1;
            _playing = false;
            Key = (int) (Random.value*12);
        }

        public void TriggerFootstep(bool isRunning)
        {
            _isRunning = isRunning;

            // if we had a previous footstep, start auto playing
            if (_lastFootstepTime > 0)
            {
                // basically tap tempo with footsteps
                var newInterval = (AudioSettings.dspTime - _lastFootstepTime)/3;

                // filter out anything lower than our latency, because that probably means it's erroneous
                if (newInterval > Latency)
                {
                    // if we're running, set the interval to 8th notes
                    if (_isRunning)
                    {
                        _interval = (AudioSettings.dspTime - _lastFootstepTime)/2;
                    }
                    // if we're walking, set the interval to triplet 8th notes
                    else
                    {
                        _interval = (AudioSettings.dspTime - _lastFootstepTime)/3;
                    }

                    // if we weren't auto-playing, start doing that
                    if (!_playing)
                    {
                        _nextTickTime = AudioSettings.dspTime;
                        _playing = true;
                    }
                }
            }
            // if we just reset, just play a note now
            else
            {
                _tuba.PlayNote(SelectNote(), _lastFootstepTime, SelectDuration());
            }

            _lastFootstepTime = AudioSettings.dspTime;
        }

        public void StopPlaying()
        {
            _playing = false;
        }

        private void Update()
        {
            if (!_playing)
            {
                return;
            }

            if (AudioSettings.dspTime + Latency > _nextTickTime)
            {
                if (SelectShouldPlayNote())
                {
                    _tuba.PlayNote(SelectNote(), _nextTickTime, SelectDuration());
                }
                _nextTickTime += _interval;
                
                // increment the tick
                // if we're running, wrap on 8 ticks (4/4, 8th note tick)
                // if we're walking, wrap on 6 ticks (6/8, 8th note tick)
                _currentTick = (_currentTick + 1)%(_isRunning ? 8 : 6);
            }
        }

        private bool SelectShouldPlayNote()
        {
            if (_isRunning)
            {
                // always play on 1 and 3
                if (_currentTick == 0 || _currentTick == 4)
                {
                    return true;
                }

                // usually play on 2 and 4
                if (_currentTick == 2 || _currentTick == 6)
                {
                    return Random.value < 0.85f;
                }

                // occasionally play the upbeats
                return Random.value < 0.25f;
            }
            else
            {
                // always play on the downbeats
                if (_currentTick == 0 || _currentTick == 3)
                {
                    return true;
                }

                // usually play the pickup to beat 2
                if (_currentTick == 2)
                {
                    return Random.value < 0.75f;
                }

                // sometimes play the pickup to beat 1
                if (_currentTick == 5)
                {
                    return Random.value < 0.5f;
                }

                // and occasionally play the others
                return Random.value < 0.1f;
            }
        }

        private int SelectNote()
        {
            if (_isRunning)
            {
                // play the root on 1 and 3
                if (_currentTick == 0 || _currentTick == 4)
                {
                    return Key + RootNote + (Random.value < 0.5 ? 12 : (Random.value < 0.75 ? 0 : 24));
                }

                // play the 5th on 2 and 4
                if (_currentTick == 2 || _currentTick == 6)
                {
                    return Key + RootNote + (Random.value < 0.5 ? 7 : 19);
                }

                // play either the 6th or 3rd on the upbeats of 1 and 3
                if (_currentTick == 1 || _currentTick == 5)
                {
                    return Key + RootNote + (Random.value < 0.5 ? 9 : 16);
                }

                // play either the 7th or 2nd on the upbeats of 2 and 4
                return Key + RootNote + (Random.value < 0.5 ? 11 : 14);
            }
            else
            {
                // play the root on 1
                if (_currentTick == 0)
                {
                    return Key + RootNote + (Random.value < 0.5 ? 12 : (Random.value < 0.75 ? 0 : 24));
                }

                // play the 5th or 4th on 2
                if (_currentTick == 3)
                {
                    return Key + RootNote + (Random.value < 0.5 ? 7 : 17);
                }

                // play the 3rd on the pickup to 2
                if (_currentTick == 2)
                {
                    return Key + RootNote + 16;
                }

                // play the 6th or 3rd on the and of 2
                if (_currentTick == 4)
                {
                    return Key + RootNote + (Random.value < 0.5 ? 9 : 16);
                }

                // play the 7th or 2nd on the pickup to 1
                if (_currentTick == 5)
                {
                    return Key + RootNote + (Random.value < 0.5 ? 11 : 14);
                }

                // or play the 7th or 2nd on the other notes
                return Key + RootNote + (Random.value < 0.5 ? 11 : 14);
            }
        }

        private double SelectDuration()
        {
            return _interval;
        }
    }
}
