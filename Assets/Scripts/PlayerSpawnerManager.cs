using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class PlayerSpawnManager : MonoBehaviour
{
    [Header("Puntos de Spawn")]
    public Transform[] spawnPoints; 

    private Dictionary<ulong, MaskType> pendingMasks = new Dictionary<ulong, MaskType>();

    private void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        response.Approved = true;
        response.CreatePlayerObject = true;

        int playerIndex = NetworkManager.Singleton.ConnectedClientsIds.Count;

        if (playerIndex >= spawnPoints.Length)
        {
            Debug.LogWarning("Se alcanzó el máximo de jugadores, spawneando en posición 0");
            playerIndex = 0;
        }

        response.Position = spawnPoints[playerIndex].position;
        response.Rotation = spawnPoints[playerIndex].rotation;

        MaskType maskToAssign = (MaskType)playerIndex;
        pendingMasks[request.ClientNetworkId] = maskToAssign;

        Debug.Log($"Aprobando Jugador {request.ClientNetworkId}. Índice: {playerIndex}, Máscara: {maskToAssign}");
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        if (pendingMasks.ContainsKey(clientId))
        {
            StartCoroutine(AsignarMascaraConRetraso(clientId));
        }
    }

    private System.Collections.IEnumerator AsignarMascaraConRetraso(ulong clientId)
    {
        yield return new WaitForSeconds(0.2f);

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
                    Debug.Log($"<color=green>Máscara asignada con éxito al cliente {clientId}</color>");
                }
            }
        }
    }
}