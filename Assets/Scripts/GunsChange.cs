using System.Collections;
using System.Collections.Generic;
using protobuf;
using UnityEngine;

public class GunsChange : MonoBehaviour
{

    public List<GameObject> guns = new List<GameObject>();
    public int currentGunId;
    public int maxGunAmount;
    public Transform Guns;
    public Transform Player;
    // Start is called before the first frame update
    public int  client_id=0;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {     

        if (guns.Count > 0)
        {
            guns[currentGunId].SetActive(true);
            var gun = guns[currentGunId].name;
            ChangeGun();        
            if(Input.GetKeyDown(KeyCode.G))
            {
                DropGun();
                guns.RemoveAt(currentGunId);
                if(currentGunId==guns.Count)
                {
                    currentGunId--;
                    if(currentGunId<0)
                    {
                        currentGunId = 0;
                    }
                }
                var msg = new FullMessage()
                {
                    Header = new MessageHeader()
                    {
                        Type = MessageType.GunInfo,
                    },
                    GunInfo = new GunInfoMsg()
                    {
                        ClientId = client_id,
                        GunName = gun,
                        Throw = 1,    
                    }
                };
                MessageMgr.SendMessageToServer(msg);
            }
        }

        
    }

    public void ChangeGun()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0) gunAimOut();

        if (scroll < 0)
        {
            guns[currentGunId].SetActive(false);
            currentGunId=(currentGunId+1)%guns.Count;
            guns[currentGunId].SetActive(true);
        }
        else if(scroll>0){
            guns[currentGunId].SetActive(false);
            if (currentGunId == 0) currentGunId = guns.Count - 1;
            else currentGunId = currentGunId - 1;
            guns[currentGunId].SetActive(true);
        }
        for(int i = 1; i < 10; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha0+i))
            {
                if(i-1<guns.Count)
                {
                    if (currentGunId == i - 1) return;
                    gunAimOut();
                    guns[currentGunId].SetActive(false);
                    currentGunId = i-1;
                    guns[currentGunId].SetActive(true);
                }
            }
        }


    }
    public void gunAimOut()
    {
        if (guns[currentGunId].gameObject.name == "shotgun")
        {
            guns[currentGunId].GetComponent<Weapon_Shotgun>().AimOut();
        }
        else
        {
            guns[currentGunId].GetComponent<Weapon_AutomaticGun>().AimOut();
        }
    }
    public void PickUpGun(string name)
    {
        if (guns.Count < maxGunAmount)
        {
            GameObject gun =transform.Find(name).gameObject;
            guns.Add(gun);
            gun.SetActive(false);
        }else if(guns.Count==maxGunAmount)
        {
            DropGun();
            guns[currentGunId] = transform.Find(name).gameObject;
            guns[currentGunId].SetActive(true);
        }

        if (client_id == 0)
        {
            client_id=Player.gameObject.GetComponent<PlayerControl>().clientId;
        }
        var msg = new FullMessage()
        {
            Header = new MessageHeader()
            {
                Type = MessageType.GunInfo,
            },
            GunInfo = new GunInfoMsg()
            {
                ClientId = client_id,
                GunName = name,
                Throw = 0,
            }
            
        };
        MessageMgr.SendMessageToServer(msg);

    }
    public void DropGun()
    {
        if (guns.Count > 0)
        {
            Guns.Find(guns[currentGunId].name).position = Player.position + new Vector3(0f, 0.2f, 0.5f);
            Guns.Find(guns[currentGunId].name).localRotation = Quaternion.Euler(0, 0, 0);
            Guns.Find(guns[currentGunId].name).GetComponent<PickUpGuns>().isRotate = false;
            Guns.Find(guns[currentGunId].name).gameObject.SetActive(true);
            guns[currentGunId].SetActive(false);

        }
    }
}
