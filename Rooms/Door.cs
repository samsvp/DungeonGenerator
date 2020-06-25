using System.Collections.Generic;

using Direction = Room.Directions;
public class Door
{
    public List<Direction> direction;
    public static Direction N;
    public static Direction S;
    public static Direction E;
    public static Direction W;
    public char lockN = ' ';
    public char lockS = ' ';
    public char lockE = ' ';
    public char lockW = ' ';
    public Door()
    {
        direction = new List<Direction>();
    }
    public Door(List<Direction> _direction)
    {
        direction = _direction;
    }

}