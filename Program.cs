using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ProceduralGeneration
{
    class Program
    {
        static void Main(string[] args)
        {

            var dungeon = new Dungeon(3);

            // Creat main path
            var path = dungeon.CreateMainPath(new int[]{40, 25});
            // Generate a map of the main path
            var map = dungeon.CreateMainPathMap(path);

            var lockedDoorLocations = dungeon.SetLockedDoorsLocations(path);
            dungeon.SetLockedDoors(map, lockedDoorLocations);

            List<Room> rooms = new List<Room>();
            
            LinkedList<int[]> fork1 = dungeon.CreateFork(path, lockedDoorLocations, 0);
            LinkedList<int[]> loop0 = dungeon.CreateLoop(path, new List<LinkedList<int[]>>(){fork1}, 
                                                         lockedDoorLocations, 0);
            LinkedList<int[]> loop1 = dungeon.CreateLoop(path, new List<LinkedList<int[]>>(){fork1, loop0}, 
                                                         lockedDoorLocations, 1);


            var maps = dungeon.CreateForkMap(fork1, path, map);
            var fork1Map = maps[0];
            map = maps[1];
            
            maps = dungeon.CreateLoopMap(loop0, path, map);
            var loop0Map = maps[0];
            map = maps[1];

            maps = dungeon.CreateLoopMap(loop1, path, map);
            var loop1Map = maps[0];
            map = maps[1];

            foreach(var kvp in map)
            {
                rooms.Add(kvp.Value);
            }
            foreach(var kvp in fork1Map)
            {
                rooms.Add(kvp.Value);
            }
            foreach(var kvp in loop0Map)
            {
                rooms.Add(kvp.Value);
            }
            foreach(var kvp in loop1Map)
            {
                rooms.Add(kvp.Value);
            }
            
            string finalMap = Dungeon.CreateMap(rooms.ToArray(), rooms.Min(r => r.y));
            System.IO.File.WriteAllText("map.txt", finalMap);
        }
    }
}
