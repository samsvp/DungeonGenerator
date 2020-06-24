using System;
using System.Collections.Generic;
using System.Linq;

using Door = Room.Door;
using Direction = Room.Directions;
using Map = System.Collections.Generic.Dictionary<System.Collections.Generic.List<int>, Room>;
public class Dungeon
{
    private static Random random = new Random();

    // The first number of the int array dictates x coordinate size, 
    // the second the y coordinate and the third the z coordinate.
    // Each array is a new floor.
    public List<int[]> mapSize;
    private Map[] mainPaths;
    private Map[] map;
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

    public Dungeon(List<int[]> _size, int _minibossCount, int _vendorCount, 
            bool _FOEs, Focus _focus, EnemyFrequency _enmFreq)
    {
        if (_size.Count == 1 && _size[0].Length != 2 && _size[0].Length != 3)
            throw new Exception("Dungeons with one floor need at least x and y coordnates.");
        else foreach (int[] floor in _size) 
            if(floor.Length != 3) throw new Exception("Floor needs an x,y and z coordinates");

        mapSize = _size;
        minibossCount = _minibossCount;
        vendorCount = _vendorCount;
        FOEs = _FOEs;
        focus = _focus;
        enmFreq = _enmFreq;
        map = new Map[_size.Count];
        mainPaths = new Map[_size.Count];
    }

    public Dungeon(int _x, int _y, int _minibossCount, int _vendorCount, 
            bool _FOEs, Focus _focus, EnemyFrequency _enmFreq)
    {
        mapSize = new List<int[]>() {new int[] {_x, _y}};
        minibossCount = _minibossCount;
        vendorCount = _vendorCount;
        FOEs = _FOEs;
        focus = _focus;
        enmFreq = _enmFreq;
        map = new Map[mapSize.Count];
        mainPaths = new Map[mapSize.Count];
    }

    ///<summary>
    /// Creates a map given a set of rooms. 
    /// It is assumed that all the rooms have the same size.
    /// A empty row means another floor.
    ///</summary>
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
                    var voidRoom = new VoidRoom(i, currentFloor, new Door[]{ });
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

        var remainingRooms = _rooms.Where((room) => room.y != currentFloor).ToArray();
        return str + CreateMap(remainingRooms, currentFloor + 1);
    }

    ///<summary>
    /// Creates the main path of the floor.
    /// Initial position will be the place where the player spawns
    /// or where the stairway of the last floor was.
    /// If it is the main boss floor then it will spawn a boss room 
    /// at the end.abstract Otherwise it will spawn a stairway
    ///</summary>
    public LinkedList<int[]> CreateMainPath(int[] floorSize, int init_x=int.MaxValue, int init_y=int.MaxValue)
    {
        LinkedList<int[]> path = new LinkedList<int[]> ();

        // Available locations to store the rooms
        List<int[]> availableLocations = new List<int[]>();

        for (int i=1; i<floorSize[0]; i++)
        for (int j=1; j<floorSize[1]; j++)
        availableLocations.Add(new int[] {i,j});

        // Set initial Location
        int[] initialLocation = new int[2];
        // Boss or stair location
        int[] goalLocation = new int[2];

        initialLocation[0] = init_x == int.MaxValue ? random.Next(floorSize[0]) : init_x;
        // Make the goal not so close from the spawn point
        goalLocation[0] = initialLocation[0] + floorSize[0] / 3 + random.Next(floorSize[0] / 2);
        if (goalLocation[0] > floorSize[0])  goalLocation[0] -= floorSize[0];

        if (init_y == int.MaxValue)
        {
            initialLocation[1] = 0;
            goalLocation[1] = floorSize[1];
        }
        else 
        {
            initialLocation[1] = init_y;
            goalLocation[1] = 0;
        }

        // Set current room location variables
        int last_x = initialLocation[0];
        int last_y = initialLocation[1];
        int x = last_x;
        int y = last_y;
        int[] currentPosition = new int[2];
        path.AddLast(initialLocation);
        // Start creating the main path
        while(x != goalLocation[0] || y != goalLocation[1])
        {
            int moveXorY;
            if (x != goalLocation[0] && y != goalLocation[1]) moveXorY = random.Next(2);
            else if (x != goalLocation[0]) moveXorY = 0;
            else moveXorY = 1;

            if (moveXorY == 0) x = last_x < goalLocation[0] ? last_x + 1 : last_x - 1;
            else y = last_y < goalLocation[1] ? last_y + 1 : last_y - 1;

            // Set the current possition door that links it to the last position door
            currentPosition[0] = x;
            currentPosition[1] = y;
            path.AddLast(new int[] {x,y});
            // Update variables
            last_x = x;
            last_y = y;
        }

        return path;
    }
    

    public Room CreateRoom(int[] coordinates, Room.RoomType prevRoomType)
    {
        return new CombatRoom(coordinates[0], coordinates[1], new Door[] {});
    }

    ///<summary>
    /// Adds doors to room based on the previous and last rooms.
    ///</summary>
    public Room CreateDoors(LinkedListNode<int[]> coordinates, Room room)
    {
        var previous = coordinates.Previous;
        var next = coordinates.Next;

        Door previousDoor;
        Door nextDoor;
        
        Func<LinkedListNode<int[]>, LinkedListNode<int[]>, Door> getDoor = (current, sequence) => {
            Door door;

            if (current.Value[0] != sequence.Value[0]) 
                door = current.Value[0] < sequence.Value[0] ? Door.E : Door.W;
            else door = current.Value[1] < sequence.Value[1] ? Door.S : Door.N;

            return door;
        };

        if (previous != null) 
        {
            previousDoor = getDoor.Invoke(coordinates, previous);
            room.doors.Add(previousDoor);
        }
        if (next != null)
        {
            nextDoor = getDoor.Invoke(coordinates, next);
            room.doors.Add(nextDoor);
        } 

        room.GetRepr();

        return room;
    }


}