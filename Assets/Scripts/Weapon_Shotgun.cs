using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]//���л�,������Inspector�������ʾ
public class shotgunSoundClips
{
    public AudioClip shootSound;//������Ч
    public AudioClip reload_open;
    public AudioClip reload_insert;
    public AudioClip reload_close;
    public AudioClip aimSound;//��׼��Ч
}

public class Weapon_Shotgun : Weapon
{
    private PlayerControl playerControl;
    public PlayerControl.MoveState state;
    public Camera gunCamera;
    public Camera mainCamera;
    public Transform Crosshair;
    private bool cameraChanged;
    [Header("�ӵ�λ��")]
    public Transform ShootPoint;//���ߴ����λ��
    public Transform BulletShootPoint;//�ӵ���Ч�����λ��
    public Transform CasingBulletSpawnPoint;//�ӵ����׳�λ��
    public Transform bulletPrefab;//�ӵ�Ԥ����
    public Transform casingPrefab;//�ӵ���Ԥ����
    [Header("��������")]
    public float range;//�������
    public float fireRate;//���ʱ����
    private float orignRate;//ԭʼ����
    private float SpreadFactor;//���ƫ����
    private float firetimer;//��ʱ����������������
    private float bulletForce;//�ӵ��������
    public bool isSilencer;//�Ƿ���������
    [Header("�����ӵ�")]
    public int bulletMag;//��������
    private int currentBullets;//��ǰ�����ӵ�
    public int bulletLeft;//����
    [Header("����������Ч")]
    public Light muzzleflashLight;//����ƹ�
    private float lightDuration;//�ƹ����ʱ��
    public ParticleSystem muzzlePatic;//����������Ч
    public ParticleSystem sparkPatic;//��������Ч,����
    public int minSparkEmission = 1;//��С�����ӷ���
    public int maxSparkEmission = 7;//�������ӷ���

    [Header("��Ч")]
    public AudioSource mainAudiioSource;
    public shotgunSoundClips soundClips;

    [Header("׼��")]
    public Image[] crossQuarterImgs;//�ķ�֮һ׼��
    private float currentExpanDegree;//��ǰ׼�����Ŷ�
    public float crossExpandDegree;//׼�����Ŷ�
    //private float maxCrossDegree;//���׼�����Ŷ�
    public TextMeshProUGUI ammoTxetUI;//��ҩUI
    public TextMeshProUGUI shootModeTextUI;//���ģʽUI

    [Header("��׼")]
    public bool isAiming;//�Ƿ���׼
    public float gunCameraAimFOV;//��׼ʱ�������Ұ
    public float gunCameraNormalFOV;//����ʱ�������Ұ
    public float mainCameraAimFOV;//��׼ʱ�������Ұ
    public float mainCameraNormalFOV;//����ʱ�������Ұ

    public ShootMode shootingMode;
    private bool GunShootInput;//�������밴�»򳤰�
    //private int modeNum;
    private string shootModeName;
    public enum ShootMode
    {
        Auto,
        //SemiAuto,
        Single
    }

    private Animator animator;//����������

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
            //�ر���׼
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

    //����ƹ�
    public IEnumerator MuzzleFlashLight()
    {
        muzzleflashLight.enabled = true;
        yield return new WaitForSeconds(lightDuration);//yield returnʵ��Э�̵���ͣ�ͼ���
        muzzleflashLight.enabled = false;
    }

    //����
    public override void GunFire()
    {
        bool currentAnimationState = animator.GetCurrentAnimatorStateInfo(0).IsName("take_out_weapon") ||
            animator.GetCurrentAnimatorStateInfo(0).IsName("reload_ammo_left") ||
            animator.GetCurrentAnimatorStateInfo(0).IsName("reload_out_of_ammo");

        if (firetimer < fireRate || currentBullets <= 0 || currentAnimationState) return;
        if (!isSilencer)
        {
            //����Э��
            StartCoroutine(MuzzleFlashLight());
            //����������Ч
            muzzlePatic.Emit(1);
            //��������Ч
            sparkPatic.Emit(Random.Range(minSparkEmission, maxSparkEmission));

        }
        //CrossFadeInFixedTime()��ʹ������Ϊ��λ��ʱ�䴴���ӵ�ǰ״̬���κ�����״̬�ĵ��뵭��Ч������һ�������Ƕ������ƣ��ڶ��������ǹ���ʱ��
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

        //�ӵ���ʵ����
        Instantiate(casingPrefab, CasingBulletSpawnPoint.position, CasingBulletSpawnPoint.rotation);

        //���������Ч
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
            Vector3 shootDirection = ShootPoint.forward;//��ǰ�����
            //���ֲ��ռ�����ƫ������ת��Ϊ����ռ��е�����
            shootDirection += ShootPoint.TransformDirection(new Vector3(Random.Range(-SpreadFactor, SpreadFactor), Random.Range(-SpreadFactor, SpreadFactor)));

                // �ӵ���ʼλ������ƫ��
                //��Ȼ�����ӵ�֮�䷢����ײ��5*6=30��
            Vector3 bulletSpawnPosition = BulletShootPoint.position + (Random.insideUnitSphere * 0.1f);

            // �ӵ�ʵ����
            Transform bullet = Instantiate(bulletPrefab, bulletSpawnPosition, BulletShootPoint.rotation);
            // �ӵ���������ٶȼ��ɷ���
            bullet.GetComponent<Rigidbody>().velocity = (bullet.transform.forward + shootDirection) * bulletForce;


        }
    }



    //׼�Ǳ����С
    public void ExpendCross(float add)
    {
        crossQuarterImgs[0].transform.localPosition += new Vector3(-add, 0, 0);
        crossQuarterImgs[1].transform.localPosition += new Vector3(add, 0, 0);
        crossQuarterImgs[2].transform.localPosition += new Vector3(0, add, 0);
        crossQuarterImgs[3].transform.localPosition += new Vector3(0, -add, 0);

        currentExpanDegree += add;//��ǰ׼�����Ŷ�
    }


    //�����ӵ���UI
    public void UpdateAmmoUI()
    {
        ammoTxetUI.text = currentBullets + " / " + bulletLeft;
        shootModeTextUI.text = shootModeName;
    }

    //����
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

            //animaotr.Play()�ǲ��Ŷ�������һ�������Ƕ������ƣ��ڶ��������Ƕ����㣬�����������Ƕ������ŵ�ʱ��
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
