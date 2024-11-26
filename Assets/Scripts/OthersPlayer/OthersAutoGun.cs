using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]//序列化,可以在Inspector面板中显示
public class OthersSoundClips
{
    public AudioClip shootSound;//开火音效
    public AudioClip silencerShootSound;//消音器开火音效
    public AudioClip reloadSoundAmmoLeft;//换弹音效
    public AudioClip reloadSoundOutOfAmmo;//换弹音效并拉枪栓
}

public class OthersAutoGun :MonoBehaviour
{
    
    public int bulletForce;
    [Header("子弹位置")]
    public Transform BulletShootPoint;//子弹特效打出的位置
    public Transform CasingBulletSpawnPoint;//子弹壳抛出位置
    public Transform bulletPrefab;//子弹预制体
    public Transform casingPrefab;//子弹壳预制体
 
    [Header("武器开火特效")]
    public Light muzzleflashLight;//开火灯光
    private float lightDuration;//灯光持续时间
    public ParticleSystem muzzlePatic;//开火粒子特效
    public ParticleSystem sparkPatic;//火花粒子特效,火星
    public int minSparkEmission = 1;//最小火花粒子发射
    public int maxSparkEmission = 7;//最大火花粒子发射

    [Header("音效")]
    public AudioSource mainAudiioSource;
    public OthersSoundClips othersSoundClips;

    private Animator animator;//动画控制器
    public bool isSilencer ;

    private int client_id;
    
    private void Start()
    {
        mainAudiioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    private bool isAiming;
    public void WhenAnimatorParam(AnimatorParamMsg msg)
    {
        var paramName = msg.ParamName;
        var value = msg.Value;
        switch (paramName)
        {
            case "Run":
            {
                animator.SetBool("Run", true);
                animator.SetBool("Walk", false);
                break;
            }
            case "Walk":
            {
                animator.SetBool("Walk", true);
                animator.SetBool("Run", false);
                break;
            }
            case "!RunOrWalk":
            {
                animator.SetBool("Run", false);
                animator.SetBool("Walk", false);
                break;
            }
            case "Inspect":
            {
                animator.SetTrigger("Inspect");
                break;
            }
            case "Aim":
            {
                animator.SetBool("Aim", value==1);
                isAiming = value == 1;
                break;
            }
            default:
                break;
        }
    }
    

    //开火灯光
    public IEnumerator MuzzleFlashLight()
    {
        muzzleflashLight.enabled = true;
        yield return new WaitForSeconds(lightDuration);//yield return实现协程的暂停和继续
        muzzleflashLight.enabled = false;
    }

    //开火
    public void GunFire(GunFireMsg msg)
    {
        if (!isSilencer)
        {
            //启功协程
            StartCoroutine(MuzzleFlashLight());
            //开火粒子特效
            muzzlePatic.Emit(1);
            //火花粒子特效
            sparkPatic.Emit(Random.Range(minSparkEmission, maxSparkEmission));
        }
        //CrossFadeInFixedTime()是使用以秒为单位的时间创建从当前状态到任何其他状态的淡入淡出效果，第一个参数是动画名称，第二个参数是过渡时间
        if (isAiming)
        {
            animator.Play("aim_fire", 0);
        }
        else
        {
            animator.CrossFadeInFixedTime("fire", 0.1f);
        }
        
        Vector3 shootDirection = new Vector3(msg.ShootDirectionX, msg.ShootDirectionY, msg.ShootDirectionZ);

        //子弹实例化 
        Transform bullet = Instantiate(bulletPrefab, BulletShootPoint.position, BulletShootPoint.rotation);
        //子弹刚体给个速度即可发射
        bullet.GetComponent<Rigidbody>().velocity = (bullet.transform.forward + shootDirection) * bulletForce;

        //子弹壳实例化
        Instantiate(casingPrefab, CasingBulletSpawnPoint.position, CasingBulletSpawnPoint.rotation);

        //播放射击音效
        mainAudiioSource.clip = isSilencer ? othersSoundClips.silencerShootSound : othersSoundClips.shootSound;
        mainAudiioSource.Play();

    }


    public  void DoReloadAnimation(ReloadBulletMsg msg)
    {
       int currentBullets = msg.BulletCount;
        if(currentBullets==0)
        {
            //animaotr.Play()是播放动画，第一个参数是动画名称，第二个参数是动画层，第三个参数是动画播放的时间
            animator.Play("reload_out_of_ammo",0,0);
            mainAudiioSource.clip = othersSoundClips.reloadSoundOutOfAmmo;
            mainAudiioSource.Play();
        }
        else
        {
            animator.Play("reload_ammo_left",0,0);
            mainAudiioSource.clip = othersSoundClips.reloadSoundAmmoLeft;
            mainAudiioSource.Play();
        }
        

    }


    


}
