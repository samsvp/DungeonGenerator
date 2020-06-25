public class BankRoom : Room
{
    public BankRoom(int x, int y, Door door) : base(x, y, door)
    { 
        rT = RoomType.M;
        Repr();
    }
}