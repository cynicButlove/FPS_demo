using System.Collections;
using System.Collections.Generic;
using protobuf;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]//���л�,������Inspector�������ʾ
public class SoundClips
{
    public AudioClip shootSound;//������Ч
    public AudioClip silencerShootSound;//������������Ч
    public AudioClip reloadSoundAmmoLeft;//������Ч
    public AudioClip reloadSoundOutOfAmmo;//������Ч����ǹ˨
    public AudioClip aimSound;//��׼��Ч
}

public class Weapon_AutomaticGun : Weapon
{
    private PlayerControl playerControl;
    public PlayerControl.MoveState state;
    public Camera gunCamera;
    public Camera mainCamera;
    public Transform Crosshair;
    public Transform Assult_Rife_Arm;
    private bool cameraChanged;
    [Header("�ӵ�λ��")]
    public Transform ShootPoint;//���ߴ����λ��
    public Transform BulletShootPoint;//�ӵ���Ч�����λ��
    public Transform CasingBulletSpawnPoint;//�ӵ����׳�λ��
    public Transform bulletPrefab;//�ӵ�Ԥ����
    public Transform casingPrefab;//�ӵ���Ԥ����
    [Header("��������")]
    public float range;//�������
    public float FireRate;//���ʱ����
    private float fireRate;//���ʱ����
    private float orignRate;//ԭʼ����
    private float SpreadFactor;//���ƫ����
    private float firetimer;//��ʱ����������������
    public float continueFireTimer;//���������ʱ��
    public float recoilForce;//������ϵ��
    private float bulletForce;//�ӵ��������
    public bool isSilencer;//�Ƿ���������
    public int maxDamage;//����˺�
    public int minDamage;//��С�˺�
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
    public SoundClips soundClips;

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
    public bool notAllowB;
    private bool GunShootInput;//�������밴�»򳤰�
    //private int modeNum;
    private string shootModeName;
    public enum ShootMode
    {
        Single,
        Auto,
        //SemiAuto,
        
    }

    private Animator animator;//����������
    private int client_id;

    private void Awake()
    {
        mainAudiioSource = GetComponent<AudioSource>();
        playerControl=GetComponentInParent<PlayerControl>();
        animator = GetComponent<Animator>();
    }
    
    private void Start()
    {
        shootingMode= ShootMode.Auto;
        GunShootInput = Input.GetMouseButton(0);
        shootModeName = "Auto";
        if(gameObject.name== "sniper")
        {
            shootingMode = ShootMode.Single;
            GunShootInput= Input.GetMouseButtonDown(0);
            shootModeName = "Single";
        }
        firetimer = 0f;
        bulletLeft = bulletMag * 100;
        currentBullets= bulletMag;
        bulletForce = 100f;

        muzzleflashLight.enabled = false;
        lightDuration = 0.02f;
        isAiming = false;
        cameraChanged = false;

        UpdateAmmoUI();
        client_id = playerControl.clientId;
    }

