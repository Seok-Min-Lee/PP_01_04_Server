using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DataView<T, U> : MonoBehaviour 
    where T : class 
    where U : DataRow<T>
{
    [SerializeField] protected Transform tableTransform;
    [SerializeField] protected U prefab;
    protected List<U> rows = new List<U>();

    public void Refresh(IEnumerable<T> data)
    {
        // Prepare Object
        int size = data.Count();

        int diff = rows.Count - size;
        if (diff < 0)
        {
            for (int i = 0; i < -diff; i++)
            {
                U summaryUI = GameObject.Instantiate<U>(prefab, tableTransform);
                rows.Add(summaryUI);
            }
        }
        else
        {
            for (int i = 0; i < diff; i++)
            {
                rows[rows.Count - i - 1].gameObject.SetActive(false);
            }
        }

        // Init Data
        for (int i = 0; i < size; i++)
        {
            rows[i].Activate(data.ElementAt(i));
            rows[i].gameObject.SetActive(true);
        }
    }
    public void OnClickSelectAll()
    {
        bool isSelectedAll = !rows.Any(x => x.gameObject.activeSelf && !x.IsSelected);

        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i].gameObject.activeSelf)
            {
                rows[i].SetSelected(!isSelectedAll);
            }
        }
    }
    public virtual void OnClickRemove()
    {

    }
    public virtual void OnClickAgain()
    {

    }
}
