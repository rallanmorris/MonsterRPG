using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class Oscillator : MonoBehaviour
{
    [SerializeField] Vector3 movementVector = new Vector3(10f, 10f, 10f);
    [SerializeField] float period = 2f;
    [SerializeField] Image crosshairs;

    [Range(0, 1)] float movementFactor; //0 for not moved, 1 for fully moved

    Vector3 startingPos;
    Vector3 currentPos;

    bool paused;

    // Start is called before the first frame update
    void Start()
    {
        paused = false;
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
            transform.position = startingPos + offSet;
            currentPos = transform.position;
        }
        else
        {
            transform.position = currentPos;
        }
        
    }

    public void PauseMoving()
    {
        paused = true;
    }

    public void continueMoving()
    {
        paused = false;
    }

    public void EnableAim(bool enabled)
    {
        crosshairs.enabled = enabled;
    }
}
