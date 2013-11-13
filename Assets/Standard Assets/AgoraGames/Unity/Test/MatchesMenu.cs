using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using AgoraGames.Hydra.Models;
using AgoraGames.Hydra.Services;

namespace AgoraGames.Hydra.Test
{
    public class MatchesMenu : BaseMenu
    {
        int selectedTemplate;
        int selectedAccess;
        int selectedMatchView;
        int selectedFriend;
        int selectedMatch;

        string[] matchTemplates = new string[] { };
        string[] accessType = new string[] { "public", "private" };
        string[] matchesType = new string[] { "my matches", "my match history", "public matches" };

        List<Match> publicMatches;
        List<Match> myMatches;
        List<Match> myHistMatches;
        List<Match> friendMatches;
        List<Match> updatedMatches = new List<Match>();

        List<Friend> friends;
        string[] friendNames;

        public MatchesMenu(Main main) : base(main)
        {
        }

        public override void Deactivate()
        {
            Client.Instance.Match.Updated -= Match_Updated;
            Client.Instance.Match.ExpirationWarning -= Match_ExpirationWarning;
        }

        public override void Render()
        {
            int y = 0;
            TestUtil.RenderHeader(this, "Match");

            selectedTemplate = GUI.SelectionGrid(new Rect(0, y += 60, 260, 25), selectedTemplate, matchTemplates, matchTemplates.Length);

            if (GUI.Button(new Rect(270, y, 90, 55), "Create"))
            {
                Match.Access access = selectedAccess == 0 ? Match.Access.Public : Match.Access.Private;
                
                //MatchHandler handler
                Client.Instance.Match.CreateNew(matchTemplates[selectedTemplate], access, delegate(Match match, Request req)
                {
                    Main.SetState(Main.State.MATCH, match);
                });
            }

            selectedAccess = GUI.SelectionGrid(new Rect(0, y += 30, 260, 25), selectedAccess, accessType, accessType.Length);

            selectedMatchView = GUI.SelectionGrid(new Rect(0, y += 60, 360, 40), selectedMatchView, matchesType, matchesType.Length);

            int oldSelectedFriend = selectedFriend;
            if(friendNames != null)
                selectedFriend = GUI.SelectionGrid(new Rect(370, y, 700, 40), selectedFriend, friendNames, friendNames.Length);

            y += 50;
            if (selectedMatchView == 0)
            {
                DoSelection(RenderMatchList(0, y, myMatches), myMatches);
            }
            else if (selectedMatchView == 1)
            {
                DoSelection(RenderMatchList(0, y, myHistMatches), myHistMatches);
            }
            else
            {
                DoSelection(RenderMatchList(0, y, publicMatches), publicMatches);
            }

            if (selectedFriend != -1)
            {
                if (oldSelectedFriend != selectedFriend)
                {
                    Client.Instance.Match.LoadCurrent(friends[selectedFriend], delegate(List<Match> matches, Request req)
                    {
                        this.friendMatches = matches;
                    });
                }

                DoSelection(RenderMatchList(370, y, friendMatches), friendMatches);
            }
        }

        protected void DoSelection(int pos, List<Match> matches)
        {
            if (pos != -1)
            {
                Main.SetState(Main.State.MATCH, matches[pos]);
            }
        }

        protected int RenderMatchList(int x, int y, List<Match> matches)
        {
            if (matches != null)
            {
                string[] names = LoadNames(matches);
                return GUI.SelectionGrid(new Rect(x, y, 360, names.Length * 32), selectedMatch, names, 1);
            }
            return -1;
        }

        protected void LoadMatchTemplates()
        {
            Client.Instance.Match.LoadMatchTemplates(delegate(List<MatchTemplate> templates, Request req)
            {
                this.matchTemplates = LoadTemplateNames(templates);
            });
        }

        protected void LoadMatches()
        {
            Client.Instance.Match.LoadPublic(1, null, delegate(List<Match> matches, Request req)
            {
                this.publicMatches = matches;
                AddNotification("Loaded " + matches.Count + " public matches");
            });

            Client.Instance.Match.LoadCurrent(delegate(List<Match> matches, Request req)
            {
                this.myMatches = matches;
                AddNotification("Loaded " + matches.Count + " current matches");
            });

            Client.Instance.Match.LoadAll(1, delegate(List<Match> matches, Request req)
            {
                this.myHistMatches = matches;
                AddNotification("Loaded " + matches.Count + " historical matches");
            });
        }

        protected void LoadFriends()
        {
            Client.Instance.MyAccount.LoadFriends(delegate(Request req)
            {
                this.friends = Client.Instance.MyAccount.GetFriends();
                friendNames = LoadNames(friends);
            });
        }

        public override void SetParam(object param)
        {
            selectedMatch = -1;
            selectedFriend = -1;

            updatedMatches.Clear();

            LoadMatchTemplates();
            LoadMatches();
            LoadFriends();

            Client.Instance.Match.Updated += Match_Updated;
            Client.Instance.Match.ExpirationWarning += Match_ExpirationWarning;
        }

        bool Match_Updated(Match match, MatchPlayer fromPlayer, string notification)
        {
            AddNotification(notification);
            updatedMatches.Add(match);
            return true;
        }

        bool Match_ExpirationWarning(Match match, string notification)
        {
            AddNotification(notification);
            return true;
        }

        protected string[] LoadNames(List<Match> matches)
        {
            string[] names = new string[matches.Count];

            for (int i = 0; i < names.Length; i++)
            {
                Match match = matches[i];
                string name = match.Name;
                if (match.IsComplete)
                    name = "(x) " + name;

                if (updatedMatches.Contains(match))
                    name += " - updated!";

                names[i] = name;
            }
            return names;
        }

        protected string[] LoadNames(List<Friend> friends)
        {
            string[] names = new string[friends.Count];

            for (int i = 0; i < names.Length; i++)
            {
                names[i] = friends[i].Identity.UserName;
            }
            return names;
        }

        protected string[] LoadTemplateNames(List<MatchTemplate> templates)
        {
            string[] names = new string[templates.Count];

            for (int i = 0; i < names.Length; i++)
            {
                names[i] = templates[i].Slug;
            }
            return names;
        }
    }
}
