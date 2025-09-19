using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorData
{
    public EditorData(int index, int id, int password, string registerDateTime, string releaseDateTime, string displayDateTime)
    {
        this.index = index;
        this.id = id;
        this.password = password;
        this.registerDateTime = registerDateTime;
        this.releaseDateTime = releaseDateTime;
        this.displayDateTime = displayDateTime;
    }
    public int index { get; private set; }
    public int id { get; private set; }
    public int password { get; private set; }
    public string registerDateTime { get; private set; }
    public string releaseDateTime { get; private set; }
    public string displayDateTime { get; private set; }
    public void SetReleaseDateTime(string value)
    {
        releaseDateTime = value;
    }
    public void SetDisplayDateTime(string value)
    {
        displayDateTime = value;
    }
}
