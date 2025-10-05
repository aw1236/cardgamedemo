using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class quit : MonoBehaviour
{
    public void Gameover()
    {
        StartCoroutine(LoadSceneWithDelay(0.5f));
    }

    IEnumerator LoadSceneWithDelay(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        Application.Quit();
    }
}
