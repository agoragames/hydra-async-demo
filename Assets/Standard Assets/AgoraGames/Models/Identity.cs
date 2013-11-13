using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.Util;

namespace AgoraGames.Hydra.Models
{
    public class Identity
    {
        public string UserName { get; protected set; }
        public string Avatar { get; set; }
        public string Email { get; set; }

        public Identity(Dictionary<object, object> data)
        {
            MapHelper mapHelper = new MapHelper(data);
            UserName = mapHelper.GetValue("username", (string)null);
            Avatar = mapHelper.GetValue("avatar", (string)null);
            Email = mapHelper.GetValue("email", (string)null);
        }

        public Identity(String username)
        {
            this.UserName = username;
        }

        public object ConvertToRequest()
        {
            Dictionary<object, object> ret = new Dictionary<object, object>();

            if (UserName != null)
            {
                ret["username"] = UserName;
            }

            if (Avatar != null)
            {
                ret["avatar"] = Avatar;
            }

            if (Email != null)
            {
                ret["email"] = Email;
            }

            return ret;
        }
    }
}
