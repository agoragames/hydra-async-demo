using System.Collections.Generic;

namespace AgoraGames.Hydra
{
    public class UUIDAuth : Auth
    {
        public override AuthType AuthType { get { return AuthType.UUID; } }
        public string UUID { get; protected set; }

        public UUIDAuth(string uuid, Dictionary<string, string> additionalData)
            : base(additionalData)
        {
            UUID = uuid;
        }

        public UUIDAuth(string uuid)
            : base(null)
        {
            UUID = uuid;
        }

        protected override object GetAuthData()
        {
            return UUID;
        }
    }
}
