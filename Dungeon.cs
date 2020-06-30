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
    private Map mainMap;
    public List<Map> forksMap;
    public List<Map> loopsMap;
    public LinkedList<int[]> mainPath = new LinkedList<int[]>();
    public List<LinkedList<int[]>> forksPath = new List<LinkedList<int[]>>();
    public List<LinkedList<int[]>> loopsPath = new List<LinkedList<int[]>>();


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

    public Dungeon(int _keyCount)
    {
        keyCount = _keyCount;
        loopsMap = new List<Map>();
        forksMap = new List<Map>();
        mainMap = new Map(new MyEqualityComparer());
    }

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
        loopsMap = new List<Map>();
        forksMap = new List<Map>();
        mainMap = new Map(new MyEqualityComparer());
    }

    ///<summary>
    /// Creates a map given a set of rooms. 
    /// It is assumed that all the rooms have the same size.
    /// A empty row means another floor.
    ///</summary>
    public static string CreateMap(Room[] _rooms, int currentFloor=0)
    {
        if (_rooms.Length == 0) return "";
        Console.WriteLine("currentFloor: " + currentFloor);
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

        var remainingRooms = _rooms.Where((room) => room.y >= currentFloor).ToArray();
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
        this.mainPath = path;

        return path;
    }

    ///<summary>
    /// Creates a fork in a region between locked doors 
    ///</summary>
    public LinkedList<int[]> CreateFork(LinkedList<int[]> mainPath, List<int[]> lockedDoorLocations,
                                        int region)
    {
        var fork = new LinkedList<int[]>();

        List<int[]> perimeter = GetRegion(mainPath, lockedDoorLocations, region);

        int forkStartIndex = random.Next((int)(0.1f * perimeter.Count), (int)(0.35f * perimeter.Count));
        int forkEndIndex = random.Next((int)(0.8f * perimeter.Count), perimeter.Count - 1);

        int[] forkStartCoords = perimeter[forkStartIndex];
        int[] forkEndCoords = perimeter[forkEndIndex];

        fork = ForkSquarePath(forkStartCoords, forkEndCoords, perimeter.GetRange(forkStartIndex, 
                                                                forkEndIndex - forkStartIndex));

        forksPath.Add(fork);

        return fork;
    }

    ///<summary>
    /// Creates a loop for a room inside a region
    ///</summary>
    public LinkedList<int[]> CreateLoop(LinkedList<int[]> mainPath, List<LinkedList<int[]>> forksPath,
                                        List<int[]> lockedDoorLocations, int region, int count=0)
    {
        var loop = new LinkedList<int[]>();

        List<int[]> perimeter = GetRegion(mainPath, lockedDoorLocations, region);

        Func<List<int[]>, List<int[]>, List<int[]>> GetPossibleLoopRooms = (mPathCoords, aPathCoords) =>
        {   
            List<int[]> mPossibleRooms = new List<int[]>();

            mPossibleRooms = mPathCoords.Where(m => {
                var roomDirectionsX = new int[][] 
                { new int[] {m[0] + 1, m[1]}, new int[] {m[0] - 1, m[1]} };
                var roomDirectionsY = new int[][] 
                { new int[] {m[0], m[1] + 1}, new int[] {m[0], m[1] - 1} };

                int counterX = 0;
                int counterY = 0;
                foreach (int[] a in aPathCoords)
                {
                    if (Enumerable.SequenceEqual(a, roomDirectionsX[0])) counterX++;
                    else if (Enumerable.SequenceEqual(a, roomDirectionsX[1])) counterX++;
                    if (Enumerable.SequenceEqual(a, roomDirectionsY[0])) counterY++;
                    else if (Enumerable.SequenceEqual(a, roomDirectionsY[1])) counterY++;
                    if (counterX == 2 || counterY == 2) return false;
                } 
                return true;
            }).ToList();
            
            return mPossibleRooms;
        };
        
        List<int[]> forksList = forksPath.SelectMany(f => f.ToList()).ToList();
        List<int[]> avoidPerimeter = forksList.Concat(mainPath).ToList();
        var possibleRooms = GetPossibleLoopRooms.Invoke(perimeter, avoidPerimeter);

        if (possibleRooms.Count == 0) return loop;

        int[] index = possibleRooms[random.Next(possibleRooms.Count)];
        loop = LoopPath(index, avoidPerimeter);
        loopsPath.Add(loop);

        return loop;
    }

    #region Paths
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

        int x = startCoords[0];
        int y = startCoords[1];

        bool addToX = x < endCoords[0];
        bool addToY = y < endCoords[1];

        path.AddFirst(startCoords);

        bool moveX = y != avoidPerimeter[1][1];
        bool moveY = x != avoidPerimeter[1][0];

        if (moveY)
        {
            while (y != endCoords[1]) 
                path.AddLast(new int[] {x, addToY ? ++y : --y});
            while (x != endCoords[0])
            {
                path.AddLast(new int[] {addToX ? ++x : --x, y});
                if (avoidPerimeter.Any((a) => Enumerable.SequenceEqual(a, path.Last.Value)))
                    break;
            } 
        }       
                    
        else if (moveX)
        {
            while (x != endCoords[0]) 
                path.AddLast(new int[] {addToX ? ++x : --x, y});
            while (y != endCoords[1])
            {
                path.AddLast(new int[] {x, addToY ? ++y : --y});
                if (avoidPerimeter.Any((a) => Enumerable.SequenceEqual(a, path.Last.Value)))
                    break;
            }
        }

        return path;
    }

    private LinkedList<int[]> LoopPath(int[] startCoords, List<int[]> avoidPerimeter)
    {
        var path = new LinkedList<int[]>();

        // These are the points where the loop will change direction
        List<int[]> keyPoints = new List<int[]>();

        int maxX = random.Next(2,5);
        int maxY = random.Next(2,5);

        int[] endCoords = new int[2];

        path.AddFirst(startCoords);

        var right = new int[] { startCoords [0] + 1, startCoords[1] };
        bool goRight = !avoidPerimeter.Any(p => Enumerable.SequenceEqual(p, right));
        endCoords[0] = goRight ? startCoords[0] + maxX : startCoords[0] - maxX;

        while (endCoords[0] != path.Last.Value[0])
        {
            var coords = goRight ? new int[] { path.Last.Value[0] + 1, path.Last.Value[1]} : 
                                   new int[] { path.Last.Value[0] - 1, path.Last.Value[1]};
            if (avoidPerimeter.Any((a) => 
                Enumerable.SequenceEqual(a, new int[] {coords[0], startCoords[1]}) ||
                Enumerable.SequenceEqual(a, coords))) 
            {
                break;
            } else path.AddLast(coords);
        }
        var up = new int[] { startCoords [0], startCoords[1] + 1};
        bool goUp = !avoidPerimeter.Any(p => Enumerable.SequenceEqual(p, up));
        endCoords[1] = goUp ? startCoords[1] + maxY : startCoords[1] - maxY;

        while (endCoords[1] != path.Last.Value[1])
        {
            var coords = goUp ? new int[] { path.Last.Value[0], path.Last.Value[1] + 1} : 
                                new int[] { path.Last.Value[0], + path.Last.Value[1] - 1};
            if (avoidPerimeter.Any((a) => 
                Enumerable.SequenceEqual(a, new int[] {startCoords[0], coords[1]}) ||
                Enumerable.SequenceEqual(a, coords))) 
            {
                break;
            } else path.AddLast(coords);
        }
        
        while (path.Last.Value[0] != path.First.Value[0])
        {
            var coords = goRight ? new int[] { path.Last.Value[0] - 1, path.Last.Value[1]} : 
                                   new int[] { path.Last.Value[0] + 1, path.Last.Value[1]};
            path.AddLast(coords);
        }
        while (path.Last.Value[1] != path.First.Value[1])
        {
            var coords = goUp ? new int[] { path.Last.Value[0], path.Last.Value[1] - 1} : 
                                new int[] { path.Last.Value[0], + path.Last.Value[1] + 1};
            path.AddLast(coords);
        }
        
        return path;
    }

    #endregion Paths

    #region Maps
    
    public Map CreateMainPathMap(LinkedList<int[]> path)
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
        this.mainMap = map;

        return map;
    }

    public Map[] CreateForkMap(LinkedList<int[]> forkPath, LinkedList<int[]> mainPath, Map mainMap)
    {
        Map map = new Map(new MyEqualityComparer());

        int[] startCoords = mainPath.First((kp) => Enumerable.SequenceEqual(kp, forkPath.First.Value));
        map[startCoords] = mainMap[startCoords];

        mainMap[startCoords] = 
            JoinRooms(forkPath.First.Next, mainPath.Find(forkPath.First.Value), mainMap[startCoords]);

        for(LinkedListNode<int[]> it = forkPath.First.Next; it != forkPath.Last;)
        {
            var key = new int[] { it.Value[0], it.Value[1] };
            ////////////////////////////////////
            /// COMBAT DOOR IS A PLACEHOLDER ///
            ////////////////////////////////////
            map[key] = CreateMainPathDoors(it, new VendorRoom(it.Value[0], it.Value[1], new Door()));
            it = it.Next;
        }

        int[] endCoords = forkPath.Last.Value;
        int[] endCoordsPrev = forkPath.Last.Previous.Value;

         mainMap[endCoords] = 
            JoinRooms(forkPath.Last.Previous, forkPath.Last, mainMap[endCoords]);

        this.forksMap.Add(map);
        this.mainMap = mainMap;

        return new Map[] {map, mainMap};
    }

    public Map[] CreateLoopMap(LinkedList<int[]> loopPath, LinkedList<int[]> mainPath, Map mainMap)
    {
        // Although the code is the same as the fork map function. the rooms logic
        // will change later

        Map map = new Map(new MyEqualityComparer());

        if (loopPath.Count == 0) return new Map[] {map, mainMap};

        int[] startCoords = mainPath.First((kp) => Enumerable.SequenceEqual(kp, loopPath.First.Value));
        map[startCoords] = mainMap[startCoords];

        mainMap[startCoords] = 
            JoinRooms(loopPath.First, mainPath.Find(loopPath.First.Value), mainMap[startCoords]);

        for(LinkedListNode<int[]> it = loopPath.First.Next; it != loopPath.Last;)
        {
            var key = new int[] { it.Value[0], it.Value[1] };
            ////////////////////////////////////
            /// COMBAT DOOR IS A PLACEHOLDER ///
            ////////////////////////////////////
            map[key] = CreateMainPathDoors(it, new HealRoom(it.Value[0], it.Value[1], new Door()));
            it = it.Next;
        }

        int[] endCoords = loopPath.Last.Value;
        int[] endCoordsPrev = loopPath.Last.Previous.Value;

        mainMap[endCoords] = 
            JoinRooms(loopPath.Last.Previous, loopPath.Last, mainMap[endCoords]);

        this.loopsMap.Add(map);
        this.mainMap = mainMap;

        return new Map[] {map, mainMap};
    }

    #endregion Maps

    #region Doors

    ///<summary>
    /// Adds doors to room based on the previous and last rooms.
    ///</summary>
    private Room CreateMainPathDoors(LinkedListNode<int[]> coordinates, Room room)
    {
        var previous = coordinates.Previous;
        var next = coordinates.Next;

        Direction previousDirection;
        Direction nextDirection;
        
        if (previous != null) 
        {
            previousDirection = GetDoorDirection(coordinates, previous);
            room.SetDoorInDirection(previousDirection);
        }
        if (next != null)
        {
            nextDirection = GetDoorDirection(coordinates, next);
            room.SetDoorInDirection(nextDirection);
        }

        return room;
    }

    private Direction GetDoorDirection(LinkedListNode<int[]> node1, LinkedListNode<int[]> node2)
    {
            Direction d;

            if (node1.Value[0] != node2.Value[0]) 
                d = node1.Value[0] < node2.Value[0] ? Direction.E : Direction.W;
            else 
                d = node1.Value[1] < node2.Value[1] ? Direction.S : Direction.N;

            return d;
    }


    public void SetLockedDoors(Map map, List<int[]> lockedDoorLocations)
    {
        foreach (var location in lockedDoorLocations)
        {
            Room room = map[location];
            List<Direction> directions = room.doors.direction;
            Direction direction = directions.ElementAt(random.Next(directions.Count));
            map[location] = SetLockedDoor(room, direction);
        }
    }

    private Room SetLockedDoor(Room room, Direction direction)
    {
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
        return room;
    }

    public List<int[]> SetLockedDoorsLocations(LinkedList<int[]> path)
    {
        List<int[]> lockedDoorLocations = new List<int[]>();

        if (keyCount == 0) return lockedDoorLocations;

        int spacing = path.Count / keyCount;

        if (spacing < 10) throw new Exception("Path is too small for this key count");

        for (int i=0; i<keyCount; i++)
        {
            int index = random.Next(spacing * i , spacing * (i + 1) );
            if (index < 5) index += 5; 
            var _location = path.ElementAt(index);
            int[] location = new int[] { _location[0], _location[1] };
            lockedDoorLocations.Add(location);
        }

        return lockedDoorLocations;
    }

    #endregion Doors

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

    #region Rooms

    private Room JoinRooms(LinkedListNode<int[]> node1,
                 LinkedListNode<int[]> node2, Room room)
    {
        Direction direction = GetDoorDirection(node2, node1);
        room.SetDoorInDirection(direction);

        return room;
    }

    public Room CreateRoom(int[] coordinates, Room.RoomType prevRoomType)
    {
        return new CombatRoom(coordinates[0], coordinates[1], new Door());
    }

    #endregion Rooms

    ///<summary>
    /// Returns a list of coordinates contained in a given region.
    /// A region is the perimeter between locked doors
    ///</summary>
    private List<int[]> GetRegion(LinkedList<int[]> mainPath, List<int[]> lockedDoorLocations,
                                        int region)
    {
        int[] startCoords = region == 0 ? mainPath.First.Value : 
            mainPath.FirstOrDefault( p => Enumerable.SequenceEqual(p, lockedDoorLocations[region - 1] ));
        int[] endCoords =  region == keyCount ? mainPath.Last.Value :
            mainPath.FirstOrDefault( p => Enumerable.SequenceEqual(p, lockedDoorLocations[region]));
        

        var mainPathList = mainPath.ToList();
        List<int[]> perimeter = mainPathList.GetRange(
            mainPathList.IndexOf(startCoords) + 1,
            mainPathList.IndexOf(endCoords) - mainPathList.IndexOf(startCoords));

        return perimeter;
    }
}