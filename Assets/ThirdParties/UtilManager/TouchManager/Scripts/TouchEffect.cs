using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class TouchEffect : MonoBehaviour
{
    public void Init(Vector3 position)
    {
        GetComponent<RectTransform>().localPosition = position;
    }
    public void OnAnimationEnd()
    {
        Destroy(gameObject);
    }
}
