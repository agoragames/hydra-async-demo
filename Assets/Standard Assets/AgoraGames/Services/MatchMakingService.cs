using System;
using System.Collections.Generic;

using AgoraGames.Hydra;
using AgoraGames.Hydra.Models;
using AgoraGames.Hydra.Util;

namespace AgoraGames.Hydra.Services
{
    public class MatchMakingService
    {
        protected ObjectMap<MatchMakingRequest> map = null;
        protected Client client = null;

        public delegate void MatchMakingHandler(MatchMakingRequest matchMaking);
        public event MatchMakingHandler MatchmakingComplete;
        public event MatchMakingHandler MatchmakingTick;
        public event MatchMakingHandler MatchmakingTimeout;

        public delegate void MatchMakingJoinListHandler(Match match, Request request);
        public delegate void MatchMakingListListHandler(List<Match> match, Request request);
        public delegate void MatchMakingCriteriaListHandler(List<MatchMakingCriteria> list, Request request);
        public delegate void MatchMakingSubmitHandler(MatchMakingRequest matchmaking, Request request);
        
        public MatchMakingService(Client client)
        {
            this.client = client;
            this.map = new ObjectMap<MatchMakingRequest>(client, (c, i) => { return new MatchMakingRequest(c, i); });
        }

        public void LoadCriteria(MatchMakingCriteriaListHandler handler)
        {
            client.DoRequest("matches/matchmaking/criteria", "get", null, delegate(Request request)
            {
                handler(resolveMatchMakingCriteria(request), request);
            });
        }

        public void Join(String criteria, Dictionary<object, object> data, MatchMakingJoinListHandler handler)
        {
            JoinImpl(criteria, null, null, data, handler);
        }

        public void JoinAsMatch(String criteria, String groupMatchId, Dictionary<object, object> data, MatchMakingJoinListHandler handler)
        {
            JoinImpl(criteria, null, groupMatchId, data, handler);
        }

        public void JoinExisting(String criteria, String existingMatchId, Dictionary<object, object> data, MatchMakingJoinListHandler handler)
        {
            JoinImpl(criteria, existingMatchId, null, data, handler);
        }

        public void JoinExistingAsMatch(String criteria, String existingMatchId, String groupMatchId, Dictionary<object, object> data, MatchMakingJoinListHandler handler)
        {
            JoinImpl(criteria, existingMatchId, groupMatchId, data, handler);
        }

        protected void JoinImpl(String criteria, String existingMatchId, String groupMatchId, Dictionary<object, object> data, MatchMakingJoinListHandler handler)
        {
            UrlGenerator urlGen = new UrlGenerator("matches/matchmaking/").Append(criteria).Append("/join");
            Dictionary<string, object> payload = new Dictionary<string, object>();
            String url;

            if(existingMatchId != null) {
                urlGen.Append("/");
                urlGen.Append(existingMatchId);
            }
            url = urlGen.ToString();

            payload["data"] = data;
            if (groupMatchId != null)
            {
                payload["match"] = groupMatchId;
            }

            client.DoRequest(url, "put", payload, delegate(Request request)
            {
                handler(client.Match.ResolveMatch(request), request);
            });
        }

        public void List(String criteria, Dictionary<object, object> data, MatchMakingListListHandler handler)
        {
            ListImpl(criteria, null, data, handler);
        }

        public void ListAsMatch(String criteria, String matchId, Dictionary<object, object> data, MatchMakingListListHandler handler)
        {
            ListImpl(criteria, matchId, data, handler);
        }

        protected void ListImpl(String criteria, String matchId, Dictionary<object, object> data, MatchMakingListListHandler handler)
        {
            String url = new UrlGenerator("matches/matchmaking/").Append(criteria).ToString();
            Dictionary<string, object> payload = new Dictionary<string, object>();

            payload["data"] = data;
            if (matchId != null)
            {
                payload["match"] = matchId;
            }

            client.DoRequest(url, "put", payload, delegate(Request request)
            {
                handler(client.Match.ResolveMatches(request), request);
            });
        }

        public void Submit(String criteria, Dictionary<object, object> data, MatchMakingSubmitHandler handler)
        {
            SubmitImpl(criteria, null, data, handler);
        }

        public void SubmitAsMatch(String criteria, String matchId, Dictionary<object, object> data, MatchMakingSubmitHandler handler)
        {
            SubmitImpl(criteria, matchId, data, handler);
        }

        protected void SubmitImpl(String criteria, String matchId, Dictionary<object, object> data, MatchMakingSubmitHandler handler)
        {
            String url = new UrlGenerator("matches/matchmaking/").Append(criteria).Append("/request").ToString();
            Dictionary<string, object> payload = new Dictionary<string, object>();

            payload["data"] = data;
            if (matchId != null)
            {
                payload["match"] = matchId;
            }

            client.DoRequest(url, "post", payload, delegate(Request request)
            {
                MatchMakingRequest matchmakingRequest = null;

                if (!request.HasError())
                {
                    matchmakingRequest = map.GetObject(request);
                }
                handler(matchmakingRequest, request);
            });

        }

        protected List<MatchMakingCriteria> resolveMatchMakingCriteria(Request request)
        {
            List<MatchMakingCriteria> ret = new List<MatchMakingCriteria>();

            if (!request.HasError())
            {
                List<object> data = (List<object>)request.Data;

                foreach (Dictionary<object, object> iter in data)
                {
                    MatchMakingCriteria bucket = new MatchMakingCriteria(iter);

                    ret.Add(bucket);
                }
            }
            return ret;
        }

        public void Dispatch(string command, Dictionary<object, object> message)
        {
            Dictionary<object, object> payload = (Dictionary<object, object>)message["payload"];
            MatchMakingRequest matchMakingRequest = map.GetObject(payload);

            if (matchMakingRequest != null)
            {
                matchMakingRequest.Dispatch(command, message);
            }

            if (command == "matchmaking-complete")
            {
                if (MatchmakingComplete != null)
                {
                    MatchmakingComplete(matchMakingRequest);
                }
            }
            else if (command == "matchmaking-timeout")
            {
                if (MatchmakingTimeout != null)
                {
                    MatchmakingTimeout(matchMakingRequest);
                }
            }
            else if (command == "matchmaking-tick")
            {
                if (MatchmakingTick != null)
                {
                    MatchmakingTick(matchMakingRequest);
                }
            }
        }
    }
}
