using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ctrl_Main : MonoBehaviour
{
    [SerializeField] private Transform studioDataTable;
    [SerializeField] private StudioDataView studioDataViewPrefab;
    [SerializeField] private Transform editorDataTable;
    [SerializeField] private EditorDataView editorDataViewPrefab;

    private void Start()
    {
        for (int i = 0; i < studioDataSamples.Length; i++)
        {
            StudioDataView studioDataView = GameObject.Instantiate<StudioDataView>(studioDataViewPrefab, studioDataTable);
            studioDataView.Activate(studioDataSamples[i]);
        }

        for (int i = 0; i < editorDataSamples.Length; i++)
        {
            EditorDataView editorDataView = GameObject.Instantiate<EditorDataView>(editorDataViewPrefab, editorDataTable);
            editorDataView.Activate(editorDataSamples[i]);
        }
    }

    private StudioData[] studioDataSamples = new StudioData[] 
    {
        new StudioData(0, 1000, "2025/01/01 23:59:59"),
        new StudioData(1, 2000, "2025/02/02 23:59:59"),
        new StudioData(2, 3000, "2025/03/03 23:59:59"),
    };

    private EditorData[] editorDataSamples = new EditorData[]
    {
        new EditorData(0, 1000, "2025/01/01 23:59:59", "2025/01/01 23:59:59"),
        new EditorData(1, 2000, "2025/02/02 23:59:59", "2025/02/02 23:59:59"),
        new EditorData(2, 3000, "2025/03/03 23:59:59", "2025/03/03 23:59:59"),
    };
}
