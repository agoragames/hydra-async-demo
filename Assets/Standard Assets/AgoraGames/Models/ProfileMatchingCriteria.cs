using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.Util;

namespace AgoraGames.Hydra.Models
{
    public class ProfileMatchingCriteria
    {
        protected string id;
        public string Id { get { return id; } }

        protected string slug;
        public string Slug { get { return slug; } }

        protected string name;
        public string Name { get { return name; } }

        public ProfileMatchingCriteria(Dictionary<object, object> data)
        {
            MapHelper mapHelper = new MapHelper(data);
            id = mapHelper.GetValue("id", (string)null);
            slug = mapHelper.GetValue("slug", (string)null);
            name = mapHelper.GetValue("name", (string)null);
        }

    }
}
