using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using Aura.Core.DTOs;

namespace Aura.Unity.Core
{
    /// <summary>
    /// Handles all communication with the Aura.Api.
    /// Uses a "Premium" async flow for responsive UI.
    /// </summary>
    public class AuraClient : MonoBehaviour
    {
        [Header("API Settings")]
        [SerializeField] private string apiBaseUrl = "http://localhost:5166"; // Updated to match launchSettings.json
        [SerializeField] private float autoRefreshRate = 1.0f;

        public event Action<GraphViewDto> OnGraphReceived;

        private void Start()
        {
            StartCoroutine(AutoRefreshRoutine());
        }

        private IEnumerator AutoRefreshRoutine()
        {
            while (true)
            {
                yield return FetchViewport();
                yield return new WaitForSeconds(autoRefreshRate);
            }
        }

        public IEnumerator FetchViewport()
        {
            using (UnityWebRequest webRequest = UnityWebRequest.Get($"{apiBaseUrl}/api/librarian/viewport"))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.Success)
                {
                    string json = webRequest.downloadHandler.text;
                    try
                    {
                        // Note: Using JsonUtility requires a wrapper for collections
                        GraphViewDto graph = JsonUtility.FromJson<GraphViewDto>(json);
                        
                        // If JsonUtility fails due to DTO structure, consider Newtonsoft.Json
                        // For now we assume the DTO is JsonUtility-compatible or the user will add Newtonsoft
                        OnGraphReceived?.Invoke(graph);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"[AuraClient] Failed to parse graph JSON: {ex.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[AuraClient] API Request failed: {webRequest.error}");
                }
            }
        }

        public void IngestThought(string content)
        {
            StartCoroutine(PostThoughtRoutine(content));
        }

        private IEnumerator PostThoughtRoutine(string content)
        {
            WWWForm form = new WWWForm();
            form.AddField("content", content);

            using (UnityWebRequest webRequest = UnityWebRequest.Post($"{apiBaseUrl}/api/thought", form))
            {
                yield return webRequest.SendWebRequest();

                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[AuraClient] Failed to ingest thought: {webRequest.error}");
                }
            }
        }
    }
}
