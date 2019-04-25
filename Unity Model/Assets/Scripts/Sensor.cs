using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Sensor : MonoBehaviour
{
    private float range = 8f;
    private Transform origin;
    private Vector3 end = new Vector3();
    private float angle;
    private Vector3 vectAngle = new Vector3();
    private RaycastHit2D hitInfo = new RaycastHit2D();
    private RaycastHit2D hitInfo2 = new RaycastHit2D();
    private bool roadSideFound = false;

    private float roadSideDist = -1f;
    private float spriteDist = -1f;
    private float vergeDist = -1f;
    private float spriteVelocity = -1f;
    private GameObject hitSprite;
    private int spriteLane;
    private AutoInput tempAutoInput;
    private Rigidbody2D tempRB;
    private DataControl tempDataControl;

    public Sensor(float a, Transform o)
    {
        this.origin = o;
        SetAngle(a);
    }

    public void CastRay()
    {
        roadSideDist = -1f;
        spriteDist = -1f;
        float currentDir = origin.rotation.eulerAngles.z;
        vectAngle.Set(Mathf.Cos((angle + currentDir + 90)/Mathf.Rad2Deg), Mathf.Sin((angle + currentDir + 90) /Mathf.Rad2Deg), 0);
        hitInfo = Physics2D.Raycast(origin.position, vectAngle, range, 1 << LayerMask.NameToLayer("Default"));

        end = origin.position + ((vectAngle) * range);
    

        // Looking for collisions with the side of the lanes

        if (hitInfo.collider != null)
        {
            Debug.DrawLine(origin.position, hitInfo.point, Color.red);
            roadSideFound = true;
            roadSideDist = Vector3.Distance(origin.position, hitInfo.point);
            hitInfo = new RaycastHit2D();
        }
        else
        {
            roadSideFound = false;
            roadSideDist = -1f;
        }

        // Looking for hits on the sprite level

        hitInfo = Physics2D.Raycast(origin.position, vectAngle, range, 1 << LayerMask.NameToLayer("Sprite"));
        hitInfo2 = Physics2D.Raycast(origin.position, vectAngle, range, 1 << LayerMask.NameToLayer("Human"));
        if (hitInfo.collider != null)
        {
            Debug.DrawLine(origin.position, hitInfo.point, Color.blue);
            spriteDist = Vector3.Distance(origin.position, hitInfo.point);
            hitSprite = hitInfo.transform.gameObject;
            tempAutoInput = hitSprite.GetComponent<AutoInput>();
            spriteLane = tempAutoInput.GetLaneNumber();
            hitInfo = new RaycastHit2D();
        }
        else if (hitInfo2.collider != null)
        {
            Debug.DrawLine(origin.position, hitInfo2.point, Color.blue);
            spriteDist = Vector3.Distance(origin.position, hitInfo2.point);
            hitSprite = hitInfo2.transform.gameObject;
            tempRB = hitSprite.GetComponent<Rigidbody2D>();
            spriteVelocity = tempRB.velocity.magnitude;
            tempDataControl = hitSprite.GetComponent<DataControl>();
            spriteLane = tempDataControl.GetLaneNumber();
            hitInfo2 = new RaycastHit2D();
        }
        else
        {
            spriteDist = -1f;
        }


        // Looking at the verge level

        hitInfo = Physics2D.Raycast(origin.position, vectAngle, range, 1 << LayerMask.NameToLayer("Verge"));
        if (hitInfo.collider != null)
        {
            Debug.DrawLine(origin.position, hitInfo.point, Color.red);
            vergeDist = Vector3.Distance(origin.position, hitInfo.point);
            hitInfo = new RaycastHit2D();
        }
        else
        {
            vergeDist = -1f;
        }



    }

    public int SpriteLaneNumber
    {
        get
        {
            return spriteLane;
        }
    }

    public void Highlight(Color c)
    {
        Debug.DrawLine(origin.position, end, c);
    }

    public float Angle
    {
        get
        {
            return angle;
        }
    }

    public float RoadSideDist
    {
        get
        {
            return roadSideDist;
        }
    }

    public float SpriteDist
    {
        get
        {
            return spriteDist;
        }
    }

    public float[] Values
    {
        get
        {
            float[] values = new float[] {spriteDist, spriteLane, roadSideDist, vergeDist, spriteVelocity};
            return values;
        }
    }

    public Vector3 End
    {
        get
        {
            return end;
        }
    }

    public bool Active
    {
        get
        {
            return roadSideFound;
        }
    }

    private void SetAngle(float a)
    {
        angle = a;
    }
}
