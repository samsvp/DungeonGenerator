using System;

public class BossRoom : Room
{
    public BossRoom(int x, int y, Door door) : base(x, y, door)
    { 
        rT = RoomType.B;
        Repr();
    }

}