using System;
using System.Collections.Generic;
using System.Linq;

using Direction = Room.Directions;
using Map = System.Collections.Generic.Dictionary<int[], Room>;
public class Dungeon
{
    private static Random random = new Random();

    // The first number of the int array dictates x coordinate size, 
    // the second the y coordinate and the third the z coordinate.
    // Each array is a new floor.
    public int[] mapSize;
    private Map mainPath;
    public Map map;
    public LinkedList<int[]> mainPathCoords;
    public List<LinkedList<int[]>> forksCoords = new List<LinkedList<int[]>>();


    private int minibossCount;
    private int vendorCount;
    private int keyCount;
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

    private Focus focus;
    private EnemyFrequency enmFreq;

    public Dungeon(int _x, int _y, int _keyCount, int _minibossCount, int _vendorCount, 
            bool _FOEs, Focus _focus, EnemyFrequency _enmFreq)
    {
        mapSize = new int[] {_x, _y};
        keyCount = _keyCount;
        minibossCount = _minibossCount;
        vendorCount = _vendorCount;
        FOEs = _FOEs;
        focus = _focus;
        enmFreq = _enmFreq;
        map = new Map(new MyEqualityComparer());
        mainPath = new Map(new MyEqualityComparer());
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
                    var voidRoom = new VoidRoom(i, currentFloor, new Door());
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

        path = ShortestPath(initialLocation, goalLocation);

        return path;
    }

    ///<summary>
    /// Creates a fork in a region between locked doors 
    ///</summary>
    public LinkedList<int[]> CreateFork(LinkedList<int[]> mainPath, List<int[]> lockedDoorLocations,
                                        int region, int forks=1)
    {
        var fork = new LinkedList<int[]>();

        int[] startCoords = region == 0 ? mainPath.First.Value : 
            mainPath.FirstOrDefault( p => Enumerable.SequenceEqual(p, lockedDoorLocations[region - 1] ));
        int[] endCoords =  region == keyCount ? mainPath.Last.Value :
            mainPath.FirstOrDefault( p => Enumerable.SequenceEqual(p, lockedDoorLocations[region]));
        

        var mainPathList = mainPath.ToList();
        List<int[]> perimeter = mainPathList.GetRange(
            mainPathList.IndexOf(startCoords) + 1,
            mainPathList.IndexOf(endCoords) - mainPathList.IndexOf(startCoords));

        int forkStartIndex = random.Next((int)(0.1f * perimeter.Count), (int)(0.35f * perimeter.Count));
        int forkEndIndex = random.Next((int)(0.8f * perimeter.Count), perimeter.Count - 1);
        

        int[] forkStartCoords = perimeter[forkStartIndex];
        int[] forkEndCoords = perimeter[forkEndIndex];

        fork = ForkSquarePath(forkStartCoords, forkEndCoords, perimeter.GetRange(forkStartIndex, 
                                                                forkEndIndex - forkStartIndex));

        forksCoords.Add(fork);

        return fork;
    }

