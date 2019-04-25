using MLAgents;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets;


public class BotAgent : Agent
{
    public CarController2d control;
    private SensorArray sensors;

    private float[] obs;
    private List<float> vectorObs;
    private List<float> sensorReadings;

    private float currentFollowDist;
    private float laneCOG;
    private int currentLane;
    private float currentSpeed;
    private float goalSpeed;
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
        sensors = GetComponent<SensorArray>();
        goalSpeed = UnityEngine.Random.Range(0f, 10f);
    }

    public override void CollectObservations()
    {
        obs = sensors.Go();
        vectorObs = new List<float>();
        sensorReadings = new List<float>();
        vectorObs.AddRange(obs);

        // New Vector Obs = 22
        currentFollowDist = FindFollowDistance(vectorObs);
        laneCOG = GetLaneCOG(vectorObs);
        currentLane = (int)vectorObs[vectorObs.Count - 2];
        currentSpeed = vectorObs[vectorObs.Count - 1];

        foreach(int[] sv in sensorValues)
        {
            sensorReadings.AddRange(SimplifySences(vectorObs, sv[0], sv[1], currentLane));
        }

        if (obs.Length < Assets.Constants.sensoryArraySize - 1)
        {
            obs = new float[Assets.Constants.sensoryArraySize - 1];
            Array.Clear(obs, 0, obs.Length);
            obs[0] = -2f;
            Debug.Log("Error on the dance floor!!!");
        }

        foreach (float o in obs)
        {
            AddVectorObs(o);
        }

        AddVectorObs(goalSpeed);
    }

    public override void AgentAction(float[] vectorAction, string textAction)
    {
        control.Move(vectorAction[1], vectorAction[0]);
    }

    //public void OnCollisionEnter2D(Collision2D coll)
    //{
    //    Destroy(gameObject);
    //}

    private float FindMaxSpeed(List<float> obs)
    {
        int floor = 300;
        int ceil = 360;
        float currentLane = obs[obs.Count - 3];
        float maxSpeed = 1000;

        float[] t_value;
        float t_speed;

        int i = 0;
        foreach (int a in Constants.ListOfAngle)
        {
            if (a < floor)
            { }
            else if (a > ceil)
                break;
            else
            {
                t_value = GetAllObsValuesFromNumber(obs, i, new int[] { 0, 3, 4 });
                if (t_value[0] != -1 && (t_value[1] > currentLane))
                {
                    t_speed = (t_value[0] < Constants.SAFEDISTANCE * 1.5) ? t_value[2] : 1000;
                    maxSpeed = (t_speed < maxSpeed) ? t_speed : maxSpeed;
                }
            }
            i++;
        }

        return maxSpeed;
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
    // Gets the vaue of the bots relative positions in the lane 
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
    // Checks if the left lane is clear
    private bool LeftLaneClear(List<float> obs, int currentLane)
    {
        int floor = 35;
        int ceil = 165;

        return CheckClear(obs, floor, ceil, currentLane);
    }
    // Checks if the right lane is clear
    private bool RightLaneClear(List<float> obs, int currentLane)
    {
        int floor = 215;
        int ceil = 345;

        return CheckClear(obs, floor, ceil, currentLane);
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
                if((t_value[0] != -1)&&(t_value[0] < spriteDist))
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

    private bool CheckClear(List<float> obs, int floor, int ceil, int currentLane)
    {
        bool clear = true;
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
                t_value = GetAllObsValuesFromNumber(obs, i, new int[] { 0, 1 });
                if (t_value[0] != -1)
                {
                    if ((t_value[1] >= currentLane - 1) && (t_value[1] <= currentLane + 1))
                    {
                        clear = false;
                        break;
                    }
                }
            }
            i++;
        }

        return clear;
    }

    private float GetObsValueFromNumber(List<float> obs, int sensorNumber, int dataType)
    {
        return obs[(sensorNumber * Constants.numberOfData) + dataType];
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
