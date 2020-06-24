using System;
using System.IO;

namespace ProceduralGeneration
{
    class Program
    {
        static void Main(string[] args)
        {
            // var r = new BossRoom(0, 0, new Room.Door[]{Room.Door.E});
            // var ir = new InitialRoom(2, 0, new Room.Door[]{Room.Door.W, Room.Door.S});
            var h1 = new HealRoom(1, 0, new Room.Door[]{Room.Door.N, Room.Door.E, Room.Door.W});
            var h2 = new HealRoom(1, 1, new Room.Door[]{Room.Door.N, Room.Door.E, Room.Door.W});
            var h3 = new HealRoom(2, 1, new Room.Door[]{Room.Door.S, Room.Door.E, Room.Door.W});
            var c4 = new CombatRoom(4, 4, new Room.Door[]{Room.Door.N});
            var c5 = new CombatRoom(2, 2, new Room.Door[]{Room.Door.N});
            var c6 = new CombatRoom(3, 2, new Room.Door[]{Room.Door.E}, 2);
            
            h1.MergeRooms(h2);
            h2.MergeRooms(h3);
            c5.MergeRooms(c6);

            // Console.WriteLine(Dungeon.CreateMap(new Room[] {h1, h2, h3, c4, c5, c6}));
            var dungeon = new Dungeon(10, 10, 2, 2, true, 
                                Dungeon.Focus.Combat, Dungeon.EnemyFrequency.Medium);
            var path = dungeon.CreateMainPath(new int[]{50, 50}, 0, true);                            
            Console.WriteLine(path);
        }
    }
}
