using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class StudioDataView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Index;
    [SerializeField] private TextMeshProUGUI Password;
    [SerializeField] private TextMeshProUGUI RegisterDateTime;

    public StudioData data { get; private set; }
    public void Activate(StudioData data)
    {
        this.data = data;

        Index.text = data.index + "";
        Password.text = data.password.ToString("D4");
        RegisterDateTime.text = data.registerDateTime + "";
    }
}
