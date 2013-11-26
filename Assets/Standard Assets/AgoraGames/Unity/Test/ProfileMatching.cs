using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.Models;
using UnityEngine;

namespace AgoraGames.Hydra.Test
{
    public class ProfileMatchingMenu : BaseMenu
    {
        List<ProfileMatchingCriteria> criteria;
        string[] criteriaNames = new string[0];

        List<Profile> profiles;
        string[] profileNames = new string[0];

        int selectedCriteria = -1;

        public ProfileMatchingMenu(Main main)
            : base(main)
        {
        }

        public override void Render()
        {
            TestUtil.RenderHeader(this, "Profile Matching");
            int y = 0;
            int x = 0;

            selectedCriteria = GUI.SelectionGrid(new Rect(x, y += 60, 300, criteriaNames.Length * 32), selectedCriteria, criteriaNames, 1);
            if (selectedCriteria != -1)
            {
                Client.Instance.Profile.FindMatch(criteria[selectedCriteria].Slug, delegate(List<Profile> profiles, Request request)
                {
                    this.profiles = profiles;
                    GenerateProfileNames();
                });
                selectedCriteria = -1;
            }

            GUI.SelectionGrid(new Rect(x += 300, y, 300, profileNames.Length * 32), -1, profileNames, 1);
        }

        protected void LoadBuckets()
        {
            Client.Instance.Profile.LoadMatchingCriteria(delegate(List<ProfileMatchingCriteria> list, Request request)
            {
                this.criteria = list;
                GenerateNames();
            });
        }

        protected void GenerateProfileNames()
        {
            profileNames = new string[profiles.Count];

            for (int i = 0; i < profiles.Count; i++)
            {
                profileNames[i] = profiles[i].Account.Identity.UserName;
            }
        }

        protected void GenerateNames()
        {
            criteriaNames = new string[criteria.Count];

            for (int i = 0; i < criteria.Count; i++)
            {
                criteriaNames[i] = criteria[i].Name;
            }
        }

        public override void SetParam(object param)
        {
            profileNames = new string[0];
            selectedCriteria = -1;
            LoadBuckets();
        }

        public override void Deactivate()
        {
        }
    }
}
