using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Ctrl_Main : MonoBehaviour
{
    [SerializeField] private DeviceMonitor[] deviceMonitors;

    [SerializeField] private Transform studioDataTable;
    [SerializeField] private StudioDataSummaryUI studioDataSummaryUIPrefab;
    private List<StudioDataSummaryUI> studioDataSummaryUIs = new List<StudioDataSummaryUI>();

    [SerializeField] private Transform editorDataTable;
    [SerializeField] private EditorDataSummaryUI editorDataSummaryUIPrefab;
    private List<EditorDataSummaryUI> editorDataSummaryUIs = new List<EditorDataSummaryUI>();

    private void Start()
    {
        Debug.Log("Server is Available? " + (Server.Instance != null));

        deviceMonitors[0].Init("Studio", "127.0.0.1");
        deviceMonitors[1].Init("Editor", "127.0.0.1");
        deviceMonitors[2].Init("Gallery", "127.0.0.1");

        DatabaseManager.instance.InitData();
        RefreshStudioDataSummaryUI(DatabaseManager.instance.StudioDataSummaryDic.Values);
        RefreshEditorDataSummaryUI(DatabaseManager.instance.EditorDataSummaryDic.Values);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            AddSampleData();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            DatabaseManager.instance.InitData();
            RefreshStudioDataSummaryUI(DatabaseManager.instance.StudioDataSummaryDic.Values);
            RefreshEditorDataSummaryUI(DatabaseManager.instance.EditorDataSummaryDic.Values);
        }
    }
    public void OnClickSelectAllStudio()
    {
        bool isSelectedAll = !studioDataSummaryUIs.Any(x => x.gameObject.activeSelf && !x.IsSelected);

        for (int i = 0; i < studioDataSummaryUIs.Count; i++)
        {
            if (studioDataSummaryUIs[i].gameObject.activeSelf)
            {
                studioDataSummaryUIs[i].SetSelected(!isSelectedAll);
            }
        }
    }
    public void OnClickSelectAllEditor()
    {
        bool isSelectedAll = !editorDataSummaryUIs.Any(x => x.gameObject.activeSelf && !x.IsSelected);

        for (int i = 0; i < editorDataSummaryUIs.Count; i++)
        {
            if (editorDataSummaryUIs[i].gameObject.activeSelf)
            {
                editorDataSummaryUIs[i].SetSelected(!isSelectedAll);
            }
        }
    }
    public void OnClickRemoveStudio()
    {
        List<int> list = new List<int>();

        for (int i = 0; i < studioDataSummaryUIs.Count; i++)
        {
            if (studioDataSummaryUIs[i].gameObject.activeSelf &&
                studioDataSummaryUIs[i].IsSelected)
            {
                list.Add(studioDataSummaryUIs[i].data.password);
            }
        }

        DatabaseManager.instance.TryDeleteStudioDataBulk(list, out string sResult);
    }
    public void OnClickRemove()
    {
        List<Tuple<int, int>> tuples = new List<Tuple<int, int>>();

        for (int i = 0; i < editorDataSummaryUIs.Count; i++)
        {
            if (editorDataSummaryUIs[i].gameObject.activeSelf &&
                editorDataSummaryUIs[i].IsSelected)
            {
                tuples.Add(new Tuple<int, int>(editorDataSummaryUIs[i].data.id, editorDataSummaryUIs[i].data.password));
            }
        }

        DatabaseManager.instance.TryDeleteEditorDataBulk(tuples, out string sResult);
    }
    public void OnClickAgain()
    {
        List<Tuple<int, int>> tuples = new List<Tuple<int, int>>();

        for (int i = 0; i < editorDataSummaryUIs.Count; i++)
        {
            if (editorDataSummaryUIs[i].gameObject.activeSelf &&
                editorDataSummaryUIs[i].IsSelected)
            {
                tuples.Add(new Tuple<int, int>(editorDataSummaryUIs[i].data.id, editorDataSummaryUIs[i].data.password));
            }
        }

        if (DatabaseManager.instance.TryUpdateEditorDataBulk(tuples, out string sResult))
        {
            Server.Instance.RequestRequestGetEditorData(tuples.Select(tuple => tuple.Item1));
        }
    }
    public void RefreshDeviceMonitorLocal(int index, bool value)
    {
        deviceMonitors[index].UpdateConnection(value);
    }
    public void RefreshStudioDataSummaryUI(IEnumerable<StudioDataSummary> summaries)
    {
        // Prepare Object
        int size = summaries.Count();

        int diff = studioDataSummaryUIs.Count - size;
        if (diff < 0)
        {
            for (int i = 0; i < -diff; i++)
            {
                StudioDataSummaryUI summaryUI = GameObject.Instantiate<StudioDataSummaryUI>(studioDataSummaryUIPrefab, studioDataTable);
                studioDataSummaryUIs.Add(summaryUI);
            }
        }
        else
        {
            for (int i = 0; i < diff; i++)
            {
                studioDataSummaryUIs[studioDataSummaryUIs.Count - i - 1].gameObject.SetActive(false);
            }
        }

        // Init Data
        for (int i = 0; i < size; i++)
        {
            studioDataSummaryUIs[i].Activate(summaries.ElementAt(i));
            studioDataSummaryUIs[i].gameObject.SetActive(true);
        }
    }
    public void RefreshEditorDataSummaryUI(IEnumerable<EditorDataSummary> summaries)
    {
        // Prepare Object
        int size = summaries.Count();

        int diff = editorDataSummaryUIs.Count - size;
        if (diff < 0)
        {
            for (int i = 0; i < -diff; i++)
            {
                EditorDataSummaryUI summaryUI = GameObject.Instantiate<EditorDataSummaryUI>(editorDataSummaryUIPrefab, editorDataTable);
                editorDataSummaryUIs.Add(summaryUI);
            }
        }
        else
        {
            for (int i = 0; i < diff; i++)
            {
                editorDataSummaryUIs[editorDataSummaryUIs.Count - i - 1].gameObject.SetActive(false);
            }
        }

        // Init Data
        for (int i = 0; i < size; i++)
        {
            editorDataSummaryUIs[i].Activate(summaries.ElementAt(i));
            editorDataSummaryUIs[i].gameObject.SetActive(true);
        }
    }
    private void AddSampleData()
    {
        int pw = UnityEngine.Random.Range(0, 10000);
        for (int i = 0; i < 9; i++)
        {
            int password = pw + i;
            byte[] texture = System.IO.File.ReadAllBytes("C:/Users/dltjr/Desktop/»õ Æú´õ (2)/" + (i + 1) + ".jpeg");

            DatabaseManager.instance.TryAddStudioData(
                password: password,
                texture: texture,
                sResult: out string sResult
            );

            DatabaseManager.instance.TryAddEditorData(
                password: password,
                filterNo: 0,
                texture: texture,
                sResult: out sResult
            );
        }
    }
}
