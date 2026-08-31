using Unity.Netcode.Components;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerActionController : NetworkBehaviour
{
    private Animator animator;
    private AudioSource audioSource;

    [SerializeField] private AudioClip kissSound;
    [SerializeField] private AudioClip jumpSound;
    [SerializeField] private AudioClip attackSound;

    private readonly NetworkVariable<bool> isDancing = new(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    // private readonly NetworkVariable<bool> isKissing = new(
    //     false,
    //     NetworkVariableReadPermission.Everyone,
    //     NetworkVariableWritePermission.Server);

    private void Awake()
    {
        animator = GetComponentInChildren<Animator>();
        audioSource = GetComponent<AudioSource>();

        isDancing.OnValueChanged += OnDanceStateChanged;
        // isKissing.OnValueChanged += OnKissStateChanged;
    }

    private void OnDestroy()
    {
        isDancing.OnValueChanged -= OnDanceStateChanged;
        // isKissing.OnValueChanged -= OnKissStateChanged;
    }

    private void Update()
    {
        if (!IsOwner)
            return;

        if (Keyboard.current.bKey.wasPressedThisFrame)
        {
            ToggleDanceServerRpc();
        }
        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            RequestKissServerRpc();
            // ToggleKissServerRpc();
        }
    }

    [ServerRpc]
    private void ToggleDanceServerRpc()
    {
        isDancing.Value = !isDancing.Value;
    }
    // [ServerRpc]
    // private void ToggleKissServerRpc()
    // {
    //     isKissing.Value = !isKissing.Value;
    // }

    private void OnDanceStateChanged(bool oldValue, bool newValue)
    {
        if (animator != null)
            animator.SetBool("IsDancing", newValue);
    }
    // private void OnKissStateChanged(bool oldValue, bool newValue)
    // {
    //     if (animator != null)
    //         // animator.SetBool("IsKissing", newValue);
            
    //     if (newValue)
    //         audioSource.PlayOneShot(kissSound);
    // }
    [ServerRpc]
    private void RequestKissServerRpc()
    {
        KissClientRpc();
    }

    [ClientRpc]
    private void KissClientRpc()
    {
        if (animator != null)
        {
            animator.SetTrigger("Kiss");
            audioSource.PlayOneShot(kissSound);
        }
    }
}