using Mono.Data.Sqlite;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using UnityEngine;

public class DatabaseManager : MonoBehaviour
{
    public Dictionary<int, int> passwordDictionary = new Dictionary<int, int>();

    private SqliteConnection conn;
    private string connectionStr { get { return "URI=file:" + Application.streamingAssetsPath + "/db_PP_01.db"; } }
    public List<StudioData> GetStudioData(string query)
    {
        List<StudioData> views = new List<StudioData>();

        using (conn = new SqliteConnection(connectionStr))
        {
            conn.Open();

            using (IDbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = query;

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
    public List<EditorData> GetEditorData(string query)
    {
        List<EditorData> views = new List<EditorData>();

        using (conn = new SqliteConnection(connectionStr))
        {
            conn.Open();

            using (IDbCommand cmd = conn.CreateCommand())
            {
                cmd.CommandText = query;

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
    public bool AddStudioData(StudioDataSample sample, out string sResult)
    {
        try
        {
            using (conn = new SqliteConnection(connectionStr))
            {
                conn.Open();

                string query =
                    "INSERT INTO " +
                        "TB_STUDIO (id, texture, register_datetime) " +
                    "VALUES " +
                        "(@id, @texture, @register_datetime)";

                using (SqliteCommand cmd = new SqliteCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@id", sample.id);
                    cmd.Parameters.AddWithValue("@texture", sample.textureRaw);
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
    public bool AddEditorData(EditorDataSample sample, out string sResult)
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
                    cmd.Parameters.AddWithValue("@password", sample.password);
                    cmd.Parameters.AddWithValue("@filterNo", sample.filterNo);
                    cmd.Parameters.AddWithValue("@isDisplayed", false);
                    cmd.Parameters.AddWithValue("@texture", sample.texture.EncodeToPNG());
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
}
