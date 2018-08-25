using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewValley;
using StardewValley.BellsAndWhistles;
using StardewValley.Menus;
using System;
using System.Diagnostics;

namespace HelloWorldMod
{
    public class HelloWorldMenu : IClickableMenu
    {
        private const int _width = 900;
        private const int _height = 400;

        private enum ButtonID : int
        {
            GlobalMessage = 1000,
            HudMessage,
            ChatMessage,
        }

        private ClickableComponent _button_GlobalMessage;
        private ClickableComponent _button_HudMessage;
        private ClickableComponent _button_ChatMessage;

        private IModHelper _helper;

        public HelloWorldMenu(IModHelper helper) : base(Game1.viewport.Width / 2 - (_width / 2) - borderWidth, Game1.viewport.Height / 2 - (_height / 2) - borderWidth, _width + borderWidth * 2, _height + borderWidth * 2, true)
        {
            var center = Utility.getTopLeftPositionForCenteringOnScreen(width, height, 0, 0);
            _button_GlobalMessage = new ClickableComponent(new Rectangle((int)center.X + borderWidth, (int)center.Y + borderWidth, width - borderWidth * 2, 50), "", "Print \"Hello World\" as Global Message")
            {
                myID = (int)ButtonID.GlobalMessage,
                downNeighborID = (int)ButtonID.HudMessage,
            };
            _button_HudMessage = new ClickableComponent(new Rectangle((int)center.X + borderWidth, (int)center.Y + borderWidth + 150, width - borderWidth * 2, 50), "", "Print \"Hello World\" as HUD Message")
            {
                myID = (int)ButtonID.HudMessage,
                upNeighborID = (int)ButtonID.GlobalMessage,
                downNeighborID = (int)ButtonID.ChatMessage,
            };
            _button_ChatMessage = new ClickableComponent(new Rectangle((int)center.X + borderWidth, (int)center.Y + borderWidth + 300, width - borderWidth * 2, 50), "", "Print \"Hello World\" as Chat Message")
            {
                myID = (int)ButtonID.ChatMessage,
                upNeighborID = (int)ButtonID.HudMessage,
            };
            _helper = helper;
            if (!Game1.options.snappyMenus || !Game1.options.gamepadControls)
                return;
            this.populateClickableComponentList();
            this.snapToDefaultClickableComponent();
        }

        public override void snapToDefaultClickableComponent()
        {
            currentlySnappedComponent = getComponentWithID((int)ButtonID.GlobalMessage);
            snapCursorToCurrentSnappedComponent();
        }

        public override void receiveLeftClick(int x, int y, bool playSound = true)
        {
            base.receiveLeftClick(x, y, playSound);

            if (_button_GlobalMessage.containsPoint(x, y) && _button_GlobalMessage.visible)
                Game1.showGlobalMessage("Hello World");
            else if (_button_HudMessage.containsPoint(x, y) && _button_HudMessage.visible)
                Game1.addHUDMessage(new HUDMessage("Hello World", 2));
            else if (_button_ChatMessage.containsPoint(x, y) && _button_ChatMessage.visible)
            {
                _helper.Reflection.GetField<Multiplayer>(typeof(Game1), "multiplayer")?.GetValue().sendChatMessage(LocalizedContentManager.CurrentLanguageCode, "Hello World");
                Game1.chatBox.receiveChatMessage(Game1.player.UniqueMultiplayerID, 0, LocalizedContentManager.CurrentLanguageCode, "Hello World");
            }

            //if (this.linkToSVSite.containsPoint(x, y) && this.linkToSVSite.visible)
            //    HelloWorldMenu.LaunchBrowser("http://www.stardewvalley.net");
            //else if (this.linkToTwitter.containsPoint(x, y) && this.linkToTwitter.visible)
            //    HelloWorldMenu.LaunchBrowser("http://www.twitter.com/ConcernedApe");
            //else if (this.linkToChucklefish.containsPoint(x, y) && this.linkToChucklefish.visible)
            //    HelloWorldMenu.LaunchBrowser("http://blog.chucklefish.org/");
            //else
            //    this.isWithinBounds(x, y);
        }

        public override void performHoverAction(int x, int y)
        {
            base.performHoverAction(x, y);

            _button_GlobalMessage.scale = 1f;
            _button_HudMessage.scale = 1f;
            _button_ChatMessage.scale = 1f;
            if (_button_GlobalMessage.containsPoint(x, y))
                _button_GlobalMessage.scale = 2f;
            if (_button_HudMessage.containsPoint(x, y))
                _button_HudMessage.scale = 2f;
            if (_button_ChatMessage.containsPoint(x, y))
                _button_ChatMessage.scale = 2f;

            //this.linkToSVSite.scale = 1f;
            //this.linkToTwitter.scale = 1f;
            //this.linkToChucklefish.scale = 1f;
            //if (this.linkToSVSite.containsPoint(x, y))
            //    this.linkToSVSite.scale = 2f;
            //else if (this.linkToTwitter.containsPoint(x, y))
            //{
            //    this.linkToTwitter.scale = 2f;
            //}
            //else
            //{
            //    if (!this.linkToChucklefish.containsPoint(x, y))
            //        return;
            //    this.linkToChucklefish.scale = 2f;
            //}
        }

        public override void receiveRightClick(int x, int y, bool playSound = true)
        {
        }

        public override void draw(SpriteBatch b)
        {
            if (Game1.options.showMenuBackground)
                drawBackground(b);
            else
                b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.4f);

