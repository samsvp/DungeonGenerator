public class StairRoom : Room
{
    public StairRoom(int x, int y, Door[] doors) : base(x, y, doors)
    { 
        rT = RoomType.F;
        Repr();
    }
}