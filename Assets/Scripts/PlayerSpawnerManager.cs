using UnityEngine;
using Unity.Netcode;
using System.Collections;
using System.Collections.Generic;

public class PlayerSpawnManager : MonoBehaviour
{
    [Header("Puntos de Spawn")]
    public Transform[] spawnPoints;

    private Dictionary<ulong, MaskType> pendingMasks = new Dictionary<ulong, MaskType>();

    private void Start()
    {
        // Solo el Servidor/Host debe ejecutar la lógica de spawn
        if (NetworkManager.Singleton != null && NetworkManager.Singleton.IsServer)
        {
            // Asignamos las funciones directamente aquí en la escena de juego
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

            Debug.Log("<color=green>SpawnManager activado en la escena de juego.</color>");
        }
    }

    private void ConfigureServer()
    {
        NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        Debug.Log("<color=cyan>Servidor configurado: Callback de aprobación listo.</color>");
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        // Esta parte SOLO corre en el Host/Server
        response.Approved = true;
        response.CreatePlayerObject = true;

        int playerIndex = NetworkManager.Singleton.ConnectedClientsIds.Count;

        if (playerIndex >= spawnPoints.Length) playerIndex = 0;

        response.Position = spawnPoints[playerIndex].position;
        response.Rotation = spawnPoints[playerIndex].rotation;

        MaskType maskToAssign = (MaskType)playerIndex;
        pendingMasks[request.ClientNetworkId] = maskToAssign;
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (pendingMasks.ContainsKey(clientId))
        {
            StartCoroutine(AsignarMascaraConRetraso(clientId));
        }
    }

    private IEnumerator AsignarMascaraConRetraso(ulong clientId)
    {
        // CAMBIO CRÍTICO: WaitForSecondsRealtime para que funcione con Time.timeScale = 0
        yield return new WaitForSecondsRealtime(0.2f);

        if (NetworkManager.Singleton.ConnectedClients.TryGetValue(clientId, out var client))
        {
            var playerNetworkObject = client.PlayerObject;
            if (playerNetworkObject != null)
            {
                PlayerMask maskScript = playerNetworkObject.GetComponent<PlayerMask>();
                if (maskScript != null)
                {
                    maskScript.assignedMask.Value = pendingMasks[clientId];
                    pendingMasks.Remove(clientId);
                    Debug.Log($"<color=green>Máscara {maskScript.assignedMask.Value} asignada al cliente {clientId}</color>");
                }
            }
        }
    }

    private void OnDestroy()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnServerStarted -= ConfigureServer;
        }
    }
}