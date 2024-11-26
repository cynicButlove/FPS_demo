using UnityEngine;

public class OthersGunsChange : MonoBehaviour
{

    public string currentGunName ="";
    public void ChangeGun(string gunName,int flag)
    {
        if (flag == 0)
        {
            if (currentGunName != "")
            {
                transform.Find(gunName).gameObject.SetActive(false);
            }
            currentGunName = gunName;
            transform.Find(gunName).gameObject.SetActive(true);
        }
        else
        {
            transform.Find(gunName).gameObject.SetActive(false);
            currentGunName = "";
        }

    }
}