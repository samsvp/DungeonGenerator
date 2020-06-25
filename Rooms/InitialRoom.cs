using System;

public class InitialRoom : Room
{
    public InitialRoom(int x, int y, Door door) : base(x, y, door)
    { 
        rT = RoomType.S;
        Repr();
    }

}