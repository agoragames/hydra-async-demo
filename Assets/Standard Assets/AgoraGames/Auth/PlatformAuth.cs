using System;
using System.Collections.Generic;

namespace AgoraGames.Hydra
{
    [Obsolete("Use any of the new authentication methods", false)]
    public class PlatformAuth : Auth
    {
        public override AuthType AuthType { get { return AuthType.PLATFORM; } }
        public string AccountId { get; protected set; }
        public string Type { get; protected set; }
        public string Token { get; protected set; }

        public PlatformAuth(string accountId, string type, string token, Dictionary<string, string> additionalData)
            : base(additionalData)
        {
            Init(accountId, type, token);
        }

        public PlatformAuth(string accountId, string type, string token)
            : base(null)
        {
            Init(accountId, type, token);
        }

        protected void Init(string accountId, string type, string token)
        {
            AccountId = accountId;
            Type = type;
            Token = token;

            if (AdditionalData == null)
            {
                AdditionalData = new Dictionary<string, string>();
            }

            AdditionalData["type"] = type;
            AdditionalData["auth"] = token;
        }

        protected override object GetAuthData()
        {
            return AccountId;
        }
    }
}
