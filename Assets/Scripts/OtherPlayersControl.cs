using System.Collections;
using System.Collections.Generic;
using protobuf;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.UI;

public class OtherPlayersControl : MonoBehaviour
{
    public int clientId;
    public string userName;
    public int currentHealth;

    public Scrollbar healthBar;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
    
    public void Init(ClientMsg msg)
    {
        clientId = msg.ClientId;
        userName = msg.Username;
        currentHealth = msg.Health;
        gameObject.name = "OtherPlayers_" + clientId;
        healthBar.size = 1;
        if (msg.GunName != "None")
        {
            transform.Find("Assult_Rife_Arm").Find("Different_Weapons").Find(msg.GunName).gameObject.SetActive(true);
        }
    }
    
    public void UpdatePlayerPosition(ClientMsg msg)
    {
        Vector3 targetPosition = new Vector3(msg.Position.X, msg.Position.Y, msg.Position.Z);
        Quaternion targetRotation = Quaternion.Euler(msg.Position.RotationX, msg.Position.RotationY, msg.Position.RotationZ);
        Quaternion targetGunRotation = Quaternion.Euler(msg.Position.GunRotationX, msg.Position.GunRotationY, msg.Position.GunRotationZ);

        transform.position = Vector3.Lerp(transform.position, targetPosition, Constants.SendDelay * 5f);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Constants.SendDelay * 5f);
        transform.GetChild(0).rotation = Quaternion.Lerp(transform.GetChild(0).rotation, targetGunRotation, Constants.SendDelay * 5f);
    }
    
    
    public void BeBulletHit(BulletHitMsg msg)
    {
        currentHealth = msg.Health;
        healthBar.size = currentHealth / 100f;
    }
}
