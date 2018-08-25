using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MoreStorageContainer.Container;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.Locations;
using StardewValley.Menus;
using StardewValley.Objects;

namespace MoreStorageContainer.Handler
{
    internal static class FreezerToFridgeHandler
    {
        internal struct FreezerIndex
        {
            internal readonly Freezer Freezer;
            internal readonly int Start;
            internal readonly int Count;

            internal FreezerIndex(Freezer freezer, int startIndex)
            {
                Freezer = freezer;
                Start = startIndex;
                Count = Freezer.Items.Count;
            }
        }

        private static List<FreezerIndex> _indices;

        internal static void OnMenuChanged(object sender, EventArgsClickableMenuChanged e)
        {
            if (!_IsCookingMenu(e.NewMenu))
                return;
            _AddFreezerItemsToFridge();
        }

        internal static void OnMenuClosed(object sender, EventArgsClickableMenuClosed e)
        {
            if (!_IsCookingMenu(e.PriorMenu))
                return;
            _RemoveItemsFromFridge();
        }

        private static bool _IsCookingMenu(IClickableMenu menu)
        {
            if (!Context.IsWorldReady || (menu as CraftingPage) == null || !ModEntry._helper.Reflection.GetField<bool>(menu, "cooking", true).GetValue())
                return false;
            return true;
        }

        private static void _AddFreezerItemsToFridge()
        {
            if (_indices != null)
                return;

            if (!(Game1.getLocationFromName("FarmHouse") is FarmHouse farmHouse))
                return;
            _indices = new List<FreezerIndex>();
            var freezers = farmHouse.objects.Values.Where(obj => obj is Freezer).Cast<Freezer>();
            foreach (var freezer in freezers)
            {
                _indices.Add(new FreezerIndex(freezer, farmHouse.fridge.Value.items.Count));
                for (int i = freezer.Items.Count - 1; i >= 0; --i)
                {
                    var itm = freezer.Items[i];
                    freezer.Items.RemoveAt(i);
                    freezer.IsInUseByCooking.Value = true;
                    farmHouse.fridge.Value.items.Add(itm);
                }
            }
        }

        private static void _RemoveItemsFromFridge()
        {
            if (_indices == null)
                return;

            if (!(Game1.getLocationFromName("FarmHouse") is FarmHouse farmHouse))
                return;
            

            for (int i = _indices.Count - 1; i >= 0; --i)
            {
                var idx = _indices[i];
                idx.Freezer.IsInUseByCooking.Value = false;
                for (int itmIdx = idx.Start + idx.Count - 1; itmIdx >= idx.Start; --itmIdx)
                {
                    var itm = farmHouse.fridge.Value.items[itmIdx];
                    farmHouse.fridge.Value.items.RemoveAt(itmIdx);
                    idx.Freezer.Items.Insert(0, itm);
                }
            }

            _indices = null;
        }
    }
}
