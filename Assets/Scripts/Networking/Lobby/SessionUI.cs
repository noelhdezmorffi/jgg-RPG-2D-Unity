using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SessionUI : MonoBehaviour
{
    public static SessionUI Instance { get; private set; }

    [SerializeField] private TMP_Text joinCodeText;
    [SerializeField] private TMP_InputField joinInput;
    [SerializeField] private Button createButton;
    [SerializeField] private Button joinButton;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        SessionManager.EnsureInstance();

        if (createButton == null || joinButton == null)
        {
            Debug.LogWarning("SessionUI: falta asignar createButton o joinButton en el inspector.");
            return;
        }

        createButton.onClick.AddListener(CreateSession);
        joinButton.onClick.AddListener(JoinSession);

        if (SessionManager.Instance != null)
            SessionManager.Instance.OnSessionCreated += UpdateJoinCode;
    }

    private async void CreateSession()
    {
        await SessionManager.Instance.CreateSession();
    }

    private async void JoinSession()
    {
        if (joinInput == null)
        {
            Debug.LogWarning("SessionUI: falta asignar joinInput en el inspector.");
            return;
        }

        await SessionManager.Instance.JoinByCode(joinInput.text);
    }

    private void UpdateJoinCode()
    {
        if (joinCodeText == null)
            return;

        joinCodeText.text =
            $"Join Code: {SessionManager.Instance?.JoinCode ?? string.Empty}";
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;

        if (SessionManager.Instance == null)
            return;

        SessionManager.Instance.OnSessionCreated -= UpdateJoinCode;
    }

    public void HideMenu()
    {
        if (createButton != null && createButton.transform.parent != null)
        {
            createButton.transform.parent.gameObject.SetActive(false);
            return;
        }

        if (joinButton != null && joinButton.transform.parent != null)
        {
            joinButton.transform.parent.gameObject.SetActive(false);
            return;
        }

        if (transform.parent != null)
        {
            transform.parent.gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(false);
    }
}