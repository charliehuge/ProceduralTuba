using UnityEngine;
using System.Collections;

namespace DerelictComputer
{
    /// <summary>
    /// Just a simple character controller to demonstrate the tuba walking
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class TubaCharacterController : MonoBehaviour
    {
        private const float ControllerDeadZone = 0.01f;

        [SerializeField] private MelodyGenerator _melodyGenerator;
        [SerializeField] private float _stepInterval = 0.5f;
        [SerializeField] private float _hopInterval = 0.3f;
        [SerializeField] private float _stepMoveSpeed = 2f;
        [SerializeField] private float _hopMoveSpeed = 3f;

        private Animator _animator;
        private Transform _camTransform;
        private float _lastStep;
        private bool _lastStepWasLeft;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _camTransform = Camera.main.transform;
        }

        private void Update()
        {
            Vector2 moveVector = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            bool hopping = Input.GetKey(KeyCode.LeftShift);

            if (moveVector.sqrMagnitude > ControllerDeadZone)
            {
                if (hopping && Time.time > _lastStep + _hopInterval)
                {
                    _animator.SetTrigger("Hop");
                    _lastStep = Time.time;
                    _melodyGenerator.TriggerFootstep();
                }
                else if (!hopping && Time.time > _lastStep + _stepInterval)
                {
                    _animator.SetTrigger(_lastStepWasLeft ? "StepRight" : "StepLeft");
                    _lastStepWasLeft = !_lastStepWasLeft;
                    _lastStep = Time.time;
                    _melodyGenerator.TriggerFootstep();
                }

                Vector3 camForward = Vector3.Scale(_camTransform.forward, new Vector3(1, 0, 1)).normalized;
                Vector3 worldMoveDir = camForward * moveVector.y + _camTransform.right * moveVector.x;
                transform.Translate(worldMoveDir * Time.deltaTime * (hopping ? _hopMoveSpeed : _stepMoveSpeed), Space.World);
                transform.forward = worldMoveDir;
            }
            else
            {
                _melodyGenerator.StopPlaying();
                _melodyGenerator.Reset();
            }
        }
    }
}