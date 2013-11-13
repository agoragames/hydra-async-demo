using AgoraGames.Hydra.Models;
using AgoraGames.Hydra.Util;
using System.Collections;
using System.Collections.Generic;

namespace AgoraGames.Hydra.Services
{
	public class AchievementsService
	{
        protected ObjectMap<Achievement> Map { get; set; }
        protected Client client = null;

        public delegate void AwardedAchievementListener(AwardedAchievement obj);
        public event AwardedAchievementListener AchievementAwarded;
        public event AwardedAchievementListener AchievementUpdated;

        public delegate void AchievementHandler(Achievement obj, Request request);
        public delegate void AchievementListHandler(List<Achievement> list, Request request);
        public delegate void AwardedAchievementListHandler(List<AwardedAchievement> list, Request request);

        protected List<Achievement> achievements;

		public AchievementsService(Client client)
		{
            this.client = client;
            Map = new ObjectMap<Achievement>(client, (c, i) => { return new Achievement(c, i); });
		}

        public void Get(string achievementID, AchievementHandler handler)
        {
            client.DoRequest("achievements/" + achievementID, "get", null, delegate(Request req)
            {
                if(!req.HasError()) 
                {
                    handler(Map.GetObject(req.Data as IDictionary), req);
                }
                else
                {
                    handler(null, req);
                }
            });
        }

        public void All(AchievementListHandler handler)
        {
            client.DoRequest("achievements", "get", null, delegate(Request req)
            {
                if (!req.HasError())
                {
                    handler(resolveAchievements(req), req);
                }
                else
                {
                    handler(null, req);
                }
            });
        }

        public void AllForPlayer(string playerID, AwardedAchievementListHandler handler)
        {
            client.DoRequest("achievements/all/" + playerID, "get", null, delegate(Request req)
            {
                if (!req.HasError())
                {
                    handler(resolveAwardedAchievements(req), req);
                }
                else
                {
                    handler(null, req);
                }
            });
        }

        public void Dispatch(string command, Dictionary<object, object> message)
        {
            // this is a simple handler to dispatch out to the user listener......
            Dictionary<object, object> payload = (Dictionary<object, object>)message["payload"];
            Achievement achievement = resolveAchievement(payload);

            if (command == "achievement-awarded")
            {
                if (AchievementAwarded != null)
                {
                    AchievementAwarded(new AwardedAchievement(achievement, payload));
                }
            }
            else if (command == "achievement-updated")
            {
                if (AchievementUpdated != null)
                {
                    AchievementUpdated(new AwardedAchievement(achievement, payload));
                }
            }
        }

        protected List<Achievement> resolveAchievements(Request request)
        {
            List<Achievement> ret = new List<Achievement>();

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

        protected Achievement resolveAchievement(Dictionary<object, object> data)
        {
            Achievement achievement = null;
            string achievementId = new MapHelper(data).GetValue("achievement_id", (string)null);

            if (achievementId != null)
            {
                client.AllAchievements.TryGetValue(achievementId, out achievement);

                return achievement;
            }
            return null;
        }

        protected List<AwardedAchievement> resolveAwardedAchievements(Request request)
        {
            List<AwardedAchievement> ret = new List<AwardedAchievement>();

            if (!request.HasError())
            {
                List<object> data = (List<object>)request.Data;

                foreach (object iter in data)
                {
                    Dictionary<object, object> achievementData = (Dictionary<object, object>)iter;
                    Achievement achievement = resolveAchievement(achievementData);

                    if (achievement != null)
                    {
                        ret.Add(new AwardedAchievement(achievement, achievementData));
                    }
                    else
                    {
                        // TODO: handle error
                    }
                }
            }
            return ret;
        }
	}
}
