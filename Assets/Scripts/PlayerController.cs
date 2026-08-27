using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;

[RequireComponent(typeof(Rigidbody2D))] 
public class PlayerController : NetworkBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private float moveSpeed = 5f;
    private Vector2 moveInput;
    private Rigidbody2D rb;
    private readonly NetworkVariable<Vector2> networkMoveInput = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);
    public Vector2 MoveInput => moveInput;
    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }
    public void SetMoveSpeed(float speed)
    {
        moveSpeed = speed;
    }
    void OnMove(InputValue value)
    {
        if (!IsOwner)
            return;

        moveInput = value.Get<Vector2>();
    }

    void FixedUpdate()
    {
        if (!IsOwner)
        {
            moveInput = networkMoveInput.Value;
            return;
        }

        rb.linearVelocity = moveInput * moveSpeed;
        SubmitMoveInputServerRpc(moveInput);
    }

    [ServerRpc]
    private void SubmitMoveInputServerRpc(Vector2 input)
    {
        networkMoveInput.Value = input;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        Debug.Log($"Player Spawned - Owner: {IsOwner}");
    }
}