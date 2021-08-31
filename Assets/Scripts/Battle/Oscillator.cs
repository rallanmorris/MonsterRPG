using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum StrikeState { Weak, Strong, Critical }

[DisallowMultipleComponent]
public class Oscillator : MonoBehaviour
{

    [SerializeField] bool isPlayer;
    [SerializeField] Vector3 movementVector = new Vector3(10f, 10f, 10f);
    [SerializeField] Vector3 movementVector2 = new Vector3(4f, 10f, 10f);
    [SerializeField] float period = 2f;
    [SerializeField] float period2 = 5f;
    [SerializeField] Image crosshairs;
    [SerializeField] BattleUnit enemyUnit;

    [Range(0, 1)] float movementFactor; //0 for not moved, 1 for fully moved

    Vector3 startingPos;
    Vector3 currentPos;

    bool paused;
    bool enemyHit;
    bool missedEnemy;
    bool currentlyShooting;

    int hits;
    int tries;

    float startPeriod;
    float startPeriod2;

    StrikeState strike;

    //Player Aim Movement
    [SerializeField] float moveSpeed;
    [SerializeField] LayerMask barrierLayer;
    private bool isMoving;
    private Vector2 input;
    private int cycleNum;

    // Start is called before the first frame update
    void Start()
    {
        missedEnemy = false;
        startPeriod = period;
        startPeriod2 = period2;
        paused = false;
        enemyHit = false;
        hits = 0;
        tries = 0;
        startingPos = transform.position;
        currentPos = startingPos;
        cycleNum = 0;
    }

    public bool IsPlayer()
    {
        return isPlayer;
    }

    public void Setup()
    {
        cycleNum = 0;
        currentlyShooting = false;
        ResetTries();
        EnableAim(true);
        ContinueMoving();
    }

    // Update is called once per frame
    public void HandleUpdate()
    {
        //AI Targeting
        if(!isPlayer)
        {
            if(!paused)
            {
                //Set movement factor automatically
                if (period <= Mathf.Epsilon) { return; }

                float cycles = Time.time / period; //grows continually from 0
                const float tau = Mathf.PI * 2;  //6.28
                float rawSinWave = Mathf.Sin(cycles * tau); //Goes from -1 to +1

                movementFactor = rawSinWave / 2f + 0.5f;

                Vector3 offSet = movementFactor * movementVector;

                float cycles2 = Time.time / period2; //grows continually from 0
                float rawSinWave2 = Mathf.Sin(cycles2 * tau); //Goes from -1 to +1
                movementFactor = rawSinWave2 / 2f + 0.5f;
                Vector3 xSet = movementFactor * movementVector2;
                transform.position = startingPos + offSet + xSet;
                currentPos = transform.position;

                if (rawSinWave2 >= 0.98)
                    cycleNum++;
            }
            else
            {
                transform.position = currentPos;
            }
        }

        //Moving as player
        else
        {
            //If the player is not currently moving
            if (!isMoving && !paused)
            {
                //Setting input x and y values to player input
                input.x = Input.GetAxisRaw("Horizontal");
                input.y = Input.GetAxisRaw("Vertical");

                //If there is any player input
                if (input != Vector2.zero)
                {
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
        }
    }

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;

        //Move until reaches target
        while ((targetPos - transform.position).sqrMagnitude > (Mathf.Epsilon + 0.0001))
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;

        isMoving = false;

    }

    private bool IsWalkable(Vector3 targetPos)
    {
        if (Physics2D.OverlapCircle(targetPos, 0.2f, barrierLayer) != null)
        {
            return false;
        }

        return true;
    }

    private void OnTriggerStay2D(Collider2D collided)
    {
        if (collided == enemyUnit.GetComponent<Collider2D>())
        {
            enemyHit = true;
        }
        else
            enemyHit = false;
    }

    private void OnTriggerExit2D(Collider2D collided)
    {
        if (collided == enemyUnit.GetComponent<Collider2D>())
        {
            enemyHit = false;
        }
    }

    public void PauseMoving()
    {
        paused = true;
    }

    public void ContinueMoving()
    {
        paused = false;
    }

    public void EnableAim(bool enabled)
    {
        crosshairs.enabled = enabled;
    }

    public bool HitEnemy()
    {
        return enemyHit;
    }

    public int GetHits()
    {
        return hits;
    }

    public void ResetTries()
    {
        tries = 0;
        hits = 0;
        period = startPeriod;
        period2 = startPeriod2;
    }

    public void AddTry(bool hitSuccess)
    {
        tries++;

        if (hitSuccess)
            hits++;

        period = (float)(period / 1.5);
        period2 = (float)(period2 / 1.5);
    }

    public int GetTries()
    {
        return tries;
    }

    public bool HasMissed()
    {
        return !enemyHit;
    }

    public bool CurrentlyShooting()
    {
        return currentlyShooting;
    }

    public void SetCurrentlyShooting(bool set)
    {
        currentlyShooting = set;
    }

    public bool WillShoot()
    {
        //Debug.Log("Cycle: " + cycleNum);
        //Debug.Log("Hit: " + HitEnemy());

        //Chance if on player
        if (HitEnemy() && (UnityEngine.Random.Range(1, 3) == 2) && !currentlyShooting)
            return true;
        //Chance to randomly happen after a certain amt of cycles
        else if ((UnityEngine.Random.Range(1, 3) == 2) && (cycleNum > 40))
        {
            currentlyShooting = false;
            cycleNum = 0;
            return true;
        }
        else
            return false;
    }

    public string GetStrike()
    {
        string strike;
        switch (hits)
        {
            case 1:
                strike = "WEAK";
                break;
            case 2:
                strike = "STRONG";
                break;
            case 3:
                strike = "CRITICAL";
                break;
            default:
                strike = "NOPE";
                break;
        }
        return strike;
    }
}
