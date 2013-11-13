using AgoraGames.Hydra.Util;
using System;
using System.Collections;
using System.Collections.Generic;

namespace AgoraGames.Hydra.Models
{
    public delegate bool MatchNotificationActiveHandler(Match match, MatchPlayer fromPlayer, string notification);
    public delegate bool MatchPlayerActiveHandler(Match obj, MatchPlayer player);
    public delegate bool MatchNoticeActiveHandler(Match match, string notification);

    public class MatchPlayer 
    {
        protected string id;
        public string Id { get { return id; } }

        protected Identity identity;
        public Identity Identity { get { return identity; } }

        protected Dictionary<object, object> data;
        public Dictionary<object, object> Data { get { return data; } }

        protected string state;
        public String State { get { return state; } }

        public MatchPlayer(Dictionary<object, object> data)
        {
            this.data = data;

            MapHelper mapHelper = new MapHelper(data);
            id = mapHelper.GetValue("account_id", (string)null);
            state = mapHelper.GetValue("state", (string)null);
            identity = new Identity(mapHelper.GetValue("identity", (Dictionary<object, object>)null));
        }

        public MatchPlayer(String id, Dictionary<object, object> data)
        {
            this.data = data;

            MapHelper mapHelper = new MapHelper(data);
            this.id = id;
            state = mapHelper.GetValue("state", (string)null);
            identity = new Identity(mapHelper.GetValue("identity", (Dictionary<object, object>)null));
        }

        public object this[string key]
        {
            get { return new MapHelper(data).GetValue(key, (object)null); }
        }
    }

    public class MatchResult
    {
        public List<MatchPlayer> WinningTeam { get; protected set; }
        public List<MatchPlayer> LosingTeam { get; protected set; }
        public bool Draw { get; set; }

        public MatchResult()
        {
            WinningTeam = new List<MatchPlayer>();
            LosingTeam = new List<MatchPlayer>();
            Draw = false;
        }

        public MatchResult(List<MatchPlayer> winningTeam, List<MatchPlayer> losingTeam, bool draw)
        {
            WinningTeam = winningTeam;
            LosingTeam = losingTeam;
            Draw = draw;
        }

        public bool IsWinner(MatchPlayer player)
        {
            return player == null ? false : IsWinner(player.Id);
        }

        public bool IsWinner(string id)
        {
            foreach (var player in WinningTeam)
            {
                if (player.Id == id)
                    return true;
            }
            return false;
        }
    }

    public class MatchNotification
    {
        public enum RecipientPreset
        {
            Everyone,
            EveryoneButMe,
            NoOne,
            Custom
        }

        public string Name { get; set; }
        public RecipientPreset Preset { get; set; }
        public List<MatchPlayer> Recipients { get; set; }

        public MatchNotification(string name, RecipientPreset preset)
        {
            Name = name;
            Preset = preset;
            Recipients = null;
        }

        public MatchNotification(string name, List<MatchPlayer> recipients)
        {
            Name = name;
            Preset = RecipientPreset.Custom;
            Recipients = recipients;
        }

        public const string EveryoneString = "all";
        public const string EveryoneButMeString = "others";
        public const string NoOneString = "none";
        public const string CustomString = "list";
        public static string GetStringFromPreset(RecipientPreset preset)
        {
            if (preset == RecipientPreset.Everyone)
                return EveryoneString;
            else if (preset == RecipientPreset.EveryoneButMe)
                return EveryoneButMeString;
            else if (preset == RecipientPreset.NoOne)
                return NoOneString;
            else if (preset == RecipientPreset.Custom)
                return CustomString;
            else
                return "unknown";
        }

        public static MatchNotification ToEveryone(string name)
        {
            return new MatchNotification(name, RecipientPreset.Everyone);
        }

        public static MatchNotification ToEveryoneButMe(string name)
        {
            return new MatchNotification(name, RecipientPreset.EveryoneButMe);
        }

        public static MatchNotification None
        {
            get { return new MatchNotification("", RecipientPreset.NoOne); }
        }
    }

    public class Match : EditableModel
    {
        public enum Type
        {
            Async,
            Realtime,
            None
        };

        public static string GetStringFromType(Type type)
        {
            switch (type)
            {
                case Type.Async:
                    return "async";
                case Type.Realtime:
                    return "realtime";
                default:
                    return "[none]";
            }
        }

        public static Type GetTypeFromString(string str)
        {
            if (str == "async")
                return Type.Async;
            else if (str == "realtime")
                return Type.Realtime;
            else
                return Type.None;
        }

        public static Type GetTypeFromObject(object obj)
        {
            return GetTypeFromString(obj as string);
        }

        public enum Access
        {
            Public,
            Private,
            MatchMaking,
            None
        };

