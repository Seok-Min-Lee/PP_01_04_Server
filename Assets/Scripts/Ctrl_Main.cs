using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ctrl_Main : MonoBehaviour
{
    [SerializeField] private Transform studioDataTable;
    [SerializeField] private StudioDataView studioDataViewPrefab;
    private List<StudioDataView> studioDataViews = new List<StudioDataView>();

    [SerializeField] private Transform editorDataTable;
    [SerializeField] private EditorDataView editorDataViewPrefab;

    private List<EditorDataView> editorDataViews = new List<EditorDataView>();
    private void Start()
    {
        //for (int i = 0; i < studioDataSamples.Length; i++)
        //{
        //    StudioDataView studioDataView = GameObject.Instantiate<StudioDataView>(studioDataViewPrefab, studioDataTable);
        //    studioDataView.Activate(studioDataSamples[i]);
        //}

        //for (int i = 0; i < editorDataSamples.Length; i++)
        //{
        //    EditorDataView editorDataView = GameObject.Instantiate<EditorDataView>(editorDataViewPrefab, editorDataTable);
        //    editorDataView.Activate(editorDataSamples[i]);
        //}
    }
    public DatabaseManager dm;
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            int pw = Random.Range(0, 10000);
            for (int i = 0; i < 9; i++)
            {
                StudioDataSample sds = new StudioDataSample();
                sds.id = pw + i;
                sds.textureRaw = System.IO.File.ReadAllBytes("C:/Users/dltjr/Desktop/»õ Æú´õ (2)/" + (i+1) + ".jpeg");
                dm.AddStudioData(sds, out string sResult);


                EditorDataSample eds = new EditorDataSample();
                eds.id = 0;
                eds.password = sds.id;
                eds.isDisplayed = false;

                Texture2D tex = new Texture2D(0, 0, TextureFormat.ARGB32, false);
                tex.LoadImage(sds.textureRaw);

                eds.texture = tex;
                dm.AddEditorData(eds, out sResult);
            }
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            List<StudioData> sds = dm.GetStudioData("SELECT id, register_datetime FROM TB_STUDIO ORDER BY id ASC");

            int newCount = studioDataViews.Count - sds.Count;

            for (int i = 0; i < sds.Count; i++)
            {
                if (i < studioDataViews.Count)
                {
                    studioDataViews[i].Activate(sds[i]);
                }
                else
                {
                    StudioDataView studioDataView = GameObject.Instantiate<StudioDataView>(studioDataViewPrefab, studioDataTable);
                    studioDataView.Activate(sds[i]);

                    studioDataViews.Add(studioDataView);
                }
            }

            List<EditorData> eds = dm.GetEditorData("SELECT password, register_datetime, display_datetime FROM TB_EDITOR ORDER BY id ASC");

            for (int i = 0; i < eds.Count; i++)
            {
                if (i < editorDataViews.Count)
                {
                    editorDataViews[i].Activate(eds[i]);
                }
                else
                {
                    EditorDataView editorDataView = GameObject.Instantiate<EditorDataView>(editorDataViewPrefab, editorDataTable);
                    editorDataView.Activate(eds[i]);

                    editorDataViews.Add(editorDataView);
                }
            }
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
