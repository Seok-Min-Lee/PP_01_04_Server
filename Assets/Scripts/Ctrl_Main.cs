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
    [SerializeField] private StudioDataView studioDataViewPrefab;
    private List<StudioDataView> studioDataViews = new List<StudioDataView>();

    [SerializeField] private Transform editorDataTable;
    [SerializeField] private EditorDataView editorDataViewPrefab;
    private List<EditorDataView> editorDataViews = new List<EditorDataView>();

    private void Start()
    {
        Debug.Log("Server is Available? " + (Server.Instance != null));

        deviceMonitors[0].Init("Studio", "127.0.0.1");
        deviceMonitors[1].Init("Editor", "127.0.0.1");
        deviceMonitors[2].Init("Gallery", "127.0.0.1");

        DatabaseManager.instance.InitData();
        RefreshStudioDataView(DatabaseManager.instance.StudioDataDictionary.Values);
        RefreshEditorDataView(DatabaseManager.instance.EditorDataDictionary.Values);
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
            RefreshStudioDataView(DatabaseManager.instance.StudioDataDictionary.Values);
            RefreshEditorDataView(DatabaseManager.instance.EditorDataDictionary.Values);
        }
    }
    public void OnClickSelectAllStudio()
    {
        bool isSelectedAll = !studioDataViews.Any(view => view.gameObject.activeSelf && !view.IsSelected);

        for (int i = 0; i < studioDataViews.Count; i++)
        {
            if (studioDataViews[i].gameObject.activeSelf)
            {
                studioDataViews[i].SetSelected(!isSelectedAll);
            }
        }
    }
    public void OnClickSelectAllEditor()
    {
        bool isSelectedAll = !editorDataViews.Any(view => view.gameObject.activeSelf && !view.IsSelected);

        for (int i = 0; i < editorDataViews.Count; i++)
        {
            if (editorDataViews[i].gameObject.activeSelf)
            {
                editorDataViews[i].SetSelected(!isSelectedAll);
            }
        }
    }
    public void OnClickRemoveStudio()
    {
        List<int> list = new List<int>();

        for (int i = 0; i < studioDataViews.Count; i++)
        {
            if (studioDataViews[i].gameObject.activeSelf &&
                studioDataViews[i].IsSelected)
            {
                list.Add(studioDataViews[i].data.password);
            }
        }

        DatabaseManager.instance.TryDeleteStudioDataBulk(list, out string sResult);
    }
    public void OnClickRemove()
    {
        List<Tuple<int, int>> tuples = new List<Tuple<int, int>>();

        for (int i = 0; i < editorDataViews.Count; i++)
        {
            if (editorDataViews[i].gameObject.activeSelf &&
                editorDataViews[i].IsSelected)
            {
                tuples.Add(new Tuple<int, int>(editorDataViews[i].data.id, editorDataViews[i].data.password));
            }
        }

        DatabaseManager.instance.TryDeleteEditorDataBulk(tuples, out string sResult);
    }
    public void OnClickAgain()
    {
        List<Tuple<int, int>> tuples = new List<Tuple<int, int>>();

        for (int i = 0; i < editorDataViews.Count; i++)
        {
            if (editorDataViews[i].gameObject.activeSelf &&
                editorDataViews[i].IsSelected)
            {
                tuples.Add(new Tuple<int, int>(editorDataViews[i].data.id, editorDataViews[i].data.password));
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
    public void RefreshStudioDataView(IEnumerable<StudioData> data)
    {
        // Prepare Object
        int size = data.Count();

        int diff = studioDataViews.Count - size;
        if (diff < 0)
        {
            for (int i = 0; i < -diff; i++)
            {
                StudioDataView studioDataView = GameObject.Instantiate<StudioDataView>(studioDataViewPrefab, studioDataTable);
                studioDataViews.Add(studioDataView);
            }
        }
        else
        {
            for (int i = 0; i < diff; i++)
            {
                studioDataViews[studioDataViews.Count - i - 1].gameObject.SetActive(false);
            }
        }

        // Init Data
        for (int i = 0; i < size; i++)
        {
            studioDataViews[i].Activate(data.ElementAt(i));
            studioDataViews[i].gameObject.SetActive(true);
        }
    }
    public void RefreshEditorDataView(IEnumerable<EditorData> data)
    {
        // Prepare Object
        int size = data.Count();

        int diff = editorDataViews.Count - size;
        if (diff < 0)
        {
            for (int i = 0; i < -diff; i++)
            {
                EditorDataView editorDataView = GameObject.Instantiate<EditorDataView>(editorDataViewPrefab, editorDataTable);
                editorDataViews.Add(editorDataView);
            }
        }
        else
        {
            for (int i = 0; i < diff; i++)
            {
                editorDataViews[editorDataViews.Count - i - 1].gameObject.SetActive(false);
            }
        }

        // Init Data
        for (int i = 0; i < size; i++)
        {
            editorDataViews[i].Activate(data.ElementAt(i));
            editorDataViews[i].gameObject.SetActive(true);
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
