using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(NetworkObject))]
[RequireComponent(typeof(AudioSource))]
public class NetworkPianola : NetworkBehaviour
{
    [SerializeField] private float interactionDistance = 2f;

    private AudioSource audioSource;
    private Animator animator;

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        animator = GetComponentInChildren<Animator>();
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
            RequestPlayPianolaServerRpc();
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RequestPlayPianolaServerRpc()
    {
        PlayPianolaClientRpc();
    }

    [ClientRpc]
    private void PlayPianolaClientRpc()
    {
        audioSource.Play();
        animator.SetBool("IsDancing", true);
    }
}