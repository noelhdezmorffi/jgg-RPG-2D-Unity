using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(AudioSource))]
public class NetworkPiano : NetworkBehaviour
{
    [SerializeField] private float interactionDistance = 2f;

    private AudioSource audioSource;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

    private void Update()
    {
        if (!IsClient || Keyboard.current == null)
            return;

        NetworkObject localPlayer = NetworkManager.Singleton.LocalClient?.PlayerObject;

        if (localPlayer == null)
            return;

        float distance = Vector2.Distance(
            transform.position,
            localPlayer.transform.position);

        if (distance <= interactionDistance &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            RequestPlayPianoServerRpc();
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestPlayPianoServerRpc()
    {
        PlayPianoClientRpc();
    }

    [ClientRpc]
    private void PlayPianoClientRpc()
    {
        audioSource.Play();
    }
}