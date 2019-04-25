using Assets;
using MLAgents;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class LearningAgent : Agent
{
    private CarController2d control;
    private SensorArray sensors;
    private Transform transform;

    private float[] obs;
    private List<float> vectorObs;
    private List<float> sensorReadings;

    private float currentFollowDist;
    private float goalSpeed;
    private float laneCOG;
    private int currentLane;
    private float currentSpeed;
    private int[][] sensorValues = new int[][]{
        new int[]{10,45},
        new int[]{45,135},
        new int[]{135,170},
        new int[]{190,225},
        new int[]{225,315},
        new int[]{315,350}
    };

    public override void InitializeAgent()
    {
        control = GetComponent<CarController2d>();
        sensors = GetComponent<SensorArray>();
        transform = GetComponent<Transform>();
        goalSpeed = Constants.GOALSPEED;
    }

    public override void CollectObservations()
    {
        obs = sensors.Go();

        if (obs.Length < Assets.Constants.sensoryArraySize)
        {
            obs = new float[Assets.Constants.sensoryArraySize];
            Array.Clear(obs, 0, obs.Length);
            Debug.Log("Error on the dance floor!!!");
        }

        vectorObs = new List<float>();
        sensorReadings = new List<float>();
        vectorObs.AddRange(obs);
        vectorObs.Add(goalSpeed);

        Debug.Log("vector Obs === " + vectorObs.Count.ToString());

        currentFollowDist = FindFollowDistance(vectorObs);
        laneCOG = GetLaneCOG(vectorObs);
        currentLane = (int)vectorObs[vectorObs.Count - 3];
        currentSpeed = vectorObs[vectorObs.Count - 2];
        AddVectorObs((float)goalSpeed);

        foreach (int[] sv in sensorValues)
        {
            sensorReadings.AddRange(SimplifySences(vectorObs, sv[0], sv[1], currentLane));
        }

        Debug.Log("SENSOR READINGS === " + sensorReadings.Count.ToString());

        AddVectorObs((float)currentFollowDist);
        AddVectorObs((float)laneCOG);
        AddVectorObs((float)currentLane);
        AddVectorObs((float)currentSpeed);

        foreach (float sr in sensorReadings)
        {
            AddVectorObs(sr);
        }

        if (transform.position.y > Assets.Constants.ROADLENGTH - 1)
        {
            Done();
        }
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        Debug.Log("Vector Action = " + vectorAction[0].ToString() + ":" + vectorAction[1].ToString());
        control.Move(Mathf.Clamp(vectorAction[1], -1, 1), Mathf.Clamp(vectorAction[0], 0, 1));
    }


    public void OnCollisionEnter2D(Collision2D coll)
    {
        End();
    }

    private void End()
    {
        GameObject control = GameObject.Find("Control");
        Spawning spawning = control.GetComponent<Spawning>();
        //Done();
        spawning.Reset();
        transform.position = new Vector3(0, -Assets.Constants.ROADLENGTH, 0);
        transform.rotation = Quaternion.identity;
    }

    // Finds the distance that a bot is in front of the bot
    private float FindFollowDistance(List<float> obs)
    {
        float currentDistance = 100f;
        float tempDistance = -1f;
        int[] indexes = new int[] { 0, 1, (Constants.LengthOfAngleList - 1) };
        int ti;

        try
        {
            // finds the shortest distance in the forward direction
            for (int i = 0; i < indexes.Length; i++)
            {
                ti = indexes[i];
                tempDistance = GetObsValueFromNumber(obs, ti, 0) * Mathf.Cos(Mathf.Deg2Rad * Constants.ListOfAngle[ti]);
                if ((tempDistance != -1f) && (tempDistance < currentDistance))
                {
                    currentDistance = tempDistance;
                }
            }
        }
        catch (Exception e)
        {
            Debug.Log(e.Message);
            currentDistance = -1;
        }

        return currentDistance;
    }

    private float GetObsValueFromNumber(List<float> obs, int sensorNumber, int dataType)
    {
        return obs[(sensorNumber * Constants.numberOfData) + dataType];
    }

    private float GetLaneCOG(List<float> obs)
    {
        int start = 1;
        int finish = Constants.LengthOfAngleList - 1;
        float sum = 0;
        int count = 0;

        int tempAngle;
        float t_firstValue;
        float t_secondValue;

        for (int i = 0; start + i < (finish - i); i++)
        {
            try
            {
                tempAngle = Constants.ListOfAngle[start + i];
            }
            catch (Exception e)
            {
                Debug.Log("ERROR EROOR EOEROER EROOR");
                Debug.Log(e.Message);
                return 0;
            }


            if (tempAngle < Constants.SteeringValues.FLOOR)
            { }
            else if (tempAngle > Constants.SteeringValues.CEIL)
            { break; }
            else
            {
                t_firstValue = GetObsValueFromNumber(obs, start + i, new int[] { 2, 3 });
                t_secondValue = GetObsValueFromNumber(obs, finish - i, new int[] { 2, 3 });

                if ((t_firstValue != -1) && (t_secondValue != -1))
                {
                    sum += (t_firstValue - t_secondValue);
                    count++;
                }
            }
        }

        try
        {
            float returnValue = sum / count;
            if (float.IsNaN(returnValue))
                return 0;
            else
                return returnValue;
        }
        catch (DivideByZeroException divideError)
        {
            Debug.LogError(divideError);
            return 0;
        }
    }

    private float GetObsValueFromNumber(List<float> obs, int sensorNumber, int[] dataType)
    {
        float currentValue = 100;
        float t_value = 0;
        bool changeFlag = false;

        foreach (int dt in dataType)
        {
            t_value = obs[(sensorNumber * Constants.numberOfData) + dt];
            if ((t_value != -1) && (t_value < currentValue))
            {
                currentValue = t_value;
                changeFlag = true;
            }
        }

        if (changeFlag)
            return currentValue;
        else
            return -1f;
    }

    private float[] SimplifySences(List<float> obs, int floor, int ceil, int currentLane)
    {
        float spriteDist = 1000;
        float spriteLane = -1;
        float roadLaneDist = 1000;
        float[] t_value;

        int i = 0;
        foreach (int a in Constants.ListOfAngle)
        {
            if (a < floor)
            { }
            else if (a > ceil)
                break;
            else
            {
                t_value = GetAllObsValuesFromNumber(obs, i, new int[] { 0, 1, 2, 3 });
                // sprite dist, sprite lane, lanechange dist, verge dist
                if ((t_value[0] != -1) && (t_value[0] < spriteDist))
                {
                    spriteDist = t_value[0];
                    spriteLane = t_value[1] - currentLane;
                }

                roadLaneDist = ((t_value[2] != -1) && (t_value[2] < roadLaneDist)) ? t_value[2] : roadLaneDist;
                roadLaneDist = ((t_value[3] != -1) && (t_value[3] < roadLaneDist)) ? t_value[3] : roadLaneDist;
            }
            i++;
        }

        spriteDist = (spriteDist == 1000) ? -1 : spriteDist;
        roadLaneDist = (roadLaneDist == 1000) ? -1 : roadLaneDist;

        return new float[] { spriteDist, spriteLane, roadLaneDist };
    }

    private float[] GetAllObsValuesFromNumber(List<float> obs, int sensorNumber, int[] dataType)
    {
        float[] returnValue = new float[dataType.Length];

        for (int i = 0; i < dataType.Length; i++)
        {
            returnValue[i] = GetObsValueFromNumber(obs, sensorNumber, dataType[i]);
        }

        return returnValue;
    }
}
