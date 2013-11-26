using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.Models;
using AgoraGames.Hydra.Util;

namespace AgoraGames.Hydra.Services
{
    public class LeaderboardsService
    {
        protected ObjectMap<Leaderboard> Map { get; set; }
        protected Client client = null;

        public delegate void LeaderboardListHandler(List<Leaderboard> list, Request request);
        public delegate void LeaderboardEntriesListHandler(List<LeaderboardEntry> entries, Request request);

        public LeaderboardsService(Client client)
        {
            this.client = client;
            Map = new ObjectMap<Leaderboard>(client, (c, i) => { return new Leaderboard(c, i); });
        }

        public void LoadLeaderboards(LeaderboardListHandler requestHandler)
        {
            client.DoRequest("leaderboards", "get", null, delegate(Request response)
            {
                List<Leaderboard> leaderboards = ResolveLeaderboards(response);

                requestHandler(leaderboards, response);
            });
        }

        public void Show(String leaderboardId, LeaderboardEntriesListHandler handler)
        {
            Show(leaderboardId, new LeaderboardOptions(), handler);
        }

        public void Show(String leaderboardId, LeaderboardOptions options, LeaderboardEntriesListHandler handler)
        {
            ShowLeaderboard(leaderboardId, "/show", options, handler);
        }

        public void Friends(String leaderboardId, LeaderboardEntriesListHandler handler)
        {
            Friends(leaderboardId, new LeaderboardOptions(), handler);
        }

        public void Friends(String leaderboardId, LeaderboardOptions options, LeaderboardEntriesListHandler handler)
        {
            ShowLeaderboard(leaderboardId, "/friends", options, handler);
        }

        public void Around(String leaderboardId, String playerId, LeaderboardEntriesListHandler handler)
        {
            Around(leaderboardId, playerId, new LeaderboardOptions(), handler);
        }

        public void Around(String leaderboardId, String playerId, LeaderboardOptions options, LeaderboardEntriesListHandler handler)
        {
            ShowLeaderboard(leaderboardId, "/around/" + playerId, options, handler);
        }

        public void AroundMe(String leaderboardId, LeaderboardEntriesListHandler handler)
        {
            AroundMe(leaderboardId, new LeaderboardOptions(), handler);
        }

        public void AroundMe(String leaderboardId, LeaderboardOptions options, LeaderboardEntriesListHandler handler)
        {
            ShowLeaderboard(leaderboardId, "/around/me", options, handler);
        }

        public void ShowLeaderboard(String leaderboardId, String endpoint, LeaderboardOptions options, LeaderboardsService.LeaderboardEntriesListHandler handler)
        {
            ShowLeaderboard(leaderboardId, endpoint, options.Page, options.Sort, options.History, options.Fields, handler);
        }

        public void ShowLeaderboard(String leaderboardId, String endpoint, int page, Sort sort, int history, List<string> fields, LeaderboardsService.LeaderboardEntriesListHandler handler)
        {
            UrlGenerator url = new UrlGenerator("leaderboards/");

            url.Append(leaderboardId).Append(endpoint);

            if (page != -1)
            {
                url.Append("page", page);
            }

            if (sort != Sort.Default)
            {
                url.Append("order", Leaderboard.GetSortString(sort));
            }

            if (history != -1)
            {
                url.Append("history", history);
            }

            if (fields != null)
            {
                url.Append("fields", fields);
            }

            client.DoRequest(url.ToString(), "get", null, delegate(Request request)
            {
                List<LeaderboardEntry> entries = ResolveLeaderboardEntries(request);
                handler(entries, request);
            });
        }

        protected List<LeaderboardEntry> ResolveLeaderboardEntries(Request request)
        {
            List<LeaderboardEntry> ret = new List<LeaderboardEntry>();

            if (!request.HasError())
            {
                // TODO: create a model around the leaderboard response, there is total_pages info in there
                Dictionary<object, object> data = (Dictionary<object, object>)request.Data;
                List<object> leaders = new MapHelper(data).GetValue("leaders", (List<object>)null);

                if (leaders != null)
                {
                    foreach (Dictionary<object, object> iter in leaders)
                    {
                        LeaderboardEntry leaderboardEntry = new LeaderboardEntry(client);
                        leaderboardEntry.Merge(iter);
                        ret.Add(leaderboardEntry);
                    }
                }
            }
            return ret;
        }

        protected List<Leaderboard> ResolveLeaderboards(Request request)
        {
            List<Leaderboard> ret = new List<Leaderboard>();

            if (!request.HasError())
            {
                List<object> data = (List<object>)request.Data;

                foreach (Dictionary<object, object> iter in data)
                {
                    ret.Add(Map.GetObject(iter));
                }
            }
            return ret;
        }
    }
}
