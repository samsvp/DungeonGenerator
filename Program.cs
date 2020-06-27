using System;
using System.Collections.Generic;
using System.IO;

namespace ProceduralGeneration
{
    class Program
    {
        static void Main(string[] args)
        {

            var dungeon = new Dungeon(40, 25, 3, 2, 2, true, 
                                Dungeon.Focus.Combat, Dungeon.EnemyFrequency.Medium);

            // Creat main path
            var path = dungeon.CreateMainPath(new int[]{40, 25});
            // Generate a map of the main path
            var map = dungeon.CreatePathMap(path);

            var lockedDoorLocations = dungeon.SetLockedDoorsLocations(path);
            dungeon.SetLockedDoors(map, lockedDoorLocations);

            List<Room> rooms = new List<Room>();
            foreach(var kvp in map)
            {
                rooms.Add(kvp.Value);
            }
            
            LinkedList<int[]> fork1 = dungeon.CreateFork(path, lockedDoorLocations, 0);
            var fork1Map = dungeon.CreatePathMap(fork1);
            
            foreach(var kvp in fork1Map)
            {
                rooms.Add(kvp.Value);
            }

            
            string finalMap = Dungeon.CreateMap(rooms.ToArray());
            System.IO.File.WriteAllText("map.txt", finalMap);
        }
    }
}
