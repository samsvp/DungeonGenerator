public class HealRoom : Room
{
    public HealRoom(int x, int y, Door[] doors) : base(x, y, doors)
    { 
        rT = RoomType.H;
        Repr();
    }

}