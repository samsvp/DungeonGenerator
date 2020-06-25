public class TrapRoom : Room
{
    public TrapRoom(int x, int y, Door door) : base(x, y, door)
    { 
        rT = RoomType.T;
        Repr();
    }
}