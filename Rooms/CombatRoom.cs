using System;
using System.Collections.Generic;
using System.Linq;

public class CombatRoom : Room
{
    private static Random random = new Random();
    public int enemyNumber = 1;
    public List<int[]> enemyLocations = new List<int[]>();

    public CombatRoom(int x, int y, Door door, int _enemyNumber=1) : base(x, y, door)
    { 
        rT = RoomType.C;
        if (enemyNumber < 1) throw new Exception("Combat room must contain at least 1 enemy");
        if (size[0] * size[1] < _enemyNumber) 
            throw new Exception("Can't fit that many enemies into the room");
        
        enemyNumber = _enemyNumber;
        SetEnemyLocations();
        Repr();
    }

    ///<summary>
    /// Sets the enemies location within the room
    ///</summary>
    public void SetEnemyLocations()
    {
        List<int[]> availableLocations = new List<int[]>();

        for (int x=1; x<size[0] + 1; x++)
        for (int y=1; y<size[1] + 1; y++)
        availableLocations.Add(new int[] {x,y});

        for(int i=0; i<enemyNumber; i++)
        {
            int index = random.Next(availableLocations.Count);
            enemyLocations.Add(availableLocations[index]);
            availableLocations.RemoveAt(index);
        }
    }


    protected override string Repr()
    {
        base.Repr();
        string[] str = repr.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach(int[] location in enemyLocations)
        {
            char[] c = str[location[0]].ToCharArray();
            c[location[1]] = 'E';
            str[location[0]] = new string(c);
        }    

        repr = "";
        foreach(string s in str) repr += s + '\n';
        
        return repr;
    }

}