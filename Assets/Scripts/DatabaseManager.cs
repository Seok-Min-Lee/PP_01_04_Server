using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

public class DatabaseManager : Singleton<DatabaseManager>
{
    public Dictionary<int, StudioDataSummary> StudioDataSummaryDic => studioDataSummaryDic;
    public Dictionary<int, EditorDataSummary> EditorDataSummaryDic => editorDataSummaryDic;
    public Dictionary<int, int> PasswordDictionary => passwordDictionary;
    
    private Dictionary<int, StudioDataSummary> studioDataSummaryDic = new Dictionary<int, StudioDataSummary>();
    private Dictionary<int, EditorDataSummary> editorDataSummaryDic = new Dictionary<int, EditorDataSummary>();
    private Dictionary<int, int> passwordDictionary = new Dictionary<int, int>();

    private Ctrl_Main ctrl => _ctrl ??= GameObject.Find("Ctrl").GetComponent<Ctrl_Main>();
    private Ctrl_Main _ctrl; 

    private int studioDataId = 1;
    private int editorDataId = 1;
    
    private SqliteConnection conn;
    private readonly string connectionStr = "URI=file:" + Application.streamingAssetsPath + "/db_PP_01.db";
    
    public void InitData()
    {
        editorDataSummaryDic = GetEditorData();
        studioDataSummaryDic = GetStudioData();
        passwordDictionary = studioDataSummaryDic.Values.Select(x => x.password).ToDictionary(k => k, v => v);
    }
    public StudioDataRaw GetStudioDataRaw(int password)
    {
        StudioDataRaw studioDataRaw = null;

        using (conn = new SqliteConnection(connectionStr))
        {
            conn.Open();

            using (IDbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT * FROM TB_STUDIO WHERE password = {password}";

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        //int password = reader.GetInt32(1);
                        byte[] textureRaw = (byte[])reader["Texture"];
                        string registerDateTime = reader.GetString(3);

                        studioDataRaw = new StudioDataRaw(
                            id: id, 
                            password: password, 
                            registerDateTime: registerDateTime, 
                            textureRaw: textureRaw
                        );
                    }
                }
            }

