public class PuzzleRoom : Room
{
    public PuzzleRoom(int x, int y, Door door) : base(x, y, door)
    { 
        rT = RoomType.P;
        Repr();
    }
}