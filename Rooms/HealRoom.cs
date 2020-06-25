public class HealRoom : Room
{
    public HealRoom(int x, int y, Door door) : base(x, y, door)
    { 
        rT = RoomType.H;
        Repr();
    }

}