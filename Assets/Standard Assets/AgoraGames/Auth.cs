using System;
using System.Collections.Generic;
using AgoraGames.Hydra.Util;

namespace AgoraGames.Hydra
{
    public enum AuthType
    {
        UUID = 1,
        FACEBOOK = 2,
        PLATFORM = 3
    }

	public class Auth
	{
		public Auth ()
		{
		}

        public static Dictionary<string, string> GenerateAuthRequest(AuthType type, string token, Dictionary<string, string> data)
        {
            Dictionary<string, string> authHash = new Dictionary<string, string>();

            authHash.Add(Auth.GetAuthString(type), token);

            if (data != null)
            {
                new MapHelper(authHash).Merge(data);
            }

            return authHash;
        }

        public static string GetAuthString(AuthType type)
        {
            switch (type)
            {
                case AuthType.FACEBOOK:
                    return "facebook";
                case AuthType.UUID:
                    return "UUID";
                case AuthType.PLATFORM:
                    return "platform";
            }
            return null;
        }
    }
}

