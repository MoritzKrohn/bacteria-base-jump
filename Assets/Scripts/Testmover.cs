using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Testmover : MonoBehaviour {
    public Vector2 velocity;
    public Vector2 direction;
    public Rigidbody2D rb2D;
    public bool Moving; // Moves if set to true
    public GameObject other;

	// Use this for initialization
	void Start ()
    {
        rb2D = GetComponent<Rigidbody2D>();
        StartCoroutine(SetHeading());
    }

    IEnumerator SetHeading()
    {
        while(true)
        {
            // Random value
            velocity = new Vector2(Random.Range(-1F, 1F), Random.Range(-1F, 1F));
            // directed value
            direction = (other.transform.position - transform.position).normalized;
            Debug.Log(direction);
            yield return new WaitForSeconds(1F);
        }
    }

    void FixedUpdate()
    {
        if(Moving)
        {
            var movementSpeed = 3;
            Debug.DrawLine(transform.position, other.transform.position);
            //rb2D.MovePosition(rb2D.position + velocity * Time.fixedDeltaTime);
            Vector2 myPosition = transform.position; // trick to convert a Vector3 to Vector2
            rb2D.MovePosition(myPosition + direction * movementSpeed * Time.deltaTime);
        }
    }

    // Update is called once per frame
    void Update () {
		
	}
}
