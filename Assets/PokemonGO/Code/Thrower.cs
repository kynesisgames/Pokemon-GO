using System.Collections.Generic;
using DG.Tweening;
using Kynesis.Utilities;
using PokemonGO.Code.Global;
using UnityEngine;
using UnityEngine.InputSystem;

namespace PokemonGO.Code
{
    public class Thrower : MonoBehaviour
    {
        [Header("Dragging Settings")]
        [SerializeField] private float _followSpeed = 10;
        [SerializeField] private float _torqueMultiplier = 0.3f;
        
        [Header("Throw Settings")]
        [SerializeField] private float _forceMultiplier = 6;
        [SerializeField] private float _heightMultiplier = 0.5f;
        [SerializeField] private float _curveInfluence = 5;
        [SerializeField] private float _minimumForce = 1f;
        [SerializeField, Range(-1f, 1f)] private float _minimumDot = 0.2f;
        
        [Header("Help Settings")]
        [SerializeField] private Vector3 _helpInfluence = new(0, 0, 0);
        [SerializeField] private float _helpRadius = 2;

        [Header("Bezier")]
        [SerializeField] private Transform _start;
        [SerializeField] private Transform _mid;
        [SerializeField] private Transform _end;
        [SerializeField, Range(1, 10)] private float _extrapolation = 2;
        [SerializeField, Range(3, 100)] private int _points = 10;
        
        [Header("Bindings")]
        [SerializeField] private PokeBall _pokeBall;
        [SerializeField] private Transform _pokeBallSlot;
        [SerializeField] private Collider _pointerCollider;
        [SerializeField] private PokeBallFactory _pokeBallFactory;
        
        private Camera _mainCamera;
        private bool _isDragging;
        
        private float Force => Input.Instance.AveragePointerDelta.magnitude * _forceMultiplier;
        private bool HasPokeBall => _pokeBall != null;
        
        private void Awake()
        {
            _mainCamera = Camera.main;
        }

        private void OnEnable()
        {
            Input.Instance.Pointer.started += OnPointerStarted;
            Input.Instance.Pointer.canceled += OnPointerCanceled;
        }

        private void OnDisable()
        {
            Input.Instance.Pointer.started -= OnPointerStarted;
            Input.Instance.Pointer.canceled -= OnPointerCanceled;
        }
        
        private void StartDragging()
        {
            _pokeBall.transform.position = GetPointerPosition();
            _pokeBall.transform.rotation = _pokeBallSlot.rotation;
            
            _pokeBall.DisableGravity();
            _pokeBall.ClearVelocities();
            
            _isDragging = true;
            StartCoroutine(DraggingEnumerator());
        }
        
        private void StopDragging()
        {
            _isDragging = false;
            
            float dot = Vector2.Dot(Input.Instance.AveragePointerDelta.normalized, Vector2.up);
            bool shouldThrow = dot > _minimumDot &&
                               Force > _minimumForce;
            if (shouldThrow)
                Throw();
            else
                Reset();
        }
        
        private void FollowPointer()
        {
            Vector3 pointerPosition = GetPointerPosition();
            Vector3 pokeBallPosition = _pokeBall.transform.position;
                
            _pokeBall.transform.position = Vector3.Slerp
            (
                pokeBallPosition, 
                pointerPosition, 
                Time.deltaTime * _followSpeed
            );
        }
        
        private void AddToque()
        {
            Vector3 pointerPosition = GetPointerPosition();
            Vector3 pokeBallPosition = _pokeBall.transform.position;
            
            Vector3 deltaDirection =  transform.TransformDirection(Input.Instance.PointerDelta.normalized);
            Vector3 directionToPokeBall = (pokeBallPosition - pointerPosition).normalized;
            
            if (deltaDirection.magnitude > 0)
            {
                Vector3 cross = Vector3.Cross(directionToPokeBall, deltaDirection);
                Vector3 torque = cross * _torqueMultiplier * -1f;
                _pokeBall.AddTorque(torque);
            }
        }
        
