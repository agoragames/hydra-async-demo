using System.Collections.Generic;

namespace AgoraGames.Hydra
{
    public class AnonymousAuth : Auth
    {
        public override AuthType AuthType { get { return AuthType.ANONYMOUS; } }

        public AnonymousAuth(Dictionary<string, string> additionalData)
            : base(additionalData)
        {
        }

        public AnonymousAuth()
            : base(null)
        {
        }

        protected override object GetAuthData()
        {
            return true;
        }
    }
}
