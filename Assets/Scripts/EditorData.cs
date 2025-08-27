using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorData : MonoBehaviour
{
    public EditorData(int index, int password, string registerDateTime, string displayDateTime)
    {
        this.index = index;
        this.password = password;
        this.registerDateTime = registerDateTime;
        this.displayDateTime = displayDateTime;
    }
    public int index { get; private set; }
    public int password { get; private set; }
    public string registerDateTime { get; private set; }
    public string displayDateTime { get; private set; }
}
