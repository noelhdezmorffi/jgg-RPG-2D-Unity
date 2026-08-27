using System;
using System.Threading.Tasks;
using Unity.Services.Multiplayer;
using UnityEngine;
using System.Collections.Generic;

public class SessionManager : MonoBehaviour
{
    public static SessionManager Instance { get; private set; }

    public static SessionManager EnsureInstance()
    {
        if (Instance != null)
            return Instance;

        var obj = new GameObject("SessionManager");
        return obj.AddComponent<SessionManager>();
    }

    public ISession CurrentSession { get; private set; }

    public bool InSession => CurrentSession != null;

    public event Action OnSessionCreated;
    public event Action OnSessionJoined;
    public event Action OnSessionLeft;
    public event Action OnPlayersChanged;
    public string JoinCode { get; private set; }
    public bool IsHost => CurrentSession != null && CurrentSession.IsHost;
    public IReadOnlyList<IReadOnlyPlayer> Players => CurrentSession?.Players;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public async Task CreateSession(int maxPlayers = 4)
    {
        try
        {
            var options = new SessionOptions
            {
                MaxPlayers = maxPlayers
            }
            .WithRelayNetwork();

            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                Debug.LogError("Unity Multiplayer Relay/QoS no soporta WebGL. Usa Standalone o una plataforma soportada para multiplayer.");
                return;
            }

            CurrentSession = await MultiplayerService.Instance.CreateSessionAsync(options);
            RegisterSessionEvents();

            JoinCode = CurrentSession != null ? CurrentSession.Code : string.Empty;
            Debug.Log($"Sesión creada. Código: {JoinCode}");

            OnSessionCreated?.Invoke();
            LobbyScreen.EnsureInstance().Show();
            await NetworkGameManager.EnsureInstance().StartForCurrentSession();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async Task JoinByCode(string code)
    {
        try
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                Debug.LogError("Unity Multiplayer Relay/QoS no soporta WebGL. Usa Standalone o una plataforma soportada para multiplayer.");
                return;
            }

            if (string.IsNullOrWhiteSpace(code))
            {
                Debug.LogWarning("No se proporcionó un código de sesión.");
                return;
            }

            CurrentSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(code);

            JoinCode = code;
            RegisterSessionEvents();
            Debug.Log($"Sesión unida con código: {code}");

            OnSessionJoined?.Invoke();
            LobbyScreen.EnsureInstance().Show();
            await NetworkGameManager.EnsureInstance().StartForCurrentSession();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    public async Task LeaveSession()
    {
        if (CurrentSession == null)
            return;

        UnregisterSessionEvents();

        await CurrentSession.LeaveAsync();

        CurrentSession = null;
        JoinCode = string.Empty;

        OnSessionLeft?.Invoke();
    }

    public async Task JoinSession(string joinCode)
    {
        try
        {
            if (Application.platform == RuntimePlatform.WebGLPlayer)
            {
                Debug.LogError("Unity Multiplayer Relay/QoS no soporta WebGL. Usa Standalone o una plataforma soportada para multiplayer.");
                return;
            }

            CurrentSession = await MultiplayerService.Instance.JoinSessionByCodeAsync(joinCode);

            JoinCode = joinCode;
            Debug.Log("Sesión unida correctamente.");

            OnSessionJoined?.Invoke();
            LobbyScreen.EnsureInstance().Show();
            await NetworkGameManager.EnsureInstance().StartForCurrentSession();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

    private void RegisterSessionEvents()
    {
        if (CurrentSession == null)
            return;

        CurrentSession.Changed += OnSessionChanged;
        CurrentSession.StateChanged += OnSessionStateChanged;

        CurrentSession.PlayerJoined += OnPlayerJoined;
        CurrentSession.PlayerLeaving += OnPlayerLeaving;
        CurrentSession.PlayerHasLeft += OnPlayerHasLeft;

        CurrentSession.SessionPropertiesChanged += OnSessionPropertiesChanged;
        CurrentSession.PlayerPropertiesChanged += OnPlayerPropertiesChanged;

        CurrentSession.RemovedFromSession += OnRemovedFromSession;
    }

    private void UnregisterSessionEvents()
    {
        if (CurrentSession == null)
            return;

        CurrentSession.Changed -= OnSessionChanged;
        CurrentSession.StateChanged -= OnSessionStateChanged;

        CurrentSession.PlayerJoined -= OnPlayerJoined;
        CurrentSession.PlayerLeaving -= OnPlayerLeaving;
        CurrentSession.PlayerHasLeft -= OnPlayerHasLeft;

        CurrentSession.SessionPropertiesChanged -= OnSessionPropertiesChanged;
        CurrentSession.PlayerPropertiesChanged -= OnPlayerPropertiesChanged;

        CurrentSession.RemovedFromSession -= OnRemovedFromSession;
    }

    private void OnSessionChanged()
    {
        Debug.Log("La sesión ha cambiado.");

        OnPlayersChanged?.Invoke();
    }
    private void OnSessionStateChanged(SessionState state)
    {
        Debug.Log($"Estado de la sesión: {state}");
    }
    private void OnPlayerJoined(string playerId)
    {
        Debug.Log($"Jugador unido: {playerId}");

        OnPlayersChanged?.Invoke();
    }
    private void OnPlayerLeaving(string playerId)
    {
        Debug.Log($"Jugador saliendo: {playerId}");

        OnPlayersChanged?.Invoke();
    }
    private void OnPlayerHasLeft(string playerId)
    {
        Debug.Log($"Jugador salió: {playerId}");

        OnPlayersChanged?.Invoke();
    }
    private void OnSessionPropertiesChanged()
    {
        Debug.Log("Propiedades de la sesión modificadas.");
    }
    private void OnPlayerPropertiesChanged()
    {
        Debug.Log("Propiedades de jugadores modificadas.");

        OnPlayersChanged?.Invoke();
    }
    private void OnRemovedFromSession()
    {
        Debug.Log("Has sido eliminado de la sesión.");

        CurrentSession = null;
        JoinCode = string.Empty;

        if (LobbyScreen.Instance != null)
            LobbyScreen.Instance.Hide();

        OnSessionLeft?.Invoke();
    }
    
}