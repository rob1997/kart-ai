using System;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;

namespace Core
{
    [RequireComponent(typeof(Rigidbody))]
    public class Motor : MonoBehaviour
    {
        [SerializeField] private Transform centerOfMass;

        [Space]

        [SerializeField] private float maxSpeed = 20f;
        
        //maximum wheel rotation force
        [SerializeField] private float maxTorque = 200f;

        //max steer angle of front wheels
        [SerializeField] private float maxSteerAngle = 30f;

        //max brake torque
        [SerializeField] private float maxBrakeTorque = 5000f;

        //max anti-roll force/ force that stops car from rolling
        [SerializeField] private float maxAntiRollForce = 5000f;

        [Space]
        
        [SerializeField] private AudioSource engineSound;
        
        [SerializeField] private AudioSource skidSound;

        [Space]
        
        [SerializeField] private float engineVibrationAmplitude = .15f;
        
        [SerializeField] private float engineVibrationFrequency = .15f;
        
        [SerializeField] private Transform body;
        
        //all wheels
        private WheelCollider[] _wheels;
        
        private WheelCollider[] _frontWheels;

        private WheelCollider[] _backWheels;

        private Transform[] _wheelMeshContainers;
        
        private ParticleSystem[] _smokeEffects;

        private float _initialEnginePitch;

        private float3 _initialBodyPosition;
        
        private float _vibrationTime;

        private bool _vibrateOut;
        
        //car rigid-body
        public Rigidbody RigidBody { get; private set; }

        public float MaxSpeed => maxSpeed;
        
        public void Initialize()
        {
            RigidBody = GetComponent<Rigidbody>();

            //set center of mass
            RigidBody.centerOfMass = centerOfMass.localPosition;

            _wheels = GetComponentsInChildren<WheelCollider>();

            int length = _wheels.Length;
            
            _wheelMeshContainers = new Transform[length];
            
            for (int i = 0; i < length; i++)
            {
                var wheel = _wheels[i];

                _wheelMeshContainers[i] = wheel.transform.GetChild(0);
            }

            _frontWheels = _wheels.GetFrontWheels(centerOfMass)
                .OrderBy(wheel => wheel.IsRightWheel(centerOfMass))
                .ToArray();

            _backWheels = _wheels.GetBackWheels(centerOfMass)
                .OrderBy(wheel => wheel.IsRightWheel(centerOfMass))
                .ToArray();
            
            _smokeEffects = new ParticleSystem[_backWheels.Length];
            
            for (int i = 0; i < _backWheels.Length; i++)
            {
                WheelCollider wheel = _backWheels[i];
                
                _smokeEffects[i] = wheel.transform.GetComponentInChildren<ParticleSystem>();
            }
            
            
            _initialEnginePitch = engineSound.pitch;
            
            _initialBodyPosition = body.localPosition;
        }

        private void Update()
        {
            CheckForSkid();
            
            // rev sound
            engineSound.pitch = _initialEnginePitch + (Mathf.Abs(RigidBody.linearVelocity.magnitude) / 25f);

            VibrateEffect();
        }

        // vibrate engine effect
        // just move the body up and down
        private void VibrateEffect()
        {
            _vibrationTime += Time.deltaTime;
            
            float normalizedTime = _vibrationTime / engineVibrationFrequency;
            
            float3 vibrationTarget = math.up() * engineVibrationAmplitude;
            
            float3 start = _initialBodyPosition;
                
            float3 end = _initialBodyPosition + vibrationTarget;
                
            if (_vibrateOut)
            {
                body.localPosition = math.lerp(start, end, normalizedTime);
            }

            else
            {
                body.localPosition = math.lerp(end, start, normalizedTime);
            }
            
            if (normalizedTime >= 1)
            {
                _vibrateOut = !_vibrateOut;

                _vibrationTime = 0;   
            }
        }
        
        private void FixedUpdate()
        {
            //anti-roll
            GroundWheels(_frontWheels[0], _frontWheels[1]);
            GroundWheels(_backWheels[0], _backWheels[1]);
        }

