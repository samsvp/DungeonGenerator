using System;

public class VendorRoom : Room
{
    public VendorRoom(int x, int y, Door[] doors) : base(x, y, doors)
    { 
        rT = RoomType.V;
        Repr();
    }

}