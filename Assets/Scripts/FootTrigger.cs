using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

namespace DerelictComputer
{
    public class FootTrigger : MonoBehaviour
    {
        private MelodyGenerator _melodyGenerator;
        private bool _probablyMoving;

        private void Start()
        {
            _melodyGenerator = FindObjectOfType<MelodyGenerator>();
        }

        private void Update()
        {
            bool wasProbablyMoving = _probablyMoving;

            float inputMag = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")).magnitude;
            _probablyMoving = inputMag > 0;

            if (_probablyMoving && !wasProbablyMoving)
            {
                _melodyGenerator.Reset();
            }
            else if (!_probablyMoving && wasProbablyMoving)
            {
                _melodyGenerator.StopPlaying();
            }
        }

        private void OnTriggerEnter(Collider otherCollider)
        {
            if (_probablyMoving && otherCollider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            {
                _melodyGenerator.TriggerFootstep();
            }
        }
    }
}
