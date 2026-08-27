using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LobbyUI : MonoBehaviour
{
    [SerializeField] private TMP_Text playersText;
    [SerializeField] private Button readyButton;
    [SerializeField] private Button startButton;

    private bool isReady;

    private void Start()
    {
        LobbyManager.EnsureInstance();
        SessionManager.EnsureInstance();

        if (SessionManager.Instance != null)
            SessionManager.Instance.OnPlayersChanged += RefreshLobby;

        if (readyButton != null)
            readyButton.onClick.AddListener(ToggleReady);

        if (startButton != null)
            startButton.onClick.AddListener(StartMatchIfHost);

        RefreshLobby();
    }

    private void OnDestroy()
    {
        if (SessionManager.Instance != null)
            SessionManager.Instance.OnPlayersChanged -= RefreshLobby;
    }

    private void Update()
    {
        RefreshLobby();
    }

    private void ToggleReady()
    {
        isReady = !isReady;
        LobbyManager.EnsureInstance().SetLocalReady(isReady);

        if (readyButton != null)
        {
            var readyText = readyButton.GetComponentInChildren<TMP_Text>();
            if (readyText != null)
                readyText.text = isReady ? "No listo" : "Listo";
        }
    }

    private async void StartMatchIfHost()
    {
        if (SessionManager.Instance == null || !SessionManager.Instance.IsHost)
            return;

        if (NetworkGameManager.Instance == null)
            NetworkGameManager.EnsureInstance();

        await NetworkGameManager.Instance.StartForCurrentSession();
    }

    private void RefreshLobby()
    {
        if (playersText == null)
            return;

        if (SessionManager.Instance == null || SessionManager.Instance.CurrentSession == null)
        {
            playersText.text = "Sin sesión";
            return;
        }

        var players = SessionManager.Instance.CurrentSession.Players;
        if (players == null || players.Count == 0)
        {
            playersText.text = "Esperando jugadores...";
            return;
        }

        var sb = new StringBuilder();
        sb.AppendLine("Jugadores conectados:");
        foreach (var player in players)
        {
            sb.AppendLine($"- {player.Id}");
        }

        playersText.text = sb.ToString();

        if (startButton != null)
            startButton.gameObject.SetActive(SessionManager.Instance.IsHost);

        if (readyButton != null)
            readyButton.gameObject.SetActive(true);
    }
}