        public void Drive(float gas, float steer, float brake)
        {
            gas = gas.BiDirectionalNormalizedClamp();

            steer = steer.BiDirectionalNormalizedClamp();

            brake = brake.NormalizedClamp();

            for (int i = 0; i < _wheels.Length; i++)
            {
                var wheel = _wheels[i];

                //apply torque
                wheel.motorTorque = gas * Torque();

                //apply brake
                wheel.brakeTorque = brake * maxBrakeTorque;

                if (wheel.IsFrontWheel(centerOfMass))
                {
                    //steer front wheels based on direction value
                    wheel.steerAngle = steer * maxSteerAngle;
                }

                //rotate wheel mesh
                wheel.GetWorldPose(out Vector3 position, out Quaternion rotation);

                var meshContainer = _wheelMeshContainers[i];

                if (meshContainer != null)
                {
                    meshContainer.position = position;
                    meshContainer.rotation = rotation;
                }
            }
        }
        
        private void CheckForSkid()
        {
            int numSkidding = 0;

            for (int i = 0; i < _wheels.Length; i++)
            {
                WheelCollider wheel = _wheels[i];
                
                int index = Array.IndexOf(_backWheels, wheel);
                
                if (wheel.GetGroundHit(out WheelHit hit))
                {
                    if (math.abs(hit.forwardSlip) >= .4f || math.abs(hit.sidewaysSlip) >= .4f)
                    {
                        numSkidding++;

                        if (numSkidding == 4 && !skidSound.isPlaying)
                        {
                            skidSound.Play();
                        }

                        if (index != - 1)
                        {
                            _smokeEffects[index].Play();
                        }
                    }

                    else
                    {
                        if (index != - 1)
                        {
                            _smokeEffects[index].Stop();
                        }
                    }
                }
            }

            if (numSkidding != 4 && skidSound.isPlaying)
            {
                skidSound.Stop();
            }
        }

        private float Torque()
        {
            float speed = math.length(RigidBody.linearVelocity);

            float normalizedSpeed = math.clamp(speed / maxSpeed, 0, 1);
            
            // the closer it gets to its maximum speed the more we decrease torque
            return math.lerp(maxTorque, 0, normalizedSpeed);
        }

        // Reset's the motor with new position and rotation
        public void Setup(float3 position, Quaternion rotation)
        {
            RigidBody.linearVelocity = Vector3.zero;
            
            RigidBody.angularVelocity = Vector3.zero;

            transform.SetPositionAndRotation(position, rotation);
        }
        
        /// <summary>
        /// anti-roll function that stops car from rolling
        /// </summary>
        void GroundWheels(WheelCollider rightWheel, WheelCollider leftWheel)
        {
            float travelRight = 1f;
            float travelLeft = 1f;

            bool isRightGrounded = GetTravel(rightWheel, ref travelRight);
            bool isLeftGrounded = GetTravel(leftWheel, ref travelLeft);

            float antiRollForce = (travelLeft - travelRight) * maxAntiRollForce;

            if (isRightGrounded)
            {
                ApplyAntiRollForce(rightWheel.transform, antiRollForce);
            }

            if (isLeftGrounded)
            {
                ApplyAntiRollForce(leftWheel.transform, -antiRollForce);
            }

            //how far the wheel has traveled from the ground along it's suspension
            bool GetTravel(WheelCollider wheel, ref float travel)
            {
                bool isGrounded = wheel.GetGroundHit(out WheelHit hit);

                if (isGrounded)
                {
                    travel = (-wheel.transform.InverseTransformPoint(hit.point).y - wheel.radius) /
                             wheel.suspensionDistance;
                }

                return isGrounded;
            }

            void ApplyAntiRollForce(Transform wheelTransform, float force)
            {
                RigidBody.AddForceAtPosition(wheelTransform.up * force, wheelTransform.position);
            }
        }
    }
}