using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using KinematicCharacterController;
using System;

namespace KinematicCharacterController.Ball
{
    public struct PlayerCharacterInputs
    {
        public float MoveAxisForward;
        public float MoveAxisRight;
        public Quaternion CameraRotation;
        public bool Respawn;
    }

    public class BallController : MonoBehaviour, ICharacterController
    {
        public KinematicCharacterMotor Motor;

        public Camera cam;

        [Header("Stable Movement")]
        public float MaxStableMoveSpeed = 10f;
        public float Drag = 0.1f;
        public float AirTimeMult = 1f;

        [Header("Misc")]
        public bool RotationObstruction;
        public Vector3 Gravity = new Vector3(0, -30f, 0);
        public Transform ParticleHolder;
        public float ParticleVelocityTrigger;

        private Vector3 _targetMovementVelocity = Vector3.zero;
        private Vector3 _addedVel = Vector3.zero;
        private float _stickyness = 0f;
        private float _airTime = 0f;
        private Vector3 _bounceVelocity;
        private Vector3 _hitNormal;
        private bool _respawn;
        private bool _shouldBounce;
        private float _squishyness;
        public Text speedText;



        private void Start()
        {
            // Assign to motor
            Motor.CharacterController = this;
        }

        private void Update()
        {
            //Motor.ForceUnground();
            

        }
        /// <summary>
        /// This is called every frame by MyPlayer in order to tell the character what its inputs are
        /// </summary>
        public void SetInputs(ref PlayerCharacterInputs inputs)
        {

            _respawn = inputs.Respawn;

        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called before the character begins its movement update
        /// </summary>
        public void BeforeCharacterUpdate(float deltaTime)
        {
            if (_respawn) {
                Debug.Log("Respawn");
                Motor.SetPosition(new Vector3(0,1,0)); 
            }
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its rotation should be right now. 
        /// This is the ONLY place where you should set the character's rotation
        /// </summary>
        public void UpdateRotation(ref Quaternion currentRotation, float deltaTime)
        {
            // Set the current rotation (which will be used by the KinematicCharacterMotor)
            currentRotation = Motor.GroundingStatus.IsStableOnGround ? 
                (Quaternion.FromToRotation(Motor.CharacterUp, Motor.GroundingStatus.GroundNormal) * currentRotation) :
                (Quaternion.FromToRotation(Motor.CharacterUp, Vector3.up) * currentRotation);

        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is where you tell your character what its velocity should be right now. 
        /// This is the ONLY place where you can set the character's velocity
        /// </summary>
        public void UpdateVelocity(ref Vector3 currentVelocity, float deltaTime)
        {
            if (_shouldBounce)
            {
                currentVelocity = _hitNormal / _squishyness;
                if (currentVelocity.magnitude > ParticleVelocityTrigger) ParticleHolder.GetComponent<ParticleSystem>().Play();
                _shouldBounce = false;

            }
            currentVelocity += _addedVel;
            Vector3 gNorm = Motor.GroundingStatus.GroundNormal;
            if (!Motor.GroundingStatus.IsStableOnGround)
            {

                _airTime += _airTime <= AirTimeMult ? 1f : 0;
            }
            else
            {
                _airTime = 1f;
            }

            // Reorient source velocity on current ground slope (this is because we don't want our smoothing to cause any velocity losses in slope changes)
            currentVelocity = Motor.GetDirectionTangentToSurface(currentVelocity, gNorm) * currentVelocity.magnitude;
            // Calculate target velocity
            Vector3 inputRight = Vector3.Cross(_addedVel, Motor.CharacterUp);
            Vector3 reorientedInput = Vector3.Cross(gNorm, inputRight).normalized * _addedVel.magnitude;
            _targetMovementVelocity = reorientedInput * MaxStableMoveSpeed;
            // Smooth movement Velocity
            currentVelocity = Vector3.Lerp(currentVelocity, _targetMovementVelocity, 0);

            currentVelocity += (Gravity * _airTime) * deltaTime;

            currentVelocity *= (1f / (1f + (((Drag * .001f) * deltaTime))));
            speedText.text = currentVelocity.magnitude + "";
            _addedVel = Vector3.zero;
        }

        /// <summary>
        /// (Called by KinematicCharacterMotor during its update cycle)
        /// This is called after the character has finished its movement update
        /// </summary>
        public void AfterCharacterUpdate(float deltaTime)
        {
        }

        public bool IsColliderValidForCollisions(Collider coll)
        {

            return true;
        }

        public void OnGroundHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            GroundStickyness groundObj = hitCollider.gameObject.GetComponent<GroundStickyness>();
            if (groundObj != null)
            {
                _stickyness = groundObj.Stickyness;
            }
            else
            {
                _stickyness = 0;
            }
        }

        public void OnMovementHit(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, ref HitStabilityReport hitStabilityReport)
        {
            GroundStickyness groundStickyness = hitCollider.gameObject.GetComponent<GroundStickyness>();

            if (Motor.GroundingStatus.GroundCollider != hitCollider)
            {
                float squish = 1.1f;
                if (groundStickyness != null)
                {
                    squish = groundStickyness.Squishyness;
                }
                _squishyness = squish;
                _hitNormal = Vector3.Reflect(Motor.Velocity, hitNormal);
                ParticleHolder.rotation = Quaternion.FromToRotation(ParticleHolder.rotation.eulerAngles, hitNormal);
                _shouldBounce = true;
            }

        }

        public void PostGroundingUpdate(float deltaTime)
        {
        }

        public void AddVelocity(Vector3 velocity)
        {
            _addedVel -= velocity;
        }

        public void ProcessHitStabilityReport(Collider hitCollider, Vector3 hitNormal, Vector3 hitPoint, Vector3 atCharacterPosition, Quaternion atCharacterRotation, ref HitStabilityReport hitStabilityReport)
        {
        }

        public void OnDiscreteCollisionDetected(Collider hitCollider)
        {
        }
    }
}