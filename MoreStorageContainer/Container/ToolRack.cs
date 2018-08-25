using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreStorageContainer.Menus;
using Netcode;
using PyTK.CustomElementHandler;
using PyTK.Extensions;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Objects;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;

namespace MoreStorageContainer.Container
{
    public class ToolRack : PySObject
    {
        public enum ItemSlot : int { BottomSlot, TopSlot }

        public NetObjectArray<Item> Items;
        public Tool BottomSlot
        {
            get { return Items[(int)ItemSlot.BottomSlot] as Tool; }
            set { Items[(int)ItemSlot.BottomSlot] = value; }
        }
        public Tool TopSlot
        {
            get { return Items[(int)ItemSlot.TopSlot] as Tool; }
            set { Items[(int)ItemSlot.TopSlot] = value; }
        }

        public ToolRack()
        {
            Items = new NetObjectArray<Item>(2);
            NetFields.AddField(Items);
        }
        public ToolRack(CustomObjectData data) : this(data, Vector2.Zero) { }
        public ToolRack(CustomObjectData data, Vector2 tileLocation) : base(data, tileLocation)
        {
            Items = new NetObjectArray<Item>(2);
            NetFields.AddField(Items);
            data.tileIndex = (int)TileIndex.ToolRack_Single;
        }

        public ToolRack LeftNeighbor { get; private set; }
        public ToolRack RightNeighbor { get; private set; }
        public bool IsConnectedToLeftNeighbor { get; private set; }


        public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
        {
            // Just checkingForActivity means that the mouse is over this item
            if (justCheckingForActivity)
                return true;

            // Otherwise the player has clicked on this object, now a menu can be opened or sth like that.
            if (Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.LeftShift))
            {
                if (TopSlot != null)
                {
                    Game1.player.addItemToInventory(TopSlot);
                    Game1.addHUDMessage(new HUDMessage(TopSlot.BaseName, 1, false, Color.White, TopSlot));
                    TopSlot = null;
                }
                if (BottomSlot != null)
                {
                    Game1.player.addItemToInventory(BottomSlot);
                    Game1.addHUDMessage(new HUDMessage(BottomSlot.BaseName, 1, false, Color.White, BottomSlot));
                    BottomSlot = null;
                }
            }
            else
                Game1.activeClickableMenu = new ToolRackMenu(this);

            return true;
        }

        public override bool performToolAction(Tool t, GameLocation location)
        {
            if (BottomSlot == null && TopSlot == null)
                return base.performToolAction(t, location);
            return false;
        }

        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            if (data == null)
            {
                base.draw(spriteBatch, x, y, alpha);
                return;
            }

            var layer = (float)((y + 1) * Game1.tileSize / 10000.0 + 9.99999974737875E-06 + TileLocation.X / 10000.0);

            var leftPos = new Vector2(TileLocation.X - 1, TileLocation.Y);
            var rightPos = new Vector2(TileLocation.X + 1, TileLocation.Y);
            var loc = Game1.player.currentLocation;
            LeftNeighbor = loc.getObjectAtTile((int)leftPos.X, (int)leftPos.Y) as ToolRack;
            IsConnectedToLeftNeighbor = (LeftNeighbor != null && !LeftNeighbor.IsConnectedToLeftNeighbor);
            RightNeighbor = loc.getObjectAtTile((int)rightPos.X, (int)rightPos.Y) as ToolRack;

            float posX = x * Game1.tileSize;
            float posY = (y - 1) * Game1.tileSize;
            float shakeOffset = (shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0);

            var tileIdx = (int)_GetTileIndexBasedOnNeighbor();
            spriteBatch.Draw(ModEntry._containerTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2(posX + shakeOffset, posY)), new Rectangle?(Game1.getSourceRectForStandardTileSheet(ModEntry._containerTexture, tileIdx, 16, 32)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, layer - (layer/100));

            if (TopSlot != null)
            {
                var data = _GetToolDisplayPair(TopSlot);
                shakeOffset = (shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0);
                var otherTool_finalPos = Game1.GlobalToLocal(Game1.viewport, new Vector2((x * Game1.tileSize) + 32 + shakeOffset, (y * Game1.tileSize) - 44));
                spriteBatch.Draw(data.Item1, otherTool_finalPos, data.Item2, new Color(1, 1, 1, alpha), MathHelper.ToRadians(-5), new Vector2(8, 8), 3, SpriteEffects.None, (float)layer);
            }

