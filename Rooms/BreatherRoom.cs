using System;

public class BreatherRoom : Room
{
    public BreatherRoom(int x, int y, Door[] doors) : base(x, y, doors)
    { 
        rT = RoomType.W;
        Repr();
    }

}