        public static string GetStringFromAccess(Access type)
        {
            switch (type)
            {
                case Access.Private:
                    return "private";
                case Access.Public:
                    return "public";
                case Access.MatchMaking:
                    return "matchmaking";
                default:
                    return "[none]";
            }
        }

        public static Access GetAccessFromString(string str)
        {
            if (str == "private")
                return Access.Private;
            else if (str == "public")
                return Access.Public;
            else if (str == "matchmaking")
                return Access.MatchMaking;
            else
                return Access.None;
        }

        public static Access GetAccessFromObject(object obj)
        {
            return GetAccessFromString(obj as string);
        }

        public override string Endpoint
        {
            get { return "matches"; }
        }

        public event MatchNotificationActiveHandler Updated;
        public event MatchPlayerActiveHandler PlayerJoined;
        public event MatchPlayerActiveHandler PlayerLeft;
        public event MatchNotificationActiveHandler Completed;
        public event MatchNoticeActiveHandler ExpirationWarning;

        protected RealtimeSession session;
        public RealtimeSession RealtimeSession 
        {
            get {
                return session;
            }
        }

        protected string key;
        public string Key
        {
            get { return key; }
        }

        protected string name;
        public string Name
        {
            get { return name; }
        }

        protected List<MatchPlayer> players = new List<MatchPlayer>();
        public List<MatchPlayer> Players
        {
            get { return players; }
        }

        protected MatchResult result = new MatchResult();
        public MatchResult Result { get { return result; } }

        protected double rand;
        protected double Rand { get { return rand; } }

        protected Access access = Access.None;
        public Access MatchAccess { get { return access; } }

        public string State { get; protected set; }
        public bool IsComplete { get { return State == "complete"; } }

        protected DateTime updatedAt;
        public DateTime UpdatedAt { get { return updatedAt; } }

        protected DateTime createdAt;
        public DateTime CreatedAt { get { return createdAt; } }

        public MatchTemplate Template { get; protected set; }

        public Match(Client client, string id)
            : base(client, id)
        {
        }

        public bool InMatch(string playerId)
        {
            MatchPlayer player = GetPlayer(playerId);

            if (player != null && player.State == "join")
            {
                return true;
            }
            return false;
        }

        public MatchPlayer GetPlayer(string playerId)
        {
            foreach (MatchPlayer player in players)
            {
                if (player.Id == playerId)
                {
                    return player;
                }
            }
            return null;
        }

        public List<String> GetIdsFromPlayers(List<MatchPlayer> playerList)
        {
            List<String> idList = new List<String>();
            foreach (MatchPlayer player in playerList)
            {
                idList.Add(player.Id);
            }
            return idList;
        }

        public List<MatchPlayer> GetPlayersFromIds(List<String> idList)
        {
            List<MatchPlayer> playerList = new List<MatchPlayer>();
            foreach (String id in idList)
            {
                MatchPlayer player = GetPlayer(id);
                if (player != null)
                    playerList.Add(player);
            }
            return playerList;
        }

        public void Update(Commands commands, MatchNotification notification, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            Update(commands, null, notification, handler);
        }

        public void Update(Commands commands, List<String> fields, MatchNotification notification, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            Update(commands, fields, null, notification, handler);
        }

        public void Update(Commands commands, List<String> fields, List<MatchPlayer> winningTeam, MatchNotification notification, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            UpdateInternal(commands.ConvertToRequest(), fields, winningTeam, notification, handler);
        }

        public void Update(Dictionary<object, object> data, MatchNotification notification, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            Update(data, null, notification, handler);
        }

        public void Update(Dictionary<object, object> data, List<String> fields, MatchNotification notification, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            Update(data, fields, notification, handler);
        }

        public void Update(Dictionary<object, object> data, List<String> fields, List<MatchPlayer> winningTeam, MatchNotification notification, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            UpdateInternal(data, fields, winningTeam, notification, handler);
        }

        protected void UpdateInternal(object param, List<String> fields, List<MatchPlayer> winningTeam, MatchNotification notification, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            UrlGenerator urlGen = new UrlGenerator(Endpoint + "/" + EndpointId).Append("fields", fields);
            urlGen.Append("custom_notification", true);

            Dictionary<object, object> payload = new Dictionary<object, object>();
            MapHelper payloadHelper = new MapHelper(payload);

            payloadHelper["notification.name"] = notification.Name;
            payloadHelper["notification.to_type"] = MatchNotification.GetStringFromPreset(notification.Preset);
            if(notification.Recipients != null)
                payloadHelper["notification.to_list"] = GetIdsFromPlayers(notification.Recipients);

            if (param != null)
            {
                payload["update_data"] = param;
            }

            if (winningTeam != null)
            {
                List<string> winningTeamIds = GetIdsFromPlayers(winningTeam);
                payload["winning_team"] = winningTeamIds;
            }

            HelperUtil.DualDelegate dual = new HelperUtil.DualDelegate(ObjectResponse, handler);
            client.DoRequest(urlGen.ToString(), "put", payload, dual.Response);
        }

