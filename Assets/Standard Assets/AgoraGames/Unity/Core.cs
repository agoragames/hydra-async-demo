using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AgoraGames.Hydra;
using AgoraGames.Hydra.Models;
using AgoraGames.Hydra.Util;

public class Core : MonoBehaviour
{
    public string url;
    public string apiKey;

    protected float nextPing;

    protected static int PING_INTERVAL = 15; // seconds

    // TODO: make this an interface abstraction?
#if UNITY_IPHONE && !UNITY_EDITOR
    IOSRemoteNotifications remoteNotifications;
#elif UNITY_ANDROID && !UNITY_EDITOR
    AndroidRemoteNotifications remoteNotifications;
#endif

    // Unity
    public void Start()
    {
        Application.runInBackground = true;
#if UNITY_IPHONE && !UNITY_EDITOR
        remoteNotifications = new IOSRemoteNotifications(Client.Instance);
        remoteNotifications.Register();
#elif UNITY_ANDROID && !UNITY_EDITOR
        remoteNotifications = new AndroidRemoteNotifications(Client.Instance);
#endif
    }

    public void OnEnable()
    {
        Client.Instance.Logger.Handler += UnityLoggerHandler;

        Client.Instance.Message.Connected += Message_Connected;
        Client.Instance.Message.Disconnected += Message_Disconnected;

        Client.Instance.Logger.Info("startup sdk");
        Client.Instance.Startup();

        Client.Instance.Init(new UnityRunner(this), null, url, apiKey);

#if UNITY_WEBPLAYER
		Application.ExternalCall("initHydraSdk",  new object[] { this.name });
#endif

        doDefaultAuth();
    }

    void Message_Connected()
    {
        Client.Instance.Logger.Info("Realtime online");
    }

    void Message_Disconnected()
    {
        Client.Instance.Logger.Info("Realtime offline");
    }

    void doDefaultAuth()
    {
        // for now we aren't going to cache any auth tokens
        //authToken = PlayerPrefs.GetString(HEADER_AUTH_TOKEN, null);

        Client.Instance.Authenticate(AuthType.UUID, SystemInfo.deviceUniqueIdentifier, null, true, delegate(Request req)
        {

#if UNITY_ANDROID && !UNITY_EDITOR
            if (Client.Instance.CurrentConfiguration.AndroidProjectNumber != null)
            {
                Client.Instance.Logger.Info("init android notifications");
                remoteNotifications.InitAndroidNotifications(this, Client.Instance.CurrentConfiguration.AndroidProjectNumber);
            }
#endif

            if (req.HasError())
            {
                Client.Instance.Logger.Error("invalid auth");
            }
            else
            {
                Client.Instance.Logger.Info("valid auth");
            }
        });
    }

    public void OnDisable()
    {
        Client.Instance.Logger.Info("shutdown sdk");

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
            Client.Instance.Logger.Info("ping");
            Client.Instance.Message.Ping();
            nextPing = Time.time + PING_INTERVAL;
        }
    }

	// Facebook
	public void AuthenticateWithFacebookAccessToken(string accessToken)
	{
		Client.Instance.Authenticate(AuthType.FACEBOOK, accessToken, delegate(Request req)
        {
            // push this
        });
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
