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

        [Header("Visualization Settings")]
        [SerializeField] private float interpolationSpeed = 5f;
        [SerializeField] private float scaleMultiplier = 10f; // API units to Unity units mapping

        private Dictionary<System.Guid, AuraNode> _activeNodes = new Dictionary<System.Guid, AuraNode>();
        private List<GameObject> _activeEdges = new List<GameObject>();

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
            HashSet<System.Guid> incomingIds = new HashSet<System.Guid>();

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
                    // Spawn new node
                    GameObject go = Instantiate(nodePrefab, targetPos, Quaternion.identity, transform);
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
            // Simple edge implementation: Clear and redraw or pool
            // For high performance, we would use a LineRenderer pool
            foreach (var edge in _activeEdges) Destroy(edge);
            _activeEdges.Clear();

            foreach (var dto in edgeDtos)
            {
                if (_activeNodes.TryGetValue(dto.SourceId, out AuraNode source) &&
                    _activeNodes.TryGetValue(dto.TargetId, out AuraNode target))
                {
                    GameObject edgeObj = Instantiate(edgePrefab, transform);
                    LineRenderer lr = edgeObj.GetComponent<LineRenderer>();
                    if (lr != null)
                    {
                        lr.positionCount = 2;
                        lr.SetPosition(0, source.transform.position);
                        lr.SetPosition(1, target.transform.position);
                        
                        // Premium visual: Width based on strength
                        float width = Mathf.Clamp(dto.Strength * 0.1f, 0.02f, 0.2f);
                        lr.startWidth = width;
                        lr.endWidth = width;
                    }
                    _activeEdges.Add(edgeObj);
                }
            }
        }
    }
}
