using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets;

public class DataControl : MonoBehaviour
{
    public Transform thisTransform;

    public int GetLaneNumber()
    {
        int laneNumber = 0;
        laneNumber = FindLane(thisTransform.position.x);
        return laneNumber;
    }

    private int FindLane(float x)
    {
        int i = 0;
        float halfLane = Constants.LANEDIFFERENCE * 0.5f;
        for (; i < Constants.NoOfLanes; i++)
        {

            if ((x >= (i * Constants.LANEDIFFERENCE) - halfLane) && (x < (i * Constants.LANEDIFFERENCE) + halfLane))
                break;
        }
        return i;
    }
}
