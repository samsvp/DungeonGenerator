using System;

public class VoidRoom : Room
{
    public VoidRoom(int x, int y, Door[] doors) : base(x, y, doors)
    { }


    protected override string Repr(char doorChar=' ')
    {
        repr = "";
        string empty = new String(' ', size[0] + 2);
        for (int i=0; i<size[1]; i++) repr += empty + '\n';
        return repr;
    }
}