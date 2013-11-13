using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AgoraGames.Hydra.Test
{
    public abstract class BaseMenu
    {
        public Main Main;

        public BaseMenu(Main main)
        {
            this.Main = main;
        }

        public abstract void Render();
        public abstract void Deactivate();
        public abstract void SetParam(object param);

        protected void AddNotification(string notification)
        {
            Main.AddNotification(this, notification);
        }
    }
}
