using System.Collections.Generic;

public class SlotPopupManager
{
    private List<Slot> _slots = new();
    public void SetCurrentSlots(List<Slot> slots)
    {
        foreach (var slot in _slots)
        {
            slot.OnSelected -= SetCurrentSlot;
        }
        _slots = slots;
        foreach (Slot slot in _slots)
        {
            slot.OnSelected += SetCurrentSlot;
        }
    }

    public void SetCurrentSlot(Slot slot)
    {
        foreach (Slot s in _slots)
        {
            s.SetSlotActive(s == slot);
        }
    }
}