            drawTextureBox(b, this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, Color.White);
            SpriteText.drawString(b, _button_GlobalMessage.label, _button_GlobalMessage.bounds.X, _button_GlobalMessage.bounds.Y, color: _button_GlobalMessage.scale == 1 ? -1 : 7);
            SpriteText.drawString(b, _button_HudMessage.label, _button_HudMessage.bounds.X, _button_HudMessage.bounds.Y, color: _button_HudMessage.scale == 1 ? -1 : 7);
            SpriteText.drawString(b, _button_ChatMessage.label, _button_ChatMessage.bounds.X, _button_ChatMessage.bounds.Y, color: _button_ChatMessage.scale == 1 ? -1 : 7);

            base.draw(b);

            drawMouse(b);

            //Vector2 centeringOnScreen = Utility.getTopLeftPositionForCenteringOnScreen(this.width, 600, 0, 0);
            //b.Draw(Game1.fadeToBlackRect, Game1.graphics.GraphicsDevice.Viewport.Bounds, Color.Black * 0.75f);
            //IClickableMenu.drawTextureBox(b, Game1.mouseCursors, new Rectangle(473, 36, 24, 24), (int)centeringOnScreen.X, (int)centeringOnScreen.Y, this.width, 550, Color.White, 4f, true);
            //SpriteText.drawString(b, Game1.content.LoadString("Strings\\UI:About_Title"), (int)centeringOnScreen.X + 32, (int)centeringOnScreen.Y + 32, 9999, -1, 9999, 1f, 0.88f, false, -1, "", 6);
            //string str = Game1.content.LoadString("Strings\\UI:About_Credit");
            //SpriteText.drawString(b, str.Replace('\n', '^'), (int)centeringOnScreen.X + 32, (int)centeringOnScreen.Y + 32, 9999, -1, 9999, 1f, 0.88f, false, -1, "", 4);
            //if (this.linkToSVSite.visible)
            //    SpriteText.drawString(b, "= " + this.linkToSVSite.label, (int)centeringOnScreen.X + 32, this.linkToSVSite.bounds.Y, 999, -1, 999, 1f, 1f, false, -1, "", (double)this.linkToSVSite.scale == 1.0 ? 3 : 7);
            //if (this.linkToTwitter.visible)
            //    SpriteText.drawString(b, "= " + this.linkToTwitter.label, (int)centeringOnScreen.X + 32, this.linkToTwitter.bounds.Y, 999, -1, 999, 1f, 1f, false, -1, "", (double)this.linkToTwitter.scale == 1.0 ? 3 : 7);
            //if (this.linkToChucklefish.visible)
            //    SpriteText.drawString(b, "< " + this.linkToChucklefish.label, (int)centeringOnScreen.X + 32, this.linkToChucklefish.bounds.Y, 999, -1, 999, 1f, 1f, false, -1, "", (double)this.linkToChucklefish.scale == 1.0 ? 3 : 7);
            //if ((double)this.linkToChucklefish.scale > 1.0)
            //    b.Draw(Game1.objectSpriteSheet, new Vector2((float)(this.linkToChucklefish.bounds.Right - 320), (float)(this.linkToChucklefish.bounds.Y - 4)), new Rectangle?(Game1.getSourceRectForStandardTileSheet(Game1.objectSpriteSheet, 128, 16, 16)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 0.89f);
            //else if ((double)this.linkToSVSite.scale <= 1.0)
            //{
            //    double scale = (double)this.linkToTwitter.scale;
            //}
            //b.Draw(Game1.mouseCursors, new Vector2((float)((double)centeringOnScreen.X + (double)this.width - 96.0), centeringOnScreen.Y + 128f), new Rectangle?(new Rectangle(540 + 13 * (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0 / 150.0), 333, 13, 13)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            //if (this.linkToSVSite.visible || this.linkToTwitter.visible || this.linkToChucklefish.visible)
            //    b.Draw(Game1.mouseCursors, new Vector2((float)((double)centeringOnScreen.X + (double)this.width - 96.0), (float)((double)centeringOnScreen.Y + 700.0 - 256.0)), new Rectangle?(new Rectangle(592 + 13 * (int)(Game1.currentGameTime.TotalGameTime.TotalMilliseconds % 600.0 / 150.0), 333, 13, 13)), Color.White, 0.0f, Vector2.Zero, 4f, SpriteEffects.None, 1f);
            //if (Game1.activeClickableMenu != null && Game1.activeClickableMenu is TitleMenu && (Game1.activeClickableMenu as TitleMenu).startupMessage.Length > 0)
            //    b.DrawString(Game1.smallFont, Game1.parseText((Game1.activeClickableMenu as TitleMenu).startupMessage, Game1.smallFont, 640), new Vector2(8f, (float)((double)Game1.viewport.Height - (double)Game1.smallFont.MeasureString(Game1.parseText((Game1.activeClickableMenu as TitleMenu).startupMessage, Game1.smallFont, 640)).Y - 4.0)), Color.White);
            //else
            //    b.DrawString(Game1.smallFont, "v" + Game1.version, new Vector2(16f, (float)((double)Game1.viewport.Height - (double)Game1.smallFont.MeasureString("v" + Game1.version).Y - 8.0)), Color.White);
        }

        public override void gameWindowSizeChanged(Rectangle oldBounds, Rectangle newBounds)
        {
            base.gameWindowSizeChanged(oldBounds, newBounds);
            //this.backButton = new ClickableComponent(new Rectangle(Game1.viewport.Width - 198 - 48, Game1.viewport.Height - 81 - 24, 198, 81), "")
            //{
            //    myID = 81114,
            //    upNeighborID = 93333
            //};
            //if (!Game1.options.snappyMenus || !Game1.options.gamepadControls)
            //    return;
            //int id = this.currentlySnappedComponent != null ? this.currentlySnappedComponent.myID : 81114;
            //this.populateClickableComponentList();
            //this.currentlySnappedComponent = this.getComponentWithID(id);
            //this.snapCursorToCurrentSnappedComponent();
        }
    }
}
