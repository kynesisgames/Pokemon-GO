using System;
using System.Collections.Generic;
using DG.Tweening;
using Kynesis.Utilities;
using UnityEngine;

namespace PokemonGO
{
    public class PokeBall : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float _chargedAngularSpeedPercentage;
        [SerializeField] private float _bounceMultiplier = 2;
        [SerializeField] private AnimationCurve _speedCurve;
        
        [Header("Bindings")]
        [SerializeField] private Rigidbody _rigidbody;
        
        private bool _isCharged;
        private Vector3 _lastFramePosition;

        private Tween _followPathTween;
        
        public event Action OnCharged;
        public event Action OnDischarged;
        public event Action OnThrown;
        public event Action<Collision> OnCollision;
        
        public bool IsCharged => _isCharged;
        private bool IsFollowingPath => _followPathTween is {active: true} && !_followPathTween.IsComplete();
        public Vector3 AngularVelocity => _rigidbody.angularVelocity;

        private void Update()
        {
            float chargedAngularSpeed = Physics.defaultMaxAngularSpeed * _chargedAngularSpeedPercentage;
            bool shouldCharge = Mathf.Abs(_rigidbody.angularVelocity.magnitude) > chargedAngularSpeed;

            if (shouldCharge && !_isCharged)
                Charge();

            if (!shouldCharge && _isCharged)
                Discharge();
        }

        private void FixedUpdate()
        {
            _lastFramePosition = _rigidbody.position;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (!IsFollowingPath)
                return;
            
            _followPathTween.Kill();
            
            EnableGravity();
            
            ContactPoint contact = other.GetContact(0);
            SimulateBounce(contact);
            
            OnCollision?.Invoke(other);
        }
        
        private void Charge()
        {
            _isCharged = true;
            
            OnCharged?.Invoke();
        }

        private void Discharge()
        {
            _isCharged = false;
            
            OnDischarged?.Invoke();
        }
        
        public void ClearVelocities()
        {
            _rigidbody.velocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
        }

        private void EnableGravity()
        {
            _rigidbody.useGravity = true;
        }
        
        public void DisableGravity()
        {
            _rigidbody.useGravity = false;
        }

        public void AddTorque(Vector3 torque)
        {
            _rigidbody.AddTorque(torque);
        }

        public void Throw(List<Vector3> path)
        {
            float magnitude = path.Magnitude();
            float duration = magnitude / _speedCurve.Evaluate(magnitude);
            
            _followPathTween = transform.DOPath(path.ToArray(), duration)
                .SetUpdate(UpdateType.Fixed)
                .SetEase(Ease.Linear)
                .OnComplete(OnCompletePath);
            
            OnThrown?.Invoke();
        }

        private void OnCompletePath()
        {
            EnableGravity();
            Vector3 lastMotion = _rigidbody.position - _lastFramePosition; 
            _rigidbody.AddForce(lastMotion, ForceMode.Impulse);
        }

        private void SimulateBounce(ContactPoint contact)
        {
            Vector3 impactDirection = (_rigidbody.position - _lastFramePosition).normalized;
            Vector3 impactVelocity = impactDirection * _bounceMultiplier;
            Vector3 normal = contact.normal;
            Vector3 force = Vector3.Reflect(impactVelocity, normal);
            _rigidbody.AddForce(force, ForceMode.Impulse);
        }
    }
}