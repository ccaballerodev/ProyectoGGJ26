using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MaskSceneManager : NetworkBehaviour
{
    private List<MaskType> availableMasks = new List<MaskType>
    {
        MaskType.Mascara1, MaskType.Mascara2, MaskType.Mascara3, MaskType.Mascara4
    };

    public override void OnNetworkSpawn()
    {
        // Solo el servidor gestiona la repartición
        if (!IsServer) return;

        // Escuchamos cuando un objeto de red (jugador) aparece
        NetworkManager.Singleton.OnClientConnectedCallback += OnPlayerJoined;

        // Por si el Host ya apareció antes de que este script cargara
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            AssignMask(client.PlayerObject);
        }
    }

    private void OnPlayerJoined(ulong clientId)
    {
        // Cuando un cliente se conecta, buscamos su objeto
        var playerObj = NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;
        AssignMask(playerObj);
    }

    private void AssignMask(NetworkObject playerObj)
    {
        if (playerObj == null) return;

        if (playerObj.TryGetComponent(out PlayerMask pm))
        {
            // Si el jugador ya tiene máscara, no hacemos nada
            if (pm.assignedMask.Value != MaskType.Ninguna) return;

            if (availableMasks.Count > 0)
            {
                int randomIndex = Random.Range(0, availableMasks.Count);
                pm.assignedMask.Value = availableMasks[randomIndex];
                availableMasks.RemoveAt(randomIndex);
            }
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && NetworkManager.Singleton != null)
            NetworkManager.Singleton.OnClientConnectedCallback -= OnPlayerJoined;
    }
}