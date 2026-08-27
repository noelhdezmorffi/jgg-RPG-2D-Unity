using System.Threading.Tasks;
using Unity.Services.Authentication;
using UnityEngine;

public class SessionTest : MonoBehaviour
{
    [SerializeField] private bool autoCreateOnStart;

    private async void Start()
    {
        AuthenticationManager.EnsureInstance();
        SessionManager.EnsureInstance();

        Debug.Log("SessionTest iniciado");

        if (!autoCreateOnStart)
        {
            Debug.Log("SessionTest: modo de prueba desactivado. La sesión se crea desde la UI.");
            return;
        }

        Debug.Log("Esperando Authentication...");

        while (!AuthenticationService.Instance.IsSignedIn)
        {
            await Task.Yield();
        }

        Debug.Log("Authentication detectada. Creando sesión...");

        await SessionManager.Instance.CreateSession();

        if (SessionManager.Instance.CurrentSession != null)
            await NetworkGameManager.EnsureInstance().StartForCurrentSession();

        Debug.Log(
            $"CurrentSession existe: " +
            $"{SessionManager.Instance.CurrentSession != null}"
        );
    }
}