using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.IO;

namespace AgoraGames.Hydra.Services
{
    public class NotificationsService
    {
        protected Client client;

        public NotificationsService(Client client)
        {
            this.client = client;
        }

        public void GetNotification(String id, AgoraGames.Hydra.Client.HydraRequestHandler requestHandler)
        {
            client.DoRequest("notifications/" + id, "get", null, requestHandler);
        }

        public void AcknowledgeNotification(string id, AgoraGames.Hydra.Client.HydraRequestHandler requestHandler)
        {
            client.DoRequest("notifications/" + id + "/acknowledge", "put", new Dictionary<object, object>(), requestHandler);
        }

        public void PollMessages(AgoraGames.Hydra.Client.HydraRequestHandler requestHandler)
        {
            AgoraGames.Hydra.Util.HelperUtil.DualDelegate dual = new Util.HelperUtil.DualDelegate(HandleNotifications, requestHandler);
            client.DoRequest("notifications/new", "get", null, dual.Response);
        }

        public void RegisterNotifications(string type, string regId, AgoraGames.Hydra.Client.HydraRequestHandler requestHandler) 
        {
            Dictionary<string, string> map = new Dictionary<string, string>();

		    map["type"] = type;
		    map["value"] = regId;

            client.DoRequest("notifications/register", "post", map, requestHandler);
	    }

        public void UnregisterNotifications(string type, string regId, AgoraGames.Hydra.Client.HydraRequestHandler requestHandler)
        {
            Dictionary<string, string> map = new Dictionary<string, string>();

		    map["type"] = type;
		    map["value"] = regId;

            client.DoRequest("notifications/unregister", "put", map, null);
	    }

        protected void HandleNotifications(Request request)
        {
            if (!request.HasError())
            {
                if (request.Data is List<object>)
                {
                    List<object> notifications = request.Data as List<object>;
                    foreach (var notification in notifications)
                    {
                        if (notification is Dictionary<object, object>)
                        {
                            Dispatch(notification as Dictionary<object, object>);
                        }
                    }
                }
            }
        }

        public void Dispatch(Dictionary<object, object> data)
        {
            if (data.ContainsKey("cmd"))
            {
                object idobj;
                bool hasId = data.TryGetValue("id", out idobj);
                string cmd = (string)data["cmd"];

                // TODO: this needs to change to something nicer!, to do this right the
                // message could have a category as well as a cmd
                if (client.Match.SupportsCommand(cmd))
                {
                    client.Match.Dispatch(cmd, data);
                }
                else if (IsMatchMakingMessage(cmd))
                {
                    client.MatchMaking.Dispatch(cmd, data);
                }
                else if (IsAchievementMessage(cmd))
                {
                    client.Achievements.Dispatch(cmd, data);
                }
                else if (IsAccountMessage(cmd))
                {
                    client.Account.Dispatch(cmd, data);
                }

                // id is optional. Notifications without id do not need to be acknowledged.
                if (hasId)
                {
                    // TODO: handle errors!
                    AcknowledgeNotification(idobj as string, null);
                }
            }
            else if (data.ContainsKey("id"))
            {
                // need to load the notification payload
                string id = (string)data["id"];
                client.Notification.GetNotification(id, delegate(Request request)
                {
                    if (request.HasError())
                    {
                        // TODO handle error!
                        client.Logger.Error("error loading notification");
                    }
                    else
                    {
                        Dispatch(request.Data as Dictionary<object, object>);
                    }
                });
            }
        }

        protected bool IsMatchMakingMessage(string cmd)
        {
            return cmd == "matchmaking-complete" || cmd == "matchmaking-tick" || cmd == "matchmaking-timeout";
        }

        protected bool IsAchievementMessage(string cmd)
        {
            return cmd == "achievement-awarded" || cmd == "achievement-updated";
        }

        protected bool IsAccountMessage(string cmd)
        {
            return cmd == "friended" || cmd == "friend-online" || cmd == "friend-offline";
        }
    }
}
