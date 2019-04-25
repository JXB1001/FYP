using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;
using Assets;

public class SensorArray : MonoBehaviour
{
    public Transform transform;
    public Rigidbody2D rigidbody;

    private int noOfSensors = 20;
    private IList<int> angles = new List<int>();
    private IList<Sensor> sensors = new List<Sensor>();
    private IList<float> result;
    private float followAngle = -1f;
    private bool rightClear;
    private bool leftClear;



    // Start is called before the first frame update
    void Start()
    {
        for(int i = 0; i < 360; i += 360/noOfSensors)
        {
            angles.Add(i);
        }
        followAngle = Mathf.Atan((Assets.Constants.LANEDIFFERENCE*0.1f)/(Assets.Constants.SAFEDISTANCE*1.2f))*Mathf.Rad2Deg;
        angles.Add((int)followAngle);
        angles.Add((int)(360f - followAngle));

        // Puting the angles in order
        angles = angles.OrderBy(n => n).ToList();

        foreach(int a in angles)
        {
            sensors.Add(new Sensor(a, transform));
        }
        Constants.ListOfAngle = angles.ToArray();
        Constants.LengthOfAngleList = angles.Count();
    }

    public float[] Go()
    {
        result = new List<float>();


        try
        {
            foreach (Sensor s in sensors)
            {
                s.CastRay();
                foreach(float v in s.Values)
                {
                    result.Add(v);
                }
             
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
        }

        result.Add(FindLane(transform.position.x));
        result.Add((float)rigidbody.velocity.magnitude);

        return result.ToArray();


    }


    private int FindLane(float x)
    {
        int i;
        float halfLane = Constants.LANEDIFFERENCE * 0.5f;
        for (i = 0; i < Constants.NoOfLanes; i++)
        {
            if ((x >= (i * Constants.LANEDIFFERENCE) - halfLane) && (x < (i * Constants.LANEDIFFERENCE) + halfLane))
                return i;
        }
        return -1;
    }
}


