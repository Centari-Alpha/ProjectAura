using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Aura.Core.DTOs;
using System.Linq;

namespace Aura.Unity.Visualization
{
    /// <summary>
    /// Orchestrates the 3D visualization of the Aura Graph.
    /// Focuses on high-end, atmospheric movement and aesthetics.
    /// </summary>
    public class AtmosphericGraphManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Core.AuraClient auraClient;
        [SerializeField] private GameObject nodePrefab;
        [SerializeField] private GameObject edgePrefab;
        [SerializeField] private Transform playerTransform;

        [Header("Visualization Settings")]
        [SerializeField] private float interpolationSpeed = 5f;
        [SerializeField] private float scaleMultiplier = 10f; // API units to Unity units mapping

        [Header("Global Cluster Settings")]
        [SerializeField] private float clusterHeightOffset = 1.2f;
        [SerializeField] private float clusterDistanceOffset = 3.0f; // Distance pushed away from the player
        [SerializeField] private float clusterOrbitSpeed = 2.0f;  // Speed of the cluster orbiting the player (Galactic)
        [SerializeField] private float clusterLocalSpinSpeed = 1.0f; // Speed of the cluster spinning on its own axis
        [SerializeField] private float lazyFollowSpeed = 0.5f;     // How slowly the cluster drifts toward the player (lower = more lag)

        private class EdgeData
        {
            public GameObject GameObject;
            public LineRenderer Renderer;
            public AuraNode SourceNode;
            public AuraNode TargetNode;
        }

        private Dictionary<string, AuraNode> _activeNodes = new Dictionary<string, AuraNode>();
        private List<EdgeData> _activeEdges = new List<EdgeData>();
        private Transform _orbitPivot;
        private Vector3 _currentPivotVelocity = Vector3.zero;

        private Dictionary<AuraNode, HashSet<AuraNode>> _adjacencyList = new Dictionary<AuraNode, HashSet<AuraNode>>();

        public static AtmosphericGraphManager Instance { get; private set; }
        public AuraNode CurrentGrabbedNode { get; private set; }

        public void SetGrabbedNode(AuraNode node) => CurrentGrabbedNode = node;
        
        public void ClearGrabbedNode(AuraNode node) 
        {
            if (CurrentGrabbedNode == node) CurrentGrabbedNode = null;
        }

        public bool AreNodesConnected(AuraNode a, AuraNode b)
        {
            if (a == null || b == null) return false;
            return _adjacencyList.TryGetValue(a, out var set) && set.Contains(b);
        }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            // Create the central pivot
            _orbitPivot = new GameObject("GalaxyCenterPivot").transform;
            
