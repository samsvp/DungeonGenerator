public class BankRoom : Room
{
    public BankRoom(int x, int y, Door[] doors) : base(x, y, doors)
    { 
        rT = RoomType.M;
        Repr();
    }
}