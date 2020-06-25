public class StairRoom : Room
{
    public StairRoom(int x, int y, Door door) : base(x, y, door)
    { 
        rT = RoomType.F;
        Repr();
    }
}