            // Parent this graph manager to the pivot
            transform.SetParent(_orbitPivot);
        }

        private void OnEnable()
        {
            if (auraClient != null) auraClient.OnGraphReceived += RefreshGraph;
        }

        private void OnDisable()
        {
            if (auraClient != null) auraClient.OnGraphReceived -= RefreshGraph;
        }

        private void RefreshGraph(GraphViewDto graph)
        {
            UpdateNodes(graph.Nodes);
            UpdateEdges(graph.Edges);
        }

        private void UpdateNodes(List<NodeViewDto> nodeDtos)
        {
            HashSet<string> incomingIds = new HashSet<string>();

            foreach (var dto in nodeDtos)
            {
                incomingIds.Add(dto.Id);
                Vector3 targetPos = new Vector3(dto.X, dto.Y, dto.Z) * scaleMultiplier;

                if (_activeNodes.TryGetValue(dto.Id, out AuraNode node))
                {
                    node.UpdateTarget(targetPos, dto.Weight, dto.Essence);
                }
                else
                {
                    // Spawn new node directly into the parent hierarchy
                    GameObject go = Instantiate(nodePrefab, transform);
                    go.transform.localPosition = targetPos; // Force LOCAL position against the offset manager!
                    
                    AuraNode newNode = go.GetComponent<AuraNode>();
                    newNode.Initialize(dto.Id, dto.Content, dto.Essence);
                    _activeNodes.Add(dto.Id, newNode);
                }
            }

            // Cleanup removed nodes
            var toRemove = _activeNodes.Keys.Where(id => !incomingIds.Contains(id)).ToList();
            foreach (var id in toRemove)
            {
                Destroy(_activeNodes[id].gameObject);
                _activeNodes.Remove(id);
            }
        }

        private void UpdateEdges(List<EdgeViewDto> edgeDtos)
        {
            // Rebuild Adjacency List for fast "Nebula Effect" lookups
            _adjacencyList.Clear();
            foreach (var node in _activeNodes.Values)
            {
                _adjacencyList[node] = new HashSet<AuraNode>();
            }

            // Object Pooling implementation: Reuse existing edges to stop stuttering
            int requiredEdges = edgeDtos.Count;
            while (_activeEdges.Count < requiredEdges)
            {
                GameObject edgeObj = Instantiate(edgePrefab, transform);
                LineRenderer lr = edgeObj.GetComponent<LineRenderer>();
                _activeEdges.Add(new EdgeData { GameObject = edgeObj, Renderer = lr });
            }

            // Deactivate any extra edges
            for (int i = requiredEdges; i < _activeEdges.Count; i++)
            {
                _activeEdges[i].GameObject.SetActive(false);
            }

            for (int i = 0; i < requiredEdges; i++)
            {
                var dto = edgeDtos[i];
                var edgeData = _activeEdges[i];

                if (_activeNodes.TryGetValue(dto.SourceId, out AuraNode source) &&
                    _activeNodes.TryGetValue(dto.TargetId, out AuraNode target))
                {
                    _adjacencyList[source].Add(target);
                    _adjacencyList[target].Add(source); // Bidirectional mapping

                    edgeData.GameObject.SetActive(true);
                    edgeData.SourceNode = source;
                    edgeData.TargetNode = target;

                    if (edgeData.Renderer != null)
                    {
                        edgeData.Renderer.positionCount = 2;
                        edgeData.Renderer.SetPosition(0, source.transform.position);
                        edgeData.Renderer.SetPosition(1, target.transform.position);
                        
                        // Premium visual: Width based on strength
                        float width = Mathf.Clamp(dto.Strength * 0.1f, 0.02f, 0.2f);
                        edgeData.Renderer.startWidth = width;
                        edgeData.Renderer.endWidth = width;
                    }
                }
                else
                {
                    edgeData.GameObject.SetActive(false);
                }
            }
        }

        private void Update()
        {
            // Swing the entire graph around the player (Galactic Orbit)
            if (_orbitPivot != null)
            {
                // Identify the player's current physical position in the room
                Vector3 playerPos = playerTransform != null ? playerTransform.position : (Camera.main != null ? Camera.main.transform.position : Vector3.zero);
                
                // Target position: player's XZ position locked to the height offset
                Vector3 targetPivotPos = new Vector3(playerPos.x, clusterHeightOffset, playerPos.z);
                
                // Lazily drift the pivot toward the player — cluster trails behind like a slow-moving nebula
                _orbitPivot.position = Vector3.SmoothDamp(
                    _orbitPivot.position,
                    targetPivotPos,
                    ref _currentPivotVelocity,
                    1f / Mathf.Max(lazyFollowSpeed, 0.01f)
                );
                
                // Spin the pivot, carrying the offset cluster around the player
                _orbitPivot.Rotate(Vector3.up, clusterOrbitSpeed * Time.deltaTime);
            }

            // Continuously apply the distance offset so we can tweak it live in the Inspector
            transform.localPosition = new Vector3(0, 0, clusterDistanceOffset);

            // Slowly rotate the actual cluster on its own axis (Solar System Spin)
            transform.Rotate(Vector3.up, clusterLocalSpinSpeed * Time.deltaTime);

            // Smoothly update line renderer positions to follow orbiting nodes frame-by-frame
            foreach (var edge in _activeEdges)
            {
                if (edge.Renderer != null && edge.SourceNode != null && edge.TargetNode != null)
                {
                    bool isSettled = !edge.SourceNode.IsStartingUp && !edge.TargetNode.IsStartingUp;
                    edge.Renderer.enabled = isSettled;

                    if (isSettled)
                    {
                        edge.Renderer.SetPosition(0, edge.SourceNode.transform.position);
                        edge.Renderer.SetPosition(1, edge.TargetNode.transform.position);
                    }
                }
            }
        }
    }
}
