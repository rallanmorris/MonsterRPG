using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Oscillator : MonoBehaviour
{
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

    int hits;
    int tries;

    float startPeriod;
    float startPeriod2;

    // Start is called before the first frame update
    void Start()
    {
        startPeriod = period;
        startPeriod2 = period2;
        paused = false;
        enemyHit = false;
        hits = 0;
        tries = 0;
        startingPos = transform.position;
        currentPos = startingPos;
    }

    // Update is called once per frame
    void Update()
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
        }
        else
        {
            transform.position = currentPos;
        }

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
}
