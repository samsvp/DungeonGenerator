public class PuzzleRoom : Room
{
    public PuzzleRoom(int x, int y, Door[] doors) : base(x, y, doors)
    { 
        rT = RoomType.P;
        Repr();
    }
}