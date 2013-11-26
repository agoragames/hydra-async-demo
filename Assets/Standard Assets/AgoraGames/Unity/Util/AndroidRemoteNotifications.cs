#if UNITY_ANDROID && !UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

namespace AgoraGames.Hydra.Util
{
    public class AndroidRemoteNotifications
    {
        protected AndroidJavaClass unityPlayerClass = new UnityEngine.AndroidJavaClass("com.unity3d.player.UnityPlayer");
        protected AndroidJavaClass gcmClass = new UnityEngine.AndroidJavaClass("com.agoragames.hydra.unity.GCM");

        protected Client client;

        public AndroidRemoteNotifications(Client client)
        {
            this.client = client;
        }

        public void InitAndroidNotifications(UnityEngine.MonoBehaviour mono, string androidProjectId)
        {
            InitAndroidNotifications(mono, androidProjectId, null);
        }

        public void InitAndroidNotifications(UnityEngine.MonoBehaviour mono, string androidProjectId, string notificationTitle)
        {
            InitAndroidNotifications(mono, androidProjectId, notificationTitle, "app_icon");
        }

        public void InitAndroidNotifications(UnityEngine.MonoBehaviour mono, string androidProjectId, string notificationTitle, string notificationIconName)
        {
            InitAndroidNotifications(mono, androidProjectId, notificationTitle, notificationIconName, null);
        }

        public void InitAndroidNotifications(UnityEngine.MonoBehaviour mono, string androidProjectId, string notificationTitle, string notificationIconName, string notificationActivityClassName)
        {
            client.Logger.Info("InitAndroidNotifications");

            UnityEngine.AndroidJavaObject currentActivity = unityPlayerClass.GetStatic<UnityEngine.AndroidJavaObject>("currentActivity");
            CallInitAndroidNotifications(currentActivity, mono.name, androidProjectId, notificationActivityClassName, notificationTitle, notificationIconName);
        }

        // This must match GCM.initNotifications in GCM.java
        protected void CallInitAndroidNotifications(AndroidJavaObject senderActivity, string gameObjectName, string androidProjectId, string notificationActivityClassName, string notificationTitle, string notificationIconName)
        {
            gcmClass.CallStatic("initNotifications", new object[] { senderActivity, gameObjectName, androidProjectId, notificationActivityClassName, notificationTitle, notificationIconName });
        }

    }
}


#endif