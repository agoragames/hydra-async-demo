using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.Models;
using UnityEngine;

namespace AgoraGames.Hydra.Test
{
    public class LeaderboardsMenu : BaseMenu
    {
        protected List<Leaderboard> leaderboardList;
        protected string[] leaderboardNames = new string[0];
        protected int selectedLeaderboard = -1;

        public LeaderboardsMenu(Main main)
            : base(main)
        {
        }

        public override void Deactivate()
        {
        }

        public override void Render()
        {
            int y = 0;

            TestUtil.RenderHeader(this, "Leaderboards");

            selectedLeaderboard = GUI.SelectionGrid(new Rect(0, y += 60, 360, leaderboardNames.Length * 32), selectedLeaderboard, leaderboardNames, 1);

            if (selectedLeaderboard != -1)
            {
                Main.SetState(Main.State.LEADERBOARD, leaderboardList[selectedLeaderboard]);
            }
        }

        protected void LoadLeaderboards()
        {
            Client.Instance.Leaderboards.LoadLeaderboards(delegate(List<Leaderboard> list, Request request)
            {
                leaderboardList = list;
                leaderboardNames = LoadNames(list);
            });
        }

        protected string[] LoadNames(List<Leaderboard> leaderboard)
        {
            string[] ret = new string[leaderboard.Count];
            for (int i = 0; i < leaderboard.Count; i++)
            {
                ret[i] = leaderboard[i].Name;
            }
            return ret;
        }

        public override void SetParam(object param)
        {
            selectedLeaderboard = -1;
            LoadLeaderboards();
        }
    }
}
