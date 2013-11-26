using System;
using System.Collections.Generic;
using AgoraGames.Hydra.Util;

namespace AgoraGames.Hydra
{
    public class RecoveryAuth : Auth
    {
        public override AuthType AuthType { get { return AuthType.RECOVERY; } }
        public string Email { get; protected set; }
        public string Code { get; protected set; }

        public RecoveryAuth(string email, string code, Dictionary<string, string> additionalData)
            : base(additionalData)
        {
            Email = email;
            Code = code;
        }

        public RecoveryAuth(string email, string code)
            : base(null)
        {
            Email = email;
            Code = code;
        }

        protected override object GetAuthData()
        {
            Dictionary<string, string> data = new Dictionary<string, string>();
            data["email"] = Email;
            data["code"] = Code;
            return data;
        }
    }
}
