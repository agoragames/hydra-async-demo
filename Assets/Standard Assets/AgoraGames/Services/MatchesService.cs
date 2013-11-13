using System;
using System.Collections.Generic;

using AgoraGames.Hydra.Models;
using AgoraGames.Hydra.Util;

namespace AgoraGames.Hydra.Services
{
	public class MatchesService
	{
        protected ObjectMap<Match> map = null;
        protected Client client = null;

        public delegate void MatchNotificationListener(Match match, MatchPlayer fromPlayer, string notification);
        public delegate void MatchPlayerListener(Match obj, MatchPlayer player);
        public delegate void MatchNoticeListener(Match match, string notification);

        public delegate void MatchHandler(Match obj, Request request);
        public delegate void MatchListHandler(List<Match> list, Request request);

        public delegate void MatchTemplatesListHandler(List<MatchTemplate> list, Request request);

        public event MatchNotificationActiveHandler Invited;
        public event MatchNotificationActiveHandler Updated;
        public event MatchPlayerActiveHandler PlayerJoined;
        public event MatchPlayerActiveHandler PlayerLeft;
        public event MatchNotificationActiveHandler Completed;
        public event MatchNoticeActiveHandler ExpirationWarning;

        public event MatchNotificationListener UnhandledInvited;
        public event MatchNotificationListener UnhandledUpdated;
        public event MatchPlayerListener UnhandledPlayerJoined;
        public event MatchPlayerListener UnhandledPlayerLeft;
        public event MatchNotificationListener UnhandledCompleted;
        public event MatchNoticeListener UnhandledExpirationWarning;

        public MatchesService(Client client)
		{
            this.client = client;
            this.map = new ObjectMap<Match>(client, (c, i) => { return new Match(c, i); });
		}

        public void CreateNew(string template, MatchHandler handler)
        {
            CreateNewImpl(template, Match.Access.Private, null, handler);
        }

        public void CreateNew(string template, Match.Access access, MatchHandler handler)
        {
            CreateNewImpl(template, access, null, handler);
	    }

        public void CreateNewFromMatch(string template, string matchId, MatchHandler handler)
        {
            CreateNewImpl(template, Match.Access.Private, matchId, handler);
        }

        public void CreateNewFromMatch(string template, string matchId, Match.Access access, MatchHandler handler)
        {
            CreateNewImpl(template, access, matchId, handler);
        }

        protected void CreateNewImpl(string template, Match.Access access, string matchId, MatchHandler handler)
        {
            Dictionary<object, object> data = new Dictionary<object, object>();

            data["template"] = template;
            data["access"] = Match.GetStringFromAccess(access);

            if (matchId != null)
            {
                data["match"] = matchId;
            }

            // we want to connect to the session if we are the one creating the match, we might want to push this
            //  out to the client side code
            client.DoRequest("matches", "post", data, delegate(Request request)
            {
                Match obj = null;

                if (!request.HasError())
                {
                    obj = map.GetObject(request);

                    // if we are the one creating the match join it right away
                    // object.joinSession();
                    //matchListener.matchCreated(r, object);
                }

                handler(obj, request);
            });
        }

        public void LoadCurrent(MatchListHandler handler)
        {
            LoadCurrent(1, null, handler);
        }

        public void LoadCurrent(int page, MatchListHandler handler)
        {
            LoadCurrent(page, null, handler);
        }

        public void LoadCurrent(int page, List<string> fields, MatchListHandler handler)
        {
            LoadCurrent("me", page, fields, handler);
        }

        public void LoadCurrent(Friend friend, MatchListHandler handler)
        {
            LoadCurrent(friend, 1, null, handler);
        }

        public void LoadCurrent(Friend friend, int page, MatchListHandler handler)
        {
            LoadCurrent(friend, page, null, handler);
        }

        public void LoadCurrent(Friend friend, int page, List<string> fields, MatchListHandler handler)
        {
            LoadCurrent(friend.AccountId, page, fields, handler);
        }

        public void LoadCurrent(string accountId, MatchListHandler handler)
        {
            LoadCurrent(accountId, 1, null, handler);
        }

        public void LoadCurrent(string accountId, int page, MatchListHandler handler)
        {
            LoadCurrent(accountId, page, null, handler);
        }

        public void LoadCurrent(string accountId, int page, List<string> fields, MatchListHandler handler)
        {
            string url = new UrlGenerator("matches/current/" + accountId).Append("fields", fields).Append("page", page).ToString();

            client.DoRequest(url, "get", null, delegate(Request request)
            {
                List<Match> ret = ResolveMatches(request);

                handler(ret, request);
            });
        }

        public void LoadAll(MatchListHandler handler)
        {
            LoadAll(1, null, handler);
        }

        public void LoadAll(int page, MatchListHandler handler)
        {
            LoadAll(page, null, handler);
        }

        public void LoadAll(int page, List<string> fields, MatchListHandler handler)
        {
            LoadAll("me", page, fields, handler);
        }

        public void LoadAll(Friend friend, MatchListHandler handler)
        {
            LoadAll(friend, 1, null, handler);
        }

        public void LoadAll(Friend friend, int page, MatchListHandler handler)
        {
            LoadAll(friend, page, null, handler);
        }

        public void LoadAll(Friend friend, int page, List<string> fields, MatchListHandler handler)
        {
            LoadAll(friend.AccountId, page, fields, handler);
        }

        public void LoadAll(string accountId, MatchListHandler handler)
        {
            LoadAll(accountId, 1, null, handler);
        }

        public void LoadAll(string accountId, int page, MatchListHandler handler)
        {
            LoadAll(accountId, page, null, handler);
        }

