using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using UnityEngine;

public class DatabaseManager : Singleton<DatabaseManager>
{
    public List<StudioData> StudioDataList
    {
        get
        {
            return studioDataList;
        }
    }
    public List<EditorData> EditorDataList
    {
        get
        {
            return editorDataList;
        }
    }
    private Dictionary<int, int> passwordDictionary = new Dictionary<int, int>();
    private List<StudioData> studioDataList = new List<StudioData>();
    private List<EditorData> editorDataList = new List<EditorData>();

    private SqliteConnection conn;
    private string connectionStr { get { return "URI=file:" + Application.streamingAssetsPath + "/db_PP_01.db"; } }
    public void Refresh()
    {
        studioDataList = GetStudioData();
        editorDataList = GetEditorData();

        passwordDictionary = studioDataList.Select(x => x.password).ToDictionary(k => k, v => v);
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

            using (IDbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT * FROM TB_EDITOR WHERE IsDisplayed = 0 ORDER BY id ASC LIMIT 1";

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        int password = reader.GetInt32(1);
                        int filterNo = reader.GetInt32(2);
                        //int isDisplayed = reader.GetInt32(3);
                        bool isDisplayed = false;
                        byte[] textureRaw = (byte[])reader["Texture"];
                        string registerDateTime = reader.GetString(5);
                        string displayDateTime = reader.GetString(5);
                        int studioId = -1;

                        raw = new EditorDataRaw(
                            id: id,
                            password: password, 
                            filterNo: filterNo, 
                            isDisplayed: isDisplayed,
                            registerDateTime: registerDateTime, 
                            displayDateTime: displayDateTime, 
                            studioId: studioId,
                            textureRaw: textureRaw
                        );
                    }
                }
            }

            if (raw != null)
            {
                string query = $"UPDATE TB_EDITOR SET IsDisplayed = 1 WHERE id = {raw.Id}";

                using (SqliteCommand cmd = new SqliteCommand(query, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }

            conn.Close();
        }

        return raw;
    }
    public int GetUnDisplayedCount()
    {
        int count = 0;

        using (conn = new SqliteConnection(connectionStr))
        {
            conn.Open();

            using (IDbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = $"SELECT count(id) FROM TB_EDITOR WHERE IsDisplayed = 0";

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
    public bool TryUpdateDisplayState(int id, out string sResult)
    {
        try
        {
            using (conn = new SqliteConnection(connectionStr))
            {
                conn.Open();

                string query = $"UPDATE TB_EDITOR SET IsDisplayed = 2, Display_datetime = datetime('now', 'localtime') WHERE id = {id}";

                using (SqliteCommand cmd = new SqliteCommand(query, conn))
                {
                    cmd.ExecuteNonQuery();
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
    public List<StudioData> GetStudioData()
    {
        List<StudioData> views = new List<StudioData>();

        using (conn = new SqliteConnection(connectionStr))
        {
            conn.Open();

            using (IDbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT password, register_datetime FROM TB_STUDIO ORDER BY id ASC";

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    int index = 1;
                    while (reader.Read())
                    {
                        int password = reader.GetInt32(0);
                        string registerDateTime = reader.GetString(1);

                        views.Add(new StudioData(
                            index: index++,
                            password: password,
                            registerDateTime: registerDateTime
                        ));
                    }
                }
            }

            conn.Close();
        }

        return views;
    }
    public List<EditorData> GetEditorData()
    {
        List<EditorData> views = new List<EditorData>();

        using (conn = new SqliteConnection(connectionStr))
        {
            conn.Open();

            using (IDbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT password, register_datetime, display_datetime FROM TB_EDITOR ORDER BY id ASC";

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    int index = 1;
                    while (reader.Read())
                    {
                        int password = reader.GetInt32(0);
                        string registerDateTime = reader.GetString(1);
                        string displayDateTime = reader.GetString(2);

                        views.Add(new EditorData(
                            index: index++,
                            password: password,
                            registerDateTime: registerDateTime,
                            displayDateTime: displayDateTime
                        ));
                    }
                }
            }

            conn.Close();
        }

        return views;
    }
    public bool AddStudioData(int password, byte[] texture, out string sResult)
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
                    cmd.Parameters.AddWithValue("@password", password);
                    cmd.Parameters.AddWithValue("@texture", texture);
                    cmd.Parameters.AddWithValue("@register_datetime", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                    cmd.ExecuteNonQuery();
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
    public bool AddEditorData(int password, int filterNo, byte[] texture, out string sResult)
    {
        try
        {
            using (conn = new SqliteConnection(connectionStr))
            {
                conn.Open();

                string query =
                    "INSERT INTO " +
                        "TB_EDITOR (password, filterNo, isDisplayed, texture, register_datetime, display_datetime) " +
                    "VALUES " +
                        "(@password, @filterNo, @isDisplayed, @texture, @register_datetime, @display_datetime)";

                using (SqliteCommand cmd = new SqliteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@password", password);
                    cmd.Parameters.AddWithValue("@filterNo", filterNo);
                    cmd.Parameters.AddWithValue("@isDisplayed", false);
                    cmd.Parameters.AddWithValue("@texture", texture);
                    cmd.Parameters.AddWithValue("@register_datetime", System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                    cmd.Parameters.AddWithValue("@display_datetime", "-");

                    cmd.ExecuteNonQuery();
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
    public int GetAvailablePassword()
    {
        int password;

        while (true)
        {
            password = UnityEngine.Random.Range(0, 10000);

            if (!passwordDictionary.ContainsKey(password))
            {
                passwordDictionary.Add(password, password);
                break;
            }
        }

        return password;
    }
    public bool IsRightPassword(int password)
    {
        return passwordDictionary.ContainsKey(password);
    }
}
