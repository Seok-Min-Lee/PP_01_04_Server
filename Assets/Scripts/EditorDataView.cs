using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EditorDataView : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI Index;
    [SerializeField] private TextMeshProUGUI Password;
    [SerializeField] private TextMeshProUGUI RegisterDateTime;
    [SerializeField] private TextMeshProUGUI ReleaseDateTime;
    [SerializeField] private TextMeshProUGUI DisplayDateTime;

    public EditorData data { get; private set; }
    public void Activate(EditorData data)
    {
        this.data = data;

        Index.text = data.index.ToString();
        Password.text = data.password.ToString("D4");
        RegisterDateTime.text = data.registerDateTime;
        ReleaseDateTime.text = data.releaseDateTime;
        DisplayDateTime.text = data.displayDateTime;
    }
}
