using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StudioView : DataView<StudioDataSummary, StudioDataSummaryUI>
{
    public override void OnClickRemove()
    {
        List<int> list = new List<int>();

        for (int i = 0; i < rows.Count; i++)
        {
            if (rows[i].gameObject.activeSelf &&
                rows[i].IsSelected)
            {
                list.Add(rows[i].data.password);
            }
        }

        DatabaseManager.instance.TryDeleteStudioDataBulk(list, out string sResult);
    }
}
