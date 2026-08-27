using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class NetworkPositionSync : NetworkBehaviour
{
    private readonly NetworkVariable<Vector2> networkPosition = new(
        default,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server);

    private Rigidbody2D rb;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        networkPosition.OnValueChanged += OnPositionChanged;

        if (IsServer)
            networkPosition.Value = rb.position;
        else
            rb.position = networkPosition.Value;
    }

    private void OnDestroy()
    {
        networkPosition.OnValueChanged -= OnPositionChanged;
    }

    private void FixedUpdate()
    {
        if (IsOwner)
        {
            SubmitPositionServerRpc(rb.position);
            return;
        }

        rb.position = networkPosition.Value;
        rb.linearVelocity = Vector2.zero;
    }

    [ServerRpc]
    private void SubmitPositionServerRpc(Vector2 position)
    {
        networkPosition.Value = position;
    }

    private void OnPositionChanged(Vector2 previous, Vector2 current)
    {
        if (!IsOwner)
            rb.position = current;
    }
}
