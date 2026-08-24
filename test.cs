using Terraria;
using TerrariaModder.Core;
using HarmonyLib;
using Microsoft.Xna.Framework;
using Terraria.UI;
using Terraria.GameInput;

public class ExtraAccessorySlot : IMod
{
    public string Id => "extra-accessory-slot";
    public string Name => "Extra Accessory Slot";
    public string Version => "1.0.0";

    // Change this number to add more slots.
    private const int ExtraSlotCount = 5;

    private static Item[] extraSlots = new Item[ExtraSlotCount];

    private static Harmony harmony;

    public void Initialize(ModContext context)
    {
        // Create the extra accessory items.
        for (int i = 0; i < ExtraSlotCount; i++)
        {
            extraSlots[i] = new Item();
        }

        harmony = new Harmony("extra-accessory-slot");

        var drawInventory = AccessTools.Method(
            typeof(Main),
            "DrawInventory"
        );

        if (drawInventory != null)
        {
            harmony.Patch(
                drawInventory,
                postfix: new HarmonyMethod(
                    typeof(ExtraAccessorySlot),
                    nameof(DrawExtraSlots)
                )
            );

            context.Logger.Info("DrawInventory patched successfully.");
        }
        else
        {
            context.Logger.Info("Could not find Main.DrawInventory.");
        }

        context.Logger.Info(
            $"Extra Accessory Slot loaded! Added {ExtraSlotCount} slots."
        );
    }

    public static void DrawExtraSlots()
    {
        if (Main.gameMenu)
            return;

        if (Main.LocalPlayer == null || !Main.LocalPlayer.active)
            return;

        if (Main.spriteBatch == null)
            return;

        // Only show on the normal equipment page.
        if (Main.EquipPage != 0)
            return;

        float scale = Main.inventoryScale;

        /*
         * Vanilla equipment slots are positioned on the
         * right side of the inventory.
         */
        int x = Main.screenWidth - 64 - 28;

        /*
         * Vanilla equipment starts around Y = 174.
         */
        int baseY = 174;

        /*
         * Work out how many vanilla accessory slots
         * are currently visible.
         */
        int vanillaSlots =
            8 + Main.LocalPlayer.GetAmountOfExtraAccessorySlotsToShow();

        /*
         * Terraria moves the equipment area on smaller
         * screens when there are many accessory slots.
         */
        int vanillaExtraSpacing = 0;

        if (Main.screenHeight < 950 && vanillaSlots >= 10)
        {
            vanillaExtraSpacing = (int)(
                56f * scale * (vanillaSlots - 9)
            );
        }

        /*
         * Terraria's inventory slot is approximately
         * 56x56 at scale 1.
         *
         * We use this for mouse detection instead of
         * TextureAssets.InventoryBack, avoiding the
         * ReLogic Asset<> dependency.
         */
        int slotSize = (int)(56f * scale);

        for (int i = 0; i < ExtraSlotCount; i++)
        {
            /*
             * Put each extra slot directly underneath
             * the final vanilla accessory slot.
             */
            int slotNumber = vanillaSlots + i;

            int y = (int)(
                baseY +
                (slotNumber * 56f * scale)
            );

            y += vanillaExtraSpacing;

            Rectangle rectangle = new Rectangle(
                x,
                y,
                slotSize,
                slotSize
            );

            /*
             * Mouse interaction.
             */
            if (
                rectangle.Contains(
                    new Point(
                        Main.mouseX,
                        Main.mouseY
                    )
                )
                &&
                !PlayerInput.IgnoreMouseInterface
            )
            {
                Main.LocalPlayer.mouseInterface = true;

                /*
                 * Context 10 is the accessory-style
                 * equipment context used by Terraria.
                 *
                 * This allows:
                 * - placing accessories
                 * - removing accessories
                 * - swapping accessories
                 * - picking accessories back up
                 */
                ItemSlot.Handle(
                    extraSlots,
                    10,
                    i,
                    true
                );
            }

            /*
             * Draw the actual Terraria item slot.
             *
             * This also draws the accessory icon when
             * an item is inside the slot.
             */
            ItemSlot.Draw(
                Main.spriteBatch,
                extraSlots,
                10,
                i,
                new Vector2(
                    x,
                    y
                )
            );
        }
    }

    public void Unload()
    {
        if (harmony != null)
        {
            var drawInventory = AccessTools.Method(
                typeof(Main),
                "DrawInventory"
            );

            if (drawInventory != null)
            {
                harmony.Unpatch(
                    drawInventory,
                    HarmonyPatchType.Postfix,
                    "extra-accessory-slot"
                );
            }

            harmony = null;
        }

        extraSlots = new Item[0];
    }
}
