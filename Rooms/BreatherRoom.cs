using System;

public class BreatherRoom : Room
{
    public BreatherRoom(int x, int y, Door door) : base(x, y, door)
    { 
        rT = RoomType.W;
        Repr();
    }

}