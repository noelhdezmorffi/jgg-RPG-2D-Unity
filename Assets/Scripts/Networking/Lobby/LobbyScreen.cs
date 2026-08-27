using System.Text;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LobbyScreen : MonoBehaviour
{
    public static LobbyScreen Instance { get; private set; }

    private TextMeshProUGUI playersText;
    private TextMeshProUGUI statusText;
    private Button readyButton;
    private Button startButton;
    private bool isReady;
    private bool uiBuilt;
    [SerializeField] private CharacterDatabase characterDatabase;
    private int selectedCharacterId = -1;
    private readonly List<Button> characterButtons = new();

    public CharacterDatabase CharacterDatabase => characterDatabase;

    public static LobbyScreen EnsureInstance()
    {
        if (Instance != null)
        {
            Instance.EnsureBuilt();
            return Instance;
        }

        var existingObjects = FindObjectsByType<LobbyScreen>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        var existing = existingObjects.Length > 0 ? existingObjects[0] : null;
        if (existing != null)
        {
            Instance = existing;
            Canvas existingCanvas = CreateLobbyCanvas();
            existing.transform.SetParent(existingCanvas.transform, false);
            existing.gameObject.SetActive(true);
            existing.EnsureBuilt();
            existing.Hide();
            return existing;
        }

        Canvas canvas = CreateLobbyCanvas();

        var root = new GameObject("LobbyOverlay");
        root.transform.SetParent(canvas.transform, false);

        var screen = root.AddComponent<LobbyScreen>();
        screen.EnsureBuilt();
        return screen;
    }

    private static Canvas CreateLobbyCanvas()
    {
        var canvasGo = new GameObject("LobbyCanvas");
        var canvas = canvasGo.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGo.AddComponent<CanvasScaler>();
        canvasGo.AddComponent<GraphicRaycaster>();
        DontDestroyOnLoad(canvasGo);
        return canvas;
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        var parentCanvas = transform.parent != null ? transform.parent.GetComponent<Canvas>() : null;
        if (parentCanvas == null)
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    private void Start()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded += OnGameSceneLoaded;
        EnsureBuilt();
        SessionManager.EnsureInstance();
        if (SessionManager.Instance != null)
            SessionManager.Instance.OnPlayersChanged += RefreshDisplay;

        if (SessionManager.Instance == null || SessionManager.Instance.CurrentSession == null)
        {
            Hide();
        }

        ApplyReadyButtonText();
        RefreshDisplay();
    }

    private void EnsureBuilt()
    {
        if (uiBuilt)
            return;

        BuildUI();
        uiBuilt = true;
    }

    private void OnDestroy()
    {
        UnityEngine.SceneManagement.SceneManager.sceneLoaded -= OnGameSceneLoaded;
        if (SessionManager.Instance != null)
            SessionManager.Instance.OnPlayersChanged -= RefreshDisplay;
    }

    private void BuildUI()
    {
        var rt = GetComponent<RectTransform>();
        if (rt == null)
        {
            rt = gameObject.AddComponent<RectTransform>();
        }

        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var panel = new GameObject("Panel", typeof(RectTransform), typeof(Image));
        var panelRt = panel.GetComponent<RectTransform>();
        panelRt.SetParent(rt, false);
        panelRt.anchorMin = new Vector2(0.35f, 0.15f);
        panelRt.anchorMax = new Vector2(0.65f, 0.85f);
        panelRt.offsetMin = Vector2.zero;
        panelRt.offsetMax = Vector2.zero;
        panel.GetComponent<Image>().color = new Color(0.07f, 0.07f, 0.13f, 0.95f);

        var title = new GameObject("Title", typeof(RectTransform), typeof(TextMeshProUGUI));
        var titleRt = title.GetComponent<RectTransform>();
        titleRt.SetParent(panelRt, false);
        titleRt.anchorMin = new Vector2(0f, 1f);
        titleRt.anchorMax = new Vector2(1f, 1f);
        titleRt.offsetMin = new Vector2(0, -50f);
        titleRt.offsetMax = new Vector2(0, 0);
        var titleText = title.GetComponent<TMP_Text>();
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.fontSize = 28;
        titleText.text = "Lobby";

        var listGo = new GameObject("Players", typeof(RectTransform), typeof(TextMeshProUGUI));
        var listRt = listGo.GetComponent<RectTransform>();
        listRt.SetParent(panelRt, false);
        listRt.anchorMin = new Vector2(0.08f, 0.42f);
        listRt.anchorMax = new Vector2(0.92f, 0.82f);
        listRt.offsetMin = Vector2.zero;
        listRt.offsetMax = Vector2.zero;
        playersText = listGo.GetComponent<TextMeshProUGUI>();
        playersText.fontSize = 20;
        playersText.alignment = TextAlignmentOptions.TopLeft;
        playersText.text = "Esperando jugadores...";

        statusText = new GameObject("Status", typeof(RectTransform), typeof(TextMeshProUGUI)).GetComponent<TextMeshProUGUI>();
        var statusRt = statusText.GetComponent<RectTransform>();
        statusRt.SetParent(panelRt, false);
        statusRt.anchorMin = new Vector2(0.08f, 0.12f);
        statusRt.anchorMax = new Vector2(0.92f, 0.17f);
        statusRt.offsetMin = Vector2.zero;
        statusRt.offsetMax = Vector2.zero;
        statusText.fontSize = 14;
        statusText.alignment = TextAlignmentOptions.Center;

        BuildCharacterSelection(panelRt);

        readyButton = CreateButton(panelRt, "ReadyButton", "Listo", new Vector2(0.08f, 0.02f), new Vector2(0.42f, 0.10f));
        readyButton.onClick.AddListener(ToggleReady);

        startButton = CreateButton(panelRt, "StartButton", "Iniciar", new Vector2(0.58f, 0.02f), new Vector2(0.92f, 0.10f));
        startButton.onClick.AddListener(StartMatchIfHost);

        ApplyReadyButtonText();
        RefreshDisplay();
    }

    private void BuildCharacterSelection(RectTransform parent)
    {
        var header = new GameObject("CharacterHeader", typeof(RectTransform), typeof(TextMeshProUGUI));
        var headerRt = header.GetComponent<RectTransform>();
        headerRt.SetParent(parent, false);
        headerRt.anchorMin = new Vector2(0.08f, 0.29f);
        headerRt.anchorMax = new Vector2(0.92f, 0.38f);
        headerRt.offsetMin = Vector2.zero;
        headerRt.offsetMax = Vector2.zero;
        var headerText = header.GetComponent<TextMeshProUGUI>();
        headerText.alignment = TextAlignmentOptions.Center;
        headerText.fontSize = 18;
        headerText.text = "Elige tu personaje";

        if (characterDatabase == null || characterDatabase.Characters == null || characterDatabase.Characters.Count == 0)
        {
            headerText.text = "No hay personajes disponibles";
            return;
        }

        float width = 0.84f / characterDatabase.Characters.Count;
        for (int i = 0; i < characterDatabase.Characters.Count; i++)
        {
            CharacterData character = characterDatabase.Characters[i];
            float minX = 0.08f + width * i;
            Button button = CreateButton(parent, $"Character_{character.id}", character.characterName,
                new Vector2(minX, 0.18f), new Vector2(minX + width - 0.02f, 0.28f));
            int characterId = character.id;
            button.onClick.AddListener(() => SelectCharacter(characterId));
            characterButtons.Add(button);
        }

        SelectCharacter(characterDatabase.Characters[0].id);
    }

    private void SelectCharacter(int characterId)
    {
        selectedCharacterId = characterId;
        foreach (Button button in characterButtons)
        {
            button.interactable = !button.name.EndsWith($"_{characterId}");
        }

        PlayerPrefs.SetInt("SelectedCharacterId", characterId);
        PlayerPrefs.Save();

        CharacterManager[] players = FindObjectsByType<CharacterManager>(
            FindObjectsInactive.Exclude,
            FindObjectsSortMode.None);
        foreach (CharacterManager player in players)
        {
            if (player.IsOwner)
                player.ApplySelectedCharacter(characterId);
        }
    }

    private Button CreateButton(RectTransform parent, string name, string text, Vector2 minAnchor, Vector2 maxAnchor)
    {
        var btnGo = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
        var btnRt = btnGo.GetComponent<RectTransform>();
        btnRt.SetParent(parent, false);
        btnRt.anchorMin = minAnchor;
        btnRt.anchorMax = maxAnchor;
        btnRt.offsetMin = Vector2.zero;
        btnRt.offsetMax = Vector2.zero;

        var btnImage = btnGo.GetComponent<Image>();
        btnImage.color = new Color(0.18f, 0.46f, 0.95f, 1f);

        var label = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
        var labelRt = label.GetComponent<RectTransform>();
        labelRt.SetParent(btnRt, false);
        labelRt.anchorMin = Vector2.zero;
        labelRt.anchorMax = Vector2.one;
        labelRt.offsetMin = Vector2.zero;
        labelRt.offsetMax = Vector2.zero;

        var labelText = label.GetComponent<TextMeshProUGUI>();
        labelText.alignment = TextAlignmentOptions.Center;
        labelText.fontSize = 18;
        labelText.text = text;

        return btnGo.GetComponent<Button>();
    }

    private void ToggleReady()
    {
        isReady = !isReady;
        LobbyManager.EnsureInstance().SetLocalReady(isReady);
        ApplyReadyButtonText();
        RefreshDisplay();
    }

    private async void StartMatchIfHost()
    {
        if (SessionManager.Instance == null || !SessionManager.Instance.IsHost)
            return;

        if (!isReady)
        {
            statusText.text = "El host debe pulsar Listo antes de iniciar";
            return;
        }

        if (NetworkGameManager.Instance == null)
            NetworkGameManager.EnsureInstance();

        await NetworkGameManager.Instance.StartForCurrentSession();
        if (NetworkGameManager.Instance.IsNetworkRunning())
        {
            NetworkManager.Singleton.SceneManager.LoadScene("SampleScene", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
    }

    private void OnGameSceneLoaded(UnityEngine.SceneManagement.Scene scene, UnityEngine.SceneManagement.LoadSceneMode mode)
    {
        if (scene.name == "SampleScene")
            Hide();
    }

    private void ApplyReadyButtonText()
    {
        if (readyButton == null)
            return;

        var label = readyButton.GetComponentInChildren<TextMeshProUGUI>();
        if (label != null)
            label.text = isReady ? "No listo" : "Listo";
    }

    public void RefreshDisplay()
    {
        if (playersText == null)
            return;

        if (statusText == null)
            return;

        if (SessionManager.Instance == null || SessionManager.Instance.CurrentSession == null)
        {
            playersText.text = "Sin sesión activa";
            statusText.text = "Crea o entra en una sesión";
            if (startButton != null) startButton.gameObject.SetActive(false);
            if (readyButton != null) readyButton.gameObject.SetActive(false);
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        var session = SessionManager.Instance.CurrentSession;
        if (session.Players == null || session.Players.Count == 0)
        {
            playersText.text = "Esperando jugadores...";
            statusText.text = session.Code ?? "Esperando";
        }
        else
        {
            var sb = new StringBuilder();
            sb.AppendLine("Jugadores conectados:");
            foreach (var p in session.Players)
            {
                sb.AppendLine($"- {p.Id}");
            }
            playersText.text = sb.ToString();
            statusText.text = $"Código: {session.Code}";
        }

        bool isHost = SessionManager.Instance.IsHost;
        if (startButton != null)
            startButton.gameObject.SetActive(isHost);

        if (readyButton != null)
            readyButton.gameObject.SetActive(true);
    }

    public void Show()
    {
        EnsureBuilt();

        var sessionUI = SessionUI.Instance != null
            ? SessionUI.Instance
            : FindFirstObjectByType<SessionUI>();
        if (sessionUI != null)
            sessionUI.HideMenu();

        transform.SetAsLastSibling();
        if (gameObject != null)
            gameObject.SetActive(true);
        statusText.text = $"Código: {SessionManager.Instance.JoinCode}";
        RefreshDisplay();
    }

    public void Hide()
    {
        if (gameObject != null)
            gameObject.SetActive(false);
    }
}
