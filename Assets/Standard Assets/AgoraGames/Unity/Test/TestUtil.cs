using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AgoraGames.Hydra.Test;
using UnityEngine;

namespace AgoraGames.Hydra
{
    public class TestUtil
    {
        private static Vector2 scrollViewVector = Vector2.zero;
        public static void RenderHeader(BaseMenu menu, string title)
        {
            GUIStyle centeredStyle = GUI.skin.GetStyle("TextArea");

            centeredStyle.wordWrap = false;
            //centeredStyle.alignment = TextAnchor.UpperLeft;
            GUI.TextArea(new Rect(60, 0, 300, 50), title, centeredStyle);

            // Notifications scroll view
            scrollViewVector = GUI.BeginScrollView(new Rect(370, 0, 300, 50), scrollViewVector, new Rect(0, 0, 270, 400));

            List<Notification> notifications = new List<Notification>(menu.Main.Notifications);
            notifications.Reverse();
            string notificationText = "";
            foreach (var notification in notifications)
            {
                notificationText += notification.Receiver.GetType().Name + ": " + notification.Text + "\n";
            }
            GUI.TextArea(new Rect(0, 0, 270, 400), notificationText);

            GUI.EndScrollView();

            if (GUI.Button(new Rect(0, 0, 50, 50), "Back"))
            {
                menu.Main.Back();
            }
        }
    }
}
