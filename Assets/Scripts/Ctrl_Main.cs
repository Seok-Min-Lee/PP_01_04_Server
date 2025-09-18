using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
        
        ReloadData();
        RefreshView();
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            AddSampleData();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            ReloadData();
            RefreshView();
        }
    }
    public void RefreshDeviceMonitorLocal(int index, bool value)
    {
        deviceMonitors[index].UpdateConnection(value);
    }
    private void ReloadData()
    {
        DatabaseManager.instance.RefreshStudioData();
        DatabaseManager.instance.RefreshEditorData();
    }
    public void RefreshView()
    {
        RefreshStudioDataView(DatabaseManager.instance.StudioDataDictionary.Values);
        RefreshEditorDataView(DatabaseManager.instance.EditorDataDictionary.Values);
    }
    private void RefreshStudioDataView(IEnumerable<StudioData> data)
    {
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

        for (int i = 0; i < size; i++)
        {
            studioDataViews[i].Activate(data.ElementAt(i));
            studioDataViews[i].gameObject.SetActive(true);
        }
    }
    private void RefreshEditorDataView(IEnumerable<EditorData> data)
    {
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

        for (int i = 0; i < size; i++)
        {
            editorDataViews[i].Activate(data.ElementAt(i));
            editorDataViews[i].gameObject.SetActive(true);
        }
    }
    private void AddSampleData()
    {
        int pw = Random.Range(0, 10000);
        for (int i = 0; i < 9; i++)
        {
            int password = pw + i;
            byte[] texture = System.IO.File.ReadAllBytes("C:/Users/dltjr/Desktop/»õ Æú´õ (2)/" + (i + 1) + ".jpeg");

            DatabaseManager.instance.AddStudioData(
                password: password,
                texture: texture,
                sResult: out string sResult
            );

            DatabaseManager.instance.AddEditorData(
                password: password,
                filterNo: 0,
                texture: texture,
                sResult: out sResult
            );
        }
    }
}
