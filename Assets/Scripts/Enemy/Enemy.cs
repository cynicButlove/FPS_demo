using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    private NavMeshAgent navMeshAgent;
    public Animator animator;
    public Slider slider;
    public GameObject attackPrefab;
    public Transform attackPoint;

    public float enemyHealth;
    public TextMeshProUGUI getDamageText;
    public GameObject deadEffect;

    public GameObject[] wayPointObj;//存放路线
    public List<Vector3> wayPoints=new List<Vector3>();//存放路线上的点
    public int randomWay;//随机路线
    public int index;//点的索引

    public EnemyBaseState currentState;
    public PatrolState patrolState ;
    public AttackState attackState ;

    private bool isdead;
    public GameObject EnemyPrefab;
    public Transform player;
    public bool isPlayerInSight;
    public AudioSource audioSource;
    public AudioClip attackSoundClip;

    public void Awake()
    {
        GameObject WayPoint1= GameObject.Find("WayPoint1");
        GameObject WayPoint2= GameObject.Find("WayPoint2");
        GameObject WayPoint3= GameObject.Find("WayPoint3");
        wayPointObj = new GameObject[] {WayPoint1,WayPoint2,WayPoint3};

        GameObject CanvasEnemy= GameObject.Find("CanvasEnemy");
        slider = CanvasEnemy.transform.Find("Slider").GetComponent<Slider>();


    }

    // Start is called before the first frame update
    void Start()
    {
        /**错误写法：这样返回的是null
         patrolState = new PatrolState();
          **/
        /*如果本身就是挂载在物体上的脚本，可以直接使用GetComponent*/
         /* patrolState = GetComponent<PatrolState>();*/

        //需要将脚本挂载到物体上，才能使用
        patrolState = gameObject.AddComponent<PatrolState>();
        attackState = gameObject.AddComponent<AttackState>();
        currentState = patrolState;


        randomWay = Random.Range(0, wayPointObj.Length);
        index = 0;
        isdead = false;
        slider.gameObject.SetActive(false);
        slider.minValue = 0;
        slider.maxValue = enemyHealth;
        slider.value = enemyHealth;

        navMeshAgent=GetComponent<NavMeshAgent>();
        animator=GetComponent<Animator>();
        TransitionState(patrolState);
        audioSource=GetComponent<AudioSource>();
        audioSource.clip = attackSoundClip;
    }

    // Update is called once per frame
    void Update()
    {
        //MoveToTarget();
        currentState.OnUpdate(this);
    }
    public void LoadRath(GameObject wayPointGo)
    {
        wayPoints.Clear();
        foreach(Transform t in wayPointGo.transform)
        {
            wayPoints.Add(t.position);
        }
    }
    public void MoveToTarget()
    {
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("JumpAttack")) { navMeshAgent.isStopped = true ; return; }
        if (Vector3.Distance(transform.position, wayPoints[index])<1.2f)
        {
            index++;
            if(index>=wayPoints.Count)
            {
                index=0;
                randomWay = Random.Range(0, wayPointObj.Length);
                LoadRath(wayPointObj[randomWay]);
            }
            
            if(animator.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
            {
                
                animator.SetBool("idle",true);
                StartCoroutine(Wait());
            }
        }
        if(animator.GetCurrentAnimatorStateInfo(0).IsName("Idle")) { navMeshAgent.isStopped = true;  }
        if (animator.GetCurrentAnimatorStateInfo(0).IsName("Walking"))
        {
            navMeshAgent.isStopped = false;
            navMeshAgent.speed = 3;
            navMeshAgent.destination = Vector3.MoveTowards(transform.position, wayPoints[index], navMeshAgent.speed);
        }

    }
    public IEnumerator Wait()
    {
        yield return new WaitForSeconds(5.3f);
        animator.SetBool("idle",false);

    }
    public void MoveToTarget(Vector3 targetPos)
    {        
       // navMeshAgent.destination = Vector3.MoveTowards(transform.position, targetPos, navMeshAgent.speed);
        if (!animator.GetCurrentAnimatorStateInfo(0).IsName("JumpAttack")&& Vector3.Distance(transform.position, targetPos) < 12f)
        {
            animator.SetTrigger("JumpAttack");
        }
        if(animator.GetCurrentAnimatorStateInfo(0).IsName("JumpAttack"))
        {
            navMeshAgent.isStopped = true;
        }
        if(animator.GetCurrentAnimatorStateInfo(0).IsName("Walking")){
            navMeshAgent.speed = 4f;
            navMeshAgent.isStopped = false;
            navMeshAgent.destination = Vector3.MoveTowards(transform.position, targetPos, navMeshAgent.speed );
        }

    } 


    public void EnemyAttack()
    {
        transform.position += transform.forward * 1.8f;

    }
    // 播放音频一遍然后停止
    void PlayAudioOnce()
    {
        audioSource.PlayOneShot(attackSoundClip);
        Invoke("StopAudio", attackSoundClip.length); // 在音频长度之后调用停止音频函数
        GameObject attack=Instantiate(attackPrefab, attackPoint.position, attackPoint.rotation);
        Destroy(attack, 3f);
        //射线检测
        for(int i = -1; i < 2; i++)
        {
            Physics.Raycast(attackPoint.position+new Vector3(i,1.5f,-1), attackPoint.forward, out RaycastHit hit, 18f);
            if(hit.collider!=null)
            {
                if(hit.collider.tag=="Player")
                {
                    player.GetComponent<PlayerControl>().currentHealth -= 10;
                    player.GetComponent<PlayerControl>().healthBar.size = (float)player.GetComponent<PlayerControl>().currentHealth / 100;
                }
            }

        }

    }

    // 停止音频
    void StopAudio()
    {
        audioSource.Stop();
    }


    public void TransitionState(EnemyBaseState state)
    {
        currentState = state;
        currentState.EnterState(this);

    }

    public void TakeDamage(float damage)
    {
        slider.gameObject.SetActive(true);
        enemyHealth -= damage;
        slider.value = enemyHealth;
        getDamageText.text = damage.ToString();
        StartCoroutine(TextDisappear());
        //Instantiate(deadEffect, transform.position, Quaternion.identity);
        if(enemyHealth<=0)
        {
            enemyHealth = 0;
            isdead = true;
            slider.gameObject.SetActive(false);
            navMeshAgent.speed = 0;
            animator.SetTrigger("die");
            StartCoroutine(ReGenerate()); 
            
        }
    }
    public IEnumerator TextDisappear()
    {
        yield return new WaitForSeconds(0.5f);
        getDamageText.text = "";
    }
    public IEnumerator ReGenerate()
    {
       
        yield return new WaitForSeconds(4);
        GameObject newEnemy= Instantiate(EnemyPrefab, new Vector3(Random.Range(-10, 10), 0, Random.Range(-40, -30)), Quaternion.identity);
        newEnemy.GetComponent<Enemy>().enemyHealth = 200;
        Destroy(gameObject);
    }

}
