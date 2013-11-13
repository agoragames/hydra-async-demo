#if UNITY_IPHONE && !UNITY_EDITOR

using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using AgoraGames.Hydra;
using AgoraGames.Hydra.Util;

namespace AgoraGames.Hydra.Util
{
    public class IOSRemoteNotifications
    {
        protected Client client;
        private string tokenError;
        private bool tokenSent;

        public IOSRemoteNotifications (Client client)
        {
            this.client = client;
        }

        public void Register ()
        {
            tokenError = null;
            tokenSent = false;
            NotificationServices.RegisterForRemoteNotificationTypes(RemoteNotificationType.Alert |
                                                                    RemoteNotificationType.Badge |
                                                                    RemoteNotificationType.Sound);
        }

        private void SendDeviceToken()
        {
            byte[] token = NotificationServices.deviceToken;
            if(token != null)
            {
                string hexToken = StreamUtils.ByteArrayToHex(token);
                client.Notification.RegisterNotifications("apns", hexToken, delegate(Request request)
                {
                    if(request.HasError())
                    {
                        // TODO: handle error!
                        client.Logger.Error("error registering for APNs remote notifications");
                    }
                });
            }
        }

        private Dictionary<object, object> IDictionaryToDictionary(IDictionary idict)
        {
            Dictionary<object, object> d = new Dictionary<object, object>();
            foreach(string k in idict.Keys)
            {
                d.Add(k, idict[k]);
            }
            return d;
        }

        public void ProcessNotifications()
        {
            foreach(RemoteNotification notification in NotificationServices.remoteNotifications)
            {
                client.Logger.Info("received remote notification");
                // since Notification service currently works in terms of Dictionary (as opposed to IDictionary interface)
                // we need to load the notification data into a concrete Dictionary before passing it along to the service
                client.Notification.Dispatch(IDictionaryToDictionary(notification.userInfo));
            }
            NotificationServices.ClearRemoteNotifications();
        }

        public void Process()
        {
            if(!client.IsInitalized)
            {
                return;
            }

            if(null == tokenError && !tokenSent)
            {
                if(null != NotificationServices.registrationError)
                {
                    tokenError = NotificationServices.registrationError;
                    client.Logger.Error(tokenError);
                    return;
                } else
                {
                    SendDeviceToken();
                    tokenSent = true;
                }
            }

            if(NotificationServices.remoteNotificationCount > 0)
            {
                ProcessNotifications();
            }
        }
    }
}

#endif // UNITY_IPHONE