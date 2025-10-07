using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DataRow<T> : MonoBehaviour where T : class
{
    [SerializeField] protected Image checkImage;

    public bool IsSelected => isSelected;
    protected bool isSelected;
    public T data { get; protected set; }
    public virtual void Activate(T data)
    {
        this.data = data;

        isSelected = false;
        checkImage.gameObject.SetActive(isSelected);
    }

    public virtual void OnClick()
    {
        AudioManager.Instance.PlaySFX(Sound.Key.Click);

        isSelected = !isSelected;
        checkImage.gameObject.SetActive(isSelected);
    }

    public virtual void SetSelected(bool value)
    {
        isSelected = value;
        checkImage.gameObject.SetActive(isSelected);
    }
}
