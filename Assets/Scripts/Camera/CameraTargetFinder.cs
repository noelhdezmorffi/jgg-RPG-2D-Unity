using System.Collections;
using UnityEngine;
using Unity.Netcode;
using Unity.Cinemachine;

public class CameraTargetFinder : MonoBehaviour
{
    private CinemachineCamera cinemachineCamera;

    private void Awake()
    {
        cinemachineCamera = GetComponent<CinemachineCamera>();
    }

    private IEnumerator Start()
    {
        while (cinemachineCamera.Target.TrackingTarget == null)
        {
            var client = NetworkManager.Singleton;
            if (client == null || !client.IsListening)
            {
                yield return null;
                continue;
            }

            var localPlayer = client.LocalClient?.PlayerObject;

            if (localPlayer != null)
            {
                cinemachineCamera.Target.TrackingTarget = localPlayer.transform;
                Debug.Log($"Tracking Target asignado al player local: {localPlayer.name}");
                yield break;
            }

            yield return null;
        }
    }
}