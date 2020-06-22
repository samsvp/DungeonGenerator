using System;

public class BossRoom : Room
{
    public BossRoom(int x, int y, Door[] doors) : base(x, y, doors)
    { 
        rT = RoomType.B;
        Repr();
    }

}