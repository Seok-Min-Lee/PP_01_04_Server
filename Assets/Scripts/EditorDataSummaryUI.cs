using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditorDataSummaryUI : DataRow<EditorDataSummary>
{
    [SerializeField] private TextMeshProUGUI Index;
    [SerializeField] private TextMeshProUGUI Password;
    [SerializeField] private TextMeshProUGUI RegisterDateTime;
    [SerializeField] private TextMeshProUGUI ReleaseDateTime;
    [SerializeField] private TextMeshProUGUI DisplayDateTime;

    public override void Activate(EditorDataSummary data)
    {
        base.Activate(data);

        Index.text = data.index.ToString();
        Password.text = data.password.ToString("D4");
        RegisterDateTime.text = data.registerDateTime;
        ReleaseDateTime.text = data.releaseDateTime;
        DisplayDateTime.text = data.displayDateTime;
    }
}
