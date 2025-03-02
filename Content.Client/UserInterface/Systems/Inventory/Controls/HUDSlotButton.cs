using Content.Client.UserInterface.Controls;
using static Content.Client.Inventory.ClientInventorySystem;

namespace Content.Client.UserInterface.Systems.Inventory.Controls;

public sealed class HUDSlotButton : HUDSlotControl
{
    public HUDSlotButton(SlotData slotData)
    {
        ButtonTexturePath = slotData.TextureName;
        FullButtonTexturePath = slotData.FullTextureName;
        Blocked = slotData.Blocked;
        Highlight = slotData.Highlighted;
        SlotName = slotData.SlotName;
        HoverName = HoverNamePrefix + slotData.SlotName;
    }
}
