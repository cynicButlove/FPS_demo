using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]//序列化,可以在Inspector面板中显示
public class shotgunSoundClips
{
    public AudioClip shootSound;//开火音效
    public AudioClip reload_open;
    public AudioClip reload_insert;
    public AudioClip reload_close;
    public AudioClip aimSound;//瞄准音效
}

public class Weapon_Shotgun : Weapon
{
    private PlayerControl playerControl;
    public PlayerControl.MoveState state;
    public Camera gunCamera;
    public Camera mainCamera;
    public Transform Crosshair;
    private bool cameraChanged;
    [Header("子弹位置")]
    public Transform ShootPoint;//射线打出的位置
    public Transform BulletShootPoint;//子弹特效打出的位置
    public Transform CasingBulletSpawnPoint;//子弹壳抛出位置
    public Transform bulletPrefab;//子弹预制体
    public Transform casingPrefab;//子弹壳预制体
    [Header("武器属性")]
    public float range;//武器射程
    public float fireRate;//射击时间间隔
    private float orignRate;//原始射速
    private float SpreadFactor;//射击偏移量
    private float firetimer;//计时器，控制武器射速
    private float bulletForce;//子弹发射的力
    public bool isSilencer;//是否有消音器
    [Header("弹夹子弹")]
    public int bulletMag;//弹夹容量
    private int currentBullets;//当前弹夹子弹
    public int bulletLeft;//备弹
    [Header("武器开火特效")]
    public Light muzzleflashLight;//开火灯光
    private float lightDuration;//灯光持续时间
    public ParticleSystem muzzlePatic;//开火粒子特效
    public ParticleSystem sparkPatic;//火花粒子特效,火星
    public int minSparkEmission = 1;//最小火花粒子发射
    public int maxSparkEmission = 7;//最大火花粒子发射

    [Header("音效")]
    public AudioSource mainAudiioSource;
    public shotgunSoundClips soundClips;

    [Header("准星")]
    public Image[] crossQuarterImgs;//四分之一准星
    private float currentExpanDegree;//当前准星扩张度
    public float crossExpandDegree;//准星扩张度
    //private float maxCrossDegree;//最大准星扩张度
    public TextMeshProUGUI ammoTxetUI;//弹药UI
    public TextMeshProUGUI shootModeTextUI;//射击模式UI

    [Header("瞄准")]
    public bool isAiming;//是否瞄准
    public float gunCameraAimFOV;//瞄准时的相机视野
    public float gunCameraNormalFOV;//正常时的相机视野
    public float mainCameraAimFOV;//瞄准时的相机视野
    public float mainCameraNormalFOV;//正常时的相机视野

    public ShootMode shootingMode;
    private bool GunShootInput;//开火输入按下或长按
    //private int modeNum;
    private string shootModeName;
    public enum ShootMode
    {
        Auto,
        //SemiAuto,
        Single
    }

    private Animator animator;//动画控制器

    private void Awake()
    {
        mainAudiioSource = GetComponent<AudioSource>();
        playerControl = GetComponentInParent<PlayerControl>();
        animator = GetComponent<Animator>();
    }
    private void Start()
    {
        shootingMode = ShootMode.Single;
        GunShootInput = Input.GetMouseButton(0);
        shootModeName = "Single";
        firetimer = 0f;
        bulletMag = 6;
        bulletLeft = bulletMag * 100;
        currentBullets = bulletMag;
        bulletForce = 100f;

        muzzleflashLight.enabled = false;
        lightDuration = 0.02f;
        isAiming = false;
        cameraChanged = false;

        UpdateAmmoUI();
    }

