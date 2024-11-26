using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Start_Aim : MonoBehaviour
{
    bool started=false;
    public Transform BackGround;

    IEnumerator EndGame()
    {
        yield return new WaitForSeconds(33);
        started=false;
        BackGround.GetComponent<Aimlabs>().EndGame();
        
    }
    public void StartGame()
    {
        if (started) return;
        started=true;
        StartCoroutine(EndGame());
        BackGround.GetComponent<Aimlabs>().StartGame();

    }
}
