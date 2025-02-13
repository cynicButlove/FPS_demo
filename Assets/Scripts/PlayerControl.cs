using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using protobuf;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 发送间隔
/// </summary>


public class PlayerControl : MonoBehaviour
{
    private CharacterController characterController;
    private  Vector3 moveDirction;

    [Header("�����ֵ")]
    public float Speed;
    private float walkSpeed;
    private float runSpeed;
    private float crouchSpeed;
    public int health;
    public int currentHealth;
    public Scrollbar healthBar;
    public int Damage = 10;

    private bool isWalking;
    private bool isRunning;
    private bool isJumping;
    public bool isGround;

    private bool isCrouching;
    public bool isCanStand;
    public MoveState state;
    private float jumpForce;
    private float fallForce;
    private float grav;
    private float crouchHeight;
    private float standHeight;

    public AudioClip walkingSound;
    public AudioClip runningSound;
    private AudioSource audioSource;

    public Button ButtonEXIT;

    public LayerMask crouchLayerMask;
    
    //private CollisionFlags collisionFlags;
    /*
       ������ CollisionFlags ö�ٵĳ���ֵ�Լ����ǵĺ��壺
        None����ʾû�з�����ײ��
        Sides����ʾ����Ĳ��淢������ײ��
        Above����ʾ������Ϸ���������ײ��
        Below����ʾ������·���������ײ��
        CollidedSides����ʾ����Ĳ��淢������ײ��
        CollidedAbove����ʾ������Ϸ���������ײ��
        CollidedBelow����ʾ������·���������ײ��
        CollidedAll����ʾ��������з��򶼷�������ײ��

    Unity ����ײ��ⷽ�������� CharacterController.Move ���� Rigidbody.MovePosition���У�
    ͨ���᷵�� CollisionFlags ö����������ײ���Ľ����
     */
    public enum MoveState
    {
        idle,
        Walking,
        Running,
        Crouching,
        Jumping,
        Dropping
        
    }

    // Start is called before the first frame update
    void Start()
    {
        isGround = true;
        characterController=GetComponent<CharacterController>();
        walkSpeed = 4f;
        runSpeed = 6f;
        crouchSpeed = 1f;
        jumpForce = 0f;
        fallForce = 0f;
        standHeight = characterController.height ;
        crouchHeight = 1f;
        audioSource=GetComponent<AudioSource>();
        healthBar.size = 1;
        ButtonEXIT.gameObject.SetActive(false);
    }
    void SyncStateToServer()
    {
        FullMessage msg = new FullMessage
        {
            Header = new MessageHeader()
            {
                Type = MessageType.PlayerState
            },
            PlayerState = new PlayerStateMsg()
            {
                Client = new ClientMsg()
                {
                    ClientId = clientId,
                    Username = userName,
                    Health = currentHealth,
                    Position = new PlayerPositionMsg()
                    {
                        X = transform.position.x,
                        Y = transform.position.y,
                        Z = transform.position.z,
                        RotationX = transform.rotation.eulerAngles.x,
                        RotationY = transform.rotation.eulerAngles.y,
                        RotationZ = transform.rotation.eulerAngles.z,
                        GunRotationX = transform.GetChild(0).rotation.eulerAngles.x,
                        GunRotationY = transform.GetChild(0).rotation.eulerAngles.y,
                        GunRotationZ = transform.GetChild(0).rotation.eulerAngles.z
                    },
                    State = (int)state,
                    GunName = "",
                }
            }
        };
        MessageMgr.SendMessageToServer(msg);
    }

    public int clientId;
    public string userName;
    public void Init(ClientMsg msg)
    {
        clientId = msg.ClientId;
        userName = msg.Username;
        currentHealth = msg.Health;
        InvokeRepeating("SyncStateToServer", 0, Constants.SendDelay);
    }


