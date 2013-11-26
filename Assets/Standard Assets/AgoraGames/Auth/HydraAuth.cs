using System;
using System.Collections.Generic;
using AgoraGames.Hydra.Util;

namespace AgoraGames.Hydra
{
    public class HydraAuth : Auth
    {
        public override AuthType AuthType { get { return AuthType.HYDRA; } }
        public string User { get; protected set; }
        public string Password { get; protected set; }

        public HydraAuth(string user, string password, Dictionary<string, string> additionalData)
            : base(additionalData)
        {
            User = user;
            Password = password;
        }

        public HydraAuth(string user, string password)
            : base(null)
        {
            User = user;
            Password = password;
        }

        protected override object GetAuthData()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data["username"] = User;
            data["password"] = Password;
            return data;
        }
    }
}
