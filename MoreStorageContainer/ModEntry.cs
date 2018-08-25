using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using PyTK.Extensions;
using PyTK.Types;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PyTK.CustomElementHandler;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using MoreStorageContainer.Container;
using StardewValley.Tools;
using MoreStorageContainer.Handler;

namespace MoreStorageContainer
{
    public enum TileIndex : int {
        ToolRack_Single, ToolRack_RightConnection, ToolRack_LeftConnection,
        Freezer = 8, Freezer_LidStart, Freezer_LidEnd = (Freezer_LidStart + 4),
    }

    public class ModEntry : Mod
    {
        internal static IMonitor _monitor;
        internal static IModHelper _helper;

        internal static Texture2D _containerTexture;

        CraftingData _toolRackCraftingData;
        CustomObjectData _toolRackObjectData;

        CraftingData _freezerCraftingData;
        CustomObjectData _freezerObjectData;

        public override void Entry(IModHelper helper)
        {
            _monitor = Monitor;
            _helper = helper;

            SaveEvents.AfterLoad += SaveEvents_AfterLoad;
            //InputEvents.ButtonPressed += InputEvents_ButtonPressed;
            
            _containerTexture = Helper.Content.Load<Texture2D>("Assets/Container.png", ContentSource.ModFolder);


            _toolRackCraftingData = new CraftingData("Tool-Rack", "388 20", "Tool-Rack", -1, true, false, "Farming 1");
            _toolRackObjectData = new CustomObjectData("Tool-Rack", "Tool-Rack/0/-300/Crafting -9/A nice way to store your tools./true/true/0/0/Tool-Rack", _containerTexture, Color.White, 0, true, typeof(ToolRack), _toolRackCraftingData);

            _freezerCraftingData = new CraftingData("Freezer", "335 2 84 5", "Freezer", -1, true, false, "Farming 1");
            _freezerObjectData = new CustomObjectData("Freezer", "Freezer/0/-300/Crafting -9/Store your foods in here to acces them via the fridge./false/true/0/0/Freezer", _containerTexture, Color.White, 3, true, typeof(Freezer), _freezerCraftingData);

            MenuEvents.MenuChanged += FreezerToFridgeHandler.OnMenuChanged;
            MenuEvents.MenuClosed += FreezerToFridgeHandler.OnMenuClosed;
        }

        //private void InputEvents_ButtonPressed(object sender, EventArgsInput e)
        //{
        //    if (e.Button == SButton.U)
        //        Game1.player.addItemToInventory(new WateringCan().getOne());
        //}

        private void SaveEvents_AfterLoad(object sender, EventArgs e)
        {
            if (!Game1.player.craftingRecipes.ContainsKey(_toolRackCraftingData.displayName) && Game1.player.FarmingLevel >= 1)
                Game1.player.craftingRecipes.Add(_toolRackCraftingData.displayName, 0);

            //if (Game1.IsServer || Game1.IsMultiplayer)
            //    return;

            if (!Game1.player.craftingRecipes.ContainsKey(_freezerCraftingData.displayName) && Game1.player.FarmingLevel >= 1)
                Game1.player.craftingRecipes.Add(_freezerCraftingData.displayName, 0);
        }
    }
}
