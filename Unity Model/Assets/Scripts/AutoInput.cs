using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Assets;

public class AutoInput : MonoBehaviour
{
    public CarController2d controller;
    public Rigidbody2D body;
    public Transform transform;
    public SensorArray sensors;

    
    public int resolution = 10;
    public string logFilepath = @"C:\Users\JXB1001\Documents\MEng Project\Simulation\simulationLogs\logs";
    private float safeTravelDistance = Assets.Constants.SAFEDISTANCE;

    private float accValue;
    private float steerValue;
    private System.IO.StreamWriter logFil;
    private int frameCount;
    private ControllerPID speedController; 
    private ControllerPID followController;
    private ControllerPID steerContoller;
    private ControllerPID laneChange;
    private int state;
    private float goalXPosition;
    private int laneNumber;
    private string botID;
    private int clearCount = 0;
    private int takeoverCount = 0;
    private bool following = false;
    //private float goalSpeed = UnityEngine.Random.Range(1f, 2f);
    private float goalSpeed;

    // Update is called once per frame
    void Start()
    {
        state = 0;
        accValue = 0f;
        steerValue = 0f;
        frameCount = 0;
        transform = GetComponent<Transform>();
        goalSpeed = UnityEngine.Random.Range(1f, 10f);
        //logFile = new System.IO.StreamWriter(speedLogFilepath);

        speedController = new ControllerPID(0.5, 0.1, 0.1, 1, 0);
        followController = new ControllerPID(-0.5, -0.1, -0.1, 1, 0);
        steerContoller = new ControllerPID(-0.5, -0.01, -0.01, 1, -1);
        //laneChange = new ControllerPID(-0.005, -0.005, -0.005, 1, -1);

        followController.SetPoint = safeTravelDistance;
        speedController.SetPoint = (double)goalSpeed;
        steerContoller.SetPoint = 0;

        Debug.Log("SPEED = "+ goalSpeed.ToString());
    }

    void Update()
    {
        //    laneNumber = FindLane(transform.position.x);
        //    //sensors.Go(laneNumber);

        //    float currentSpeed = body.velocity.magnitude;
        //    //float currentFD = sensors.FollowDist;
        //    //float laneAngle = sensors.LaneCOG;
        //    //bool leftClear = sensors.LeftClear;
        //    //bool rightClear = sensors.RightClear;

        //    //Debug.Log(botID + " Left Clear: " + leftClear.ToString());
        //    //Debug.Log(botID + " Right Clear: " + rightClear.ToString());

        //    if (frameCount%1 == 0)
        //    {
        //        if ((currentFD < 0f) || (currentFD > safeTravelDistance) || (currentSpeed > goalSpeed))
        //        {
        //            speedController.ProcessVariable = (double)currentSpeed;
        //            accValue = (float)speedController.ControlVariable(TimeSpan.FromSeconds(Time.deltaTime));
        //            following = false;
        //        }
        //        else
        //        {
        //            followController.ProcessVariable = (double)currentFD;
        //            accValue = (float)followController.ControlVariable(TimeSpan.FromSeconds(Time.deltaTime));
        //            following = true;
        //        }
        //    }

        //    if(state == 1)
        //    {
        //        float currentX = transform.position.x;
        //        if ((currentX < goalXPosition + 0.3) && (currentX > goalXPosition - 0.3))
        //        {
        //            state = 0;
        //            //Debug.Log(botID + ": State 0");
        //        }
        //        else
        //        {
        //            steerValue = 0.01f;
        //        }
        //    }
        //    else if(state == 2)
        //    {
        //        float currentX = transform.position.x;
        //        if ((currentX < goalXPosition + 0.3) && (currentX > goalXPosition - 0.3))
        //        {
        //            state = 0;
        //            //Debug.Log(botID + ": State 0");
        //        }
        //        else
        //        {
        //            steerValue = -0.01f;
        //        }
        //    }

        //    if (state == 0)
        //    {
        //        steerContoller.ProcessVariable = (double)laneAngle;
        //        steerValue = (float)steerContoller.ControlVariable(TimeSpan.FromSeconds(Time.deltaTime));

        //        // checking if you can not overtake
        //        if ((rightClear)&&(laneNumber>0)&&(!following))
        //        {
        //            clearCount++;
        //        }
        //        else
        //        {
        //            clearCount = 0;
        //        }


        //        // Checking if you can overtake
        //        if((leftClear) && (laneNumber<(Constants.NoOfLanes-1)) && (following))
        //        {
        //            if(currentSpeed < goalSpeed*0.95)
        //                takeoverCount++;
        //        }
        //        else
        //        {
        //            takeoverCount--;
        //        }

        //        if(takeoverCount > 100)
        //        {
        //            takeoverCount = 0;
        //            clearCount = 0;
        //            state = 2;
        //            //Debug.Log(botID + ": State 2");
        //            goalXPosition = transform.position.x + Assets.Constants.LANEDIFFERENCE;
        //        }

        //        if(clearCount > 100)
        //        {
        //            clearCount = 0;
        //            takeoverCount = 0;
        //            state = 1;
        //            //Debug.Log(botID + ": State 1");
        //            goalXPosition = transform.position.x - Assets.Constants.LANEDIFFERENCE;
        //        }

        //    }

        //    if(frameCount%resolution == 0)
        //    {
        //        logFil.WriteLine(frameCount.ToString() + "," + transform.position.y);
        //    }

        //    controller.Move(steerValue, accValue);
        //}

        //public int FindLane(float x)
        //{
        //    int i = 0;
        //    float halfLane = Constants.LANEDIFFERENCE * 0.5f;
        //    for(; i < Constants.NoOfLanes; i++)
        //    {

        //        if ((x >= (i*Constants.LANEDIFFERENCE) - halfLane) && (x < (i*Constants.LANEDIFFERENCE) + halfLane))
        //            break;
        //    }
        //    return i;
        //}

        //public int GetLaneNumber()
        //{
        //    return laneNumber;
        //}

        //public void SetBotID(int i, int j)
        //{
        //    botID = i.ToString() + "_" + j.ToString();
        //    try
        //    {
        //        logFil = new System.IO.StreamWriter(logFilepath + "/" + botID + ".txt");
        //    }
        //    catch(Exception e)
        //    {
        //        //Debug.Log(e.Message);
        //    }

        //}

        //void OnApplicationQuit()
        //{
        //    logFil.Close();
        //}
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

    public int GetLaneNumber()
    {
        return FindLane(transform.position.x);
    }

    public void SetBotID(int i, int j)
    {
        botID = i.ToString() + "_" + j.ToString();
    }

}
