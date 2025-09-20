using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

public class Server : MonoSingleton<Server>
{
    private Telepathy.Server server = new Telepathy.Server(1920 * 1080 + 1024);
    private Dictionary<int, int> connectionIdMonitorIdDictionary = new Dictionary<int, int>();

    private Ctrl_Main ctrl => _ctrl ??= FindObjectOfType<Ctrl_Main>();
    private Ctrl_Main _ctrl;
    private void Awake()
    {
        // update even if window isn't focused, otherwise we don't receive.
        Application.runInBackground = true;

        // use Debug.Log functions for Telepathy so we can see it in the console
        Telepathy.Log.Info = Debug.Log;
        Telepathy.Log.Warning = Debug.LogWarning;
        Telepathy.Log.Error = Debug.LogError;

        server.OnConnected = (connectionId) => OnConnect(connectionId);
        server.OnData = (connectionId, message) => ReceiveMessage(connectionId, message);
        server.OnDisconnected = (connectionId) => OnDisconnect(connectionId);
    }

    private void Update()
    {
        if (server.Active)
        {
            // tick to process messages
            // (even if not active so we still process disconnect messages)
            server.Tick(1000);
        }
        else
        {
            server.Start(45604);
        }
    }

    private void OnApplicationQuit()
    {
        // the client/server threads won't receive the OnQuit info if we are
        // running them in the Editor. they would only quit when we press Play
        // again later. this is fine, but let's shut them down here for consistency
        server.Stop();
    }
    public void OnConnect(int connectionId)
    {
        Debug.Log($"{connectionId} Connected");
    }
    public void OnDisconnect(int connectionId)
    {
        Debug.Log($"{connectionId} Disconnected");

        int monitorId = connectionIdMonitorIdDictionary[connectionId];
        ctrl.RefreshDeviceMonitor(monitorId, false);

        connectionIdMonitorIdDictionary.Remove(connectionId);
    }
    public void ReceiveMessage(int connectionId, ArraySegment<byte> message)
    {
        // clear previous message
        byte[] messageBytes = new byte[message.Count];
        for (int i = 0; i < messageBytes.Length; i++)
        {
            messageBytes[i] = message.Array[i];
        }

        byte[] commandBytes = new byte[4];
        Buffer.BlockCopy(messageBytes, 0, commandBytes, 0, 4);
        int command = BitConverter.ToInt32(commandBytes);

        switch (command)
        {
                // Studio
            case ConstantValues.CMD_REQUEST_CONNECT_STUDIO:
                ResponseDeviceConnectResult(connectionId, 0);
                break;
            case ConstantValues.CMD_REQUEST_GET_PASSWORD:
                ResponseGetPassword(connectionId);
                break;
            case ConstantValues.CMD_REQUEST_ADD_STUDIO_DATA:
                ResponseAddStudioData(connectionId, ref messageBytes);
                break;
                // Editor
            case ConstantValues.CMD_REQUEST_CONNECT_EDITOR:
                ResponseDeviceConnectResult(connectionId, 1);
                break;
            case ConstantValues.CMD_REQUEST_CHECK_PASSWORD:
                ResponseCheckPassword(connectionId, ref messageBytes);
                break;
            case ConstantValues.CMD_REQUEST_GET_STUDIO_DATA:
                ResponseGetStudioData(connectionId, ref messageBytes);
                break;
            case ConstantValues.CMD_REQUEST_ADD_EDITOR_DATA:
                ResponseAddEditorData(connectionId, ref messageBytes);
                break;
                // Gallery
            case ConstantValues.CMD_REQUEST_CONNECT_GALLERY:
                ResponseDeviceConnectResult(connectionId, 2);
                break;
            case ConstantValues.CMD_REQUEST_GET_UNDISPLAYED_ID_LIST:
                ResponseGetUndisplayedIdList(connectionId, ref messageBytes);
                break;
            case ConstantValues.CMD_REQUEST_GET_EDITOR_DATA:
                ResponseGetEditorData(connectionId, ref messageBytes);
                break;
            case ConstantValues.CMD_REQUEST_UPDATE_DISPLAY_STATE:
                ResponseUpdateDisplayState(connectionId, ref messageBytes);
                break;
            default:
                break;
        }
    }
    private void ResponseDeviceConnectResult(int connectionId, int monitorId)
    {
        bool result = false;

        // UI Update
        if (!connectionIdMonitorIdDictionary.ContainsKey(connectionId))
        {
            connectionIdMonitorIdDictionary.Add(connectionId, monitorId);
            ctrl.RefreshDeviceMonitor(monitorId, true);

            result = true;
        }

        // Response Client
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            bw.Write(ConstantValues.CMD_RESPONSE_CONNECT_RESULT);
            bw.Write(result);

            server.Send(connectionId, ms.ToArray());
        }

