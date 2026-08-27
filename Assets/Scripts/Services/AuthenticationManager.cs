using System;
using System.Threading.Tasks;
using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;

public class AuthenticationManager : MonoBehaviour
{
    public static AuthenticationManager Instance { get; private set; }

    public static AuthenticationManager EnsureInstance()
    {
        if (Instance != null)
            return Instance;

        var obj = new GameObject("AuthenticationManager");
        return obj.AddComponent<AuthenticationManager>();
    }

    public bool IsInitialized { get; private set; }

    public string PlayerId =>
        AuthenticationService.Instance.IsSignedIn
            ? AuthenticationService.Instance.PlayerId
            : "";

    public event Action OnInitialized;

    private async void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        await InitializeAsync();
    }

    public async Task InitializeAsync()
    {
        if (IsInitialized)
            return;

        try
        {
            await UnityServices.InitializeAsync();

            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }

            IsInitialized = true;

            Debug.Log($"Authentication OK");
            Debug.Log($"PlayerId: {PlayerId}");

            OnInitialized?.Invoke();
        }
        catch (Exception e)
        {
            Debug.LogException(e);
        }
    }
}