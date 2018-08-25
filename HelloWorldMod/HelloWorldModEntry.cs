using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HelloWorldMod
{
    public class HelloWorldModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {
            InputEvents.ButtonPressed += _OnInputEventsButtonPressed;
        }

        private void _OnInputEventsButtonPressed(object sender, EventArgsInput e)
        {
            if (!Context.IsWorldReady)
                return;

            if (e.Button != SButton.Space)
                return;

            if (Game1.activeClickableMenu is HelloWorldMenu)
                Game1.activeClickableMenu = null;
            else
                Game1.activeClickableMenu = new HelloWorldMenu(Helper);
        }
    }
}
