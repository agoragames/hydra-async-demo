using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AgoraGames.Hydra.Test
{
    public class MainMenu : BaseMenu
    {
        public static int BUTTON_WIDTH = 300;
        public static int BUTTON_HEIGHT = 50;

        protected List<Entry> mainEntries = new List<Entry>();

        public MainMenu(Main main) : base(main)
        {
            mainEntries.Add(new Entry("Account", Main.State.ACCOUNT));
            mainEntries.Add(new Entry("Profile", Main.State.PROFILE));
            mainEntries.Add(new Entry("Match", Main.State.MATCHES));
            mainEntries.Add(new Entry("Match Making", Main.State.MATCHMAKING));
            mainEntries.Add(new Entry("Profile Matching", Main.State.PROFILE_MATCHING));
            mainEntries.Add(new Entry("Leaderboards", Main.State.LEADERBOARDS));
            mainEntries.Add(new Entry("Achievements", Main.State.ACHIEVEMENTS));
            mainEntries.Add(new Entry("Broadcast Channels", Main.State.BROADCAST_CHANNELS));
            mainEntries.Add(new Entry("Lobby Chat", Main.State.LOBBY_CHAT));
        }

        public override void SetParam(object param)
        {
        }

        public override void Deactivate()
        {
        }

        public override void Render()
        {
            int y = 0;
            foreach(Entry e in mainEntries) 
            {
                if (GUI.Button(new Rect(0, y, BUTTON_WIDTH, BUTTON_HEIGHT), e.title))
                {
                    Main.SetState(e.state, null);
                }
                y += BUTTON_HEIGHT + 10;
            }
        }

        public class Entry {
            public string title;
            public Main.State state;

            public Entry(string title, Main.State state)
            {
                this.title = title;
                this.state = state;
            }
        }
    }
}
