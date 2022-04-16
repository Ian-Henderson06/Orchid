using Orchid;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

//Code taken from Tom Weiland: https://www.youtube.com/watch?v=fVotjwJQ5zM&t=179s
public class OrchidInterpolator : MonoBehaviour
{
    [SerializeField] private float movementThreshold = 0.05f;
    
    private float timeElapsed = 0f;
    private float timeToReachTarget = 0.05f;

    private float squareMovementThreshold; //Square so we dont need to use sqrt function to calculate distance
    private List<PositionAtTick> positionAtTick = new List<PositionAtTick>();
    
    private PositionAtTick lastPosition;
    private PositionAtTick fromPosition;
    private PositionAtTick nextPosition;

    private void Start()
    {
        squareMovementThreshold = movementThreshold * movementThreshold;
        lastPosition = new PositionAtTick(OrchidNetwork.Instance.GetLastServerTick(), transform.position);
        fromPosition = new PositionAtTick(OrchidNetwork.Instance.GetLastServerTick(), transform.position);
        nextPosition = new PositionAtTick(OrchidNetwork.Instance.GetCurrentTick(), transform.position);
        
        positionAtTick.Add(new PositionAtTick(OrchidNetwork.Instance.GetLastServerTick(), transform.position)); //Keep it a tick behind by adding a position to begin with
    }

    private void Update()
    {
        for (int i = 0; i < positionAtTick.Count; i++)
        {
            if (OrchidNetwork.Instance.GetCurrentTick() >= positionAtTick[i].tick)
            {
                lastPosition = nextPosition;
                nextPosition = positionAtTick[i];
                fromPosition  = new PositionAtTick(OrchidNetwork.Instance.GetLastServerTick(), transform.position);
            }
            
            positionAtTick.RemoveAt(i);
            i--;
            timeElapsed = 0;
            timeToReachTarget = (nextPosition.tick - lastPosition.tick) * Time.fixedDeltaTime; //Convert tick values to actual time
        }

        timeElapsed += Time.deltaTime;
        Interpolate(timeElapsed/timeToReachTarget); //If we have went by the target this will keep increasing causing extrapolation to take place
    }

    private void Interpolate(float lerpAmount)
    {
        if ((nextPosition.position - lastPosition.position).sqrMagnitude < squareMovementThreshold) // Supposed to be stopped
        {
            if (nextPosition.position != fromPosition.position) //If not reached target position
                transform.position = Vector3.Lerp(fromPosition.position, nextPosition.position, lerpAmount);

            return;
        }
        
        //Supposed to be moving
        transform.position = Vector3.LerpUnclamped(fromPosition.position, nextPosition.position, lerpAmount); //Extrapolate
    }

    public void AddPosition(uint tick, Vector3 position)
    {
        if (tick <= OrchidNetwork.Instance.GetLastServerTick())
            return;

        for (int i = 0; i < positionAtTick.Count; i++) //Insert position into correct place in loop - This could be optimised by stepping through ticks?
        {
            if (tick < positionAtTick[i].tick)
            {
                positionAtTick.Insert(i, new PositionAtTick(tick, position));
                return;
            }
        }
        
        //If not older - just add to end
        positionAtTick.Add(new PositionAtTick(tick, position));
    }
}
