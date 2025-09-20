using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;

public class Ctrl_Main : MonoBehaviour
{
    [SerializeField] private DeviceMonitor[] deviceMonitors;

    [SerializeField] private StudioView studioView;
    [SerializeField] private EditorView editorView;

    private void Start()
    {
        Debug.Log("Server is Available? " + (Server.Instance != null));

        deviceMonitors[0].Init("Studio", "127.0.0.1");
        deviceMonitors[1].Init("Editor", "127.0.0.1");
        deviceMonitors[2].Init("Gallery", "127.0.0.1");

        DatabaseManager.instance.InitData();
        studioView.Refresh(DatabaseManager.instance.StudioDataSummaryDic.Values);
        editorView.Refresh(DatabaseManager.instance.EditorDataSummaryDic.Values);
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            AddSampleData();
        }
    }
    public void RefreshDeviceMonitor(int index, bool value)
    {
        deviceMonitors[index].UpdateConnection(value);
    }
    public void RefreshStudioView(IEnumerable<StudioDataSummary> data)
    {
        studioView.Refresh(data);
    }
    public void RefreshEditorView(IEnumerable<EditorDataSummary> data)
    {
        editorView.Refresh(data);
    }
    private void AddSampleData()
    {
        int pw = UnityEngine.Random.Range(0, 10000);
        string[] pathes = Directory.GetFiles($"{Application.streamingAssetsPath}/samples", "*.jpg");

        for (int i = 0; i < pathes.Length; i++)
        {
            int password = pw + i;
            byte[] texture = System.IO.File.ReadAllBytes(pathes[i]);

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