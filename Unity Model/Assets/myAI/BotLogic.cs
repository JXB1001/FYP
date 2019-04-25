using MLAgents;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Assets;
using System;

public class BotLogic : Decision
{

    private ControllerPID speedControl;
    private ControllerPID followControl;
    private ControllerPID steerControl;
    private ControllerPID laneChangeControl;

    private float currentFollowDist = -1f;
    private float maxSpeed = 1000f;
    private float accValue = 0f;
    private float steerValue = 0f;
    private float goalSpeed;
    private float currentSpeed;
    private float laneCOG;
    private int currentLane;
    private int goalLane;
    private bool following;
    private bool rightClear;
    private bool leftClear;

    private float speedControlLPV;
    private float followControlLPV;
    private float steerControlLPV;

    public void OnEnable()
    {
        // Initialising Controllers
        speedControl    = new ControllerPID(0.2, 0.1, 0.1, 1, 0);
        followControl   = new ControllerPID(-0.5, -0.1, -0.1, 1, 0);
        steerControl    = new ControllerPID(-0.4, -0.05, 0 , 1, -1);

        // Setting Contoller Set Points
        followControl.SetPoint  = Constants.SAFEDISTANCE;
        steerControl.SetPoint   = 0;
    }


    public override float[] Decide(List<float> vectorObs, List<Texture2D> visualObs, float reward, bool done, List<float> memory)
    {
        int state = 0;
        bool followingBehind;
        //bool followingAlt;
        //bool useRegFol;

        // Extracting details from the observation vector
        currentFollowDist   = FindFollowDistance(vectorObs);
        maxSpeed            = FindMaxSpeed(vectorObs);
        laneCOG             = GetLaneCOG(vectorObs);
        currentLane         = (int)vectorObs[vectorObs.Count - 3];
        leftClear           = LeftLaneClear(vectorObs, currentLane);
        rightClear          = RightLaneClear(vectorObs, currentLane);
        currentSpeed        = vectorObs[vectorObs.Count - 2];
        goalSpeed           = vectorObs[vectorObs.Count - 1];
        following           = false;

        // Extracting Memory
        if (memory.Count > 0)
        {
            state = (int)memory[2];
            speedControlLPV = memory[4];
            followControlLPV = memory[5];
            steerControlLPV = memory[6];
        }

        speedControl.SetPoint = (maxSpeed < goalSpeed) ? (double)maxSpeed : (double)goalSpeed;

        followingBehind = ((currentFollowDist >= 0f) && (currentFollowDist < Constants.SAFEDISTANCE)) ? true : false;

        //Finding the acceleration value--------------------------------------
        if ((!followingBehind) || (currentSpeed > goalSpeed))
        {
            // If the not following another vehicle - use the speedControl system

            speedControl.ProcessVariableLast = (double)speedControlLPV;
            speedControl.ProcessVariable = (double)currentSpeed;
            accValue = (float)speedControl.ControlVariable(TimeSpan.FromSeconds(Time.deltaTime));
        }
        else
        {
            // If the following another vehicle - use the followControl system
            
            followControl.ProcessVariable = (double)currentFollowDist;
            followControl.ProcessVariableLast = (double)followControlLPV;
   
            accValue = (float)followControl.ControlVariable(TimeSpan.FromSeconds(Time.deltaTime));
        }
        // ---------------------------------------------------------------------
  


        //Finding the steering value based on state based architecture ---------
        switch (state)
        {
            case 0:
                steerControl.ProcessVariableLast = (double)steerControlLPV;
                steerControl.ProcessVariable = (double)laneCOG;
                steerValue = (float)steerControl.ControlVariable(TimeSpan.FromSeconds(Time.deltaTime));
                break;

            // for mving down a lane
            case 1:
                steerValue = -Constants.TURNING;
                break;

            // for moving up a lane
            case 2:
                steerValue = Constants.TURNING;
                break;
        }

        // ----------------------------------------------------------------------

        return new float[] {accValue, steerValue};
    }

