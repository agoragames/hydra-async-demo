using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.Models;
using UnityEngine;

namespace AgoraGames.Hydra.Test
{
    public class AchievementsMenu : BaseMenu
    {
        protected List<AwardedAchievement> list;
        protected string[] achievements = new string[0];

        public AchievementsMenu(Main main)
            : base(main)
        {
        }

        public override void Deactivate()
        {
        }

        public override void Render()
        {
            int y = 0;
            TestUtil.RenderHeader(this, "Achievement");

            GUI.SelectionGrid(new Rect(0, y += 60, 360, achievements.Length * 32), -1, achievements, 1);
        }

        protected void LoadAchievements()
        {
            //public delegate void AwardedAchievementListHandler(List<AwardedAchievement> list, HydraRequest request);

            Client.Instance.Achievements.AllForPlayer("me", delegate(List<AwardedAchievement> list, Request request)
            {
                this.list = list;

                achievements = new string[list.Count];
                for(int i = 0; i < list.Count; i++)
                {
                    achievements[i] = String.Format("{0} {1} {2} {3}", list[i].Achievement.Name, list[i].Awarded, list[i].Progress, list[i].UpdatedAt);
                }
            });
        }

        public override void SetParam(object param)
        {
            LoadAchievements();
        }
    }
}