    private void Update()
    {
        state = playerControl.state;
        //Debug.Log(state);   
        if (Input.GetKeyDown(KeyCode.B)&&!notAllowB)
        {
            if(shootingMode == ShootMode.Auto)
            {
                shootingMode = ShootMode.Single;
                shootModeName = "Single";
            }
            else if(shootingMode == ShootMode.Single)
            {
                shootingMode = ShootMode.Auto;
                shootModeName = "Auto";
            }
            UpdateAmmoUI();

        }
        if(shootingMode == ShootMode.Auto)
        {
            fireRate = FireRate;
            GunShootInput = Input.GetMouseButton(0);
            if(GunShootInput&&isAiming&&currentBullets>0)//�����ҿ���
            {
                continueFireTimer += Time.deltaTime;
                VerticalRecoil(recoilForce,continueFireTimer);
            }
            else
            {
                continueFireTimer = 0;
            }
        }
        else if(shootingMode == ShootMode.Single)
        {
            fireRate = 2*FireRate;
            GunShootInput = Input.GetMouseButtonDown(0);
        }



        if (state == PlayerControl.MoveState.Running||state==PlayerControl.MoveState.Jumping)
        {
            //�ر���׼
            AimOut();

            ExpaningCrossUpdate(crossExpandDegree * 2);
           if (GunShootInput)
           {
                SpreadFactor = 0.2f;
                GunFire();
                ExpaningCrossUpdate(crossExpandDegree * 3);
           }
        }
        else if(state == PlayerControl.MoveState.Walking)
        {
            ExpaningCrossUpdate(crossExpandDegree );
            if (GunShootInput)
            {
                SpreadFactor = 0.1f;
                GunFire();
                ExpaningCrossUpdate(crossExpandDegree * 2);
            }
        }
        else if(state == PlayerControl.MoveState.Crouching)
        {
            ExpaningCrossUpdate(-crossExpandDegree);
            if (GunShootInput)
            {
                SpreadFactor = 0.05f;
                GunFire();
                ExpaningCrossUpdate(0);
            }
        }
        else
        {
            ExpendCross(-currentExpanDegree);
            if (GunShootInput)
            {
                SpreadFactor = 0.15f;
                GunFire();
                ExpaningCrossUpdate(crossExpandDegree );
            }
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
            DoReloadAnimation();
        }

        // animator.SetBool("Run",state==PlayerControl.MoveState.Running);
        // animator.SetBool("Walk",state==PlayerControl.MoveState.Walking);
        if (state == PlayerControl.MoveState.Running && animator.GetBool("Run") == false)
        {
            animator.SetBool("Run",true);
            animator.SetBool("Walk",false);
            var msg=new FullMessage
            {
                Header = new MessageHeader()
                {
                    Type = MessageType.AnimatorParam,
                },
                AnimatorParam = new AnimatorParamMsg()
                {
                    ClientId = client_id,
                    ParamName = "Run",
                    Value = 1,
                    GunName = gameObject.name,
                }
            };
            // MessageMgr.SendMessageToServer(msg);
        }
        else if(state==PlayerControl.MoveState.Walking && animator.GetBool("Walk") == false)
        {
            animator.SetBool("Run",false);
            animator.SetBool("Walk",true);
            var msg=new FullMessage
            {
                Header = new MessageHeader()
                {
                    Type = MessageType.AnimatorParam,
                },
                AnimatorParam = new AnimatorParamMsg()
                {
                    ClientId = client_id,
                    ParamName = "Walk",
                    Value = 1,
                    GunName = gameObject.name,
                }
            };
            // MessageMgr.SendMessageToServer(msg);
        }
        else if(animator.GetBool("Run")||animator.GetBool("Walk"))
        {
            animator.SetBool("Run",false);
            animator.SetBool("Walk",false);
            var msg=new FullMessage
            {
                Header = new MessageHeader()
                {
                    Type = MessageType.AnimatorParam,
                },
                AnimatorParam = new AnimatorParamMsg()
                {
                    ClientId = client_id,
                    ParamName = "!RunOrWalk",
                    Value = 0,
                    GunName = gameObject.name,
                }
            };
            // MessageMgr.SendMessageToServer(msg);
        }
        
        
        
        if (Input.GetKeyDown(KeyCode.J) && state == PlayerControl.MoveState.idle)
        {
            animator.SetTrigger("Inspect");
            var msg=new FullMessage
            {
                Header = new MessageHeader()
                {
                    Type = MessageType.AnimatorParam,
                },
                AnimatorParam = new AnimatorParamMsg()
                {
                    ClientId = client_id,
                    ParamName = "Inspect",
                    Value = 1,
                    GunName = gameObject.name,
                }
            };
            MessageMgr.SendMessageToServer(msg);
        }

        if(Input.GetMouseButtonDown(1))
        {
            if(isAiming)
            {             
                isAiming = false;
                animator.SetBool("Aim",false);
                var msg=new FullMessage
                {
                    Header = new MessageHeader()
                    {
                        Type = MessageType.AnimatorParam,
                    },
                    AnimatorParam = new AnimatorParamMsg()
                    {
                        ClientId = client_id,
                        ParamName = "Aim",
                        Value = 0,
                        GunName = gameObject.name,
                    }
                };
                MessageMgr.SendMessageToServer(msg);
            }
            else
            {
                isAiming = true;
                animator.SetBool("Aim",true);
                var msg=new FullMessage
                {
                    Header = new MessageHeader()
                    {
                        Type = MessageType.AnimatorParam,
                    },
                    AnimatorParam = new AnimatorParamMsg()
                    {
                        ClientId = client_id,
                        ParamName = "Aim",
                        Value = 1,
                        GunName = gameObject.name,
                    }
                };
                MessageMgr.SendMessageToServer(msg);
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
        bool currentAnimationState= animator.GetCurrentAnimatorStateInfo(0).IsName("take_out_weapon")||
            animator.GetCurrentAnimatorStateInfo(0).IsName("reload_ammo_left")||
            animator.GetCurrentAnimatorStateInfo(0).IsName("reload_out_of_ammo");

        if(firetimer<fireRate||currentBullets<=0||currentAnimationState) return;
        if (!isSilencer)
        {
            //����Э��
            StartCoroutine(MuzzleFlashLight());
            //����������Ч
            muzzlePatic.Emit(1);
            //��������Ч
            sparkPatic.Emit(Random.Range(minSparkEmission,maxSparkEmission));

        }
        //CrossFadeInFixedTime()��ʹ������Ϊ��λ��ʱ�䴴���ӵ�ǰ״̬���κ�����״̬�ĵ��뵭��Ч������һ�������Ƕ������ƣ��ڶ��������ǹ���ʱ��
        if (isAiming)
        {
            if (!cameraChanged) { AimIn(); }
            animator.Play("aim_fire",0);
            SpreadFactor = 0.01f;
        }
        else
        {
            if(cameraChanged) { AimOut(); }
            animator.CrossFadeInFixedTime("fire",0.1f);
        }
        
        
        //RaycastHit hit;
        Vector3 shootDirecton = ShootPoint.forward;//��ǰ�����
        //���ֲ��ռ�����ƫ������ת��Ϊ����ռ��е�����
        shootDirecton += ShootPoint.TransformDirection(new Vector3(Random.Range(-SpreadFactor,SpreadFactor),Random.Range(-SpreadFactor,SpreadFactor)));
   
        //�ӵ�ʵ���� 
        Transform bullet= Instantiate(bulletPrefab, BulletShootPoint.position, BulletShootPoint.rotation);
        //�ӵ���������ٶȼ��ɷ���
        bullet.GetComponent<Rigidbody>().velocity = (bullet.transform.forward + shootDirecton) * bulletForce;

        //�ӵ���ʵ����
        Instantiate(casingPrefab, CasingBulletSpawnPoint.position, CasingBulletSpawnPoint.rotation);

        //���������Ч
        mainAudiioSource.clip = isSilencer?soundClips.silencerShootSound: soundClips.shootSound;
        mainAudiioSource.Play();


        currentBullets--;
        firetimer = 0f;

        UpdateAmmoUI();

        // ���Ϳ���message
        FullMessage msg = new FullMessage
        {
            Header = new MessageHeader
            {
                Type = MessageType.GunFire,
            },
            GunFire=new GunFireMsg
            {
                ClientId = client_id,
                ShootDirectionX = shootDirecton.x,
                ShootDirectionY = shootDirecton.y,
                ShootDirectionZ = shootDirecton.z,
            }
        };
        MessageMgr.SendMessageToServer(msg);
    }

    //��ֱ������
    public void VerticalRecoil(float _recoilForce,float _continueFireTimer)
    {
        float delatRotateY = _recoilForce * _continueFireTimer * _continueFireTimer*Time.deltaTime;
        Assult_Rife_Arm.GetComponent<MouseLook>().yRotation -= delatRotateY;
    }


    //׼�Ǳ����С
    public void ExpendCross(float add)
    {
        crossQuarterImgs[0].transform.localPosition += new Vector3(-add,0,0);
        crossQuarterImgs[1].transform.localPosition += new Vector3(add, 0, 0);
        crossQuarterImgs[2].transform.localPosition += new Vector3(0, add, 0);
        crossQuarterImgs[3].transform.localPosition += new Vector3(0, -add, 0);

        currentExpanDegree += add;//��ǰ׼�����Ŷ�
        //currentExpanDegree = Mathf.Clamp(currentExpanDegree,0,maxCrossDegree);//���Ƶ�ǰ׼�����Ŷ�
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
            int bulletToLadd = bulletMag - currentBullets;
            int bulletToDeduct = (bulletLeft >= bulletToLadd) ? bulletToLadd : bulletLeft;
            bulletLeft -= bulletToDeduct;
            currentBullets += bulletToDeduct;
            UpdateAmmoUI();
        }
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
            if(currentBullets==0)
            {
                //animaotr.Play()�ǲ��Ŷ�������һ�������Ƕ������ƣ��ڶ��������Ƕ����㣬�����������Ƕ������ŵ�ʱ��
                animator.Play("reload_out_of_ammo",0,0);
                mainAudiioSource.clip = soundClips.reloadSoundOutOfAmmo;
                mainAudiioSource.Play();
            }
            else
            {
                animator.Play("reload_ammo_left",0,0);
                mainAudiioSource.clip = soundClips.reloadSoundAmmoLeft;
                mainAudiioSource.Play();
            }
            var msg=new FullMessage
            {
                Header = new MessageHeader()
                {
                    Type = MessageType.ReloadBullet,
                },
                ReloadBullet = new ReloadBulletMsg()
                {
                    ClientId = client_id,
                    BulletCount = currentBullets,
                }
            };
            MessageMgr.SendMessageToServer(msg);
            Reload();
        }

    }