        private void Throw()
        {   
            // Start position
            Vector3 startPosition = _pokeBall.transform.position;
            _start.position = startPosition;
            
            // End position
            Vector2 pointerInfluence = new Vector2(1f, 0f); 
            Vector3 pointerDirection = Input.Instance.AveragePointerDelta.normalized;
            Vector3 influencedPointerDirection = Vector3.Scale(pointerDirection, pointerInfluence);
            Vector3 localPointerDirection = transform.TransformDirection(influencedPointerDirection);
            Vector3 throwVector = (localPointerDirection + transform.forward).normalized * Force;
            Vector3 endPosition = startPosition + throwVector;
            
            // Mid position
            Vector3 midPosition = Vector3.Lerp(startPosition, endPosition, 0.5f);
            midPosition.y += Force * _heightMultiplier * (_pokeBall.IsCharged ? 0.5f : 1);
            _mid.position = midPosition;
            
            // Curve influence
            if (_pokeBall.IsCharged)
            {
                Vector3 curveDirection = new Vector3(-localPointerDirection.x, 0, 0).normalized;
                endPosition += curveDirection * _curveInfluence;
            }

            // Approximate the throw to the target
            Transform pokemon = GameObject.FindWithTag(Tag.Pokemon).transform;
            bool isOnHelpRange = Vector3.Distance(pokemon.position, endPosition) < _helpRadius;
            if(isOnHelpRange)
            {
                endPosition.x = Mathf.Lerp(endPosition.x, pokemon.position.x, _helpInfluence.x);
                endPosition.y = Mathf.Lerp(endPosition.y, pokemon.position.y, _helpInfluence.y);
                endPosition.z = Mathf.Lerp(endPosition.z, pokemon.position.z, _helpInfluence.z);
            
                _end.position = endPosition;
            }
            
            // Generate extrapolated path
            List<Vector3> path = Bezier.GetExtrapolatedPath
                (startPosition, midPosition, endPosition, 0f, _extrapolation, _points);
            
            // Throw
            _pokeBall.Throw(path);
            _pokeBall.transform.SetParent(null);
            _pokeBall = null;
            DOVirtual.DelayedCall(1, SpawnPokeBall);
        }
        
        private void SpawnPokeBall()
        {
            _pokeBall = _pokeBallFactory.Create(_pokeBallSlot.position, _pokeBallSlot.rotation);
            _pokeBall.transform.SetParent(_pokeBallSlot);
        }

        private void Reset()
        {   
            _pokeBall.ClearVelocities();
            _pokeBall.transform.position = _pokeBallSlot.position;
            _pokeBall.transform.rotation = _pokeBallSlot.rotation;
        }
        
        private Vector3 GetPointerPosition()
        {
            Vector2 pointerPosition = Input.Instance.PointerPosition;
            Ray ray = _mainCamera.ScreenPointToRay(pointerPosition);
            Physics.Raycast(ray, out RaycastHit grab, float.MaxValue, Layer.Mask.Pointer);
            return grab.point;
        }
        
        private bool IsOnPointerCollider()
        {
            Ray ray = _mainCamera.ScreenPointToRay(Input.Instance.PointerPosition);
            Physics.Raycast(ray, out RaycastHit pointerHit, float.MaxValue, Layer.Mask.Pointer);

            return pointerHit.collider != null && pointerHit.collider == _pointerCollider;
        }
        
        private void OnPointerStarted(InputAction.CallbackContext callbackContext)
        {
            if (!IsOnPointerCollider() || !HasPokeBall) 
                return;
            
            StartDragging();
        }

        private void OnPointerCanceled(InputAction.CallbackContext callbackContext)
        {
            if (!_isDragging) 
                return;
            
            StopDragging();
        }
        
        private IEnumerator<float> DraggingEnumerator()
        {   
            while (_isDragging)
            {
                FollowPointer();
                AddToque();
                yield return 0f;
            }
        }

        private void OnDrawGizmos()
        {
            if (_start == null || _mid == null || _end == null)
                return;
            
            List<Vector3> path = Bezier.GetPath(_start.position, _mid.position, _end.position, _points);
            foreach (Vector3 point in path)
                Gizmos.DrawSphere(point, 0.05f);
        }
    }
}
