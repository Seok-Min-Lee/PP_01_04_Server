using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

[Serializable]
public class StudioDataRaw
{
    public StudioDataRaw(int id, int password, string registerDateTime, byte[] textureRaw)
    {
        this.id = id;
        this.password = password;
        this.registerDateTime = registerDateTime;
        this.textureRaw = textureRaw;
    }
    public int Id => id;
    public int Password => password;
    public string RegisterDateTime => registerDateTime;
    public byte[] TextureRaw => textureRaw;
    [SerializeField] private int id;
    [SerializeField] private int password;
    [SerializeField] private string registerDateTime;
    [NonSerialized] private byte[] textureRaw;
    public override string ToString()
    {
        return JsonUtility.ToJson(this, true);
    }
    public byte[] ToBytes()
    {
        int textureLength = textureRaw != null ? textureRaw.Length : 0;

        Header headerObj = new Header(
            id,
            password,
            registerDateTime,
            textureLength
        );

        string headerJson = JsonUtility.ToJson(headerObj);
        byte[] headerBytes = Encoding.UTF8.GetBytes(headerJson);

        int headerLength = headerBytes.Length;
        byte[] headerLengthBytes = BitConverter.GetBytes(headerLength);

        byte[] packet = new byte[4 + headerLength + textureLength];

        Buffer.BlockCopy(headerLengthBytes, 0, packet, 0, 4);
        Buffer.BlockCopy(headerBytes, 0, packet, 4, headerLength);

        if (textureRaw.Length > 0)
        {
            Buffer.BlockCopy(textureRaw, 0, packet, 4 + headerLength, textureRaw.Length);
        }

        return packet;
    }

    [Serializable]
    public class Header
    {
        public Header(int id, int password, string registerDateTime, int textureLength)
        {
            this.id = id;
            this.password = password;
            this.registerDateTime = registerDateTime;
            this.textureLength = textureLength;
        }
        public int Id => id;
        public int Password => password;
        public string RegisterDateTime => registerDateTime;
        public int TextureLength => textureLength;

        [SerializeField] private int id;
        [SerializeField] private int password;
        [SerializeField] private string registerDateTime;
        [SerializeField] private int textureLength;
    }
}
