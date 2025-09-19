using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

public class DatabaseManager : Singleton<DatabaseManager>
{
    public Dictionary<int, StudioData> StudioDataDictionary => studioDataDictionary;
    public Dictionary<int, EditorData> EditorDataDictionary => editorDataDictionary;
    public Dictionary<int, int> PasswordDictionary => passwordDictionary;

    private Dictionary<int, StudioData> studioDataDictionary = new Dictionary<int, StudioData>();
    private Dictionary<int, EditorData> editorDataDictionary = new Dictionary<int, EditorData>();
    private Dictionary<int, int> passwordDictionary = new Dictionary<int, int>();

    private Ctrl_Main ctrl => _ctrl ??= GameObject.Find("Ctrl").GetComponent<Ctrl_Main>();
    private Ctrl_Main _ctrl; 

    private int studioDataId = 1;
    private int editorDataId = 1;
    
    private SqliteConnection conn;
    private readonly string connectionStr = "URI=file:" + Application.streamingAssetsPath + "/db_PP_01.db";
    
    public void InitData()
    {
        editorDataDictionary = GetEditorData();
        studioDataDictionary = GetStudioData();
        passwordDictionary = studioDataDictionary.Values.Select(x => x.password).ToDictionary(k => k, v => v);
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
    public EditorDataRaw GetEditorDataRaw()
    {
        EditorDataRaw raw = null;

        using (conn = new SqliteConnection(connectionStr))
        {
            conn.Open();

            // Get Data
            using (IDbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT * FROM TB_EDITOR WHERE stateNo = 0 ORDER BY id ASC LIMIT 1";

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int id = reader.GetInt32(0);
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
                    editorDataDictionary[raw.Id].SetReleaseDateTime(datetime);
                    ctrl.RefreshEditorDataView(editorDataDictionary.Values);
                }
            }

            conn.Close();
        }

        return raw;
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
                    editorDataDictionary[id].SetDisplayDateTime(displayeDatetime);
                    ctrl.RefreshEditorDataView(editorDataDictionary.Values);
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
                    StudioData data = new StudioData(
                        index: studioDataId++, 
                        password: password, 
                        registerDateTime: datetime
                    );
                    studioDataDictionary.Add(data.index, data);

                    //
                    ctrl.RefreshStudioDataView(studioDataDictionary.Values);
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
                    int tempId = editorDataDictionary.Count > 0 ? editorDataDictionary.Keys.Max() + 1 : 1;
                    EditorData data = new EditorData(
                        index: editorDataId++, 
                        id: tempId, 
                        password: password, 
                        registerDateTime: datetime, 
                        releaseDateTime: "-",
                        displayDateTime: "-"
                    );
                    editorDataDictionary.Add(data.id, data);

                    ctrl.RefreshEditorDataView(editorDataDictionary.Values);
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
    public int GetUnDisplayedCount()
    {
        // 이걸 db에서 처리하는게 적절한가?
        int count = 0;

        using (conn = new SqliteConnection(connectionStr))
        {
            conn.Open();

            using (IDbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT count(id) FROM TB_EDITOR WHERE stateNo = {(int)EditorDataRaw.State.Registered}";

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        count = reader.GetInt32(0);
                    }
                }
            }

            conn.Close();
        }

        return count;
    }
    private Dictionary<int, StudioData> GetStudioData()
    {
        Dictionary<int, StudioData> dictionary = new Dictionary<int, StudioData>();

        int index = studioDataId;
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

                        StudioData data = new StudioData(
                            index: index++,
                            password: password,
                            registerDateTime: registerDateTime
                        );
                        dictionary.Add(data.index, data);
                    }
                }
            }

            conn.Close();
        }

        studioDataId = index;

        return dictionary;
    }
    private Dictionary<int, EditorData> GetEditorData()
    {
        Dictionary<int, EditorData> dictionary = new Dictionary<int, EditorData>();

        int index = editorDataId;

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

                        EditorData data = new EditorData(
                            index: index++,
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

        editorDataId = index;

        return dictionary;
    }
}
