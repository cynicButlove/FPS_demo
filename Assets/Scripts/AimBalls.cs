using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimBalls : MonoBehaviour
{
    private GameObject targetPrefab;
    private Transform Background;
    // Start is called before the first frame update
    void Start()
    {
        targetPrefab = Resources.Load<GameObject>("AimBall");
        Background = GameObject.Find("Aimlabs").transform.Find("BackGround");
        StartCoroutine(DestoryAfter());
    }
    private void Update()
    {
        if (Background.GetComponent<Aimlabs>().end)
        {
            Destroy(gameObject);
        }
    }
    private IEnumerator DestoryAfter()
    {
        yield return new WaitForSeconds(3f);
        Background.GetComponent<Aimlabs>().scoreNum--;
        DestroySelf();
    }

    public void DestroySelf()
    {
        Background.GetComponent<Aimlabs>().scoreNum++;
        Background.GetComponent<Aimlabs>().score.text = "Score: " + Background.GetComponent<Aimlabs>().scoreNum;
        //生成新的
        GameObject target = Instantiate(gameObject);
        target.transform.position = new Vector3(-15 + Random.Range(-10, 10), Random.Range(10, 20), 55 + Random.Range(-2, 2));
        Destroy(gameObject);
    }

}
