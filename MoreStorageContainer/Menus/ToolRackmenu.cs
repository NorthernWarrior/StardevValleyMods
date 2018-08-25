using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreStorageContainer.Container;
using StardewModdingAPI;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreStorageContainer.Menus
{
    public class ToolRackMenu : StorageContainer
    {
        public static ToolRackMenu Current { get; private set; }
        private readonly ToolRack _toolRack;

        //public ToolRackMenu(ToolRack toolRack) : base(new List<Item>() { toolRack.WateringCan_Slot, toolRack.OtherTool_Slot }, 2, 1, _OnItemChange)
        public ToolRackMenu(ToolRack toolRack) : base(toolRack.Items, 2, 1, _OnItemChange, _OnItemHighlight)
        {
            Current = this;

            _toolRack = toolRack;
        }

        private static bool _OnItemHighlight(Item i)
        {
            return (ToolRack.FitsTopSlot(i) || ToolRack.FitsBottomSlot(i));
        }

        private static bool _OnItemChange(Item i, int position, Item old, StorageContainer container, bool onRemoval)
        {
            if (!(container is ToolRackMenu curr))
                return false;

            if (onRemoval)
            {
                if (ToolRack.FitsBottomSlot(i))
                {
                    curr._toolRack.BottomSlot = null;
                    return true;
                }
                else if (i is Tool && !ToolRack.FitsBottomSlot(i))
                {
                    curr._toolRack.TopSlot = null;
                    return true;
                }
            }
            else
            {
                if (ToolRack.FitsBottomSlot(i))
                {
                    if (position == (int)ToolRack.ItemSlot.BottomSlot)
                    {
                        curr._toolRack.BottomSlot = i as Tool;
                        return true;
                    }
                    else if (curr._toolRack.BottomSlot == null)
                    {
                        curr._toolRack.BottomSlot = i as Tool;
                        curr.ItemsToGrabMenu.actualInventory[position] = null;
                        curr.ItemsToGrabMenu.actualInventory[(int)ToolRack.ItemSlot.BottomSlot] = i;
                        return true;
                    }
                }
                else if (ToolRack.FitsTopSlot(i))
                {
                    if (position == (int)ToolRack.ItemSlot.TopSlot)
                    {
                        curr._toolRack.TopSlot = i as Tool;
                        return true;
                    }
                    else if (curr._toolRack.TopSlot == null)
                    {
                        curr._toolRack.TopSlot = i as Tool;
                        curr.ItemsToGrabMenu.actualInventory[position] = null;
                        curr.ItemsToGrabMenu.actualInventory[(int)ToolRack.ItemSlot.TopSlot] = i;
                        return true;
                    }
                }
            }

            Game1.player.addItemToInventory(i);
            curr.ItemsToGrabMenu.actualInventory[position] = null;

            return false;
        }

        //public override void draw(SpriteBatch b)
        //{
        //    if (Game1.options.showMenuBackground)
        //        drawBackground(b);
        //    else
        //        b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);

        //    //drawTextureBox(b, xPositionOnScreen, yPositionOnScreen, width, height, Color.White);

        //    base.draw(b);

        //    drawMouse(b);
        //}

    }
}
