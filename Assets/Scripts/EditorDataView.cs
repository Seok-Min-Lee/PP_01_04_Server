using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EditorDataView : MonoBehaviour
{
    [SerializeField] private Image checkImage;

    [SerializeField] private TextMeshProUGUI Index;
    [SerializeField] private TextMeshProUGUI Password;
    [SerializeField] private TextMeshProUGUI RegisterDateTime;
    [SerializeField] private TextMeshProUGUI ReleaseDateTime;
    [SerializeField] private TextMeshProUGUI DisplayDateTime;

    public bool IsSelected => isSelected;
    private bool isSelected;
    public EditorData data { get; private set; }
    public void Activate(EditorData data)
    {
        this.data = data;

        Index.text = data.index.ToString();
        Password.text = data.password.ToString("D4");
        RegisterDateTime.text = data.registerDateTime;
        ReleaseDateTime.text = data.releaseDateTime;
        DisplayDateTime.text = data.displayDateTime;

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
