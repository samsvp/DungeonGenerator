public class ItemRoom : Room
{
    public ItemRoom(int x, int y, Door[] doors) : base(x, y, doors)
    { 
        rT = RoomType.I;
        Repr();
    }
}