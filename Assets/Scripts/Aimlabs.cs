using System.Collections;
using System.Collections.Generic;
using protobuf;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class Aimlabs : MonoBehaviour
{
    public TextMeshProUGUI timeleft;
    public TextMeshProUGUI score;
    public TextMeshProUGUI time321;
    public TextMeshProUGUI historyscore;
    //��¼��ʷ������txt
    public string historyscorePath="Assets/Scripts/HistoryScore.txt";
    //target��prefab
    public GameObject targetPrefab;
    public int scoreNum=0;
    public bool end=false;

    private int client_id;
    // Start is called before the first frame update
    void Start()
    {
        if(!System.IO.File.Exists(historyscorePath))
        {
            System.IO.File.Create(historyscorePath);
            System.IO.File.WriteAllText(historyscorePath,"0");
        }
        string historyscoreStr=System.IO.File.ReadAllText(historyscorePath);
        historyscore.text="History Highest Score: "+historyscoreStr;
    }

    IEnumerator waitForSecond(int i)
    {
        yield return new WaitForSeconds(i);
    }
    IEnumerator StartTime321()
    {
        //����ʱ321
        time321.gameObject.SetActive(true);
        for(int i=3;i>0;i--)
        {
            time321.text=i.ToString();
            yield return new WaitForSeconds(1);
        }
        time321.text="";
        GenTarget(3);
        score.gameObject.SetActive(true);
        score.text="Score: 0";

    }
    private void GenTarget(int num)
    {
        for(int i=0;i<num;i++)
        {
            GameObject target=Instantiate(targetPrefab);
            target.transform.position=new Vector3(-15+Random.Range(-10,10),Random.Range(10,20),55+Random.Range(-2,2));
        }

    }
    IEnumerator TimeLeft()
    {
        yield return new WaitForSeconds(3);
        int i;
        for( i=30;i>0;i--)
        {
            timeleft.text="Time Left: "+i.ToString();
            yield return new WaitForSeconds(1);
        }
    }

    public void StartGame()
    {
        scoreNum=0;
        end=false;
        StartCoroutine(StartTime321());
        StartCoroutine(TimeLeft());
        string historyscoreStr=System.IO.File.ReadAllText(historyscorePath);
        historyscore.text="History Highest Score: "+historyscoreStr;

    }
    public void EndGame()
    {            
        timeleft.text="";
        score.gameObject.SetActive(false);
        end = true;
        time321.text="score: "+scoreNum.ToString();
        string historyscoreStr=System.IO.File.ReadAllText(historyscorePath);
        int historyscoreNum=int.Parse(historyscoreStr);
        if(scoreNum>historyscoreNum)
        {
            System.IO.File.WriteAllText(historyscorePath,scoreNum.ToString());
            time321.text+="\nNew Highest Score!";
            historyscore.text="History Highest Score: "+scoreNum.ToString(); 
        }

        if (client_id <= 0)
        {
            client_id = GameObject.Find("PlayerParent").transform.Find("Player").GetComponent<PlayerControl>().clientId;
        }
        var msg = new FullMessage()
        {
            Header = new MessageHeader()
            {
                Type = MessageType.RankScore
            },
            RankScore = new RankScoreMsg()
            {
                ClientId = client_id,
                Score = scoreNum
            }
        };
        MessageMgr.SendMessageToServer(msg);
    }
}
