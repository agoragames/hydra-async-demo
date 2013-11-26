using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.Util;
using AgoraGames.Hydra.Services;
using System.Collections;

namespace AgoraGames.Hydra.Models
{
    public enum Sort
    {
        Default,
        Ascending,
        Desending
    }

    public class LeaderboardOptions
    {
        public Sort Sort { get; set; }
        public int Page { get; set; }
        public int History { get; set; }
        public List<string> Fields { get; set; }

        public LeaderboardOptions()
        {
            Sort = Models.Sort.Default;
            Page = -1;
            History = -1;
            Fields = null;
        }

        public LeaderboardOptions(Sort sort, int page, int history, List<string> fields)
        {
            Sort = sort;
            Page = page;
            History = history;
            Fields = fields;
        }

        public LeaderboardOptions SetSort(Sort sort)
        {
            Sort = sort;
            return this;
        }

        public LeaderboardOptions SetPage(int page)
        {
            Page = page;
            return this;
        }

        public LeaderboardOptions SetHistory(int history)
        {
            History = history;
            return this;
        }

        public LeaderboardOptions SetFields(List<string> fields)
        {
            Fields = fields;
            return this;
        }
    }

    public class Leaderboard : Model
    {
        protected string slug;
        public string Slug { get { return slug; } }

        protected string name;
        public string Name { get { return name; } }

        public override string Endpoint
        {
            get { return "leaderboards"; }
        }

        public Leaderboard(Client client, string id) : base(client, id)
        {

        }

        public override void Merge(IDictionary map)
        {
            base.Merge(map);

            MapHelper mapHelper = new MapHelper(map);
            slug = mapHelper.GetValue("slug", (string)null);
            name = mapHelper.GetValue("name", (string)null);
        }

        public void Show(LeaderboardsService.LeaderboardEntriesListHandler handler)
        {
            Client.Leaderboards.Show(id, handler);
        }

        public void Show(LeaderboardOptions options, LeaderboardsService.LeaderboardEntriesListHandler handler)
        {
            Client.Leaderboards.Show(id, options, handler);
        }

        public void Friends(LeaderboardsService.LeaderboardEntriesListHandler handler)
        {
            Client.Leaderboards.Friends(id, handler);
        }

        public void Friends(LeaderboardOptions options, LeaderboardsService.LeaderboardEntriesListHandler handler)
        {
            Client.Leaderboards.Friends(id, options, handler);
        }

        public void Around(String playerID, LeaderboardsService.LeaderboardEntriesListHandler handler)
        {
            Client.Leaderboards.Around(id, playerID, handler);
        }

        public void Around(String playerID, LeaderboardOptions options, LeaderboardsService.LeaderboardEntriesListHandler handler)
        {
            Client.Leaderboards.Around(id, playerID, options, handler);
        }

        public void AroundMe(LeaderboardsService.LeaderboardEntriesListHandler handler)
        {
            Client.Leaderboards.AroundMe(id, handler);
        }

        public void AroundMe(LeaderboardOptions options, LeaderboardsService.LeaderboardEntriesListHandler handler)
        {
            Client.Leaderboards.AroundMe(id, options, handler);
        }

        public static string GetSortString(Sort type)
        {
            switch (type)
            {
                case Sort.Ascending:
                    return "asc";
                case Sort.Desending:
                    return "desc";
            }
            return null;
        }
    }
}
