using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DeviceMonitor : MonoBehaviour
{
    [SerializeField] private Image state;
    [SerializeField] private TextMeshProUGUI name;

    public string IP => ip;
    private string ip;

    public void Init(string name, string ip)
    {
        this.name.text = name;
        this.ip = ip;
    }
    public void UpdateConnection(bool value)
    {
        state.color = value ? Color.green : Color.red;
    }
}
