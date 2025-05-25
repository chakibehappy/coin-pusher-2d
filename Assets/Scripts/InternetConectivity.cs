using System;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Networking;

public class InternetConnectivity : MonoBehaviour
{
#if UNITY_WEBGL && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern int IsOnline();

    [DllImport("__Internal")]
    private static extern void SetConnectivityCallback(ConnectivityChangeDelegate callback);

    private delegate void ConnectivityChangeDelegate(int isOnline);
#endif

    public static event Action<bool> OnConnectivityChanged;

    private static bool isConnected = true;
    public static bool IsConnected => isConnected;

    public float internetCheckingTimeRate = 2f; // only for editor

    private void Start()
    {
#if UNITY_WEBGL && !UNITY_EDITOR
        SetConnectivityCallback(OnConnectionChanged);
        bool currentStatus = IsOnline() == 1;
        isConnected = currentStatus;
        OnConnectivityChanged?.Invoke(currentStatus);
#else
        Debug.Log("Running in Editor/Standalone: Starting connectivity simulation");
        InvokeRepeating(nameof(CheckInternetConnection), 1f, internetCheckingTimeRate);
#endif
    }

#if UNITY_WEBGL && !UNITY_EDITOR
    [AOT.MonoPInvokeCallback(typeof(ConnectivityChangeDelegate))]
    private static void OnConnectionChanged(int isOnline)
    {
        bool online = isOnline == 1;
        isConnected = online;
        Debug.Log("Connectivity changed: " + (online ? "Online" : "Offline"));
        OnConnectivityChanged?.Invoke(online);
    }
#endif

#if !UNITY_WEBGL || UNITY_EDITOR
    private void CheckInternetConnection()
    {
        StartCoroutine(CheckConnectionCoroutine());
    }

    private System.Collections.IEnumerator CheckConnectionCoroutine()
    {
        using UnityWebRequest request = new UnityWebRequest("https://www.google.com")
        {
            method = UnityWebRequest.kHttpVerbHEAD,
            timeout = 3,
        };
        yield return request.SendWebRequest();

        bool connected = !request.result.HasFlag(UnityWebRequest.Result.ConnectionError);

        if (connected != isConnected)
        {
            isConnected = connected;
            Debug.Log("Editor simulated connectivity: " + (connected ? "Online" : "Offline"));
            OnConnectivityChanged?.Invoke(connected);
        }
    }
#endif
}
