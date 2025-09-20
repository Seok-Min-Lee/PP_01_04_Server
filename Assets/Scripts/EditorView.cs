using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EditorView : DataView<EditorDataSummary, EditorDataSummaryUI>
{
    public override void OnClickRemove()
    {
        List<Tuple<int, int>> tuples = new List<Tuple<int, int>>();

        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i].gameObject.activeSelf &&
                rows[i].IsSelected)
            {
                tuples.Add(new Tuple<int, int>(rows[i].data.id, rows[i].data.password));
            }
        }

        DatabaseManager.instance.TryDeleteEditorDataBulk(tuples, out string sResult);
    }
    public override void OnClickAgain()
    {
        List<Tuple<int, int>> tuples = new List<Tuple<int, int>>();

        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i].gameObject.activeSelf &&
                rows[i].IsSelected)
            {
                tuples.Add(new Tuple<int, int>(rows[i].data.id, rows[i].data.password));
            }
        }

        if (DatabaseManager.instance.TryUpdateEditorDataBulk(tuples, out string sResult))
        {
            Server.Instance.SendRequsetGetUndisplayedIdList();
        }
    }
}
