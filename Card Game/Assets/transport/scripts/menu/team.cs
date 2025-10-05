using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class team : MonoBehaviour
{
    public void LoadScene()
    {
        StartCoroutine(LoadSceneWithDelay(0.5f));
    }

    IEnumerator LoadSceneWithDelay(float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        SceneManager.LoadScene(2);
    }
}
