using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Respawn : MonoBehaviour
{
    void OnCollsion2D(Collision2D coll)
    {
        Destroy(gameObject);
    }
}
