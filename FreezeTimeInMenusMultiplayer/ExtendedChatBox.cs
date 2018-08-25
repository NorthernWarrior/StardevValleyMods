using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreezeTimeInMenusMultiplayer
{
    public class ExtendedChatBox : ChatBox
    {
        public override void receiveChatMessage(long sourceFarmer, int chatKind, LocalizedContentManager.LanguageCode language, string message)
        {
            if (message.StartsWith("/FreezeTime"))
            {
                if (Game1.IsServer)
                    ModEntry.FarmhandHasMenuOpen[sourceFarmer] = true;
                return;
            }
            else if (message.StartsWith("/UnfreezeTime"))
            {
                if (Game1.IsServer)
                    ModEntry.FarmhandHasMenuOpen[sourceFarmer] = false;
                return;
            }

            base.receiveChatMessage(sourceFarmer, chatKind, language, message);
        }
    }
}