            if (BottomSlot != null)
            {
                var data = _GetToolDisplayPair(BottomSlot);
                shakeOffset = (shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0);
                var wateringCan_finalPos = Game1.GlobalToLocal(Game1.viewport, new Vector2((x * Game1.tileSize) + 32 + shakeOffset, (y * Game1.tileSize) + 16));
                spriteBatch.Draw(data.Item1, wateringCan_finalPos, data.Item2, new Color(1, 1, 1, alpha), 0, new Vector2(8, 8), 3, SpriteEffects.None, (float)layer);
            }
        }

        private Tuple<Texture2D, Rectangle> _GetToolDisplayPair(Tool item)
        {
            Texture2D resultTex = null;
            Rectangle? resultRect = null;
            if (item is MeleeWeapon)
            {
                resultTex = Tool.weaponsTexture;
                resultRect = _GetWeaponRect(item.IndexOfMenuItemView);
            }
            else
            {
                resultTex = Game1.toolSpriteSheet;
                resultRect = _GetToolRect(item.IndexOfMenuItemView);
            }
            return new Tuple<Texture2D, Rectangle>(resultTex, resultRect.Value);
        }
        private Rectangle _GetWeaponRect(int indexOfMenuItemView)
        {
            return Game1.getSquareSourceRectForNonStandardTileSheet(Tool.weaponsTexture, 16, 16, indexOfMenuItemView);
        }
        private Rectangle _GetToolRect(int indexOfMenuItemView)
        {
            return Game1.getSquareSourceRectForNonStandardTileSheet(Game1.toolSpriteSheet, 16, 16, indexOfMenuItemView);
        }

        public override bool isPlaceable()
        {
            if (!base.isPlaceable())
                return false;
            var loc = Game1.player.currentLocation;
            if (loc != null && (loc.Name == "FarmHouse" || loc.Name == "Farm"))
                return true;
            return false;
        }


        public override Item getOne()
        {
            var result = new ToolRack(data);
            result.TileLocation = Vector2.Zero;
            result.BottomSlot = BottomSlot;
            result.TopSlot = TopSlot;
            return result;
        }

        public override object getReplacement()
        {
            var result = base.getReplacement() as Chest;
            if (Items != null)
            {
                foreach (var itm in Items)
                    result.addItem(itm);
            }
            return result;
        }

        public override ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            var customObjectData = CustomObjectData.collection[additionalSaveData["id"]];
            var replChest = replacement as Chest;
            var toolRack = new ToolRack(customObjectData, replChest.TileLocation);
            foreach (var itm in replChest.items)
            {
                if (itm is WateringCan)
                    toolRack.BottomSlot = itm as WateringCan;
                else
                    toolRack.TopSlot = itm as Tool;
            }
            return toolRack;
        }

        private TileIndex _GetTileIndexBasedOnNeighbor()
        {
            if (LeftNeighbor != null && !LeftNeighbor.IsConnectedToLeftNeighbor)
                return TileIndex.ToolRack_LeftConnection;
            else if (RightNeighbor != null)
                return TileIndex.ToolRack_RightConnection;

            return TileIndex.ToolRack_Single;

        }

        public static bool FitsTopSlot(Item toCheckFor)
        {
            if (toCheckFor == null)
                return false;
            var t = toCheckFor.GetType();
            if (t == typeof(Axe))
                return true;
            if (t == typeof(FishingRod))
                return true;
            if (t == typeof(Hoe))
                return true;
            if (t == typeof(Lantern))
                return true;
            if (t == typeof(MagnifyingGlass))
                return true;
            if (t == typeof(Pickaxe))
                return true;
            if (t == typeof(Shears))
                return true;
            if (t == typeof(Slingshot))
                return true;
            return false;
        }
        public static bool FitsBottomSlot(Item toCheckFor)
        {
            if (toCheckFor == null)
                return false;
            var t = toCheckFor.GetType();
            if (t == typeof(MilkPail))
                return true;
            if (t == typeof(Pan))
                return true;
            if (t == typeof(WateringCan))
                return true;
            return false;
        }
    }
}
