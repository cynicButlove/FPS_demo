using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RankList : MonoBehaviour
{

    public List<TMP_Text> tops;
    public void UpdateRankList(RankListMsg msg)
    {
        //clear
        tops[0].text = "";
        tops[1].text = "";
        tops[2].text = "";
        var lists = msg.RankList;
        for (int i = 0; i < lists.Count; i++)
        {
            var rank = lists[i];
            var client_id = rank.ClientId;
            var score=rank.Score;
            tops[i].text =  client_id + " : " + score;
        }
        
    }
}
