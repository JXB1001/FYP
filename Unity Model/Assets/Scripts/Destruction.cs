using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Destruction : MonoBehaviour
{
    private Rigidbody2D rb2d;
    private GameObject reference;
    private BoxCollider2D col;

    // Start is called before the first frame update
    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        col = GetComponent<BoxCollider2D>();
    }

    public void AddReference(GameObject r)
    {
        reference = r;
    }

    public void OnCollisionEnter2D(Collision2D coll)
    {
        Debug.Log("DESTROYING BOT");
        //Destroy(gameObject);
    }
}
