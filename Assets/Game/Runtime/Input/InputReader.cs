using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BubbleShot.Runtime.Input
{
    /// <summary>
    /// Facade bridging Unity Input System touch and mouse pointer inputs to gameplay commands.
    /// </summary>
    public class InputReader : MonoBehaviour
    {
        public event Action<Vector2>? AimingStarted;
        public event Action<Vector2>? AimingUpdated;
        public event Action<Vector2>? AimingReleased;

        [SerializeField] private bool _inputEnabled = true;

        public bool IsInputEnabled
        {
            get => _inputEnabled;
            set => _inputEnabled = value;
        }

        private bool _isAiming;

        public void ProcessPointerDown(Vector2 screenPosition)
        {
            if (!_inputEnabled) return;
            _isAiming = true;
            AimingStarted?.Invoke(screenPosition);
        }

        public void ProcessPointerMove(Vector2 screenPosition)
        {
            if (!_inputEnabled || !_isAiming) return;
            AimingUpdated?.Invoke(screenPosition);
        }

        public void ProcessPointerUp(Vector2 screenPosition)
        {
            if (!_inputEnabled || !_isAiming) return;
            _isAiming = false;
            AimingReleased?.Invoke(screenPosition);
        }
    }
}
