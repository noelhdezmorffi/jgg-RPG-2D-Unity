using Unity.Netcode;

public enum PlayerState
{
    Connected,
    ChoosingCharacter,
    Ready,
    Loading,
    Playing
}
public class PlayerSession
{
    public ulong ClientId;

    public int CharacterId = -1;

    public bool Ready;

    public string PlayerName = "Player";

    public PlayerState State = PlayerState.Connected;
}