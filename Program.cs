using System;
using System.Collections.Generic;
using System.IO;

namespace ProceduralGeneration
{
    class Program
    {
        static void Main(string[] args)
        {

            var dungeon = new Dungeon(25, 25, 3, 2, 2, true, 
                                Dungeon.Focus.Combat, Dungeon.EnemyFrequency.Medium);

            // Creat main path
            var path = dungeon.CreateMainPath(new int[]{25, 25});
            // Generate a map of the main path
            var map = dungeon.CreateMainPathMap(path);

            var lockedDoorLocations = dungeon.SetLockedDoorsLocations(path);
            dungeon.SetLockedDoors(map, lockedDoorLocations);

            List<Room> rooms = new List<Room>();
            foreach(var kvp in map)
            {
                rooms.Add(kvp.Value);
            }
            
            string finalMap = Dungeon.CreateMap(rooms.ToArray());
            System.IO.File.WriteAllText("map.txt", finalMap);
        }
    }
}
