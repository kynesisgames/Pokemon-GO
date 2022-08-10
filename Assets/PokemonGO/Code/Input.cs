using System.Collections.Generic;
using Kynesis.Patterns;
using Kynesis.Utilities;
using UnityEngine;
using UnityEngine.InputSystem;
using PlayerInput = PokemonGO.Code.Generated.PlayerInput;

namespace PokemonGO
{
    public class Input : Singleton<Input>
    {
        [SerializeField] private int _deltaHistorySize = 10;
        
        private PlayerInput _input;
        private PlayerInput.PlayerActions _actions;

        private readonly Queue<Vector2> _deltaHistory = new();
        
        public InputAction Pointer => _actions.Pointer;
        public Vector2 PointerPosition => _actions.PointerPosition.ReadValue<Vector2>();
        public Vector2 PointerDelta => _actions.PointerDelta.ReadValue<Vector2>();
        public Vector2 AveragePointerDelta => _deltaHistory.Average();
        
        protected override void Awake()
        {
            base.Awake();
            _input = new PlayerInput();
            _actions = _input.Player;
            
            _input.Enable();
        }
        
        private void Update()
        {
            _deltaHistory.Enqueue(PointerDelta);
            if(_deltaHistory.Count > _deltaHistorySize)
                _deltaHistory.Dequeue();
        }
    }
}
