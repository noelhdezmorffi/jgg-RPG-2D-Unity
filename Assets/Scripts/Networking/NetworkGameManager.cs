using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using UnityEngine;

public class NetworkGameManager : MonoBehaviour
{
    public static NetworkGameManager Instance { get; private set; }

    public static NetworkGameManager EnsureInstance()
    {
        if (Instance != null)
            return Instance;

        var obj = new GameObject("NetworkGameManager");
        return obj.AddComponent<NetworkGameManager>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        ConfigureTransport();
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton == null)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        NetworkManager.Singleton.OnServerStarted += OnServerStarted;
        NetworkManager.Singleton.OnClientStarted += OnClientStarted;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton == null)
            return;

        NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        NetworkManager.Singleton.OnServerStarted -= OnServerStarted;
        NetworkManager.Singleton.OnClientStarted -= OnClientStarted;
    }

    private void OnServerStarted()
    {
        Debug.Log("Servidor Netcode iniciado.");
        if (LobbyManager.Instance != null)
        {
            LobbyManager.Instance.RegisterPlayer(0);
        }
    }

    public bool IsNetworkRunning()
    {
        if (NetworkManager.Singleton == null)
            return false;

        return NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer;
    }

    private void OnClientStarted()
    {
        Debug.Log("Cliente Netcode iniciado.");
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Cliente conectado: {clientId}");

        if (LobbyManager.Instance != null)
            LobbyManager.Instance.RegisterPlayer(clientId);
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Cliente desconectado: {clientId}");

        if (LobbyManager.Instance != null)
            LobbyManager.Instance.RemovePlayer(clientId);
    }

    private void ConfigureTransport()
    {
        if (NetworkManager.Singleton == null)
            return;

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();
        if (transport == null)
            transport = NetworkManager.Singleton.gameObject.AddComponent<UnityTransport>();

        transport.UseWebSockets = false;
        transport.SetConnectionData("127.0.0.1", 7777);
    }

    public void StartHost()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("No hay NetworkManager en la escena.");
            return;
        }

        if (IsNetworkRunning())
        {
            Debug.Log("La red ya está arrancada.");
            return;
        }

        ConfigureTransport();
        LobbyManager.EnsureInstance();

        bool started = NetworkManager.Singleton.StartHost();
        Debug.Log($"StartHost: {started}");
    }

    public void StartClient()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("No hay NetworkManager en la escena.");
            return;
        }

        if (IsNetworkRunning())
        {
            Debug.Log("La red ya está arrancada.");
            return;
        }

        ConfigureTransport();
        LobbyManager.EnsureInstance();

        bool started = NetworkManager.Singleton.StartClient();
        Debug.Log($"StartClient: {started}");
    }

    public void StartServer()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("No hay NetworkManager en la escena.");
            return;
        }

        if (IsNetworkRunning())
        {
            Debug.Log("La red ya está arrancada.");
            return;
        }

        ConfigureTransport();
        LobbyManager.EnsureInstance();

        bool started = NetworkManager.Singleton.StartServer();
        Debug.Log($"StartServer: {started}");
    }

    public async Task StartForCurrentSession()
    {
        if (SessionManager.Instance == null)
            SessionManager.EnsureInstance();

        if (SessionManager.Instance == null || SessionManager.Instance.CurrentSession == null)
        {
            Debug.LogWarning("No hay sesión activa para iniciar la red.");
            return;
        }

        if (IsNetworkRunning())
        {
            Debug.Log("La red ya está arrancada; no se reinicia la conexión.");
            return;
        }

        ConfigureTransport();

        if (SessionManager.Instance.IsHost)
        {
            await RelayManager.EnsureInstance().PrepareHostRelayAsync(4);
            StartHost();
        }
        else
        {
            await RelayManager.EnsureInstance().PrepareClientRelayAsync(SessionManager.Instance.JoinCode);
            StartClient();
        }
    }
}