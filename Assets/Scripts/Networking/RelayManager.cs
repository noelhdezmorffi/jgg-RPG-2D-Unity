using System;
using System.Linq;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;

public class RelayManager : MonoBehaviour
{
    public static RelayManager Instance { get; private set; }

    public static RelayManager EnsureInstance()
    {
        if (Instance != null)
            return Instance;

        var obj = new GameObject("RelayManager");
        return obj.AddComponent<RelayManager>();
    }

    public string JoinCode { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        EnsureTransportConfig();
    }

    public void EnsureTransportConfig()
    {
        var transport = GetTransport();
        if (transport == null)
        {
            Debug.LogWarning("No se encontró UnityTransport para preparar la conexión Relay.");
            return;
        }

        transport.UseWebSockets = false;
        transport.SetConnectionData("127.0.0.1", 7777);
    }

    public async Task PrepareHostRelayAsync(int maxPlayers)
    {
        var transport = GetTransport();
        if (transport == null)
        {
            Debug.LogError("No hay UnityTransport para iniciar la conexión Relay del host.");
            return;
        }

        transport.UseWebSockets = false;

        try
        {
            var allocation = await RelayService.Instance.CreateAllocationAsync(Mathf.Max(2, maxPlayers));
            JoinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);

            var relayServerData = BuildRelayServerData(allocation);
            transport.SetRelayServerData(relayServerData);

            Debug.Log($"Relay host listo. Join code: {JoinCode}");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async Task PrepareClientRelayAsync(string joinCode)
    {
        var transport = GetTransport();
        if (transport == null)
        {
            Debug.LogError("No hay UnityTransport para iniciar la conexión Relay del cliente.");
            return;
        }

        if (string.IsNullOrWhiteSpace(joinCode))
        {
            Debug.LogError("No se ha recibido un join code válido para Relay.");
            return;
        }

        transport.UseWebSockets = false;

        try
        {
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            var relayServerData = BuildRelayServerData(joinAllocation);
            transport.SetRelayServerData(relayServerData);

            JoinCode = joinCode;
            Debug.Log($"Relay cliente listo. Join code: {joinCode}");
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private static RelayServerData BuildRelayServerData(Allocation allocation)
    {
        var endpoint = allocation.ServerEndpoints.FirstOrDefault(x => string.Equals(x.ConnectionType, "udp", StringComparison.OrdinalIgnoreCase));
        if (endpoint == null)
            throw new InvalidOperationException("No existe un endpoint Relay UDP disponible.");

        return new RelayServerData(
            endpoint.Host,
            (ushort)endpoint.Port,
            allocation.AllocationIdBytes,
            allocation.ConnectionData,
            allocation.ConnectionData,
            allocation.Key,
            endpoint.Secure,
            false);
    }

    private static RelayServerData BuildRelayServerData(JoinAllocation allocation)
    {
        var endpoint = allocation.ServerEndpoints.FirstOrDefault(x => string.Equals(x.ConnectionType, "udp", StringComparison.OrdinalIgnoreCase));
        if (endpoint == null)
            throw new InvalidOperationException("No existe un endpoint Relay UDP disponible para el cliente.");

        return new RelayServerData(
            endpoint.Host,
            (ushort)endpoint.Port,
            allocation.AllocationIdBytes,
            allocation.ConnectionData,
            allocation.HostConnectionData,
            allocation.Key,
            endpoint.Secure,
            false);
    }

    private static UnityTransport GetTransport()
    {
        if (NetworkManager.Singleton != null)
        {
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
            if (transport != null)
                return transport;
        }

        var networkManager = FindFirstObjectByType<NetworkManager>();
        if (networkManager != null)
        {
            var transport = networkManager.GetComponent<UnityTransport>();
            if (transport != null)
                return transport;
        }

        return null;
    }
}