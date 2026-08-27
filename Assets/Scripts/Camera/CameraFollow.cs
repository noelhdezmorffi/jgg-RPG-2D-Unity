using Unity.Netcode;
using Unity.Cinemachine;
using UnityEngine;

public class CameraFollow : NetworkBehaviour
{
    public override void OnNetworkSpawn()
    {
        Debug.Log($"CameraFollow -> {name} Owner={IsOwner}");

        if (!IsOwner)
            return;

        CinemachineCamera camera = FindFirstObjectByType<CinemachineCamera>();

        if (camera == null)
        {
            Debug.LogError("No encontré ninguna CinemachineCamera.");
            return;
        }

        Debug.Log("Cinemachine encontrada.");

        camera.Follow = transform;
    }
}