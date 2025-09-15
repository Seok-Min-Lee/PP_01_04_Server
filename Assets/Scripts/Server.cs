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
    private void Awake()
    {
        // update even if window isn't focused, otherwise we don't receive.
        Application.runInBackground = true;

        // use Debug.Log functions for Telepathy so we can see it in the console
        Telepathy.Log.Info = Debug.Log;
        Telepathy.Log.Warning = Debug.LogWarning;
        Telepathy.Log.Error = Debug.LogError;

        server.OnConnected = (connectionId) => Debug.Log(connectionId + " Connected");
        server.OnData = (connectionId, message) => ReceiveMessage(connectionId, message);
        server.OnDisconnected = (connectionId) => Debug.Log(connectionId + " Disconnected");
    }

    private void Update()
    {
        if (server.Active)
        {
            // tick to process messages
            // (even if not active so we still process disconnect messages)
            server.Tick(100);
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
            case ConstantValues.CMD_REQUEST_GET_PASSWORD:
                ResponseGetPassword(connectionId);
                break;
            case ConstantValues.CMD_REQUEST_ADD_STUDIO_DATA:
                ResponseAddStudioData(connectionId, ref messageBytes);
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
            case ConstantValues.CMD_REQUEST_GET_UNDISPLAYED_COUNT:
                ResponseGetUndisplayedCount(connectionId, ref messageBytes);
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
    public void ResponseGetPassword(int connectionId)
    {
        byte[] messages = new byte[8];

        int command = ConstantValues.CMD_RESPONSE_GET_PASSWORD;
        byte[] commandBytes = BitConverter.GetBytes(command);
        Buffer.BlockCopy(commandBytes, 0, messages, 0, 4);

        int password = DatabaseManager.instance.GetAvailablePassword();
        byte[] passwordBytes = BitConverter.GetBytes(password);
        Buffer.BlockCopy(passwordBytes, 0, messages, 4, 4);

        server.Send(connectionId, messages);
        Debug.Log($"Response Get Password::{connectionId}::{password}");
    }
    public void ResponseAddStudioData(int connectionId, ref byte[] message)
    {
        // Receive Studio Data
        byte[] passwordBytes = new byte[4];
        Buffer.BlockCopy(message, 4, passwordBytes, 0, 4);
        int password = BitConverter.ToInt32(passwordBytes);

        byte[] lengthBytes = new byte[4];
        Buffer.BlockCopy(message, 8, lengthBytes, 0, 4);
        int length = BitConverter.ToInt32(lengthBytes);

        byte[] textureByte = new byte[length];
        Buffer.BlockCopy(message, 12, textureByte, 0, length);

        bool bResult = DatabaseManager.instance.AddStudioData(
            password: password, 
            texture: textureByte, 
            sResult: out string sResult
        );

        // Response Result
        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            bw.Write(ConstantValues.CMD_RESPONSE_ADD_STUDIO_DATA_RESULT);
            bw.Write(bResult);

            server.Send(connectionId, ms.ToArray());
        }

        Debug.Log($"Response Add Studio Data Result::{connectionId}::{bResult}");
    }
    private void ResponseCheckPassword(int connectionId, ref byte[] message)
    {
        byte[] passwordBytes = new byte[4];
        Buffer.BlockCopy(message, 4, passwordBytes, 0, 4);
        int password = BitConverter.ToInt32(passwordBytes);

        bool bResult = DatabaseManager.instance.IsRightPassword(password);

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
        byte[] bytes = new byte[4];
        Buffer.BlockCopy(message, 4, bytes, 0, 4);
        int password = BitConverter.ToInt32(bytes);

        StudioDataRaw sdr = DatabaseManager.instance.GetStudioDataRaw(password);

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
        byte[] headerLengthBytes = new byte[4];
        Buffer.BlockCopy(message, 4, headerLengthBytes, 0, 4);
        int headerLength = BitConverter.ToInt32(headerLengthBytes);

        byte[] headerBytes = new byte[headerLength];
        Buffer.BlockCopy(message, 8, headerBytes, 0, headerLength);
        string headerStr = Encoding.UTF8.GetString(headerBytes);

        EditorDataRaw.Header header = JsonUtility.FromJson<EditorDataRaw.Header>(headerStr);

        byte[] textureBytes = new byte[header.TextureLength];
        Buffer.BlockCopy(message, 8 + headerLength, textureBytes, 0, header.TextureLength);

        EditorDataRaw raw = new EditorDataRaw(
            id: header.Id,
            password: header.Password,
            filterNo: header.FilterNo,
            isDisplayed: header.IsDisplayed,
            registerDateTime: header.RegisterDateTime,
            displayDateTime: header.DisplayDateTime,
            studioId: header.StudioId,
            textureRaw: textureBytes
        );

        bool result = DatabaseManager.instance.AddEditorData(
            password: raw.Password, 
            filterNo: raw.FilterNo, 
            texture: raw.Texture, 
            sResult: out string sResult
        );

        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            bw.Write(ConstantValues.CMD_RESPONSE_ADD_EDITOR_DATA);
            bw.Write(result);

            server.Send(connectionId, ms.ToArray());
        }

        Debug.Log($"Response Add Editor Data::{connectionId}::{result}");
    }
    private void ResponseGetUndisplayedCount(int connectionId, ref byte[] message)
    {
        int count = DatabaseManager.instance.GetUnDisplayedCount();

        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            bw.Write(ConstantValues.CMD_RESPONSE_GET_UNDISPLAYED_COUNT);
            bw.Write(count);

            server.Send(connectionId, ms.ToArray());
        }

        Debug.Log($"Response Get Undisplayed Count::{connectionId}::{count}");
    }
    private void ResponseGetEditorData(int connectionId, ref byte[] message)
    {
        EditorDataRaw edr = DatabaseManager.instance.GetEditorDataRaw();

        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            bw.Write(ConstantValues.CMD_RESPONSE_GET_EDITOR_DATA);
            bw.Write(edr.ToBytes());

            server.Send(connectionId, ms.ToArray());
        }

        Debug.Log($"Response Get Editor Data::{connectionId}::{edr}");
    }
    private void ResponseUpdateDisplayState(int connectionId, ref byte[] message)
    {
        byte[] idBytes = new byte[4];
        Buffer.BlockCopy(message, 4, idBytes, 0, 4);
        int id = BitConverter.ToInt32(idBytes);

        bool result = DatabaseManager.instance.TryUpdateDisplayState(id, out string sResult);

        using (MemoryStream ms = new MemoryStream())
        using (BinaryWriter bw = new BinaryWriter(ms))
        {
            bw.Write(ConstantValues.CMD_RESPONSE_UPDATE_DISPLAY_STATE);
            bw.Write(id);
            bw.Write(result);

            server.Send(connectionId, ms.ToArray());
        }

        Debug.Log($"Response Update Display State::{connectionId}::{id}::{result}");
    }
}
