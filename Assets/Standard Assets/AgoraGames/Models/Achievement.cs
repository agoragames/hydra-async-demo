using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.Util;

namespace AgoraGames.Hydra.Models
{
    public class Achievement : Model
    {
        protected string image;
        public string Image { get { return image; } }

        protected string name;
        public string Name { get { return name; } }

        protected string description;
        public string Description { get { return description; } }

        protected long points;
        public long Points { get { return points; } }

        public Achievement(Client client, string id)
            : base(client, id)
        {
        }

        public override void Merge(System.Collections.IDictionary map)
        {
            base.Merge(map);

            MapHelper mapHelper = new MapHelper(map);
            name = mapHelper.GetValue("name", (string)null);
            description = mapHelper.GetValue("description", (string)null);
            points = mapHelper.GetValue("points", (long)0);
            image = mapHelper.GetValue("image_url", (string)null);
        }

        public override string Endpoint
        {
            get { return "/achievements/"; }
        }
    }
}
