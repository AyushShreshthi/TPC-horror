using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartAndEndScript : MonoBehaviour
{
    public void Exit()
    {
        Application.Quit();

    }
    public void StartBtn()
    {
        SceneManager.LoadScene(1);
    }
}