    private LinkedList<int[]> ShortestPath(int[] startCoords, int[] endCoords)
    {
        LinkedList<int[]> path = new LinkedList<int[]>();

        // Set current room location variables
        int last_x = startCoords[0];
        int last_y = startCoords[1];
        int x = last_x;
        int y = last_y;

        int[] currentPosition = new int[2];

        path.AddLast(startCoords);

        // Start creating the main path
        while(x != endCoords[0] || y != endCoords[1])
        {
            int moveXorY;
            if (x != endCoords[0] && y != endCoords[1]) moveXorY = random.Next(2);
            else if (x != endCoords[0]) moveXorY = 0;
            else moveXorY = 1;

            if (moveXorY == 0) x = last_x < endCoords[0] ? last_x + 1 : last_x - 1;
            else y = last_y < endCoords[1] ? last_y + 1 : last_y - 1;

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

    private LinkedList<int[]> ForkSquarePath(int[] startCoords, int[] endCoords, List<int[]> avoidPerimeter)
    {
        LinkedList<int[]> path = new LinkedList<int[]>();

        if (avoidPerimeter.Count < 2) return path;

        int x = startCoords[0];
        int y = startCoords[1];

        bool addToX = x < endCoords[0];
        bool addToY = y < endCoords[1];

        path.AddFirst(startCoords);

        Console.WriteLine("Start coords: " + startCoords[0] + ", " + startCoords[1]);
        Console.WriteLine("End coords: " + endCoords[0] + ", " + endCoords[1]);

        bool moveX = y != avoidPerimeter[1][1];
        bool moveY = x != avoidPerimeter[1][0];

        while(x != endCoords[0] || y != endCoords[1])
        {
            if (moveY)
            {
                while (y != endCoords[1]) path.AddLast(new int[] {x, addToY ? ++y : --y});
                while (x != endCoords[0]) path.AddLast(new int[] {addToX ? ++x : --x, y});
            }
            else if (moveX)
            {
                while (x != endCoords[0]) path.AddLast(new int[] {addToX ? ++x : --x, y});
                while (y != endCoords[1]) path.AddLast(new int[] {x, addToY ? ++y : --y});
            }
        }

        return path;
    } 

    public Map CreatePathMap(LinkedList<int[]> path)
    {
        Map map = new Map(new MyEqualityComparer());
   
        for(LinkedListNode<int[]> it = path.First; it != null;)
        {
            var key = new int[] { it.Value[0], it.Value[1] };
            ////////////////////////////////////
            /// COMBAT DOOR IS A PLACEHOLDER ///
            ////////////////////////////////////
            map[key] = CreateMainPathDoors(it, new CombatRoom(it.Value[0], it.Value[1], new Door()));
            it = it.Next;
        }

        return map;
    }

    ///<summary>
    /// Adds doors to room based on the previous and last rooms.
    ///</summary>
    private Room CreateMainPathDoors(LinkedListNode<int[]> coordinates, Room room)
    {
        var previous = coordinates.Previous;
        var next = coordinates.Next;

        Direction previousDirection;
        Direction nextDirection;
        
        Func<LinkedListNode<int[]>, LinkedListNode<int[]>, Direction> getDirection = (current, sequence) => {
            Direction d;

            if (current.Value[0] != sequence.Value[0]) 
                d = current.Value[0] < sequence.Value[0] ? Direction.E : Direction.W;
            else d = current.Value[1] < sequence.Value[1] ? Direction.S : Direction.N;

            return d;
        };

        if (previous != null) 
        {
            previousDirection = getDirection.Invoke(coordinates, previous);
            room.SetDoorInDirection(previousDirection);
        }
        if (next != null)
        {
            nextDirection = getDirection.Invoke(coordinates, next);
            room.SetDoorInDirection(nextDirection);
        }

        return room;
    }

    public void SetLockedDoors(Map map, List<int[]> lockedDoorLocations)
    {
        foreach (var location in lockedDoorLocations)
        {
            Room room = map[location];
            List<Direction> directions = room.doors.direction;
            Direction direction = directions.ElementAt(random.Next(directions.Count));

            switch (direction)
            {
                case Direction.N:
                    room.doors.lockN = '-';
                    break;

                case Direction.S:
                    room.doors.lockS = '-';
                    break;
                    
                case Direction.E:
                    room.doors.lockE = '|';
                    break;
                
                case Direction.W:
                    room.doors.lockW = '|';
                    break;

                default:
                    break;
            }
            room.SetDoorInDirection(direction);  
        }
    }

    public List<int[]> SetLockedDoorsLocations(LinkedList<int[]> path)
    {
        List<int[]> lockedDoorLocations = new List<int[]>();

        if (keyCount == 0) return lockedDoorLocations;

        int spacing = path.Count / keyCount;

        for (int i=0; i<keyCount; i++)
        {
            int index = random.Next(spacing * i , spacing * (i + 1) ); 
            var _location = path.ElementAt(index);
            int[] location = new int[] { _location[0], _location[1] };
            lockedDoorLocations.Add(location);
        }

        return lockedDoorLocations;
    }

    public List<int[]> SetKeyLocations(LinkedList<int[]> path, List<int[]> lockedDoorLocations)
    {
        List<int[]> keyLocations = new List<int[]>();

        if (lockedDoorLocations.Count == 0) return keyLocations;

        for (int i = 0; i < lockedDoorLocations.Count; i++)
        {
            // Limit determines which element of the list can be reached without the key
            int limit = path.TakeWhile(p => p != lockedDoorLocations[i]).Count();
            var _location = path.ElementAt(random.Next(limit));
            int[] location = new int[] { _location[0], _location[1] };
            keyLocations.Add(location);
        }

        return keyLocations;
    }

    public Room CreateRoom(int[] coordinates, Room.RoomType prevRoomType)
    {
        return new CombatRoom(coordinates[0], coordinates[1], new Door());
    }
}