            conn.Close();
        }

        return studioDataRaw;
    }
    public EditorDataRaw GetEditorDataRaw(int id)
    {
        EditorDataRaw raw = null;

        using (conn = new SqliteConnection(connectionStr))
        {
            conn.Open();

            // Get Data
            using (IDbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT * FROM TB_EDITOR WHERE id = {id} AND stateNo = {(int)EditorDataRaw.State.Registered}";

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        //int id = reader.GetInt32(0);
                        int password = reader.GetInt32(1);
                        int filterNo = reader.GetInt32(2);
                        int stateNo = reader.GetInt32(3);
                        byte[] textureRaw = (byte[])reader["Texture"];
                        string registerDateTime = reader.GetString(5);
                        string releaseDateTime = reader.GetString(6);
                        string displayDateTime = reader.GetString(7);

                        raw = new EditorDataRaw(
                            id: id,
                            password: password, 
                            filterNo: filterNo, 
                            stateNo: stateNo,
                            registerDateTime: registerDateTime, 
                            releaseDateTime: releaseDateTime,
                            displayDateTime: displayDateTime, 
                            textureRaw: textureRaw
                        );
                    }
                }
            }

            // Update Data
            if (raw != null)
            {
                string query = $"UPDATE TB_EDITOR SET stateNo = @stateNo, release_datetime = @release_datetime WHERE id = @id";

                using (SqliteCommand cmd = new SqliteCommand(query, conn))
                {
                    int stateNo = (int)EditorDataRaw.State.Released;
                    string datetime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    cmd.Parameters.AddWithValue("@stateNo", stateNo);
                    cmd.Parameters.AddWithValue("@release_datetime", datetime);
                    cmd.Parameters.AddWithValue("@id", raw.Id);

                    cmd.ExecuteNonQuery();

                    //
                    editorDataSummaryDic[raw.Id].SetReleaseDateTime(datetime);
                    ctrl.RefreshEditorDataSummaryUI(editorDataSummaryDic.Values);
                }
            }

            conn.Close();
        }

        return raw;
    }
    public bool TryAddStudioData(int password, byte[] texture, out string sResult)
    {
        try
        {
            using (conn = new SqliteConnection(connectionStr))
            {
                conn.Open();

                string query =
                    "INSERT INTO " +
                        "TB_STUDIO (password, texture, register_datetime) " +
                    "VALUES " +
                        "(@password, @texture, @register_datetime)";

                using (SqliteCommand cmd = new SqliteCommand(query, conn))
                {
                    string datetime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    cmd.Parameters.AddWithValue("@password", password);
                    cmd.Parameters.AddWithValue("@texture", texture);
                    cmd.Parameters.AddWithValue("@register_datetime", datetime);

                    cmd.ExecuteNonQuery();

                    //
                    StudioDataSummary summary = new StudioDataSummary(
                        index: studioDataId++, 
                        password: password, 
                        registerDateTime: datetime
                    );
                    studioDataSummaryDic.Add(summary.password, summary);

                    //
                    ctrl.RefreshStudioDataSummaryUI(studioDataSummaryDic.Values);
                }

                conn.Close();
            }
        }
        catch (Exception e)
        {
            sResult = e.Message;
            return false;
        }

        sResult = string.Empty;
        return true;
    }
    public bool TryAddEditorData(int password, int filterNo, byte[] texture, out string sResult)
    {
        try
        {
            using (conn = new SqliteConnection(connectionStr))
            {
                conn.Open();

                string query =
                    "INSERT INTO " +
                        "TB_EDITOR (password, filterNo, stateNo, texture, register_datetime, release_datetime, display_datetime) " +
                    "VALUES " +
                        "(@password, @filterNo, @stateNo, @texture, @register_datetime, @release_datetime, @display_datetime)";

                using (SqliteCommand cmd = new SqliteCommand(query, conn))
                {
                    int stateNo = (int)EditorDataRaw.State.Registered;
                    string datetime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    cmd.Parameters.AddWithValue("@password", password);
                    cmd.Parameters.AddWithValue("@filterNo", filterNo);
                    cmd.Parameters.AddWithValue("@stateNo", stateNo);
                    cmd.Parameters.AddWithValue("@texture", texture);
                    cmd.Parameters.AddWithValue("@register_datetime", datetime);
                    cmd.Parameters.AddWithValue("@release_datetime", "-");
                    cmd.Parameters.AddWithValue("@display_datetime", "-");

                    cmd.ExecuteNonQuery();

                    //
                    int tempId = editorDataSummaryDic.Count > 0 ? editorDataSummaryDic.Keys.Max() + 1 : 1;
                    EditorDataSummary summary = new EditorDataSummary(
                        index: editorDataId++, 
                        id: tempId, 
                        password: password, 
                        registerDateTime: datetime, 
                        releaseDateTime: "-",
                        displayDateTime: "-"
                    );
                    editorDataSummaryDic.Add(summary.id, summary);

                    ctrl.RefreshEditorDataSummaryUI(editorDataSummaryDic.Values);
                }

                conn.Close();
            }
        }
        catch (Exception e)
        {
            sResult = e.Message;
            return false;
        }

        sResult = string.Empty;
        return true;
    }
    public bool TryUpdateDisplayState(int id, out string sResult)
    {
        try
        {
            using (conn = new SqliteConnection(connectionStr))
            {
                conn.Open();

                string query = "UPDATE TB_EDITOR SET stateNo = @stateNo, display_datetime = @display_datetime WHERE id = @id";

                using (SqliteCommand cmd = new SqliteCommand(query, conn))
                {
                    int stateNo = (int)EditorDataRaw.State.Displayed;
                    string displayeDatetime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    cmd.Parameters.AddWithValue("@stateNo", stateNo);
                    cmd.Parameters.AddWithValue("@display_datetime", displayeDatetime);
                    cmd.Parameters.AddWithValue("@id", id);

                    cmd.ExecuteNonQuery();

                    //
                    editorDataSummaryDic[id].SetDisplayDateTime(displayeDatetime);
                    ctrl.RefreshEditorDataSummaryUI(editorDataSummaryDic.Values);
                }

                conn.Close();
            }
        }
        catch (Exception e)
        {
            sResult = e.Message;
            return false;
        }

        sResult = string.Empty;
        return true;
    }
    public bool TryDeleteStudioDataBulk(IEnumerable<int> passwords, out string sResult)
    {
        try
        {
            using (conn = new SqliteConnection(connectionStr))
            {
                conn.Open();

                using (SqliteTransaction transaction = conn.BeginTransaction())
                using (SqliteCommand cmd = new SqliteCommand(conn))
                {
                    cmd.Transaction = transaction;

                    List<string> parts = new List<string>();

                    for (int i = 0; i < passwords.Count(); i++)
                    {
                        int password = passwords.ElementAt(i);

                        string _pw = $"@pw_{i}";
                        parts.Add(_pw);

                        cmd.Parameters.AddWithValue(_pw, password);
                    }

                    cmd.CommandText = $"DELETE FROM TB_STUDIO WHERE password IN ({string.Join(", ", parts)})";

                    int count = cmd.ExecuteNonQuery();
                    transaction.Commit();

                    Debug.Log($"[DBManager] Delete Studio Data Count={count}");
                }

                // VACUUM & Refresh
                using (SqliteCommand cmd = new SqliteCommand("VACUUM;", conn))
                {
                    cmd.ExecuteNonQuery();
                    Debug.Log("[DBManager] VACUUM Complete");

                    for (int i = 0; i < passwords.Count(); i++)
                    {
                        int password = passwords.ElementAt(i);

                        studioDataSummaryDic.Remove(password);
                        passwordDictionary.Remove(password);
                    }
                    ctrl.RefreshStudioDataSummaryUI(studioDataSummaryDic.Values);
                    Debug.Log("[DBManager] Refresh Field & UI");
                }

                conn.Close();
            }
        }
        catch (Exception e)
        {
            sResult = e.Message;
            return false;
        }

        sResult = string.Empty;
        return true;
    }
    public bool TryDeleteEditorDataBulk(IEnumerable<Tuple<int, int>> tuples, out string sResult)
    {
        try
        {
            using (conn = new SqliteConnection(connectionStr))
            {
                conn.Open();

                using (SqliteTransaction transaction = conn.BeginTransaction())
                using (SqliteCommand cmd = new SqliteCommand(conn))
                {
                    cmd.Transaction = transaction;

                    List<string> parts = new List<string>();

                    for (int i = 0; i < tuples.Count(); i++)
                    {
                        Tuple<int, int> tuple = tuples.ElementAt(i);

                        string _id = $"@id_{i}";
                        string _pw = $"@pw_{i}";

                        parts.Add($"(id = {_id} AND password = {_pw})");

                        cmd.Parameters.AddWithValue(_id, tuple.Item1);
                        cmd.Parameters.AddWithValue(_pw, tuple.Item2);
                    }

                    cmd.CommandText = $"DELETE FROM TB_EDITOR WHERE {string.Join(" OR ", parts)}";

                    int count = cmd.ExecuteNonQuery();
                    transaction.Commit();

                    Debug.Log($"[DBManager] Delete Editor Data Count={count}");
                }

                // VACUUM & Refresh
                using (SqliteCommand cmd = new SqliteCommand("VACUUM;", conn))
                {
                    cmd.ExecuteNonQuery();
                    Debug.Log("[DBManager] VACUUM Complete");

                    for (int i = 0; i < tuples.Count(); i++)
                    {
                        Tuple<int, int> tuple = tuples.ElementAt(i);

                        editorDataSummaryDic.Remove(tuple.Item1);
                    }
                    ctrl.RefreshEditorDataSummaryUI(editorDataSummaryDic.Values);
                    Debug.Log("[DBManager] Refresh Field & UI");
                }

                    conn.Close();
            }
        }
        catch (Exception e)
        {
            sResult = e.Message;
            return false;
        }

        sResult = string.Empty;
        return true;
    }
    public bool TryUpdateEditorDataBulk(IEnumerable<Tuple<int, int>> tuples, out string sResult)
    {
        try
        {
            using (conn = new SqliteConnection(connectionStr))
            {
                conn.Open();

                using (SqliteTransaction transaction = conn.BeginTransaction())
                using (SqliteCommand cmd = new SqliteCommand(conn))
                {
                    cmd.Transaction = transaction;

                    List<string> parts = new List<string>();

                    for (int i = 0; i < tuples.Count(); i++)
                    {
                        Tuple<int, int> tuple = tuples.ElementAt(i);

                        string _id = $"@id_{i}";
                        string _pw = $"@pw_{i}";

                        parts.Add($"(id = {_id} AND password = {_pw})");

                        cmd.Parameters.AddWithValue(_id, tuple.Item1);
                        cmd.Parameters.AddWithValue(_pw, tuple.Item2);
                    }

                    cmd.CommandText = $"UPDATE TB_EDITOR SET stateNo = {(int)EditorDataRaw.State.Registered}, release_datetime = '-', display_datetime = '-'  WHERE {string.Join(" OR ", parts)}";

                    int count = cmd.ExecuteNonQuery();
                    transaction.Commit();

                    Debug.Log($"[DBManager] Update Editor Data Count={count}");
                }

                // Refresh
                for (int i = 0; i < tuples.Count(); i++)
                {
                    Tuple<int, int> tuple = tuples.ElementAt(i);

                    editorDataSummaryDic[tuple.Item1].SetReleaseDateTime("-");
                    editorDataSummaryDic[tuple.Item1].SetDisplayDateTime("-");
                }
                ctrl.RefreshEditorDataSummaryUI(editorDataSummaryDic.Values);
                Debug.Log("[DBManager] Refresh Field & UI");

                conn.Close();
            }
        }
        catch (Exception e)
        {
            sResult = e.Message;
            return false;
        }

        sResult = string.Empty;
        return true;
    }
    public List<int> GetUnDisplayedIdList()
    {
        List<int> ids = new List<int>();

        using (conn = new SqliteConnection(connectionStr))
        {
            conn.Open();

            using (IDbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT id FROM TB_EDITOR WHERE stateNo = {(int)EditorDataRaw.State.Registered}";

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ids.Add(reader.GetInt32(0));
                    }
                }
            }

            conn.Close();
        }

        return ids;
    }
    private Dictionary<int, StudioDataSummary> GetStudioData()
    {
        Dictionary<int, StudioDataSummary> dictionary = new Dictionary<int, StudioDataSummary>();

        studioDataId = 1;
        using (conn = new SqliteConnection(connectionStr))
        {
            conn.Open();

            using (IDbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT password, register_datetime FROM TB_STUDIO ORDER BY id ASC";

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int password = reader.GetInt32(0);
                        string registerDateTime = reader.GetString(1);

                        StudioDataSummary data = new StudioDataSummary(
                            index: studioDataId++,
                            password: password,
                            registerDateTime: registerDateTime
                        );
                        dictionary.Add(data.password, data);
                    }
                }
            }

            conn.Close();
        }

        return dictionary;
    }
    private Dictionary<int, EditorDataSummary> GetEditorData()
    {
        Dictionary<int, EditorDataSummary> dictionary = new Dictionary<int, EditorDataSummary>();

        editorDataId = 1;

        using (conn = new SqliteConnection(connectionStr))
        {
            conn.Open();

            using (IDbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id, password, register_datetime, release_datetime, display_datetime FROM TB_EDITOR ORDER BY id ASC";

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        int password = reader.GetInt32(1);
                        string registerDateTime = reader.GetString(2);
                        string releaseDateTime = reader.GetString(3);
                        string displayDateTime = reader.GetString(4);

                        EditorDataSummary data = new EditorDataSummary(
                            index: editorDataId++,
                            id: id,
                            password: password,
                            registerDateTime: registerDateTime,
                            releaseDateTime: releaseDateTime,
                            displayDateTime: displayDateTime
                        );
                        dictionary.Add(data.id, data);
                    }
                }
            }

            conn.Close();
        }

        return dictionary;
    }
}
