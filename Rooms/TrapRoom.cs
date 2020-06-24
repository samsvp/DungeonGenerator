public class TrapRoom : Room
{
    public TrapRoom(int x, int y, Door[] doors) : base(x, y, doors)
    { 
        rT = RoomType.T;
        Repr();
    }
}