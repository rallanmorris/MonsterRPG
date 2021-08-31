using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    public MonsterController MonsterCon { get; set; }

    [SerializeField] Vector3 range;
    public float moveSpeed;
    public LayerMask solidObjectsLayer;
    //public LayerMask grassLayer;

    public event Action OnEncountered;

    public bool isMoving;
    public bool isStopped;
    private Vector2 input;

    private Animator animator;

    private void Awake()
    {
        //Setting animator on awake
        animator = GetComponent<Animator>();
    }

    public void HandlePlayerUpdate()
    {
        //If the monster is not currently moving
        if (!isMoving && !isStopped)
        {
            //Setting input x and y values to player input
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            //If there is any player input
            if (input != Vector2.zero)
            {
                //Set 2d movement floats to current input
                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);

                //Set target to move player to
                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                if (IsWalkable(targetPos))
                {
                    //Coroutine moves player to target
                    StartCoroutine(Move(targetPos));
                }
            }
        }
        

        animator.SetBool("isMoving", isMoving);
    }

    public void HandleEnemyUpdate()
    {
        if(!isMoving)
        {
            MoveRandomly(range);
        }
    }

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;

        //Move player until reaches target
        while (((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon + 0.0001))
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        isMoving = false;
        //isStopped = false;

        //CheckForEncounters();
    }

    public void MoveRandomly(Vector3 range)
    {
        var targetPos = transform.position;
        targetPos.x += UnityEngine.Random.Range(-range.x, range.x);
        targetPos.y += UnityEngine.Random.Range(-range.y, range.y);

        if (IsWalkable(targetPos))
            StartCoroutine(Move(targetPos));
    }

    public void StopMoveAnimation()
    {
        isMoving = false;
        animator.SetBool("isMoving", isMoving);
    }

    /*private void CheckForEncounters()
    {
        if (Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer) != null)
        {
            if (UnityEngine.Random.Range(1, 101) <= 10)
            {
                animator.SetBool("isMoving", false);
                OnEncountered();
            }
        }
    }*/

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.1f, solidObjectsLayer) != null)
        {
            Debug.Log("Not Walkable");
            return false;
        }

        Debug.Log("Walkable");
        return true;
    }

}

