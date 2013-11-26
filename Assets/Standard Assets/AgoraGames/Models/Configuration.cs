using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.Util;

namespace AgoraGames.Hydra.Models
{
    public class Configuration
    {
        public bool GCMEnabled { get; protected set; }
        public string AndroidProjectNumber { get; protected set; }

        public bool APNSEnabled { get; protected set; }

        public bool RealtimeEnabled { get; protected set; }
        public string RealtimeEndpoint { get; protected set; }

        public Configuration(Dictionary<object, object> data)
        {
            MapHelper mapHelper = new MapHelper(data);

            Dictionary<object, object> gcmData = mapHelper.GetValue("gcm", (Dictionary<object, object>)null);
            if (gcmData != null)
            {
                MapHelper gcmMapHelper = new MapHelper(gcmData);
                GCMEnabled = gcmMapHelper.GetValue("enabled", false);
                AndroidProjectNumber = gcmMapHelper.GetValue("project_number", (string)null);
            }

            Dictionary<object, object> apnsData = mapHelper.GetValue("apns", (Dictionary<object, object>)null);
            if (apnsData != null)
            {
                MapHelper apnsMapHelper = new MapHelper(apnsData);
                APNSEnabled = apnsMapHelper.GetValue("enabled", false);
            }

            // realtime info
            Dictionary<object, object> realtimeData = mapHelper.GetValue("realtime", (Dictionary<object, object>)null);
            MapHelper realtimeMapHelper = new MapHelper(realtimeData);
            RealtimeEnabled = realtimeMapHelper.GetValue("enabled", false);
            RealtimeEndpoint = realtimeMapHelper.GetValue("endpoint", (string)null);
        }
    }
}
