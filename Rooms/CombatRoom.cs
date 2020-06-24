using System;
using System.Collections.Generic;
using System.Linq;

public class CombatRoom : Room
{
    private static Random random = new Random();
    public int enemyNumber = 1;
    public List<int[]> enemyLocations = new List<int[]>();

    public CombatRoom(int x, int y, Door[] doors, int _enemyNumber=1) : base(x, y, doors)
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

        for (int i=1; i<size[0]-1; i++)
        for (int j=1; j<size[1]-1; j++)
        availableLocations.Add(new int[] {i,j});

        for(int i=0; i<enemyNumber; i++)
        {
            int index = random.Next(availableLocations.Count);
            enemyLocations.Add(availableLocations[index]);
            availableLocations.RemoveAt(index);
        }
    }


    protected override string Repr(char doorChar=' ')
    {
        base.Repr(doorChar);
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