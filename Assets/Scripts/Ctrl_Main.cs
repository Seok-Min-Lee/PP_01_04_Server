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
        Debug.Log("Server is Available? " + (Server.Instance != null));
    }
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
                DatabaseManager.instance.AddStudioData(sds, out string sResult);

                EditorDataSample eds = new EditorDataSample();
                eds.id = 0;
                eds.password = sds.id;
                eds.isDisplayed = false;

                Texture2D tex = new Texture2D(0, 0, TextureFormat.ARGB32, false);
                tex.LoadImage(sds.textureRaw);

                eds.texture = tex;
                DatabaseManager.instance.AddEditorData(eds, out sResult);
            }
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            List<StudioData> sds = DatabaseManager.instance.GetStudioData("SELECT id, register_datetime FROM TB_STUDIO ORDER BY id ASC");

            int diff = studioDataViews.Count - sds.Count;
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

            for (int i = 0; i < sds.Count; i++)
            {
                studioDataViews[i].Activate(sds[i]);
                studioDataViews[i].gameObject.SetActive(true);
            }

            List<EditorData> eds = DatabaseManager.instance.GetEditorData("SELECT password, register_datetime, display_datetime FROM TB_EDITOR ORDER BY id ASC");

            diff = editorDataViews.Count - eds.Count;
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

            for (int i = 0; i < eds.Count; i++)
            {
                editorDataViews[i].Activate(eds[i]);
                editorDataViews[i].gameObject.SetActive(true);
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