    public override List<float> MakeMemory(List<float> vectorObs, List<Texture2D> visualObs, float reward, bool done, List<float> memory)
    {
        if (memory.Count == 0)
            return new List<float> { 0, 0, 0, 0, 0, 0, 0 };
            // Creating Memory:
            //      0 - dropLaneCount
            //      1 - takeoverCount
            //      2 - state
            //      3 - goalLane
            //      4 - speedControlLPV
            //      5 - followControlLPV
            //      6 - steerControlLPV

        string empty = "";

        foreach(float m in memory)
        {
            empty += (m + ":");
        }
        Debug.Log("MakeMemory Memory == " + empty);

        int dropLaneCount   = (int)memory[0];
        int takeoverCount   = (int)memory[1];
        int state           = (int)memory[2];
        int goalLane        = (int)memory[3];

        // setting presvious process variables
        float speedControlLPV = currentSpeed;
        float followControlLPV = currentFollowDist;
        float steerControlLPV = laneCOG;

        // Extracting details from the observation vector
        currentFollowDist   = FindFollowDistance(vectorObs);
        laneCOG             = GetLaneCOG(vectorObs);
        currentLane         = (int)vectorObs[vectorObs.Count - 3];
        leftClear           = LeftLaneClear(vectorObs, currentLane);
        rightClear          = RightLaneClear(vectorObs, currentLane);
        currentSpeed        = vectorObs[vectorObs.Count - 2];
        following           = false;

        if ((currentFollowDist < 0f) || (currentFollowDist > Constants.SAFEDISTANCE))
            following = false;
        else
            following = true;


        switch (state)
        {
            case 0:
                // Checking to see if the boot should drop down a lane
                if ((leftClear) && (currentLane > 0) && (!following))
                    dropLaneCount++;
                else
                    dropLaneCount = 0;

                // Checking to see if the bot should move up a lane
                if ((rightClear) && (currentLane < Constants.NoOfLanes - 1) && (following))
                    takeoverCount++;
                else
                    takeoverCount = 0;

                // Reseting once count has reached a value;
                if (dropLaneCount > 100)
                {
                    state = 2;
                    dropLaneCount = 0;
                    takeoverCount = 0;
                    goalLane = currentLane - 1;
                }

                if (takeoverCount > 80)
                {
                    state = 1;
                    takeoverCount = 0;
                    dropLaneCount = 0;
                    goalLane = currentLane + 1;
                }
                break;

            // checking to see if the condition has been reached the agent can return to state 0
            case 1:
            case 2:
                if (currentLane == goalLane)
                {
                    state = 0;
                }
                break;
        }



        return new List<float> { dropLaneCount, takeoverCount, state, goalLane, speedControlLPV, currentFollowDist, laneCOG};
    }
    
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
                t_value = GetAllObsValuesFromNumber(obs, i, new int[]{0, 3, 4});
                if(t_value[0] != -1 && (t_value[1] > currentLane))
                {
                    t_speed = (t_value[0] < Constants.SAFEDISTANCE*1.5) ? t_value[2] : 1000;
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
        int [] indexes = new int[]{0, 1, (Constants.LengthOfAngleList-1)};
        int ti;

        try
        {
            // finds the shortest distance in the forward direction
            for (int i = 0; i < indexes.Length; i++)
            {
                ti = indexes[i];
                tempDistance = GetObsValueFromNumber(obs, ti, 0)*Mathf.Cos( Mathf.Deg2Rad*Constants.ListOfAngle[ti] );
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
        int finish = Constants.LengthOfAngleList-1;
        float sum = 0;
        int count = 0;

        int tempAngle;
        float t_firstValue;
        float t_secondValue;

        for(int i = 0; start+i < (finish-i); i++)
        {
            try
            {
                tempAngle = Constants.ListOfAngle[start + i];
            }
            catch(Exception e)
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

                if((t_firstValue != -1)&&(t_secondValue != -1))
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
        catch(DivideByZeroException divideError)
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
    private bool CheckClear(List<float> obs, int floor, int ceil, int currentLane)
    {
        bool clear = true;
        float[] t_value;

        int i = 0;
        foreach(int a in Constants.ListOfAngle)
        {
            if (a < floor)
            { }
            else if (a > ceil)
                break;
            else
            {
                t_value = GetAllObsValuesFromNumber(obs, i, new int[] { 0, 1 });
                if(t_value[0] != -1)
                {
                    if((t_value[1] >= currentLane-1)&&(t_value[1] <= currentLane + 1))
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
        return obs[(sensorNumber * Constants.numberOfData)+dataType];
    }
    private float GetObsValueFromNumber(List<float> obs, int sensorNumber, int[] dataType)
    {
        float currentValue = 100;
        float t_value = 0;
        bool changeFlag = false;

        foreach(int dt in dataType)
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

        for(int i = 0; i < dataType.Length; i++)
        {
            returnValue[i] = GetObsValueFromNumber(obs, sensorNumber, dataType[i]);
        }

        return returnValue;
    }

}
