using UnityEngine;
using System;
using Oculus.Interaction;
using Oculus.Interaction.Surfaces;

namespace Aura.Unity.Visualization
{
    /// <summary>
    /// Representation of a single Thought Node.
    /// Implements smooth movement and "Aura" visual effects.
    /// </summary>
    [DefaultExecutionOrder(200)] // Ensure this runs AFTER Meta XR Interaction SDK overrides
    public class AuraNode : MonoBehaviour
    {
        public string Id { get; private set; }
        public string Content { get; private set; }
        public string Essence { get; private set; }

        [Header("Movement")]
        [SerializeField] private float smoothTime = 0.3f;
        private Vector3 _targetPosition;
        private Vector3 _currentAnchor;
        private Vector3 _currentVelocity;

        [Header("Orbiting")]
        [SerializeField] private float orbitRadiusBase = 2f;
        [SerializeField] private float orbitSpeedBase = 15f;
        
        private float _orbitAngle;
        private float _orbitSpeed;
        private float _orbitRadius;
        private float _orbitYOffset;

        [Header("Startup Phase")]
        [SerializeField] private float startupDuration = 4.0f;
        [SerializeField] private float startupSpeedMultiplier = 8.0f;
        private float _startupTimer;
        
        public bool IsStartingUp => _startupTimer > 0;

        [Header("Visuals")]
        [SerializeField] private MeshRenderer glowRenderer;
        [SerializeField] private float pulseSpeed = 2f;
        [SerializeField] private float pulseAmount = 0.1f;
        
        private Vector3 _baseScale;
        private Vector3 _initialPrefabScale;
        private Vector3 _currentScale;
        private Material _glowMaterial;
        
        private static bool _rayDistanceFixed = false;

        public void Initialize(string id, string content, string essence)
        {
            Id = id;
            Content = content;
            Essence = essence;
            _targetPosition = transform.localPosition;
            _currentAnchor = _targetPosition;
            _initialPrefabScale = transform.localScale;
            _baseScale = _initialPrefabScale;
            _currentScale = _baseScale;
            
            // Create a deterministic but pseudo-random orbit profile based on ID
            int hash = string.IsNullOrEmpty(id) ? 0 : id.GetHashCode();
            System.Random random = new System.Random(hash);
            
            _orbitRadius = orbitRadiusBase + (float)(random.NextDouble() * 3f);
            
            // Speed and direction
            _orbitSpeed = orbitSpeedBase + (float)(random.NextDouble() * 10f);
            if (random.NextDouble() > 0.5) _orbitSpeed *= -1; 
            
            // Starting position within the orbit
            _orbitAngle = (float)(random.NextDouble() * 360f);
            _orbitYOffset = (float)((random.NextDouble() - 0.5) * 4f); // Vertical distribution

            if (glowRenderer != null)
            {
                _glowMaterial = glowRenderer.material;
                ApplyEssenceColor();
            }
            
            _startupTimer = startupDuration;

            // Automatically detect Meta XR Grabbable component to wire up grabbing
            var pointable = GetComponent<PointableElement>();
            if (pointable != null)
            {
                pointable.WhenPointerEventRaised += HandlePointerEvent;
            }


            // Globally increase the reach of all XR Rays so you can grab distant nodes
            if (!_rayDistanceFixed)
            {
                _rayDistanceFixed = true;
                foreach (var ray in FindObjectsOfType<RayInteractor>(true))
                {
                    ray.MaxRayLength = 25f;
                }
            }
        }

        private void OnDestroy()
        {
            var pointable = GetComponent<PointableElement>();
            if (pointable != null)
            {
                pointable.WhenPointerEventRaised -= HandlePointerEvent;
            }
        }

        private void HandlePointerEvent(PointerEvent evt)
        {
            // Turn grabbing into a toggle instead of a hold
            if (evt.Type == PointerEventType.Select)
            {
                if (IsGrabbed)
                {
                    // If it's already grabbed, a second trigger pull dismisses it
                    OnGrabEnd();
                }
                else
                {
                    // If another node is currently active as the terminal, dismiss it first
                    var graphMgr = AtmosphericGraphManager.Instance;
                    if (graphMgr != null && graphMgr.CurrentGrabbedNode != null && graphMgr.CurrentGrabbedNode != this)
                    {
                        graphMgr.CurrentGrabbedNode.OnGrabEnd();
                    }
                    
                    OnGrabBegin();
                }
            }
            // We ignore Unselect because we want it to stay docked when the trigger is released
        }

        public void UpdateTarget(Vector3 position, float weight, string essence)
        {
            _targetPosition = position;
            Essence = essence;
            
            // Adjust scale based on weight (importance) but respect the prefab's starting scale!
            float targetScale = 1.0f + (weight * 0.5f);
            if (_initialPrefabScale == Vector3.zero) _initialPrefabScale = transform.localScale;
            _baseScale = _initialPrefabScale * targetScale;
            
            ApplyEssenceColor();
        }

        public bool IsGrabbed { get; private set; }

        public void OnGrabBegin()
        {
            IsGrabbed = true;
            AtmosphericGraphManager.Instance?.SetGrabbedNode(this);
            
            // Short "Ping" interaction scale bump
            _currentScale = _baseScale * 1.4f;
            transform.localScale = _currentScale;
        }

