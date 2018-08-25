using Netcode;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FreezeTimeInMenusMultiplayer
{
    public class ModEntry : Mod
    {
        private IClickableMenu _previousMenu;
        private int _frozenTime;
        private ExtendedChatBox _extendedChatBox;

        private Multiplayer _multiplayer;
        public Multiplayer Multiplayer => _multiplayer ?? (_multiplayer = Helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer")?.GetValue());

        public static Dictionary<long, bool> FarmhandHasMenuOpen = new Dictionary<long, bool>();

        public override void Entry(IModHelper helper)
        {
            GameEvents.UpdateTick += _OnGameUpdateTick;
            _previousMenu = Game1.activeClickableMenu;
            _extendedChatBox = new ExtendedChatBox();

            MenuEvents.MenuChanged += _OnMenuChanged;
            MenuEvents.MenuClosed += _OnMenuClosed;
        }

        private void _OnMenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            //Monitor.Log("Active Menu: " + e.NewMenu.GetType());

            if (!Context.IsMultiplayer || Game1.IsServer)
                return;
            if (!_ThisMenuStopsTime(e.NewMenu))
            {
                Multiplayer.sendChatMessage(LocalizedContentManager.CurrentLanguageCode, "/UnfreezeTime");
                return;
            }
            Multiplayer.sendChatMessage(LocalizedContentManager.CurrentLanguageCode, "/FreezeTime");
        }
        private void _OnMenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            if (!Context.IsMultiplayer || Game1.IsServer)
                return;
            Multiplayer.sendChatMessage(LocalizedContentManager.CurrentLanguageCode, "/UnfreezeTime");
        }

        private void _OnGameUpdateTick(object sender, EventArgs e)
        {
            if (!Context.IsMultiplayer)
                return;
            Game1.chatBox = _extendedChatBox;

            //if (!Game1.IsServer)
            //{
            //    if (_previousMenu != Game1.activeClickableMenu)
            //    {
            //        _previousMenu = Game1.activeClickableMenu;
            //        if (_previousMenu != null)
            //        {
            //            _frozenTime = Game1.timeOfDay;
            //            if (!Game1.IsServer)
            //                Multiplayer.sendChatMessage(LocalizedContentManager.CurrentLanguageCode, "/FreezeTime");
            //        }
            //        else if (!Game1.IsServer)
            //            Multiplayer.sendChatMessage(LocalizedContentManager.CurrentLanguageCode, "/UnfreezeTime");
            //    }
            //}

            if (_ThisMenuStopsTime(Game1.activeClickableMenu) || _AnyActiveFarmhandHasMenuOpen())
                Game1.timeOfDay = _frozenTime;

            _frozenTime = Game1.timeOfDay;
        }

        private bool _AnyActiveFarmhandHasMenuOpen()
        {
            bool someBodyHasMenuOpen = false;
            foreach (var keyValue in FarmhandHasMenuOpen)
            {
                var onlineFarmers = Game1.getOnlineFarmers();
                if (onlineFarmers.FirstOrDefault(of => of.UniqueMultiplayerID == keyValue.Key) == null)
                    continue;
                if (keyValue.Value == true)
                    someBodyHasMenuOpen = true;
            }
            return someBodyHasMenuOpen;
        }

        private bool _ThisMenuStopsTime(IClickableMenu menu)
        {
            if (menu == null)
                return false;

            if (menu is ReadyCheckDialog)
                return false;

            return true;
        }
    }
}
