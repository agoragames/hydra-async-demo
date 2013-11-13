using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.Models;
using UnityEngine;

namespace AgoraGames.Hydra.Test
{
    public class MatchMakingMenu : BaseMenu
    {
        List<MatchMakingCriteria> criteria;
        string[] bucketNames = new string[0];
        string[] join = new string[0];
        string[] list = new string[0];
        string[] submit = new string[0];

        bool showList = true;

        // match listing
        string[] matchNames = new string[0];
        List<Match> matches;
        int selectedMatch = -1;

        // submit
        MatchMakingRequest matchmakingRequest;

        int selectedJoin = -1;
        int selectedList = -1;
        int selectedSubmit = -1;

        public MatchMakingMenu(Main main)
            : base(main)
        {
        }

        public override void Render()
        {
            TestUtil.RenderHeader(this, "MatchMaking Criteria");
            int y = 0;
            int x = 0;

            GUI.SelectionGrid(new Rect(x, y+=60, 160, bucketNames.Length * 32), -1, bucketNames, 1);

            int width = 60;
            selectedJoin = GUI.SelectionGrid(new Rect(x += 170, y, width, bucketNames.Length * 32), selectedJoin, join, 1);
            selectedList = GUI.SelectionGrid(new Rect(x += width, y, width, bucketNames.Length * 32), selectedList, list, 1);
            selectedSubmit = GUI.SelectionGrid(new Rect(x += width + 10, y, width, bucketNames.Length * 32), selectedSubmit, submit, 1);

            if (selectedJoin != -1)
            {
                Client.Instance.MatchMaking.Join(criteria[selectedJoin].Slug, null, delegate(Match match, Request request)
                {
                    Main.SetState(Main.State.MATCH, match);
                });
                selectedJoin = -1;
            }

            if (selectedList != -1)
            {
                Client.Instance.MatchMaking.List(criteria[selectedList].Slug, null, delegate(List<Match> matches, Request request)
                {
                    showList = true;
                    this.matches = matches;
                    this.matchNames = LoadNames(matches);
                });
                selectedList = -1;
            }

            if (selectedSubmit != -1)
            {
                Client.Instance.MatchMaking.Submit(criteria[selectedSubmit].Slug, null, delegate(MatchMakingRequest matchmakingRequest, Request request)
                {
                    showList = false;
                    this.matchmakingRequest = matchmakingRequest;
                });
                selectedSubmit = -1;
            }

            if (showList)
            {
                selectedMatch = GUI.SelectionGrid(new Rect(x += width + 10, y, 300, matchNames.Length * 32), selectedMatch, matchNames, 1);
                if (selectedMatch != -1)
                {
                    Match match = matches[selectedMatch];

                    match.Join(delegate(Request request)
                    {
                        if (!request.HasError())
                        {
                            Main.SetState(Main.State.MATCH, match);
                        }
                    });
                    selectedMatch = -1;
                }
            }
            else
            {
                // TODO: get pending matchmaking requets
                int xpos = x + width + 10;
                GUI.TextField(new Rect(x = xpos, y, 100, 30), "Wait Time:");
                GUI.TextField(new Rect(x += 100, y, 100, 30), this.matchmakingRequest.Wait.ToString());

                GUI.TextField(new Rect(x = xpos, y += 30, 100, 30), "Found Match:");
                GUI.TextField(new Rect(x += 100, y, 100, 30), this.matchmakingRequest.Found.ToString());

                GUI.TextField(new Rect(x = xpos, y += 30, 100, 30), "Complete:");
                GUI.TextField(new Rect(x += 100, y, 100, 30), this.matchmakingRequest.IsCompleted.ToString());

                GUI.TextField(new Rect(x = xpos, y += 30, 100, 30), "Ticks:");
                GUI.TextField(new Rect(x += 100, y, 100, 30), this.matchmakingRequest.TickCount.ToString());

                if (matchmakingRequest.Found)
                {
                    if (GUI.Button(new Rect(x = xpos, y += 30, 200, 30), "Join")) 
                    {
                        matchmakingRequest.Match.Join(delegate(Request r)
                        {
                            Main.SetState(Main.State.MATCH, matchmakingRequest.Match);
                        });
                    }
                }
            }
        }

        protected void LoadBuckets()
        {
            Client.Instance.MatchMaking.LoadCriteria(delegate(List<MatchMakingCriteria> list, Request request)
            {
                this.criteria = list;
                GenerateNames();
            });
        }

        protected string[] LoadNames(List<Match> matches)
        {
            string[] names = new string[matches.Count];

            for (int i = 0; i < names.Length; i++)
            {
                names[i] = matches[i].Name;
            }
            return names;
        }

        protected void GenerateNames()
        {
            bucketNames = new string[criteria.Count];
            join = new string[criteria.Count];
            submit = new string[criteria.Count];
            list = new string[criteria.Count];

            for (int i = 0; i < criteria.Count; i++)
            {
                bucketNames[i] = criteria[i].Name;
                join[i] = "Join";
                submit[i] = "Submit";
                list[i] = "List";
            }
        }

        public override void SetParam(object param)
        {
            selectedJoin = -1;
            selectedList = -1;
            selectedSubmit = -1;
            selectedMatch = -1;
            matchNames = new string[0];
            matches = null;
            LoadBuckets();

            Client.Instance.MatchMaking.MatchmakingTimeout += MatchmakingTimeout;
            Client.Instance.MatchMaking.MatchmakingTick += MatchmakingTick;
            Client.Instance.MatchMaking.MatchmakingComplete += MatchmakingComplete;
        }

        public override void Deactivate()
        {
            Client.Instance.MatchMaking.MatchmakingTimeout -= MatchmakingTimeout;
            Client.Instance.MatchMaking.MatchmakingTick -= MatchmakingTick;
            Client.Instance.MatchMaking.MatchmakingComplete -= MatchmakingComplete;
        }

        void MatchmakingComplete(MatchMakingRequest matchMaking)
        {
            Debug.Log("complete");
        }

        void MatchmakingTick(MatchMakingRequest matchMaking)
        {
            Debug.Log("tick");
        }
        
        void MatchmakingTimeout(MatchMakingRequest matchMaking)
        {
            Debug.Log("timeout");
        }
    }
}
