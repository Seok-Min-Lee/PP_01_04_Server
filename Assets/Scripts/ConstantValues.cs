using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ConstantValues
{
    public const int CMD_REQUEST_GET_PASSWORD = 1000;
    public const int CMD_RESPONSE_GET_PASSWORD = 1001;
    public const int CMD_REQUEST_ADD_STUDIO_DATA = 1002;
    public const int CMD_RESPONSE_ADD_STUDIO_DATA_RESULT = 1003;

    public const int CMD_REQUEST_CHECK_PASSWORD = 2000;
    public const int CMD_RESPONSE_CHECK_PASSWORD_RESULT = 2001;
    public const int CMD_REQUEST_GET_STUDIO_DATA = 2002;
    public const int CMD_RESPONSE_GET_STUDIO_DATA = 2003;
    public const int CMD_REQUEST_ADD_EDITOR_DATA = 2004;
    public const int CMD_RESPONSE_ADD_EDITOR_DATA = 2005;

    public const int CMD_REQUEST_GET_UNDISPLAYED_COUNT = 3000;
    public const int CMD_RESPONSE_GET_UNDISPLAYED_COUNT = 3001;
    public const int CMD_REQUEST_GET_EDITOR_DATA = 3002;
    public const int CMD_RESPONSE_GET_EDITOR_DATA = 3003;
    public const int CMD_REQUEST_UPDATE_DISPLAY_STATE = 3004;
    public const int CMD_RESPONSE_UPDATE_DISPLAY_STATE = 3005;

    public const int CMD_REQUEST_CONNECT_STUDIO = 1100;
    public const int CMD_REQUEST_CONNECT_EDITOR = 2100;
    public const int CMD_REQUEST_CONNECT_GALLERY = 3100;
    public const int CMD_RESPONSE_CONNECT_RESULT = 9100;
}
