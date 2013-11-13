using System;

using AgoraGames.Hydra;
using AgoraGames.Hydra.Models;
using AgoraGames.Hydra.Util;
using System.Collections.Generic;

namespace AgoraGames.Hydra.Services
{
	public class ProfilesService
	{
        protected ObjectMap<Profile> map = null;
        protected Client client = null;

        public delegate void ProfileHandler(Profile obj, Request request);
        public delegate void ProfileListHandler(List<Profile> obj, Request request);
        public delegate void ProfileMatchingCriteriaListHandler(List<ProfileMatchingCriteria> list, Request request);

        public ProfilesService(Client client)
		{
            this.client = client;
            this.map = new ObjectMap<Profile>(client, (c, i) => { return new Profile(c, i); });
		}

        public void Load(ProfileHandler handler)
        {
            Load("me", handler);
        }

        public void Load(string id, ProfileHandler handler)
        {
            Load(id, null, handler);
        }

        public void Load(string id, List<string> fields, ProfileHandler handler)
        {
            string url = new UrlGenerator("profiles/" + id).Append("fields", fields).ToString();

            client.DoRequest(url, "get", null, delegate(Request request)
            {
                handler(map.GetObject(request), request);
            });
        }

        public void FindMatch(string criteria, ProfileListHandler handler)
        {
            FindMatch(criteria, new Dictionary<object, object>(), handler);
        }

        public void FindMatch(string criteria, Dictionary<object, object> data, ProfileListHandler handler)
        {
            String url = new UrlGenerator("profiles/matching/").Append(criteria).Append("/find").ToString();

            client.DoRequest(url, "put", data, delegate(Request request)
            {
                handler(ResolveProfiles(request), request);
            });
        }

        public void LoadMatchingCriteria(ProfileMatchingCriteriaListHandler handler)
        {
            client.DoRequest("profiles/matching/criteria", "get", null, delegate(Request request)
            {
                handler(ResolveProfileMatchingCriteria(request), request);
            });
        }

        public Profile ResolveProfile(Dictionary<object, object> data)
        {
            return map.GetObject(data);
        }

        public List<Profile> ResolveProfiles(Request request)
        {
            List<Profile> ret = new List<Profile>();

            if (!request.HasError())
            {
                List<object> data = (List<object>)request.Data;

                foreach (Dictionary<object, object> iter in data)
                {
                    Dictionary<object, object> accountData = (Dictionary<object, object>)iter["account"];
                    Dictionary<object, object> profileData = (Dictionary<object, object>)iter["profile"];

                    // Make sure to save the account object so the profile can populate its 'Account' property
                    client.Account.Resolve(accountData);
                    Profile profile = map.GetObject(profileData);

                    ret.Add(profile);
                }
            }
            return ret;
        }

        protected List<ProfileMatchingCriteria> ResolveProfileMatchingCriteria(Request request)
        {
            List<ProfileMatchingCriteria> ret = new List<ProfileMatchingCriteria>();

            if (!request.HasError())
            {
                List<object> data = (List<object>)request.Data;

                foreach (Dictionary<object, object> iter in data)
                {
                    ProfileMatchingCriteria criteria = new ProfileMatchingCriteria(iter);

                    ret.Add(criteria);
                }
            }
            return ret;
        }

	}
}

