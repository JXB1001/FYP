using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Speedo : MonoBehaviour
{
    public Rigidbody2D human;
    public Text speedText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        speedText.text = (human.velocity.magnitude* 2.23694f).ToString("0") + " mph";
    }
}
