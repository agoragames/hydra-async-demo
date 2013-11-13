using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.Models;
using UnityEngine;

namespace AgoraGames.Hydra.Test
{
    public class LeaderboardMenu: BaseMenu
    {
        Leaderboard leaderboard;

        List<LeaderboardEntry> entries;
        string[] entryNames;

        string[] views = new string[] { "all", "around me", "friends" };
        int selectedLeaderboard = 0;
        int lastLeaderboard = -1;

        int selectedLeaderboardEntry = -1;

        public LeaderboardMenu(Main main)
            : base(main)
        {
        }

        public override void Deactivate()
        {
        }

        public override void Render()
        {
            TestUtil.RenderHeader(this, "Leaderboard");
            int y = 0;

            if(leaderboard != null)
            {
                string metadata = "Metadata";
                metadata += "\n" + MiniJSON.Json.Serialize(leaderboard["data"], true);
                metadata += "\n";
                metadata += "\nRaw data";
                metadata += "\n" + MiniJSON.Json.Serialize(leaderboard.Data, true);
                metadata += "\n";
                metadata += "\n Name: " + leaderboard["data.name"];
                GUI.TextArea(new Rect(370, y, 360, 360), metadata);
            }

            selectedLeaderboard = GUI.SelectionGrid(new Rect(0, y += 60, 360, 40), selectedLeaderboard, views, views.Length);

            if (lastLeaderboard != selectedLeaderboard)
            {
                if (selectedLeaderboard == 0)
                {
                    List<string> fields = new List<string>();
                    fields.Add("data.kills");
                    leaderboard.Show(new LeaderboardOptions().SetPage(0).SetFields(fields), leaderboardHandler);
                }
                else if (selectedLeaderboard == 1)
                {
                    leaderboard.AroundMe(leaderboardHandler);
                }
                else if (selectedLeaderboard == 2)
                {
                    leaderboard.Friends(leaderboardHandler);
                }
                lastLeaderboard = selectedLeaderboard;
            }

            if (entryNames != null)
            {
                selectedLeaderboardEntry = GUI.SelectionGrid(new Rect(0, y += 50, 360, entryNames.Length * 32), selectedLeaderboardEntry, entryNames, 1);
                if (selectedLeaderboardEntry >= 0)
                {
                    LeaderboardEntry entry = entries[selectedLeaderboardEntry];
                    string metadata = entry.Profile.Account.Identity.UserName;
                    metadata += "\nData";
                    metadata += "\n" + MiniJSON.Json.Serialize(entry.Profile["data"], true);
                    metadata += "\n";
                    metadata += "\nRaw data";
                    metadata += "\n" + MiniJSON.Json.Serialize(entry.Profile.Data, true);
                    metadata += "\n";
                    metadata += "\n Kills: " + entry.Profile["data.kills"];
                    GUI.TextArea(new Rect(740, 0, 360, 520), metadata);
                }
            }
        }

        public void leaderboardHandler(List<LeaderboardEntry> entries, Request request)
        {
            this.entries = entries;
            entryNames = new string[entries.Count];
            for (int i = 0; i < entries.Count; i++)
            {
                LeaderboardEntry entry = entries[i];

                entryNames[i] = String.Format("({0}) {1} - {2}", entry.Rank, entry.Profile.Account.Identity.UserName, entry.Score);
            }
        }

        public override void SetParam(object param)
        {
            lastLeaderboard = -1;
            leaderboard = (Leaderboard)param;
        }
    }
}
