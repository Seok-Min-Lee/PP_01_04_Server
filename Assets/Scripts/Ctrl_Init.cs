using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ctrl_Init : MonoBehaviour
{
    private void Start()
    {
        Debug.Log("Server is Available? " + (Server.Instance != null));

        AudioManager.Instance.Load(() =>
        {
            AudioManager.Instance.Init(volumeBGM: 1f, volumeSFX: 1f);
        });

        Debug.Log(TouchManager.Instance.Canvas == null);

        UnityEngine.SceneManagement.SceneManager.LoadScene("01_Main");
    }
}