        public void OnGrabEnd()
        {
            IsGrabbed = false;
            AtmosphericGraphManager.Instance?.ClearGrabbedNode(this);
        }

        private void LateUpdate()
        {
            if (IsGrabbed)
            {
                // Dock the node to the lower center of the user's viewport (Model Terminal style)
                Camera cam = Camera.main;
                if (cam == null) cam = FindObjectOfType<Camera>(); // Fallback if MainCamera tag is missing!

                if (cam != null)
                {
                    // 0.6 meters forward, 0.25 meters down
                    Vector3 targetPos = cam.transform.position + (cam.transform.forward * 0.6f) + (cam.transform.up * -0.25f);
                    
                    // Face the user
                    Quaternion targetRot = Quaternion.LookRotation(transform.position - cam.transform.position);

                    // Smoothly transition to the viewport dock
                    transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * 10f);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, Time.deltaTime * 10f);

                    // Update the anchor in local space so it returns smoothly when released
                    if (transform.parent != null)
                    {
                        _currentAnchor = transform.parent.InverseTransformPoint(transform.position);
                    }
                    else
                    {
                        _currentAnchor = transform.position;
                    }
                }

                // Scale down to a basketball size (approx 0.18x of base scale) when docked
                float grabbedPulse = 1.0f + Mathf.Sin(Time.time * pulseSpeed * 2f) * (pulseAmount * 0.5f);
                Vector3 dockedScale = _baseScale * 0.18f * grabbedPulse;
                _currentScale = Vector3.Lerp(_currentScale, dockedScale, Time.deltaTime * 10f);
                transform.localScale = _currentScale;
                
                return;
            }

            // Determine orbit speed and anchor point based on startup phase
            float currentSpeedMultiplier = 1f;
            float targetScaleMultiplier = 1f;
            Vector3 desiredAnchor = _targetPosition;

            if (_startupTimer > 0)
            {
                _startupTimer -= Time.deltaTime;
                
                // Calculate progress (1 = just spawned, 0 = fully settled)
                float startupProgress = Mathf.Clamp01(_startupTimer / startupDuration);
                
                // Smooth easing curve so it doesn't snap abruptly
                float easeProgress = Mathf.Pow(startupProgress, 2f); 
                
                currentSpeedMultiplier = 1f + (startupSpeedMultiplier * easeProgress);
                
                // All nodes converge from the map origin (0,0,0) and spiral outward to their target positions.
                desiredAnchor = Vector3.Lerp(_targetPosition, Vector3.zero, easeProgress);
            }
            else
            {
                // Nebula Effect: Drift towards grabbed star if related
                var graphMgr = AtmosphericGraphManager.Instance;
                if (graphMgr != null && graphMgr.CurrentGrabbedNode != null && graphMgr.CurrentGrabbedNode != this)
                {
                    if (graphMgr.AreNodesConnected(this, graphMgr.CurrentGrabbedNode))
                    {
                        Vector3 grabTargetLocal = graphMgr.CurrentGrabbedNode.transform.localPosition;
                        
                        // We want the related node to drift toward the held one, 
                        // but stay orbitRadius clear of its exact center.
                        Vector3 directionToGrabbed = (grabTargetLocal - _targetPosition).sqrMagnitude > 0.01f ? 
                            (grabTargetLocal - _targetPosition).normalized : UnityEngine.Random.onUnitSphere;

                        // Desired anchor shifts towards the grabbed star
                        desiredAnchor = grabTargetLocal - (directionToGrabbed * (_orbitRadius * 1.25f));
                        
                        // Slow down the orbit so the user can easily read/grab other nodes in the cluster
                        currentSpeedMultiplier = 0.2f; 
                        
                        // Shrink the related nodes so they don't block the screen as they drift closer (approx double the grabbed node)
                        targetScaleMultiplier = 0.36f;
                    }
                }
            }

            // Smoothly move the actual anchor towards the desired anchor
            _currentAnchor = Vector3.Lerp(_currentAnchor, desiredAnchor, Time.deltaTime * 3.0f);

            // Update Orbit Angle
            _orbitAngle += _orbitSpeed * currentSpeedMultiplier * Time.deltaTime;
            
            // Calculate orbital offset around the anchor position
            Vector3 orbitOffset = new Vector3(
                Mathf.Cos(_orbitAngle * Mathf.Deg2Rad) * _orbitRadius,
                _orbitYOffset,
                Mathf.Sin(_orbitAngle * Mathf.Deg2Rad) * _orbitRadius
            );

            // Smooth movement towards the orbiting position inside Local Space
            Vector3 finalTarget = _currentAnchor + orbitOffset;
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, finalTarget, ref _currentVelocity, smoothTime);

            // Premium Pulse Effect (Smoothly lerp back to normal size if ungrabbed)
            float pulse = 1.0f + Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;
            Vector3 orbitScale = _baseScale * targetScaleMultiplier * pulse;
            _currentScale = Vector3.Lerp(_currentScale, orbitScale, Time.deltaTime * 10f);
            transform.localScale = _currentScale;
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
