using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAgents;
using Cinemachine;
using System;

public class Spawning : MonoBehaviour
{
    //enviromental constants
    public int numberOfLanes;

    //bot constants
    public float botDensity;

    public bool test = false;
    
    // Objects to be instantiated
    public GameObject bot = null;
    public GameObject lane = null;
    public GameObject verge = null;
    public GameObject human = null;
    public GameObject pretendHuman = null;
    public GameObject cam = null;

    // BRAINS!!!!
    public Brain botBrain;
    public Brain humanBrain;
    public Brain learningBrain;

    // Private Variables
    private float laneShift = Assets.Constants.LANEDIFFERENCE;
    private GameObject laneInst;
    private GameObject botInst;
    private Loop botLoop;
    private AutoInput autoInput;
    private List<Collider2D> laneColliderList = new List<Collider2D>();
    private Transform humanTransform;
    private GameObject camInst;
    private CinemachineVirtualCamera cvc;
    private Agent agent;
    private GameObject hud;
    private Speedo speedo;
    private Rigidbody2D rb2d;
    private Destruction destruction;

    // Start is called before the first frame update
    void Start()
    {
        Assets.Constants.NoOfLanes = numberOfLanes;
        int i;

        // Getting speedometer
        hud = GameObject.Find("HUD");
        speedo = hud.GetComponentInChildren<Speedo>();

        // Initialising the Camera
        camInst = Instantiate(cam, new Vector3(0, 0, -10), Quaternion.identity);

        // Initialising the lanes
        for(i = 0; i < numberOfLanes; i++)
        {
            laneInst = Instantiate(lane, new Vector3(i*laneShift, 0, 0), Quaternion.identity);
            laneColliderList.Add(laneInst.GetComponent<BoxCollider2D>());
        }
        Instantiate(verge, new Vector3(i*laneShift, 0, 0), Quaternion.identity);
        Instantiate(verge, new Vector3(-1*laneShift, 0, 0), Quaternion.identity);

        // Working out the required density of the bots
        float numOfBots = ((botDensity)*0.01f)*((Assets.Constants.ROADLENGTH*2)/Assets.Constants.SAFEDISTANCE);
        Debug.Log(botDensity.ToString());
        Debug.Log(numOfBots.ToString());
        numOfBots = (int)numOfBots;
        float spacing = Assets.Constants.ROADLENGTH * 2 / numOfBots;

        // Instatiating the bots
        for(int j = 0; j < numberOfLanes; j++)
        {
            for (i = 0; i < numOfBots; i++)
            {
                if((i == 0)&&(j == 0))
                //if(false)
                {
                    // If test running instatiate a taught agent object
                    if(test)
                    {
                        botInst = Instantiate(pretendHuman, new Vector3(j * laneShift, -Assets.Constants.ROADLENGTH + i * spacing, 0), Quaternion.identity);
                        humanTransform = botInst.GetComponent<Transform>();
                        rb2d = botInst.GetComponent<Rigidbody2D>();
                        agent = botInst.GetComponent<Agent>();
                        agent.GiveBrain(learningBrain);
                        speedo.human = rb2d;

                        try
                        {
                            cvc = camInst.GetComponentInChildren<CinemachineVirtualCamera>();
                            Debug.Log(cvc.ToString());
                            cvc.Follow = humanTransform;
                        }
                        catch (Exception e)
                        {
                            Debug.Log("Exception Caught");
                            Debug.Log(e.Message);
                        }

                    }
                    // Otherwise instatiate a human controlled bot
                    else
                    {
                        botInst = Instantiate(human, new Vector3(j * laneShift, -Assets.Constants.ROADLENGTH + i * spacing, 0), Quaternion.identity);
                        humanTransform = botInst.GetComponent<Transform>();
                        rb2d = botInst.GetComponent<Rigidbody2D>();
                        agent = botInst.GetComponent<Agent>();
                        agent.GiveBrain(humanBrain);
                        speedo.human = rb2d;

                        try
                        {
                            cvc = camInst.GetComponentInChildren<CinemachineVirtualCamera>();
                            Debug.Log(cvc.ToString());
                            cvc.Follow = humanTransform;
                        }
                        catch (Exception e)
                        {
                            Debug.Log("Exception Caught");
                            Debug.Log(e.Message);
                        }
                    }
                }
                else
                {
                    botInst = Instantiate(bot, new Vector3(j * laneShift, -Assets.Constants.ROADLENGTH + i * spacing, 0), Quaternion.identity);
                    botInst.name = i.ToString() + "_" + j.ToString();
                    botInst.tag = "botClone";
                    autoInput = botInst.GetComponent<AutoInput>();
                    autoInput.SetBotID(j, i);
                    agent = botInst.GetComponent<Agent>();
                    agent.GiveBrain(botBrain);
                }

                botLoop = botInst.GetComponent<Loop>();

                foreach (Collider2D col in laneColliderList)
                {
                    botLoop.IgnoreLane(col);
                }
            }
        }
    }

    public void Reset()
    {
        var clones = GameObject.FindGameObjectsWithTag("botClone");
        foreach (var clone in clones)
        {
            Destroy(clone);
        }

        float numOfBots = ((botDensity) * 0.01f) * ((Assets.Constants.ROADLENGTH * 2) / Assets.Constants.SAFEDISTANCE);
        Debug.Log(botDensity.ToString());
        Debug.Log(numOfBots.ToString());
        numOfBots = (int)numOfBots;
        float spacing = Assets.Constants.ROADLENGTH * 2 / numOfBots;

        for (int j = 0; j < numberOfLanes; j++)
        {
            for (int i = 0; i < numOfBots; i++)
            {
                if ((i == 0) && (j == 0))
                {

                }
                else
                {
                    botInst = Instantiate(bot, new Vector3(j * laneShift, -Assets.Constants.ROADLENGTH + i * spacing, 0), Quaternion.identity);
                    botInst.name = i.ToString() + "_" + j.ToString();
                    botInst.tag = "botClone";
                    autoInput = botInst.GetComponent<AutoInput>();
                    autoInput.SetBotID(j, i);
                    agent = botInst.GetComponent<Agent>();
                    agent.GiveBrain(botBrain);

                    botLoop = botInst.GetComponent<Loop>();

                    foreach (Collider2D col in laneColliderList)
                    {
                        botLoop.IgnoreLane(col);
                    }
                }
            }
        }
    }
}
