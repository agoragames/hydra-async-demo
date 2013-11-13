using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.Util;

namespace AgoraGames.Hydra.Models
{
    public class AwardedAchievement
    {
        protected string id;
        public string Id { get { return id; } }

        protected string achievementId;
        public string AchievementId { get { return achievementId; } }

        protected Achievement achievement;
        public Achievement Achievement { get { return achievement; } }

        protected string accountId;
        public string AccountId { get { return accountId; } }

        protected double progress;
        public double Progress { get { return progress; } }

        protected bool awarded;
        public bool Awarded { get { return awarded; } }

        protected DateTime updatedAt;
        public DateTime UpdatedAt { get { return updatedAt; } }

        protected DateTime createdAt;
        public DateTime CreatedAt { get { return createdAt; } }

        public AwardedAchievement(Achievement achievement, Dictionary<object, object> data)
        {
            this.achievement = achievement;

            MapHelper mapHelper = new MapHelper(data);
            id = mapHelper.GetValue("id", (string)null);
            accountId = mapHelper.GetValue("account_id", (string)null);
            achievementId = mapHelper.GetValue("achievement_id", (string)null);
            progress = mapHelper.GetValue("progress", 0.0);
            awarded = mapHelper.GetValue("awarded", false);

            createdAt = mapHelper.GetValue("created_at", DateTime.Now);
            updatedAt = mapHelper.GetValue("updated_at", DateTime.Now);
        }
    }
}