        public void Invite(string friendId, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            Invite(friendId, null, handler);
        }

        public void Invite(string friendId, List<string> fields, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            Invite(friendId, true, null, handler);
        }

        public void Invite(string friendId, bool reserve, List<string> fields, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            string url = new UrlGenerator("matches/" + Id + "/invite").Append("fields", fields).ToString();

            data["id"] = friendId;
            data["reserve"] = reserve;

            client.DoRequest(url, "put", data, delegate(Request request)
            {
                if (!request.HasError())
                {
                }

                ObjectResponse(request);
                handler(request);
            });
        }

        public void InviteByUsername(string friendId, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            InviteByUsername(friendId, null, handler);
        }

        public void InviteByUsername(string friendId, List<string> fields, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            InviteByUsername(friendId, true, fields, handler);
        }

        public void InviteByUsername(string friendId, bool reserve, List<string> fields, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            Dictionary<string, object> data = new Dictionary<string, object>();
            string url = new UrlGenerator("matches/" + Id + "/invite").Append("fields", fields).ToString();

            data["type"] = "name";
            data["id"] = friendId;
            data["reserve"] = reserve;

            client.DoRequest(url, "put", data, delegate(Request request)
            {
                if (!request.HasError())
                {
                }

                ObjectResponse(request);
                handler(request);
            });
        }

        public void Join(AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            JoinImpl(null, null, handler);
        }

        public void Join(List<string> fields, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            JoinImpl(null, fields, handler);
        }

        public void JoinAsMatch(string matchId, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            JoinImpl(matchId, null, handler);
        }

        public void JoinAsMatch(string matchId, List<string> fields, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            JoinImpl(matchId, fields, handler);
        }

        protected void JoinImpl(string matchId, List<string> fields, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            string url = new UrlGenerator("matches/" + Id + "/join").Append("fields", fields).ToString();
            Dictionary<object, object> data = new Dictionary<object, object>();

            if (matchId != null)
            {
                data["match"] = matchId;
            }

            client.DoRequest(url, "put", data, delegate(Request request)
            {
                handleMatchJoin(request);
                handler(request);
            });
        }

        public void Complete(AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            Complete((List<string>)null, handler);
        }

        public void Complete(List<string> fields, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            Complete(null, fields, handler);
        }

        public void Complete(MatchResult result, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            Complete(result, null, handler);
        }

        public void Complete(MatchResult result, List<string> fields, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            string url = new UrlGenerator("matches/" + Id + "/complete").Append("fields", fields).ToString();

            Dictionary<object, object> data = new Dictionary<object, object>();

            if (result != null && result.WinningTeam.Count > 0 && result.LosingTeam.Count > 0)
            {
                data.Add("win", GetIdsFromPlayers(result.WinningTeam));
                data.Add("loss", GetIdsFromPlayers(result.LosingTeam));
                data.Add("draw", result.Draw);
            }

            client.DoRequest(url, "put", data, delegate(Request request)
            {
                // TODO: something!
                ObjectResponse(request);
                handler(request);
            });
        }

        public void Leave(AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            Leave(null, handler);
        }

        public void Leave(List<string> fields, AgoraGames.Hydra.Client.HydraRequestHandler handler)
        {
            string url = new UrlGenerator("matches/" + Id + "/leave").Append("fields", fields).ToString();

            client.DoRequest(url, "put", new Dictionary<object, object>(), delegate(Request request)
            {
                // TODO: i think we can just ignore the response since we have left
                ObjectResponse(request);
                handler(request);
            });
        }

        public void JoinSession()
        {
            // we will need to pull the logic type from the match template eventually
            //  for now we'll just hardcode match
            Dictionary<string, string> data = new Dictionary<string, string>();

            data["key"] = key;

            session = client.Message.JoinSession(RealtimeLogicFactory.LogicType.Match, id, data);
        }

        internal static bool CallActiveHandlers(MatchNotificationActiveHandler activeHandlers, Match match, MatchPlayer fromPlayer, string notification)
        {
            bool ret = false;
            if (activeHandlers != null)
            {
                foreach (Delegate d in activeHandlers.GetInvocationList())
                {
                    MatchNotificationActiveHandler handler = (MatchNotificationActiveHandler)d;
                    ret |= (bool)handler(match, fromPlayer, notification);
                }
            }
            return ret;
        }

