using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine; 

public class PlayerCameraSetup : NetworkBehaviour
{

    [SerializeField] private CinemachineCamera myOrbitCamera;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {

            myOrbitCamera.Priority = 10;
            myOrbitCamera.enabled = true;

            if (GetComponentInChildren<AudioListener>() != null)
            {
                GetComponentInChildren<AudioListener>().enabled = true;
            }
        }
        else
        {
            myOrbitCamera.Priority = 0;
            myOrbitCamera.enabled = false;
        }
    }
}