        public void LoadAll(string accountId, int page, List<string> fields, MatchListHandler handler)
        {
            string url = new UrlGenerator("matches/all/" + accountId).Append("fields", fields).Append("page", page).ToString();

            client.DoRequest(url, "get", null, delegate(Request request)
            {
                List<Match> ret = ResolveMatches(request);

                handler(ret, request);
            });
        }

        public void LoadPublic(MatchListHandler handler)
        {
            LoadPublic(1, null, handler);
        }

        public void LoadPublic(List<string> fields, MatchListHandler handler)
        {
            LoadPublic(1, fields, handler);
        }

        public void LoadPublic(int page, List<string> fields, MatchListHandler handler)
        {
            string url = new UrlGenerator("matches/public").Append("fields", fields).Append("page", page).ToString();

            client.DoRequest(url, "get", null, delegate(Request request)
            {
                List<Match> ret = ResolveMatches(request);

                handler(ret, request);
            });
	    }

        public void Load(string id, MatchHandler handler)
        {
            Load(id, null, handler);
        }

        public void Load(string id, List<string> fields, MatchHandler handler)
        {
            string url = new UrlGenerator("matches/").Append(id).Append("fields", fields).ToString();

            client.DoRequest(url, "get", null, delegate(Request request)
            {
				Match obj = map.GetObject(request);

                handler(obj, request);
		    });
	    }

        // templates
        public void LoadMatchTemplates(MatchTemplatesListHandler handler)
        {
            client.DoRequest("matches/templates", "get", null, delegate(Request request)
            {
                List<MatchTemplate> templates = resolveMatchTemplates(request);

                handler(templates, request);
            });
        }

        public bool SupportsCommand(string command)
        {
            return Match.SupportsCommand(command) || command == "invite";
        }

        public void Dispatch(string command, Dictionary<object, object> message) 
        {
            Match match = ResolveMatchFromMessage(message);
            MatchPlayer player = match.ResolvePlayer(message);
            string header = message.ContainsKey("header") ? message["header"] as string : "";

            if (command == "invite") {
                TriggerMatchNotificationEvents(Invited, UnhandledInvited, false, match, player, header);
            }
            else if (Match.SupportsCommand(command))
            {
                bool handled = match.Dispatch(command, message);

                if (command == "update")
                {
                    TriggerMatchNotificationEvents(Updated, UnhandledUpdated, handled, match, player, header);
                }
                else if (command == "join")
                {
                    TriggerMatchNotificationEvents(PlayerJoined, UnhandledPlayerJoined, handled, match, player);
                }
                else if (command == "leave")
                {
                    TriggerMatchNotificationEvents(PlayerLeft, UnhandledPlayerLeft, handled, match, player);
                }
                else if (command == "complete")
                {
                    TriggerMatchNotificationEvents(Completed, UnhandledCompleted, handled, match, player, header);
                }
                else if (command == "expiration-warning")
                {
                    TriggerMatchNotificationEvents(ExpirationWarning, UnhandledExpirationWarning, handled, match, header);
                }
            }
        }

        protected static void TriggerMatchNotificationEvents(MatchNotificationActiveHandler activeHandlers, MatchNotificationListener passiveHandlers, bool handled,
                                                                Match match, MatchPlayer player, string notification)
        {
            // We'll always trigger the active handlers, even if the individual object handlers handled it.
            // It's only the passive handlers that may not get called at all.
            handled |= Match.CallActiveHandlers(activeHandlers, match, player, notification);

            if (!handled && passiveHandlers != null)
                passiveHandlers(match, player, notification);
        }

        protected static void TriggerMatchNotificationEvents(MatchPlayerActiveHandler activeHandlers, MatchPlayerListener passiveHandlers, bool handled,
                                                                Match match, MatchPlayer player)
        {
            // We'll always trigger the active handlers, even if the individual object handlers handled it.
            // It's only the passive handlers that may not get called at all.
            handled |= Match.CallActiveHandlers(activeHandlers, match, player);

            if (!handled && passiveHandlers != null)
                passiveHandlers(match, player);
        }

        protected static void TriggerMatchNotificationEvents(MatchNoticeActiveHandler activeHandlers, MatchNoticeListener passiveHandlers, bool handled,
                                                                Match match, string notification)
        {
            // We'll always trigger the active handlers, even if the individual object handlers handled it.
            // It's only the passive handlers that may not get called at all.
            handled |= Match.CallActiveHandlers(activeHandlers, match, notification);

            if (!handled && passiveHandlers != null)
                passiveHandlers(match, notification);
        }

        public List<Match> ResolveMatches(Request request)
        {
            List<Match> ret = new List<Match>();

            if (!request.HasError())
            {
                List<object> data = (List<object>)request.Data;

                foreach (Dictionary<object, object> iter in data)
                {
                    ret.Add(map.GetObject(iter));
                }
            }
            return ret;
        }

        protected List<MatchTemplate> resolveMatchTemplates(Request request)
        {
            List<MatchTemplate> ret = new List<MatchTemplate>();

            if (!request.HasError())
            {
                List<object> data = (List<object>)request.Data;

                foreach (Dictionary<object, object> iter in data)
                {
                    MatchTemplate template = new MatchTemplate(iter);

                    ret.Add(template);
                }
            }
            return ret;
        }

        public Match ResolveMatch(Request request)
        {
            return map.GetObject(request);
        }

        public Match ResolveMatchFromMessage(Dictionary<object, object> message)
        {
            Dictionary<object, object> payload = (Dictionary<object, object>)message["payload"];
            Dictionary<object, object> match = (Dictionary<object, object>)payload["match"];

            return ResolveMatch(match);
        }

        public Match ResolveMatch(Dictionary<object, object> data)
        {
            return map.GetObject(data);
        }

    }
}