        Debug.Log($"Response Connect Reulst::{connectionId}::{monitorId}::{result}");
    }
    private void ResponseGetPassword(int connectionId)
    {
        int password = -1;

        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            password = GetAvailablePassword();

            bw.Write(ConstantValues.CMD_RESPONSE_GET_PASSWORD);
            bw.Write(password);

            server.Send(connectionId, ms.ToArray());
        }

        Debug.Log($"Response Get Password::{connectionId}::{password}");
    }
    private void ResponseAddStudioData(int connectionId, ref byte[] message)
    {
        // Receive Data
        byte[] passwordBytes = new byte[4];
        Buffer.BlockCopy(message, 4, passwordBytes, 0, 4);
        int password = BitConverter.ToInt32(passwordBytes);

        byte[] lengthBytes = new byte[4];
        Buffer.BlockCopy(message, 8, lengthBytes, 0, 4);
        int length = BitConverter.ToInt32(lengthBytes);

        byte[] textureByte = new byte[length];
        Buffer.BlockCopy(message, 12, textureByte, 0, length);

        // Add Database
        bool bResult = DatabaseManager.instance.TryAddStudioData(
            password: password, 
            texture: textureByte, 
            sResult: out string sResult
        );

        // Response Cient
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            bw.Write(ConstantValues.CMD_RESPONSE_ADD_STUDIO_DATA_RESULT);
            bw.Write(bResult);

            server.Send(connectionId, ms.ToArray());
        }

        Debug.Log($"Response Add Studio Data Result::{connectionId}::{bResult}/{sResult}");
    }
    private void ResponseCheckPassword(int connectionId, ref byte[] message)
    {
        // Receive Data
        byte[] passwordBytes = new byte[4];
        Buffer.BlockCopy(message, 4, passwordBytes, 0, 4);
        int password = BitConverter.ToInt32(passwordBytes);

        // Check Password
        bool bResult = DatabaseManager.instance.PasswordDictionary.ContainsKey(password);

        // Response Client
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            bw.Write(ConstantValues.CMD_RESPONSE_CHECK_PASSWORD_RESULT);
            bw.Write(bResult);

            server.Send(connectionId, ms.ToArray());
        }

        Debug.Log($"Response Check Password::{connectionId}::{bResult}");
    }
    private void ResponseGetStudioData(int connectionId, ref byte[] message)
    {
        // Receive Data
        byte[] bytes = new byte[4];
        Buffer.BlockCopy(message, 4, bytes, 0, 4);
        int password = BitConverter.ToInt32(bytes);

        // Get Studio Data
        StudioDataRaw sdr = DatabaseManager.instance.GetStudioDataRaw(password);

        // Response Client
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            bw.Write(ConstantValues.CMD_RESPONSE_GET_STUDIO_DATA);
            bw.Write(sdr.ToBytes());

            server.Send(connectionId, ms.ToArray());
        }

        Debug.Log($"Response Get Studio Data::{connectionId}::{sdr.ToString()}");
    }
    private void ResponseAddEditorData(int connectionId, ref byte[] message)
    {
        // Receive Data
        byte[] headerLengthBytes = new byte[4];
        Buffer.BlockCopy(message, 4, headerLengthBytes, 0, 4);
        int headerLength = BitConverter.ToInt32(headerLengthBytes);

        byte[] headerBytes = new byte[headerLength];
        Buffer.BlockCopy(message, 8, headerBytes, 0, headerLength);
        string headerStr = Encoding.UTF8.GetString(headerBytes);

        // Data Parsing
        EditorDataRaw.Header header = JsonUtility.FromJson<EditorDataRaw.Header>(headerStr);

        byte[] textureBytes = new byte[header.TextureLength];
        Buffer.BlockCopy(message, 8 + headerLength, textureBytes, 0, header.TextureLength);

        EditorDataRaw raw = new EditorDataRaw(
            id: header.Id,
            password: header.Password,
            filterNo: header.FilterNo,
            stateNo: header.StateNo,
            registerDateTime: header.RegisterDateTime,
            releaseDateTime: header.ReleaseDateTime,
            displayDateTime: header.DisplayDateTime,
            textureRaw: textureBytes
        );

        // Add Database
        bool result = DatabaseManager.instance.TryAddEditorData(
            password: raw.Password, 
            filterNo: raw.FilterNo, 
            texture: raw.Texture, 
            sResult: out string sResult
        );

        // Response Client
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            bw.Write(ConstantValues.CMD_RESPONSE_ADD_EDITOR_DATA);
            bw.Write(result);

            server.Send(connectionId, ms.ToArray());
        }

        // Request Gallery
        if (result)
        {
            SendRequsetGetUndisplayedIdList();
        }

        Debug.Log($"Response Add Editor Data::{connectionId}::{result}/{sResult}");
    }
    private void ResponseGetUndisplayedIdList(int connectionId, ref byte[] message)
    {
        List<int> ids = DatabaseManager.instance.GetUnDisplayedIdList();

        // Response Client
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            bw.Write(ConstantValues.CMD_RESPONSE_GET_UNDISPLAYED_ID_LIST);
            bw.Write(ids.Count);

            for (int i = 0; i < ids.Count; i++)
            {
                bw.Write(ids[i]);
            }

            server.Send(connectionId, ms.ToArray());
        }

        Debug.Log($"Response Get Undisplayed Count::{connectionId}::{ids.Count}");
    }
    private void ResponseGetEditorData(int connectionId, ref byte[] message)
    {
        // Receive Data
        byte[] idBytes = new byte[4];
        Buffer.BlockCopy(message, 4, idBytes, 0, 4);
        int id = BitConverter.ToInt32(idBytes);

        EditorDataRaw edr = DatabaseManager.instance.GetEditorDataRaw(id);

        if (edr != null)
        {
            // Response Client
            using (MemoryStream ms = new MemoryStream())
            using (BinaryWriter bw = new BinaryWriter(ms))
            {
                bw.Write(ConstantValues.CMD_RESPONSE_GET_EDITOR_DATA);
                bw.Write(edr.ToBytes());

                server.Send(connectionId, ms.ToArray());
            }
        }

        Debug.Log($"Response Get Editor Data::{connectionId}::{edr}");
    }
    private void ResponseUpdateDisplayState(int connectionId, ref byte[] message)
    {
        // Receive Data
        byte[] idBytes = new byte[4];
        Buffer.BlockCopy(message, 4, idBytes, 0, 4);
        int id = BitConverter.ToInt32(idBytes);

        // Update Database
        bool result = DatabaseManager.instance.TryUpdateDisplayState(id, out string sResult);

        // Response Client
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            bw.Write(ConstantValues.CMD_RESPONSE_UPDATE_DISPLAY_STATE);
            bw.Write(id);
            bw.Write(result);

            server.Send(connectionId, ms.ToArray());
        }

        Debug.Log($"Response Update Display State::{connectionId}::{id}::{result}/{sResult}");
    }
    public void SendRequsetGetUndisplayedIdList()
    {
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            bw.Write(ConstantValues.CMD_SEND_REQUEST_GET_UNDISPLAYED_ID_LIST);

            //
            List<int> galleries = connectionIdMonitorIdDictionary.Where(kvp => kvp.Value == 2).Select(kvp => kvp.Key).ToList();

            for (int i = 0; i < galleries.Count; i++)
            {
                server.Send(galleries[i], ms.ToArray());
            }

            Debug.Log($"Send Request Get Undisplayed Id List::{string.Join(", ", galleries)}");
        }
    }
    private int GetAvailablePassword()
    {
        int password;
        Dictionary<int, int> dictionary = DatabaseManager.instance.PasswordDictionary;

        while (true)
        {
            password = UnityEngine.Random.Range(0, 10000);

            if (!dictionary.ContainsKey(password))
            {
                dictionary.Add(password, password);
                break;
            }
        }

        return password;
    }
}
