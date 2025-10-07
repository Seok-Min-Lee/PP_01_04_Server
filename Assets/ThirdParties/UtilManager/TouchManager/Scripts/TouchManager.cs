using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TouchManager : MonoSingleton<TouchManager>
{
    [SerializeField] private TouchEffect prefab;

    public Canvas Canvas
    {
        get
        {
            if (_canvas == null)
            {
                _canvas = FindObjectOfType<Canvas>();
            }

            return _canvas;
        }
    }
    private Canvas _canvas;

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (Canvas == null)
            {
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                rect: Canvas.transform as RectTransform,
                screenPoint: Input.mousePosition,
                cam: null,
                localPoint: out Vector2 point
            );

            TouchEffect te = GameObject.Instantiate<TouchEffect>(prefab, _canvas.transform);
            te.Init(point);
        }
    }
}
