using System;

public class InitialRoom : Room
{
    public InitialRoom(int x, int y, Door[] doors) : base(x, y, doors)
    { 
        rT = RoomType.I;
        Repr();
    }

}