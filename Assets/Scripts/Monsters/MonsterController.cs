using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterController : MonoBehaviour
{
    public float moveSpeed;
    public LayerMask solidObjectsLayer;
    //public LayerMask grassLayer;

    public event Action OnEncountered;

    private bool isMoving;
    private Vector2 input;

    private Animator animator;

    private void Awake()
    {
        //Setting animator on awake
        animator = GetComponent<Animator>();
    }

    public void HandleUpdate()
    {
        //If the monster is not currently moving
        if (!isMoving)
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

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;

        //Move player until reaches target
        while ((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        isMoving = false;

        //CheckForEncounters();
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

