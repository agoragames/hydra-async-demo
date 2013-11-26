using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AgoraGames.Hydra;
using AgoraGames.Hydra.Models;
using AgoraGames.Hydra.Util;

public class Core : MonoBehaviour
{
    public enum AuthScheme
    {
        UUID,
        Anonymous,
        Custom
    }

    public string url;
    public string apiKey;
    public AuthScheme authenticationScheme = AuthScheme.UUID;

#if UNITY_ANDROID
    public bool defaultNotificationSetup = true;
#endif

    protected float nextPing;
    protected static int PING_INTERVAL = 15; // seconds

    // TODO: make this an interface abstraction?
#if UNITY_IPHONE && !UNITY_EDITOR
    IOSRemoteNotifications remoteNotifications;
#elif UNITY_ANDROID && !UNITY_EDITOR
    public AndroidRemoteNotifications RemoteNotifications { get; protected set; }
#endif

    // Unity
    public void Start()
    {
        Application.runInBackground = true;
#if UNITY_IPHONE && !UNITY_EDITOR
        remoteNotifications = new IOSRemoteNotifications(Client.Instance);
#elif UNITY_ANDROID && !UNITY_EDITOR
        RemoteNotifications = new AndroidRemoteNotifications(Client.Instance);
#endif
    }

    public void OnEnable()
    {
        Client.Instance.Logger.Handler += UnityLoggerHandler;

        Client.Instance.Message.Connected += Message_Connected;
        Client.Instance.Message.Disconnected += Message_Disconnected;

        Client.Instance.Init(new UnityRunner(this), null, url, apiKey, new UnityAuthTokenManager());

        if (Client.Instance.AuthToken != null)
        {
            StartupHydra();
        }
        else if (authenticationScheme == AuthScheme.UUID)
        {
            StartupHydra(new UUIDAuth(SystemInfo.deviceUniqueIdentifier));
        }
        else if (authenticationScheme == AuthScheme.Anonymous)
        {
            StartupHydra(new AnonymousAuth());
        }
    }

    void Message_Connected()
    {
        Client.Instance.Logger.Info("Realtime online");
    }

    void Message_Disconnected()
    {
        Client.Instance.Logger.Info("Realtime offline");
    }

    public void StartupHydra()
    {
        Client.Instance.Logger.Info("Starting up Hydra SDK with saved auth token");
        Client.Instance.Logger.Info("Connecting to " + url);

        Client.Instance.Startup(true, HandleStartupResponse);
    }

    public void StartupHydra(Auth auth)
    {
        Client.Instance.Logger.Info("Starting up Hydra SDK with " + Auth.GetAuthString(auth.AuthType) + " auth");
        Client.Instance.Logger.Info("Connecting to " + url);

        Client.Instance.Startup(auth, true,  HandleStartupResponse);
    }
        
    protected void HandleStartupResponse(Request req)
    {
#if UNITY_IPHONE && !UNITY_EDITOR
        if (Client.Instance.CurrentConfiguration.APNSEnabled)
        {
            Client.Instance.Logger.Info("Initializing iOS notifications");
            remoteNotifications.Register();
        }
#elif UNITY_ANDROID && !UNITY_EDITOR
        if (defaultNotificationSetup && Client.Instance.CurrentConfiguration.GCMEnabled)
        {
            Client.Instance.Logger.Info("Initializing android notifications");
            RemoteNotifications.InitAndroidNotifications(this, Client.Instance.CurrentConfiguration.AndroidProjectNumber);
        }
#endif

        if (req.HasError())
        {
            Client.Instance.Logger.Error("Hydra SDK failed to start");
        }
        else
        {
            Client.Instance.Logger.Info("Hydra SDK started");
        }
    }

    public void OnDisable()
    {
        Client.Instance.Logger.Info("Shutting down Hydra SDK");

        Client.Instance.Shutdown();
        Client.Instance.Logger.Handler -= UnityLoggerHandler;
    }

    void Update()
    {
        Client.Instance.Dispatcher.Process();
        Client.Instance.EventDispatcher.Process();

        UpdatePing();
#if UNITY_IPHONE && !UNITY_EDITOR
        remoteNotifications.Process();
#endif
    }

    void UpdatePing()
    {
        if (Time.time > nextPing && Client.Instance.Message.IsConnected)
        {
            Client.Instance.Logger.Info("Realtime ping");
            Client.Instance.Message.Ping();
            nextPing = Time.time + PING_INTERVAL;
        }
    }

    // GCM
    public void OnRegistered(string id)
    {
        // TODO: check to see if we need to send this to the server!
        Client.Instance.Notification.RegisterNotifications("gcm", id, delegate(Request request)
        {
            if (request.HasError())
            {
                // TODO: handle error!
            }
        });
    }

    public void OnUnregistered(string id)
    {
        Client.Instance.Notification.UnregisterNotifications("gcm", id, delegate(Request request)
        {
            if (request.HasError())
            {
                // TODO: handle error!
            }
        });
    }

    public void OnError(string error)
    {
        Client.Instance.Logger.Error(error);
    }

    public void OnMessage(string message)
    {
        Client.Instance.Logger.Info("Received message : " + message);
        Dictionary<object, object> data = (Dictionary<object, object>) MiniJSON.Json.Deserialize(message);
        Client.Instance.Notification.Dispatch(data);
    }

    // GCM

    void UnityLoggerHandler(Logger.Level level, string message)
    {
        if (level == Logger.Level.Info)
        {
            Debug.Log(message);
        }
        else if (level == Logger.Level.Warn)
        {
            Debug.LogWarning(message);
        }
        else if (level == Logger.Level.Error)
        {
            Debug.LogError(message);
        }
    }
}
