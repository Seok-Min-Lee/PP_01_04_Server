using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StudioDataSummaryUI : MonoBehaviour
{
    [SerializeField] private Image checkImage;

    [SerializeField] private TextMeshProUGUI Index;
    [SerializeField] private TextMeshProUGUI Password;
    [SerializeField] private TextMeshProUGUI RegisterDateTime;

    public bool IsSelected => isSelected;
    private bool isSelected;
    public StudioDataSummary data { get; private set; }
    public void Activate(StudioDataSummary data)
    {
        this.data = data;

        Index.text = data.index + "";
        Password.text = data.password.ToString("D4");
        RegisterDateTime.text = data.registerDateTime + "";

        isSelected = false;
        checkImage.gameObject.SetActive(isSelected);
    }
    public void OnClick()
    {
        isSelected = !isSelected;
        checkImage.gameObject.SetActive(isSelected);
    }
    public void SetSelected(bool value)
    {
        isSelected = value;
        checkImage.gameObject.SetActive(isSelected);
    }
}