    public override void ExpaningCrossUpdate(float expanDegree)
    {
        if (currentExpanDegree < expanDegree)
        {
            //Debug.Log("currentExpanDegree1:"+currentExpanDegree);
            ExpendCross(150 * Time.deltaTime);
        }else if(currentExpanDegree > expanDegree )
        {
            ExpendCross(-500 * Time.deltaTime);
            //Debug.Log("currentExpanDegree2:"+currentExpanDegree);
        }
    }


    public override void AimIn()
    {
        Crosshair.gameObject.SetActive(false);
        gunCamera.fieldOfView=Mathf.Lerp(gunCamera.fieldOfView,gunCameraAimFOV,1f);
        mainCamera.fieldOfView=Mathf.Lerp(mainCamera.fieldOfView,mainCameraAimFOV,1f);
        mainAudiioSource.clip = soundClips.aimSound;
        mainAudiioSource.Play();
        cameraChanged = true;
    }

    public override void AimOut()
    {
        if (isAiming)
        {
            var msg = new FullMessage
            {
                Header = new MessageHeader()
                {
                    Type = MessageType.AnimatorParam,
                },
                AnimatorParam = new AnimatorParamMsg()
                {
                    ClientId = client_id,
                    ParamName = "Aim",
                    Value = 0,
                    GunName = gameObject.name,
                }
            };
            MessageMgr.SendMessageToServer(msg);
            isAiming = false;
            animator.SetBool("Aim", false);
        
        
            Crosshair.gameObject.SetActive(true);
            gunCamera.fieldOfView = Mathf.Lerp(gunCamera.fieldOfView, gunCameraNormalFOV, 1f);
            mainCamera.fieldOfView = Mathf.Lerp(mainCamera.fieldOfView, mainCameraNormalFOV, 1f);
            cameraChanged = false;
        }
    }



}
