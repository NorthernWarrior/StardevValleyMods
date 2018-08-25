using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MoreStorageContainer.Menus;
using Netcode;
using PyTK.CustomElementHandler;
using StardewValley;
using StardewValley.Menus;
using StardewValley.Network;
using StardewValley.Objects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MoreStorageContainer.Container
{
    public class Freezer : PySObject
    {
        public NetObjectList<Item> Items = new NetObjectList<Item>();

        public bool IsEmpty => Items.ToList().TrueForAll(i => i == null);
        public readonly NetInt StartingLidFrame = new NetInt((int)TileIndex.Freezer_LidStart);
        public readonly NetInt FrameCounter = new NetInt(501);
        public readonly NetMutex Mutex = new NetMutex();
        public NetBool IsInUseByCooking = new NetBool(false);

        private int _currentLidFrame;

        public Freezer()
        {
            StartingLidFrame.Value = (int)TileIndex.Freezer_LidStart;
            _currentLidFrame = (int)TileIndex.Freezer_LidStart;
        }
        public Freezer(CustomObjectData data) : this(data, Vector2.Zero) { }
        public Freezer(CustomObjectData data, Vector2 tileLocation) : base(data, tileLocation)
        {
            data.tileIndex = (int)TileIndex.Freezer;
            StartingLidFrame.Value = (int)TileIndex.Freezer_LidStart;
            _currentLidFrame = (int)TileIndex.Freezer_LidStart;
        }

        protected override void initNetFields()
        {
            base.initNetFields();
            NetFields.AddFields(Items, StartingLidFrame, FrameCounter, Mutex.NetFields, IsInUseByCooking);
        }



        public void ResetLidFrame()
        {
            _currentLidFrame = StartingLidFrame.Value;
        }

        public void FixLidFrame()
        {
            if (_currentLidFrame == 0)
                _currentLidFrame = StartingLidFrame.Value;

            if (Mutex.IsLocked() && !Mutex.IsLockHeld())
            {
                _currentLidFrame = 135;
            }
            else
            {
                if (Mutex.IsLocked())
                    return;
                _currentLidFrame = StartingLidFrame.Value;
            }
        }


        public override void updateWhenCurrentLocation(GameTime time, GameLocation environment)
        {
            FixLidFrame();
            base.updateWhenCurrentLocation(time, environment);
            Mutex.Update(environment);

            if (FrameCounter.Value > -1 && _currentLidFrame <= (int)TileIndex.Freezer_LidEnd)
            {
                --FrameCounter.Value;

                if (FrameCounter.Value > 0 || !Mutex.IsLockHeld())
                    return;

                if (_currentLidFrame == (int)TileIndex.Freezer_LidEnd)
                {
                    Game1.activeClickableMenu = _GetFreezerMenu();
                    FrameCounter.Value = -1;
                }
                else
                {
                    FrameCounter.Value = 5;
                    ++_currentLidFrame;
                }
            }
            else
            {
                if (FrameCounter.Value != -1 || _currentLidFrame <= (int)TileIndex.Freezer_LidStart || (Game1.activeClickableMenu != null || !Mutex.IsLockHeld()))
                    return;
                Mutex.ReleaseLock();
                _currentLidFrame = (int)TileIndex.Freezer_LidEnd;
                FrameCounter.Value = 2;
                environment.localSound("doorCreakReverse");
            }
        }


        public virtual Item AddItem(Item item)
        {
            for (int index = 0; index < Items.Count; ++index)
            {
                if (Items[index] != null && Items[index].canStackWith(item))
                {
                    item.Stack = Items[index].addToStack(item.Stack);
                    if (item.Stack <= 0)
                        return null;
                }
            }
            if (Items.Count >= 36)
                return item;
            Items.Add(item);
            return null;
        }


        private void _GrabItemFromChest(Item item, Farmer who)
        {
            if (!who.couldInventoryAcceptThisItem(item))
                return;
            Items.Remove(item);
            ClearNulls();
        }

        private void _GrabItemFromInventory(Item item, Farmer who)
        {
            if (item.Stack == 0)
                item.Stack = 1;
            Item obj = AddItem(item);
            if (obj == null)
                who.removeItemFromInventory(item);
            else
                obj = who.addItemToInventory(obj);
            ClearNulls();
            var id = Game1.activeClickableMenu.currentlySnappedComponent != null ? Game1.activeClickableMenu.currentlySnappedComponent.myID : -1;
            Game1.activeClickableMenu = _GetFreezerMenu();
            (Game1.activeClickableMenu as ItemGrabMenu).heldItem = obj;
            if (id == -1)
                return;
            Game1.activeClickableMenu.currentlySnappedComponent = Game1.activeClickableMenu.getComponentWithID(id);
            Game1.activeClickableMenu.snapCursorToCurrentSnappedComponent();
        }
        
        private IClickableMenu _GetFreezerMenu()
        {
            return new ItemGrabMenu(Items, 
                false, 
                true, 
                new InventoryMenu.highlightThisItem(GoesIntoFreezer), 
                _GrabItemFromInventory, 
                "Hello World", 
                _GrabItemFromChest, 
                false, 
                true, 
                true, 
                true, 
                true, 
                1, 
                this,
                -1,
                this);
        }

        public virtual void ClearNulls()
        {
            for (int index = Items.Count - 1; index >= 0; --index)
            {
                if (Items[index] == null)
                    Items.RemoveAt(index);
            }
        }

        public override bool checkForAction(Farmer who, bool justCheckingForActivity = false)
        {
            if (IsInUseByCooking.Value)
                return false;

            // Just checkingForActivity means that the mouse is over this item
            if (justCheckingForActivity)
                return true;

            // Otherwise the player has clicked on this object, now a menu can be opened or sth like that.
            //Game1.activeClickableMenu = new FreezerMenu(this);

            Mutex.RequestLock((() => {
                FrameCounter.Value = 5;
                Game1.playSound("openChest");
                Game1.player.Halt();
                Game1.player.freezePause = 1000;
            }));

            return true;
        }

        public override bool performToolAction(Tool t, GameLocation location)
        {
            if (IsEmpty)
                return base.performToolAction(t, location);
            return false;
        }

        public override bool isPlaceable()
        {
            if (!base.isPlaceable())
                return false;
            var loc = Game1.player.currentLocation;
            if (loc != null && (loc.Name == "FarmHouse"))
                return true;
            return false;
        }


        public override void draw(SpriteBatch spriteBatch, int x, int y, float alpha = 1)
        {
            spriteBatch.Draw(ModEntry._containerTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0)), (float)((y - 1) * 64))), new Rectangle?(Game1.getSourceRectForStandardTileSheet(ModEntry._containerTexture, (int)TileIndex.Freezer, 16, 32)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 4) / 10000f);
            spriteBatch.Draw(ModEntry._containerTexture, Game1.GlobalToLocal(Game1.viewport, new Vector2((float)(x * 64 + (this.shakeTimer > 0 ? Game1.random.Next(-1, 2) : 0)), (float)((y - 1) * 64))), new Rectangle?(Game1.getSourceRectForStandardTileSheet(ModEntry._containerTexture, _currentLidFrame, 16, 32)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, (float)(y * 64 + 5) / 10000f);
        }

        public override Item getOne()
        {
            var result = new Freezer(data);
            result.TileLocation = Vector2.Zero;
            result.Items = Items;
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
            var freezer = new Freezer(customObjectData, replChest.TileLocation);
            freezer.Items.AddRange(replChest.items);
            return freezer;
        }

        public static bool GoesIntoFreezer(Item i)
        {
            if (i == null)
                return false;

            return true;
        }

    }
}
