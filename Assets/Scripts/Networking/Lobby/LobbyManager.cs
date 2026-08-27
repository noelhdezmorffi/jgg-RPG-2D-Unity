using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    public static LobbyManager Instance { get; private set; }

    public static LobbyManager EnsureInstance()
    {
        if (Instance != null)
            return Instance;

        var obj = new GameObject("LobbyManager");
        return obj.AddComponent<LobbyManager>();
    }

    private readonly Dictionary<ulong, PlayerSession> players = new();
    private bool localReady;

    public IReadOnlyDictionary<ulong, PlayerSession> Players => players;
    public bool LocalReady => localReady;

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

    public void RegisterPlayer(ulong clientId)
    {
        if (players.ContainsKey(clientId))
            return;

        PlayerSession session = new PlayerSession
        {
            ClientId = clientId,
            CharacterId = -1,
            Ready = false,
            PlayerName = $"Player {clientId}",
            State = PlayerState.Connected
        };

        players.Add(clientId, session);

        Debug.Log($"Registrado {session.PlayerName}");
    }

    public void RemovePlayer(ulong clientId)
    {
        players.Remove(clientId);

        Debug.Log($"Jugador eliminado: {clientId}");
    }

    public void SetLocalReady(bool ready)
    {
        localReady = ready;
        Debug.Log($"Ready del local: {localReady}");
    }

    public bool IsLocalReady() => localReady;

    public bool SelectCharacter(ulong clientId, int characterId)
    {
        if (!players.TryGetValue(clientId, out PlayerSession player))
            return false;

        foreach (PlayerSession other in players.Values)
        {
            if (other.ClientId == clientId)
                continue;

            if (other.CharacterId == characterId)
                return false;
        }

        player.CharacterId = characterId;
        player.State = PlayerState.ChoosingCharacter;

        Debug.Log($"Jugador {clientId} eligió personaje {characterId}");

        return true;
    }

    public PlayerSession GetPlayer(ulong clientId)
    {
        players.TryGetValue(clientId, out PlayerSession session);

        return session;
    }

    public bool IsCharacterTaken(int characterId)
    {
        foreach (PlayerSession player in players.Values)
        {
            if (player.CharacterId == characterId)
                return true;
        }

        return false;
    }
}