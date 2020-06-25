public class ItemRoom : Room
{
    public ItemRoom(int x, int y, Door door) : base(x, y, door)
    { 
        rT = RoomType.I;
        Repr();
    }
}