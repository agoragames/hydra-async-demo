#if UNITY_ANDROID && !UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace AgoraGames.Hydra.Util
{
    public class AndroidRemoteNotifications
    {
        protected UnityEngine.AndroidJavaClass unityPlayerClass = new UnityEngine.AndroidJavaClass("com.unity3d.player.UnityPlayer");
        protected UnityEngine.AndroidJavaClass gcmClass = new UnityEngine.AndroidJavaClass("com.agoragames.hydra.unity.GCM");
        protected UnityEngine.AndroidJavaObject currentActivity;

        protected Client client;

        public AndroidRemoteNotifications(Client client)
        {
            this.client = client;
        }

        public void InitAndroidNotifications(UnityEngine.MonoBehaviour mono, string id)
        {
            initAndroid(mono, id);
        }

        // TODO: move me to the unity folder
        protected void initAndroid(UnityEngine.MonoBehaviour mono, string id)
        {
            client.Logger.Info("initAndroid");

            currentActivity = unityPlayerClass.GetStatic<UnityEngine.AndroidJavaObject>("currentActivity");

            gcmClass.CallStatic("initNotifications", new object[] { currentActivity, mono.name, id });
        }

    }
}


#endif