using Content.Client.UserInterface.Controls;
using Content.Shared.Hands.Components;
using static Content.Client.Inventory.ClientInventorySystem;

namespace Content.Client.UserInterface.Systems.Inventory.Controls;

public sealed class HUDHandButton : HUDSlotControl
{
    public HandLocation HandLocation { get; }

    public string HandName { get; }

    public HUDHandButton(string handName, HandLocation handLocation)
    {
        HandLocation = handLocation;
        Name = "hand_" + handName;
        HandName = handName;
        SlotName = handName;
        SetBackground(handLocation);


        switch (handLocation)
        {
            case HandLocation.Left:
                HoverName = HoverNamePrefix + "hand-left";
                break;
            case HandLocation.Middle:
                HoverName = HoverNamePrefix + "hand";
                break;
            case HandLocation.Right:
                HoverName = HoverNamePrefix + "hand-right";
                break;
        }
    }

    private void SetBackground(HandLocation handLoc)
    {
        ButtonTexturePath = handLoc switch
        {
            HandLocation.Left => "Slots/hand_l",
            HandLocation.Middle => "Slots/hand_m",
            HandLocation.Right => "Slots/hand_r",
            _ => ButtonTexturePath
        };
    }
}
