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
            NetworkObject[] networkObjects =
                FindObjectsByType<NetworkObject>(
                    FindObjectsSortMode.None
                );

            foreach (NetworkObject networkObject in networkObjects)
            {
                if (networkObject.IsOwner)
                {
                    cinemachineCamera.Target.TrackingTarget =
                        networkObject.transform;

                    Debug.Log(
                        $"Tracking Target asignado a: {networkObject.name}"
                    );

                    yield break;
                }
            }

            yield return null;
        }
    }
}