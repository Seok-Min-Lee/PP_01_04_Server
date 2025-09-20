using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StudioDataSummaryUI : DataRow<StudioDataSummary>
{
    [SerializeField] private TextMeshProUGUI Index;
    [SerializeField] private TextMeshProUGUI Password;
    [SerializeField] private TextMeshProUGUI RegisterDateTime;
    public override void Activate(StudioDataSummary data)
    {
        base.Activate(data);

        Index.text = data.index + "";
        Password.text = data.password.ToString("D4");
        RegisterDateTime.text = data.registerDateTime + "";
    }
}
