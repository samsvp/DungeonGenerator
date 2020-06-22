using System;
using System.Collections.Generic;
using System.Linq;

public class Dungeon
{

    public int[] size;
    public int minibossCount;
    public int vendorCount;
    public bool FOEs; // FOEs are elite enemies that pursuit the player through the levels

    public enum Focus
    {
        Combat,
        Exploration,
        CombatAndExploration
    };

    public enum EnemyFrequency
    {
        Low,
        Medium,
        High
    }

    public Focus focus;
    public EnemyFrequency enmFreq;

    Dungeon(int[] _size, int _minibossCount, int _vendorCount, 
            bool _FOEs, Focus _focus, EnemyFrequency _enmFreq)
    {
        size = _size;
        minibossCount = _minibossCount;
        vendorCount = _vendorCount;
        FOEs = _FOEs;
        focus = _focus;
        enmFreq = _enmFreq;
    }

    ///<sumary>
    /// Creates a map given a set of rooms. 
    /// It is assumed that all the rooms have the same size.
    /// A empty row means another floor.
    ///</sumary>
    public static string CreateMap(Room[] _rooms, int currentFloor=0)
    {
        if (_rooms.Length == 0) return "";
        
        Room[] roomStrings = _rooms.Where((room) => room.y == currentFloor).ToArray();

        int maxX = roomStrings.Length > 0 ? 
            Enumerable.Max(roomStrings.Select((room) => room.x)) : 0;
        // We assume that all rooms have the same size
        int maxXLength = _rooms[0].size[0];
        int maxYLength = _rooms[0].size[1] + 3;

        List<string[]> roomsToPrint = new List<String[]>();
        for (int i = 0; i <= maxX; i++)
        {
            string[] strs;
            try
                {strs = roomStrings.First((room) => room.x == i).ToString().Split('\n');}
            catch
                {
                    var voidRoom = new VoidRoom(i, currentFloor, new Room.Door[]{ });
                    voidRoom.SetSize(maxXLength, maxYLength);
                    strs = voidRoom.ToString().Split('\n');
                }
            roomsToPrint.Add(strs);
        }
        
        string str = "";
        for (int i=0; i < maxYLength - 1; i++)
        {
            foreach (string[] s in roomsToPrint) 
                if (i < s.Length) str += s[i];
            str += '\n';
        }

        //throw new Exception("");

        var remainingRooms = _rooms.Where((room) => room.y != currentFloor).ToArray();
        return str + CreateMap(remainingRooms, currentFloor + 1);
    }
    
}