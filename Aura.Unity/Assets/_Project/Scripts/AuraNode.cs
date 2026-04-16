using UnityEngine;
using System;

namespace Aura.Unity.Visualization
{
    /// <summary>
    /// Representation of a single Thought Node.
    /// Implements smooth movement and "Aura" visual effects.
    /// </summary>
    public class AuraNode : MonoBehaviour
    {
        public Guid Id { get; private set; }
        public string Content { get; private set; }
        public string Essence { get; private set; }

        [Header("Movement")]
        [SerializeField] private float smoothTime = 0.3f;
        private Vector3 _targetPosition;
        private Vector3 _currentVelocity;

        [Header("Visuals")]
        [SerializeField] private MeshRenderer glowRenderer;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseAmount = 0.1f;
        
        private Vector3 _baseScale;
        private Material _glowMaterial;

        public void Initialize(Guid id, string content, string essence)
        {
            Id = id;
            Content = content;
            Essence = essence;
            _targetPosition = transform.position;
            _baseScale = transform.localScale;
            
            if (glowRenderer != null)
            {
                _glowMaterial = glowRenderer.material;
                ApplyEssenceColor();
            }
        }

        public void UpdateTarget(Vector3 position, float weight, string essence)
        {
            _targetPosition = position;
            Essence = essence;
            
            // Adjust scale based on weight (importance)
            float targetScale = 1.0f + (weight * 0.5f);
            _baseScale = Vector3.one * targetScale;
            
            ApplyEssenceColor();
        }

        private void Update()
        {
            // Smooth movement towards target (physics calculated on backend)
            transform.position = Vector3.SmoothDamp(transform.position, _targetPosition, ref _currentVelocity, smoothTime);

            // Premium Pulse Effect
            float pulse = 1.0f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            transform.localScale = _baseScale * pulse;
        }

        private void ApplyEssenceColor()
        {
            if (_glowMaterial == null) return;

            // Map "Essence" string to visual colors (Aura specific)
            Color essenceColor = Color.white;
            switch (Essence?.ToLower())
            {
                case "analytical": essenceColor = new Color(0.2f, 0.6f, 1.0f); break; // Blue
                case "creative":   essenceColor = new Color(1.0f, 0.4f, 0.8f); break; // Pink/Neon
                case "chaotic":    essenceColor = new Color(1.0f, 0.3f, 0.1f); break; // Orange/Red
                case "zen":        essenceColor = new Color(0.4f, 1.0f, 0.6f); break; // Emerald
                default:           essenceColor = Color.cyan; break;
            }

            _glowMaterial.SetColor("_EmissionColor", essenceColor * 2.0f); // HDR intensity
            _glowMaterial.color = essenceColor;
        }
    }
}
