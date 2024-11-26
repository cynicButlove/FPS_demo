using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Detection : MonoBehaviour
{
    public Transform NPC;
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            NPC.GetComponent<Enemy>().TransitionState(NPC.GetComponent<Enemy>().attackState);
            NPC.GetComponent<Enemy>().isPlayerInSight = true;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            NPC.GetComponent<Enemy>().player = other.transform;

        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            NPC.GetComponent<Enemy>().TransitionState(NPC.GetComponent<Enemy>().patrolState);
            NPC.GetComponent<Enemy>().animator.SetBool("JumpAttack",false);
            NPC.GetComponent<Enemy>().isPlayerInSight = false;
        }
    }
}
