using UnityEngine;

namespace DerelictComputer
{
    public class MelodyGenerator : MonoBehaviour
    {
        private const int RootNote = 24; // the root if our key is C

        public int Key;

        [SerializeField] private Tuba _tuba;

        private bool _playing;
        private int _currentTick;
        private double _interval = 0.18; // triplet 8ths at 120BPM 
        private double _latency = 0.1;
        private double _nextTickTime;
        private double _lastFootstepTime;

        public void Reset()
        {
            _currentTick = 0;
            _lastFootstepTime = -1;
            _playing = false;
            Key = (int) (Random.value*12);
        }

        public void TriggerFootstep()
        {
            if (_lastFootstepTime > 0)
            {
                // basically tap tempo with footsteps
                var newInterval = (AudioSettings.dspTime - _lastFootstepTime)/3;

                // filter out anything lower than our latency, because that probably means it's erroneous
                if (newInterval > _latency)
                {
                    _interval = (AudioSettings.dspTime - _lastFootstepTime) / 3;
                    Debug.Log(_interval);

                    if (!_playing)
                    {
                        _nextTickTime = AudioSettings.dspTime;
                        _playing = true;
                    }
                }
            }
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

            if (AudioSettings.dspTime + _latency > _nextTickTime)
            {
                if (SelectShouldPlayNote())
                {
                    _tuba.PlayNote(SelectNote(), _nextTickTime, SelectDuration());
                }
                _nextTickTime += _interval;
                _currentTick = (_currentTick + 1)%6;
            }
        }

        private bool SelectShouldPlayNote()
        {
            if (_currentTick == 0 || _currentTick == 3)
            {
                return true;
            }

            if (_currentTick == 2)
            {
                return Random.value < 0.75f;
            }

            if (_currentTick == 5)
            {
                return Random.value < 0.5f;
            }
            
            return Random.value < 0.1f;
        }

        private int SelectNote()
        {
            if (_currentTick == 0)
            {
                return Key + RootNote + (Random.value < 0.5 ? 12 : (Random.value < 0.75 ? 0 : 24));
            }

            if (_currentTick == 3)
            {
                return Key + RootNote + (Random.value < 0.5 ? 7 : 17);
            }

            if (_currentTick == 2)
            {
                return Key + RootNote + 16;
            }

            if (_currentTick == 4)
            {
                return Key + RootNote + (Random.value < 0.5 ? 9 : 16);
            }

            if (_currentTick == 5)
            {
                return Key + RootNote + (Random.value < 0.5 ? 11 : 14);
            }

            return Key + RootNote + (Random.value < 0.5 ? 11 : 14);
        }

        private double SelectDuration()
        {
            return _interval;
        }
    }
}
