using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public class Server : MonoBehaviour
{
    private Telepathy.Server server = new Telepathy.Server(1920 * 1080 + 1024);
    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        // update even if window isn't focused, otherwise we don't receive.
        Application.runInBackground = true;

        // use Debug.Log functions for Telepathy so we can see it in the console
        Telepathy.Log.Info = Debug.Log;
        Telepathy.Log.Warning = Debug.LogWarning;
        Telepathy.Log.Error = Debug.LogError;

        server.OnConnected = (connectionId) => Debug.Log(connectionId + " Connected");
        //server.OnData = (connectionId, message) => Debug.Log(connectionId + " Data: " + BitConverter.ToString(message.Array, message.Offset, message.Count));
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
        string sMessage = Encoding.UTF8.GetString(message);
        Debug.Log(connectionId + "/" + sMessage);

        //// clear previous message
        //byte[] messageBytes = new byte[message.Count];
        //for (int i = 0; i < messageBytes.Length; i++)
        //{
        //    messageBytes[i] = message.Array[i];
        //}

        //byte[] commandBytes = new byte[4];
        //Array.Copy(messageBytes, 0, commandBytes, 0, 4);
        //int command = BitConverter.ToInt32(commandBytes);
    }
}
