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

    private Dictionary<int, int> passwordDictionary = new Dictionary<int, int>();
    private Dictionary<int, StudioData> studioDataDictionary = new Dictionary<int, StudioData>();
    private Dictionary<int, EditorData> editorDataDictionary = new Dictionary<int, EditorData>();

    private SqliteConnection conn;
    private string connectionStr { get { return "URI=file:" + Application.streamingAssetsPath + "/db_PP_01.db"; } }
    public void RefreshStudioData()
    {
        studioDataDictionary = GetStudioData();
        passwordDictionary = studioDataDictionary.Values.Select(x => x.password).ToDictionary(k => k, v => v);
    }
    public void RefreshEditorData()
    {
        editorDataDictionary = GetEditorData();
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

                string query = "UPDATE TB_EDITOR SET IsDisplayed = @IsDisplayed, Display_datetime = @Display_datetime WHERE id = @Id";

                using (SqliteCommand cmd = new SqliteCommand(query, conn))
                {
                    int isDisplayed = 2;
                    string displayeDatetime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                    cmd.Parameters.AddWithValue("@IsDisplayed", isDisplayed);
                    cmd.Parameters.AddWithValue("@Display_datetime", displayeDatetime);
                    cmd.Parameters.AddWithValue("@Id", id);

                    cmd.ExecuteNonQuery();

                    editorDataDictionary[id].SetDisplayDateTime(displayeDatetime);
                    GameObject.Find("Ctrl").GetComponent<Ctrl_Main>().RefreshView();
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
    private Dictionary<int, StudioData> GetStudioData()
    {
        Dictionary<int, StudioData> dictionary = new Dictionary<int, StudioData>();

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

        return dictionary;
    }
    private Dictionary<int, EditorData> GetEditorData()
    {
        Dictionary<int, EditorData> dictionary = new Dictionary<int, EditorData>();

        using (conn = new SqliteConnection(connectionStr))
        {
            conn.Open();

            using (IDbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = "SELECT id, password, register_datetime, display_datetime FROM TB_EDITOR ORDER BY id ASC";

                using (IDataReader reader = cmd.ExecuteReader())
                {
                    int index = 1;
                    while (reader.Read())
                    {
                        int id = reader.GetInt32(0);
                        int password = reader.GetInt32(1);
                        string registerDateTime = reader.GetString(2);
                        string displayDateTime = reader.GetString(3);

                        EditorData data = new EditorData(
                            index: index++,
                            id: id,
                            password: password,
                            registerDateTime: registerDateTime,
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
