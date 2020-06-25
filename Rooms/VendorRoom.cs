using System;

public class VendorRoom : Room
{
    public VendorRoom(int x, int y, Door door) : base(x, y, door)
    { 
        rT = RoomType.V;
        Repr();
    }

}