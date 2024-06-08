using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using Palmmedia.ReportGenerator.Core.Reporting.Builders;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    Animator animator;
    [SerializeField] int hp = 100;
    //[SerializeField] int attackValue;
    [SerializeField] float attackCD;
    float lastAttackTime;
    bool hasTarget;
    [SerializeField] PlayerController player;
    bool isDead;
    NavMeshAgent agent;

    AudioSource audioSource;
    [SerializeField] AudioClip attackSound;
    [SerializeField] AudioClip dieSound;
    private void Start()
    {
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
        audioSource = GetComponent<AudioSource>();
    }
    void Update()
    {
        if (isDead)
        {
            return;
        }
        if (Vector3.Distance(transform.position, player.transform.position) > 10f)
        {
            hasTarget = false;
            agent.isStopped = true;
            animator.SetFloat("MoveState", 0);
            return;
        }
        else
        {
            hasTarget = true;
        }
        if (hasTarget)
        {
            if (Vector3.Distance(transform.position, player.transform.position) <= 1.5f)
            {
                agent.isStopped = true;
                animator.SetFloat("MoveState", 0);
                Attack();
            }
            else
            {
                if (!audioSource.isPlaying)
                {
                    audioSource.Play();
                }
                agent.isStopped = false;
                animator.SetFloat("MoveState", 1);
                agent.SetDestination(player.transform.position);
            }
        }

    }
    public void TakeDamage(int attackValue)
    {
        animator.SetTrigger("Hit");
        hp -= attackValue;
        if (hp <= 0)
        {
            Die();
        }
    }
    void Attack()
    {
        if (Time.time - lastAttackTime > attackCD)
        {
            animator.SetTrigger("Attack");
            lastAttackTime = Time.time;
            //player.TakeDamage(attackValue);
            if (audioSource.isPlaying)
            {
                audioSource.Stop();
            }
            Invoke("DelayPlaySound", 1f);
        }
    }
    void DelayPlaySound()
    {
        audioSource.PlayOneShot(attackSound);
    }
    void Die()
    {
        animator.SetBool("Die", true);
        agent.isStopped = true;
        isDead = true;
        audioSource.Stop();
        audioSource.PlayOneShot(dieSound);
    }
}