    // Update is called once per frame
    void Update()
    {
        isGround=Physics.Raycast(transform.position, Vector3.down, 0.1f,crouchLayerMask);
        canStand();
        if (Input.GetKey(KeyCode.C)){
            Crouch(true);
        }
        else{
            Crouch(false);
        }

        gravity();

        Moving(); 
        Jump();
        PlaySoundFoot();

        if (Input.GetKeyDown(KeyCode.Escape)) {   ExitGame();}
    }
    public void ExitGame()
    {
            if (ButtonEXIT.gameObject.activeSelf==true) { 
                ButtonEXIT.gameObject.SetActive(false); 
            }
            else
            {
                ButtonEXIT.gameObject.SetActive(true);
                ButtonEXIT.onClick.AddListener(Application.Quit);
            }   
    }

    public void Moving()
    {
        if (state == PlayerControl.MoveState.Jumping) return;
        float horizontal= Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        isWalking=(Mathf.Abs(horizontal)>0||Mathf.Abs(vertical)>0)?true:false;
        isRunning = Input.GetKey(KeyCode.LeftShift) && isWalking;
        if (isCrouching)
        {
            state = MoveState.Crouching;
            Speed = crouchSpeed;
        }
        else if (isGround&&isRunning)
        {
            state = MoveState.Running;
            Speed = runSpeed;
        }else if(isGround&&isWalking)
        {
            state = MoveState.Walking;
            Speed = walkSpeed;
        }
        else if(isGround&&!isWalking)
        {
            state = MoveState.idle;
            Speed = 0f;
        }
        else
        {
            state = MoveState.Dropping;
            Speed -= Speed * Time.deltaTime;
        }


        moveDirction = (transform.right * horizontal + transform.forward * vertical).normalized;
        characterController.Move(moveDirction * Speed * Time.deltaTime);

    }
    private void gravity()
    {
        if (state ==PlayerControl.MoveState.Dropping)
        {
            characterController.Move(new Vector3(0,-9.8f * Time.deltaTime,0));
        }
}
    public void Jump()
    {
        if (!isCanStand) return;
        
        if (Input.GetKeyDown(KeyCode.Space) && isGround)
        {
            isJumping = true ;
            isGround = false;
            jumpForce = 5f;
            fallForce = 1.5f* jumpForce;
        }
        if(isGround&&isJumping)
        {
            isJumping = false;
            jumpForce = 0f;
            state = MoveState.idle;
        }
        if (!isGround&&isJumping)
        {
            state = MoveState.Jumping;
            jumpForce = jumpForce - fallForce* Time.deltaTime;
            //����ͬʱ���ֱ��ܹ���
            Vector3 jump = new Vector3(0, jumpForce * Time.deltaTime, 0) + moveDirction * Speed * Time.deltaTime;
            characterController.Move(jump);
        }

    }

    public void canStand()
    {
        //ģ��ͷ��
        Vector3 posFrom = transform.position + Vector3.up * crouchHeight;
        Vector3 posTo = transform.position + Vector3.up * standHeight;
        //ͷ���Ƿ����ص�
        isCanStand = (Physics.OverlapCapsule(posFrom,posTo, characterController.radius, crouchLayerMask)).Length == 1;//����


    }
    public void Crouch(bool newCrouching)
    {
        if (!isCanStand) return;
        if (state == MoveState.Jumping) return;
        isCrouching=newCrouching;
        characterController.height = isCrouching ? crouchHeight : standHeight;
        characterController.center = characterController.height / standHeight * Vector3.up;
    }
    public void PlaySoundFoot()
    {
        if (state==MoveState.Running)
        {
            audioSource.clip = runningSound;
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else if (state==MoveState.Walking)
        {
            audioSource.clip = walkingSound;
            if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            audioSource.Pause();
        }
    }
    
    
    public void BeBulletHit()
    {
        currentHealth -= Damage;
        healthBar.size = (float)currentHealth / health;
        if (currentHealth <= 0)
        {
            currentHealth = 0;
            healthBar.size = 0;
            // Destroy(gameObject);
        }

        var msg = new FullMessage()
        {
            Header = new MessageHeader()
            {
                Type = MessageType.BulletHit,
            },
            BulletHit = new BulletHitMsg()
            {
                ClientId = clientId,
                Health = currentHealth
            }
        };
        MessageMgr.SendMessageToServer(msg);
    }

}
