using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public LayerMask solidObjectsLayer;
    public LayerMask grassLayer;

    public event Action OnEncountered;

    private bool isMoving;
    private Vector2 input;
    private Vector2 moveActionInput;

    private Animator animator;

    [SerializeField] GameObject playerGun;
    Vector3 oldAimVector;

    Rigidbody2D rb;
    Rigidbody2D gunRB;
    Vector2 lookDirection;
    public float aimAngle;

    private void Awake()
    {
        //Setting animator on awake
        animator = GetComponent<Animator>();

        oldAimVector = new Vector3(0, 0, 0);
        aimAngle = 0f;
        rb = GetComponent<Rigidbody2D>();
        gunRB = playerGun.GetComponent<Rigidbody2D>();
    }

    public void Movement(InputAction.CallbackContext context)
    {
        moveActionInput = context.ReadValue<Vector2>();
    }

    public void AimGun(InputAction.CallbackContext context)
    {
        //Debug.Log(context.ReadValue<Vector2>());

        lookDirection = context.ReadValue<Vector2>();
        aimAngle = Mathf.Atan2(lookDirection.y, lookDirection.x) * Mathf.Rad2Deg - 180f;
        gunRB.rotation = aimAngle;
    }

    public float GetAimAngle()
    {
        return aimAngle;
    }

    public void HandleUpdate()
    {
        //Set off encountered action all the time to test battle system
        //OnEncountered();

        //If the player is not currently moving
        if (!isMoving)
        {
            //Setting input x and y values to player input
            //input.x = Input.GetAxisRaw("Horizontal");
            //input.y = Input.GetAxisRaw("Vertical");
            input = moveActionInput;

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

        CheckForEncounters();
    }

    private void CheckForEncounters()
    {
        if(Physics2D.OverlapCircle(transform.position, 0.2f, grassLayer) != null)
        {
            if(UnityEngine.Random.Range(1, 101) <= 10)
            {
                animator.SetBool("isMoving", false);
                OnEncountered();
            }
        }
    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if(Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectsLayer) != null)
        {
            return false;
        }

        return true;
    }

}
