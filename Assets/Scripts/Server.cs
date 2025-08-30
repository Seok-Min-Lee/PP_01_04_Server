using System;
using System.Collections;
using System.Collections.Generic;
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
            server.Tick(10);
        }
        else
        {
            server.Start(45604);
        }

        if (Input.GetKeyDown(KeyCode.A))
        {
            string str = "Message from Server";
            byte[] bytes = Encoding.UTF8.GetBytes(str);
            server.Send(1, bytes);
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
        Array.Copy(messageBytes, 0, commandBytes, 0, 4);
        int command = BitConverter.ToInt32(commandBytes);

        switch (command)
        {
            case ConstantValues.CMD_REQUEST_PASSWORD:
                ResponsePassword(connectionId);
                break;
            case ConstantValues.CMD_SEND_STUDIO_DATA:
                ReceiveStudioData(ref messageBytes);
                break;
            default:
                break;
        }
    }

    public void ResponsePassword(int connectionId)
    {
        byte[] messages = new byte[8];

        int command = ConstantValues.CMD_RESPONSE_PASSWORD;
        byte[] commandBytes = BitConverter.GetBytes(command);
        Array.Copy(commandBytes, 0, messages, 0, 4);

        int password = UnityEngine.Random.Range(0, 10000);
        byte[] passwordBytes = BitConverter.GetBytes(password);
        Array.Copy(passwordBytes, 0, messages, 4, 4);

        server.Send(connectionId, messages);
    }

    public void ReceiveStudioData(ref byte[] message)
    {
        byte[] passwordBytes = new byte[4];
        Array.Copy(message, 4, passwordBytes, 0, 4);
        int password = BitConverter.ToInt32(passwordBytes);

        byte[] lengthBytes = new byte[4];
        Array.Copy(message, 8, lengthBytes, 0, 4);
        int length = BitConverter.ToInt32(lengthBytes);

        byte[] textureByte = new byte[length];
        Array.Copy(message, 12, textureByte, 0, length);

        StudioDataSample sds = new StudioDataSample();
        sds.id = password;
        sds.textureRaw = textureByte;
        DatabaseManager.instance.AddStudioData(sds, out string sResult);
    }
}
