using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


public class PickUpGuns : MonoBehaviour
{
    public TextMeshProUGUI pickUpText;
    public Transform Different_Weapons;
    public bool isPickedUp = false;
    public bool isRotate = true;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (isPickedUp&& Input.GetKeyDown(KeyCode.F))
        {
            gameObject.SetActive(false);
            pickUpText.gameObject.SetActive(false);
            Different_Weapons.GetComponent<GunsChange>().PickUpGun(gameObject.name);

        }
        if (isRotate)
        {
            Rotate();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
        
            pickUpText.gameObject.SetActive(true);
            pickUpText.text = "Pick Up " + gameObject.name;
            isPickedUp = true;
            
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            pickUpText.gameObject.SetActive(false);
            isPickedUp = false;
        }
    }
    public void Rotate()
    {
        transform.localEulerAngles+=new Vector3(0,0,120)*Time.deltaTime;
    }


}
