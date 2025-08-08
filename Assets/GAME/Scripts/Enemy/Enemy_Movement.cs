using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Movement : MonoBehaviour
{
    public float speed;
    public float attackRange = 2;
    public float attackCooldown = 2;
    public float playerDetectRange = 5;
    public Transform detectionPoint;
    public LayerMask playerLayer;

    private float attackCooldownTimer;
    private int facingDirection = 1;
    private EnemyState enemyState;

    private Rigidbody2D rb;
    private Transform player;
    private Animator anim;
    private SpriteRenderer sr; 



    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        sr = GetComponent<SpriteRenderer>();
        ChangeState(EnemyState.Idle);
    }




    // Update is called once per frame
    void Update()
    {
        if (enemyState != EnemyState.Knockback)
        {
            CheckForPlayer();

            if (attackCooldownTimer > 0)
            {
                attackCooldownTimer -= Time.deltaTime;
            }


            if (enemyState == EnemyState.Chasing)
            {
                Chase();
            }
            else if (enemyState == EnemyState.Attacking)
            {
                //Do Attacking
                rb.linearVelocity = Vector2.zero;
            }
        }
    }



    void Chase()
    {
        if (player.position.x > transform.position.x && facingDirection == -1 ||
            player.position.x < transform.position.x && facingDirection == 1)
        {
            Flip();
        }

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * speed;
    }




    void Flip()
    {
        /* facingDirection *= -1;
        transform.localScale = new Vector3(transform.localScale.x * -1, transform.localScale.y, transform.localScale.z); */
        facingDirection *= -1;
        sr.flipX = facingDirection == -1;
    }



    private void CheckForPlayer()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(detectionPoint.position, playerDetectRange, playerLayer);

        if (hits.Length > 0)
        {
            player = hits[0].transform;
            //if the player is in the attac range AND cooldown is ready
            if (Vector2.Distance(transform.position, player.position) <= attackRange && attackCooldownTimer <= 0)
            {
                attackCooldownTimer = attackCooldown;
                ChangeState(EnemyState.Attacking);
                //GetComponent<Enemy_Combat>().Attack();
            }

            else if (Vector2.Distance(transform.position, player.position) > attackRange && enemyState != EnemyState.Attacking)
            {
                ChangeState(EnemyState.Chasing);
            }
        }
        else
        {
            rb.linearVelocity = Vector2.zero;
            ChangeState(EnemyState.Idle);
        }        
    }




    public void ChangeState(EnemyState newState)
    {
        //Exit the current anim
        if (enemyState == EnemyState.Idle)
        { anim.SetBool("isIdle", false); }
        else if (enemyState == EnemyState.Chasing)
        { anim.SetBool("isChasing", false); }
        else if (enemyState == EnemyState.Attacking)
        { anim.SetBool("isAttacking", false); }

        //Update our current state
        enemyState = newState;

        //Update the new anim
        if (enemyState == EnemyState.Idle)
        { anim.SetBool("isIdle", true); }
        else if (enemyState == EnemyState.Chasing)
        { anim.SetBool("isChasing", true); }
        else if (enemyState == EnemyState.Attacking)
        { anim.SetBool("isAttacking", true); }
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(detectionPoint.position, playerDetectRange);
    }
}


public enum EnemyState
{
    Idle,
    Chasing,
    Attacking,
    Knockback,
}