        internal static bool CallActiveHandlers(MatchPlayerActiveHandler activeHandlers, Match match, MatchPlayer fromPlayer)
        {
            bool ret = false;
            if (activeHandlers != null)
            {
                foreach (Delegate d in activeHandlers.GetInvocationList())
                {
                    MatchPlayerActiveHandler handler = (MatchPlayerActiveHandler)d;
                    ret |= (bool)handler(match, fromPlayer);
                }
            }
            return ret;
        }

        internal static bool CallActiveHandlers(MatchNoticeActiveHandler activeHandlers, Match match, string notification)
        {
            bool ret = false;
            if (activeHandlers != null)
            {
                foreach (Delegate d in activeHandlers.GetInvocationList())
                {
                    MatchNoticeActiveHandler handler = (MatchNoticeActiveHandler)d;
                    ret |= (bool)handler(match, notification);
                }
            }
            return ret;
        }

        internal static bool SupportsCommand(string command)
        {
            return command == "update" ||
                command == "join" ||
                command == "leave" ||
                command == "complete" ||
                command == "expiration-warning";
        }

        public bool Dispatch(string command, Dictionary<object, object> message)
        {
            MatchPlayer player = ResolvePlayer(message);
            string header = message.ContainsKey("header") ? message["header"] as string : "";

            if (command == "update" && Updated != null)
            {
                return CallActiveHandlers(Updated, this, player, header);
            }
            else if (command == "join" && PlayerJoined != null)
            {
                return CallActiveHandlers(PlayerJoined, this, player);
            }
            else if (command == "leave" && PlayerLeft != null)
            {
                return CallActiveHandlers(PlayerLeft, this, player);
            }
            else if (command == "complete" && Completed != null)
            {
                return CallActiveHandlers(Completed, this, player, header);
            }
            else if (command == "expiration-warning" && ExpirationWarning != null)
            {
                return CallActiveHandlers(ExpirationWarning, this, header);
            }

            return false;
        }

        public MatchPlayer ResolvePlayer(Dictionary<object, object> message)
        {
            MatchPlayer player = null;

            Dictionary<object, object> payload = (Dictionary<object, object>)message["payload"];
            object frmobj = null;
            if (payload.TryGetValue("frm", out frmobj))
            {
                Dictionary<object, object> frm = frmobj as Dictionary<object, object>;
                MapHelper mapHelper = new MapHelper(frm);
                string accountId = mapHelper.GetValue("id", (string)null);

                player = GetPlayer(accountId);
                if (player == null)
                {
                    // TODO: should we add this to the match?
                    player = new MatchPlayer(accountId, frm);
                }
            }
         
            return player;
        }

        protected void ResolvePlayers(List<object> playersData)
        {
            if (playersData != null)
            {
                this.players.Clear();

                foreach (object iter in playersData)
                {
                    Dictionary<object, object> playerData = (Dictionary<object, object>)iter;

                    this.players.Add(new MatchPlayer(playerData));
                }
            }
        }

        public override void Merge(IDictionary map)
        {
            base.Merge(map);

            MapHelper mapHelper = new MapHelper(map);
            key = mapHelper.GetValue("key", key);
            name = mapHelper.GetValue("name", name);
            rand = mapHelper.GetValue("rand", rand);
            access = mapHelper.GetValue("access", access, GetAccessFromObject);
            State = mapHelper.GetValue("state", State);

            List<string> winningIds = GetIdsFromPlayers(Result.WinningTeam);
            winningIds = mapHelper.GetValue("win", winningIds, ObjectListToStringList);
            List<string> losingIds = GetIdsFromPlayers(Result.LosingTeam);
            losingIds = mapHelper.GetValue("loss", losingIds, ObjectListToStringList);
            bool draw = mapHelper.GetValue("draw", result.Draw);
            result = new MatchResult(GetPlayersFromIds(winningIds), GetPlayersFromIds(losingIds), draw);

            createdAt = mapHelper.GetValue("created_at", createdAt);
            updatedAt = mapHelper.GetValue("updated_at", updatedAt);

            Dictionary<object, object> templateData = mapHelper.GetValue("template", (Dictionary<object, object>)null);
            if (templateData != null)
                Template = new MatchTemplate(templateData);

            List<object> playersData = mapHelper.GetValue("players.all", (List<object>)null);
            ResolvePlayers(playersData);
        }

        protected List<string> ObjectListToStringList(object obj)
        {
            List<string> strList = new List<string>();

            if (obj != null && obj is List<object>)
            {
                foreach (var entry in obj as List<object>)
                {
                    strList.Add(entry as string);
                }
            }

            return strList;
        }

        protected void handleMatchJoin(Request request)
        {
            if (!request.HasError())
            {
                // lets connect to the session now, maybe the response might have our key instead of
                //  the inital invite?
                //JoinSession();
            }

            // TODO: something!
            ObjectResponse(request);
        }
    }
}

