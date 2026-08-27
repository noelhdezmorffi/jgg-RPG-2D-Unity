using UnityEngine;
using Unity.Netcode;

public class CharacterManager : NetworkBehaviour
{
    [Header("Current Character")]
    [SerializeField] private CharacterData characterData;

    private Animator graphicsAnimator;
    private SpriteRenderer graphicsRenderer;

    public CharacterData CharacterData => characterData;
    private PlayerController playerController;
    private readonly NetworkVariable<int> networkCharacterId = new(-1);

    private void Awake()
    {
        playerController = GetComponent<PlayerController>();

        FindGraphics();

        ApplyCharacter();
    }

    public void SetCharacter(CharacterData newCharacter)
    {
        characterData = newCharacter;

        ApplyCharacter();
    }

    public void ApplySelectedCharacter(int characterId)
    {
        if (!IsOwner || !IsSpawned)
            return;

        CharacterData selected = FindCharacter(characterId);
        if (selected == null)
            return;

        SetCharacter(selected);
        SubmitCharacterServerRpc(characterId);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        networkCharacterId.OnValueChanged += OnCharacterChanged;

        if (IsOwner)
        {
            int selectedId = PlayerPrefs.GetInt("SelectedCharacterId", characterData != null ? characterData.id : 0);
            SubmitCharacterServerRpc(selectedId);
        }

        ApplyNetworkCharacter(networkCharacterId.Value);
        Debug.Log($"Player Spawned - Owner: {IsOwner}");
    }

    public override void OnDestroy()
    {
        networkCharacterId.OnValueChanged -= OnCharacterChanged;
        base.OnDestroy();
    }

    [ServerRpc]
    private void SubmitCharacterServerRpc(int characterId)
    {
        networkCharacterId.Value = characterId;
        ApplyNetworkCharacter(characterId);
    }

    private void OnCharacterChanged(int previousId, int newId)
    {
        ApplyNetworkCharacter(newId);
    }

    private void ApplyNetworkCharacter(int characterId)
    {
        CharacterData selected = FindCharacter(characterId);
        if (selected != null)
            SetCharacter(selected);
    }

    private CharacterData FindCharacter(int characterId)
    {
        if (characterId < 0)
            return null;

        CharacterDatabase database = GameSession.Instance != null
            ? GameSession.Instance.CharacterDatabase
            : null;

        if (database == null && LobbyScreen.Instance != null)
            database = LobbyScreen.Instance.CharacterDatabase;

        return database != null ? database.GetCharacter(characterId) : null;
    }

    private void FindGraphics()
    {
        Transform graphics = transform.Find("Graphics");

        if (graphics == null)
        {
            Debug.LogError("No existe un hijo llamado Graphics.");
            return;
        }

        graphicsAnimator = graphics.GetComponent<Animator>();
        graphicsRenderer = graphics.GetComponent<SpriteRenderer>();
    }

    private void ApplyCharacter()
{
    if (characterData == null)
    {
        Debug.LogWarning("CharacterData no asignado.");
        return;
    }

    if (graphicsAnimator != null)
    {
        graphicsAnimator.runtimeAnimatorController =
            characterData.animatorOverride;
    }
    
    if (playerController != null)
    {
        playerController.SetMoveSpeed(characterData.moveSpeed);
    }

    Debug.Log($"Personaje aplicado: {characterData.characterName}");
}
}