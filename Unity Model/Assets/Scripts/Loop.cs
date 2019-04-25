using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Loop : MonoBehaviour
{
    public float xBoundary = Assets.Constants.ROADLENGTH;
    public float yBoundary = Assets.Constants.ROADLENGTH;

    public Transform transform;

    // Called to avoid the vehicles collider from interacting with the lanes collider
    public void IgnoreLane(Collider2D collider)
    {
        Physics2D.IgnoreCollision(GetComponent<BoxCollider2D>(), collider);
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 tempTransform;

        string str = "Y position " + transform.position.y.ToString();

        if(transform.position.y > yBoundary)
        {
            tempTransform = transform.position;
            transform.position = new Vector3(tempTransform.x, -yBoundary, tempTransform.z);
        }
        else if(transform.position.y < -yBoundary)
        {
            tempTransform = transform.position;
            transform.position = new Vector3(tempTransform.x, yBoundary, tempTransform.z);
        }

    }

    void OnCollisionEnter(Collision col)
    {
        if(col.gameObject.layer == 8)
        {
            Destroy(col.gameObject);
        }
    }
}
