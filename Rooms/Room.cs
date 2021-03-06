using System;
using System.Collections.Generic;
using System.Linq;


public abstract class Room
{
    public int x;
    public int y;
    public Interactable[] inters;

    public string repr = "";
    public int[] size = new int[2] {5, 5};

    public enum Directions
    { N,S,E,W };
    public enum RoomType
    { 
        B, // Boss
        W, // Breather
        C, // Combat
        V, // Vendor
        S, // Initial
        H, // Heal
        T, // Trap
        M, // Bank
        P, // Puzzle
        I, // Item
        F // Floor Change
    };
    public Door doors;
    public RoomType rT;

    public Room(int _x, int _y, Door _doors)
    {
        x = _x;
        y = _y;
        doors = _doors;
    }

    public Room(int _x, int _y, Door _doors, int _sizeX, int _sizeY)
    {
        x = _x;
        y = _y;
        doors = _doors;
        size[0] = _sizeX;
        size[1] = _sizeY;
        Repr();
    }

    private Directions[] GetAdjacentRoomDirections(Room room)
    {
        Directions thisDirection;
        Directions otherDirection;

        if (room.y == y)
        {
            int dif = room.x - x;
            if (Math.Abs(dif) != 1) throw new Exception("Can only merge adjacent rooms");

            if (dif == 1)
            {
                thisDirection = Directions.E;
                otherDirection = Directions.W;
            }
            else
            {
                thisDirection = Directions.W;
                otherDirection = Directions.E;
            }
        }
        else if (room.x == x)
        {
            int dif = room.y - y;
            if (Math.Abs(dif) != 1) throw new Exception("Can only merge adjacent rooms");
            
            if (room.size[0] < size[0]) room.SetSize(size[0], room.size[1]);
            else if (room.size[0] > size[0]) SetSize(room.size[0], size[1]);
            
            if (dif == 1)
            {
                thisDirection = Directions.S;
                otherDirection = Directions.N;
            }
            else
            {
                thisDirection = Directions.N;
                otherDirection = Directions.S;
            }
        }
        else throw new Exception("Can only merge adjacent rooms");
        return new Directions[] { thisDirection, otherDirection };
    }

    public virtual void MergeRooms(Room room)
    {
        Directions[] directions = GetAdjacentRoomDirections(room);
        
        Directions thisRemoveDirection = directions[0];
        Directions roomRemoveDirection = directions[1];
        
        RemoveWalls(thisRemoveDirection);
        room.RemoveWalls(roomRemoveDirection);
    }


    public void RemoveWalls(Directions dir)
    {
        string[] str = repr.Split('\n', StringSplitOptions.RemoveEmptyEntries);
        
        switch (dir)
        {
            case Directions.N:
                str[0] = '#' + new String(' ', size[0]) + '#';
                break;

            case Directions.S:
                str[str.Length - 1] = '#' + new String(' ', size[0]) + '#';
                break;

            case Directions.W:
                for (int i=1; i<str.Length-1; i++) str[i] = ' ' + str[i].Substring(1);
                break;

            case Directions.E:
                for (int i=1; i<str.Length-1; i++) str[i] = str[i].Substring(0, str.Length - 1) + ' ';
                break;

            default:
                break;
        }
        repr = "";
        foreach(string s in str) repr += s + '\n';
    }

    public void SetDoorInDirection(Directions direction)
    {
        doors.direction.Add(direction);
        Repr();
    }

    public void SetSize(int _x, int _y)
    {
        SetSize(new int[] {_x, _y});
    }

    public void SetSize(int[] _size)
    {
        if (size.Length != 2) throw new System.Exception("Invalid array size. Enter an array of size 2.");
        if ((size[0] < 5) || (size[1] < 5)) throw new System.Exception("Room dimensions are too small");
        size = _size;
        Repr(); // Updates room visualization
    }


    ///<summary>
    /// Sets objects that the player can interact with inside the room
    ///</summary>
    public virtual void SetInteractables()
    {

    }


    ///<summary>
    /// Set obstacles inside the room
    ///</summary>
    public virtual void SetObstacles()
    {

    }

    ///<summary>
    /// Creates a string representation of the Room
    ///</summary>
    protected virtual string Repr()
    {
        int offset = size[0] % 2;
        string empty = new String(' ', size[0]);
        repr = "";

        repr += doors.direction.Contains(Directions.N) ? 
            new String('#', (size[0]) / 2) + new String(doors.lockN, 2) + new String('#', (size[0] + 2 )/ 2 + (offset - 1)) + '\n' :
            new String('#', size[0] + 2) + '\n';
        
        for(int i=0; i < size[1] / 2; i++) repr += '#' + empty + "#\n";

        repr += doors.direction.Contains(Directions.W) ? new String(doors.lockW, 1) : "#";
        repr += doors.direction.Contains(Directions.E) ? 
            new String(' ', size[0] / 2) + rT + new String(' ', size[0] / 2) + doors.lockE + '\n' : 
            new String(' ', size[0] / 2) + rT + new String(' ', size[0] / 2 + 1 - offset) + "#\n";
        
        for(int i=0;i < (size[1] + (offset - 1)) /2 ; i++) repr += '#' + empty + "#\n";

        repr += doors.direction.Contains(Directions.S) ? 
            new String('#', (size[0]) / 2) + new String(doors.lockS, 2) + new String('#', (size[0] + 2 )/ 2 + (offset - 1)) + '\n' :
            new String('#', size[0] + 2) + '\n';
        return repr;
    }

    public string GetRepr()
    {
        return Repr();
    }

    public override string ToString()
    {
        return repr;
    }
}
