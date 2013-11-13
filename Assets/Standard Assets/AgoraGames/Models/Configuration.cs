using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.Util;

namespace AgoraGames.Hydra.Models
{
    public class Configuration
    {
        protected string androidNumber;
        public string AndroidProjectNumber { get { return androidNumber; } }

        protected bool realtimeEnabled;
        public bool RealtimeEnabled { get { return realtimeEnabled; } }

        protected string realtimeEndpoint;
        public string RealtimeEndpoint { get { return realtimeEndpoint; } }

        public Configuration(Dictionary<object, object> data)
        {
            MapHelper mapHelper = new MapHelper(data);
            Dictionary<object, object> gcmData = mapHelper.GetValue("gcm", (Dictionary<object, object>)null);

            if (gcmData != null)
            {
                MapHelper gcmMapHelper = new MapHelper(gcmData);
                androidNumber = gcmMapHelper.GetValue("project_number", (string)null);
            }

            // realtime info
            Dictionary<object, object> realtimeData = mapHelper.GetValue("realtime", (Dictionary<object, object>)null);
            MapHelper realtimeMapHelper = new MapHelper(realtimeData);
            realtimeEnabled = realtimeMapHelper.GetValue("enabled", false);
            realtimeEndpoint = realtimeMapHelper.GetValue("endpoint", (string)null);
        }
    }
}
