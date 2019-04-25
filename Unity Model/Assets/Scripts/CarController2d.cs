using System.Collections;
using UnityEngine;

public class CarController2d : MonoBehaviour
{ 
 
    public float accelerationFactor;
    public float steeringFactor;
    public Rigidbody2D rb;
    public float speed;

    void Start()
    {
        //rb = GetComponent<Rigidbody2D>();
    }

    public void Move(float h, float v)
    {
        h = Mathf.Clamp(h, -1, 1);
        v = Mathf.Clamp(v, -1, 1);

        Vector2 speed = transform.up * (v * accelerationFactor);
        rb.AddForce(speed);

        float direction = Vector2.Dot(rb.velocity, rb.GetRelativeVector(Vector2.up));
        if (direction >= 0.0f)
        {
            rb.rotation += h * steeringFactor * (rb.velocity.magnitude / 5.0f);
            //rb.AddTorque((h * steeringFactor) * (rb.velocity.magnitude / 10.0f));
        }
        else
        {
            rb.rotation -= h * steeringFactor * (rb.velocity.magnitude / 5.0f);
            //rb.AddTorque((-h * steeringFactor) * (rb.velocity.magnitude / 10.0f));
        }

        Vector2 forward = new Vector2(0.0f, 0.5f);
        float steeringFactorRightAngle;
        if (rb.angularVelocity > 0)
        {
            steeringFactorRightAngle = -90;
        }
        else
        {
            steeringFactorRightAngle = 90;
        }

        Vector2 rightAngleFromForward = Quaternion.AngleAxis(steeringFactorRightAngle, Vector3.forward) * forward;
        Debug.DrawLine((Vector3)rb.position, (Vector3)rb.GetRelativePoint(rightAngleFromForward), Color.green);

        float driftForce = Vector2.Dot(rb.velocity, rb.GetRelativeVector(rightAngleFromForward.normalized));

        Vector2 relativeForce = (rightAngleFromForward.normalized * -1.0f) * (driftForce * 10.0f);


        Debug.DrawLine((Vector3)rb.position, (Vector3)rb.GetRelativePoint(relativeForce), Color.red);

        //rb.AddForce(rb.GetRelativeVector(relativeForce));
    }
 }
