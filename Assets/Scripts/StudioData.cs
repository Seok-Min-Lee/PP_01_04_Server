using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StudioData
{
    public StudioData(int index, int password, string registerDateTime)
    {
        this.index = index;
        this.password = password;
        this.registerDateTime = registerDateTime;
    }
    public int index { get; private set; }
    public int password { get; private set; }
    public string registerDateTime { get; private set; }
}