    private void Update()
    {
        state = playerControl.state;
        //Debug.Log(state);   


        fireRate = 0.3f;
        GunShootInput = Input.GetMouseButtonDown(0);
        
        if (state == PlayerControl.MoveState.Running || state == PlayerControl.MoveState.Jumping)
        {
            //关闭瞄准
            AimOut();

            ExpaningCrossUpdate(crossExpandDegree * 2);
            if (GunShootInput)
            {
                SpreadFactor = 0.4f;
                GunFire();
                ExpaningCrossUpdate(crossExpandDegree * 3);
            }
        }
        else if (state == PlayerControl.MoveState.Walking)
        {
            ExpaningCrossUpdate(crossExpandDegree);
            if (GunShootInput)
            {
                SpreadFactor = 0.3f;
                GunFire();
                ExpaningCrossUpdate(crossExpandDegree * 2);
            }
        }
        else//idle or crouching
        {
            ExpendCross(-currentExpanDegree);
            if (GunShootInput)
            {
                SpreadFactor = 0.1f;
                GunFire();
                ExpaningCrossUpdate(crossExpandDegree);
            }
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            DoReloadAnimation();
        }

        animator.SetBool("Run", state == PlayerControl.MoveState.Running);
        animator.SetBool("Walk", state == PlayerControl.MoveState.Walking);
        if (Input.GetKeyDown(KeyCode.J) && state == PlayerControl.MoveState.idle) { animator.SetTrigger("Inspect"); }

        if (Input.GetMouseButtonDown(1))
        {
            if (isAiming)
            {
                isAiming = false;
                animator.SetBool("Aim", false);
            }
            else
            {
                isAiming = true;
                animator.SetBool("Aim", true);
            }

        }

        if (firetimer < fireRate)
        {
            firetimer += Time.deltaTime;
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
    public override void GunFire()
    {
        bool currentAnimationState = animator.GetCurrentAnimatorStateInfo(0).IsName("take_out_weapon") ||
            animator.GetCurrentAnimatorStateInfo(0).IsName("reload_ammo_left") ||
            animator.GetCurrentAnimatorStateInfo(0).IsName("reload_out_of_ammo");

        if (firetimer < fireRate || currentBullets <= 0 || currentAnimationState) return;
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
            if (!cameraChanged) { AimIn(); }
            animator.Play("aim_fire", 0);
        }
        else
        {
            if (cameraChanged) { AimOut(); }
            animator.CrossFadeInFixedTime("fire", 0.1f);
        }


        StartCoroutine(GenerateBullets());

        //子弹壳实例化
        Instantiate(casingPrefab, CasingBulletSpawnPoint.position, CasingBulletSpawnPoint.rotation);

        //播放射击音效
        mainAudiioSource.clip =  soundClips.shootSound;
        mainAudiioSource.Play();


        currentBullets--;
        firetimer = 0f;

        UpdateAmmoUI();
    }

    public IEnumerator GenerateBullets()
    {
        yield return null;

        for (int i = 0; i < 6; i++)
        {
            RaycastHit hit;
            Vector3 shootDirection = ShootPoint.forward;//向前方射击
            //将局部空间的随机偏移向量转换为世界空间中的向量
            shootDirection += ShootPoint.TransformDirection(new Vector3(Random.Range(-SpreadFactor, SpreadFactor), Random.Range(-SpreadFactor, SpreadFactor)));

                // 子弹初始位置稍作偏移
                //不然这里子弹之间发生碰撞，5*6=30次
            Vector3 bulletSpawnPosition = BulletShootPoint.position + (Random.insideUnitSphere * 0.1f);

            // 子弹实例化
            Transform bullet = Instantiate(bulletPrefab, bulletSpawnPosition, BulletShootPoint.rotation);
            // 子弹刚体给个速度即可发射
            bullet.GetComponent<Rigidbody>().velocity = (bullet.transform.forward + shootDirection) * bulletForce;


        }
    }



    //准星变大或变小
    public void ExpendCross(float add)
    {
        crossQuarterImgs[0].transform.localPosition += new Vector3(-add, 0, 0);
        crossQuarterImgs[1].transform.localPosition += new Vector3(add, 0, 0);
        crossQuarterImgs[2].transform.localPosition += new Vector3(0, add, 0);
        crossQuarterImgs[3].transform.localPosition += new Vector3(0, -add, 0);

        currentExpanDegree += add;//当前准星扩张度
    }


    //更新子弹的UI
    public void UpdateAmmoUI()
    {
        ammoTxetUI.text = currentBullets + " / " + bulletLeft;
        shootModeTextUI.text = shootModeName;
    }

    //换弹
    public override void Reload()
    {
        if (currentBullets < bulletMag && bulletLeft > 0)
        {
            currentBullets++;
            mainAudiioSource.clip = soundClips.reload_insert;
            mainAudiioSource.Play();
            animator.SetInteger("CurrentBullets", currentBullets);
            UpdateAmmoUI();
        }
    }
    public void ReloadClose()
    {
        mainAudiioSource.clip = soundClips.reload_close;
        mainAudiioSource.Play();
        animator.SetBool("Reloading", false);
        UpdateAmmoUI();
    }
    public override void DoReloadAnimation()
    {
        if (state == PlayerControl.MoveState.Jumping) return;
        if (isAiming)
        {
            AimOut();
        }
        if (currentBullets < bulletMag && bulletLeft > 0)
        {

            //animaotr.Play()是播放动画，第一个参数是动画名称，第二个参数是动画层，第三个参数是动画播放的时间
            animator.Play("reload_open", 0, 0);
            mainAudiioSource.clip = soundClips.reload_open;
            mainAudiioSource.Play();
            animator.SetBool("Reloading",true);
            animator.SetInteger("CurrentBullets", currentBullets);

            Reload();
        }

    }

    public override void ExpaningCrossUpdate(float expanDegree)
    {
        if (currentExpanDegree < expanDegree)
        {
            //Debug.Log("currentExpanDegree1:"+currentExpanDegree);
            ExpendCross(150 * Time.deltaTime);
        }
        else if (currentExpanDegree > expanDegree)
        {
            ExpendCross(-500 * Time.deltaTime);
            //Debug.Log("currentExpanDegree2:"+currentExpanDegree);
        }
    }


    public override void AimIn()
    {
        Crosshair.gameObject.SetActive(false);
        gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView, gunCameraAimFOV, 1f);
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, mainCameraAimFOV, 1f);
        mainAudiioSource.clip = soundClips.aimSound;
        mainAudiioSource.Play();
        cameraChanged = true;
    }

    public override void AimOut()
    {
        isAiming = false;
        animator.SetBool("Aim", false);
        Crosshair.gameObject.SetActive(true);
        gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView, gunCameraNormalFOV, 1f);
        mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, mainCameraNormalFOV, 1f);
        cameraChanged = false;
    }



}
