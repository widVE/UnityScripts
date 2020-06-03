using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orbit : MonoBehaviour

{
    //<summary>
    //This is a copy of the script that caused the sprite to orbit
    //<summary>

    public enum State { START, HAND, MOVE, TARGET }
    public State state;
    public Transform hand;
    public Transform target;
    public float speed = 10f;
    public float forceMult = 200;
    public Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        state = State.START;
    }

    void Update()
    {
        OVRInput.Update();

        if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) == 1)
        {
            state = State.HAND;
        }

        else if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) == 1)
        {
            state = State.MOVE;
        }

        else if (Vector3.Distance(transform.position, target.transform.position) == 0)
        {
            state = State.TARGET;
        }

        switch (state)
        {
            //So the sprite doesn't start in the person's hand right away
            case State.START:
                break;

            case State.HAND:
                StartCoroutine(Hand());
                break;

            case State.MOVE:
                Move();
                break;

            case State.TARGET:
                print("Target");
                break;
        }
    }

    IEnumerator Hand()
    {
        while (Vector3.Distance(transform.position, hand.transform.position) > 0.05)
        {
            Vector3 handdirection = hand.transform.position - transform.position;
            Vector3 handvector = Vector3.Normalize(handdirection) * speed * Time.deltaTime;
            rb.velocity = handvector;
            yield return null;
        }

        if (Vector3.Distance(transform.position, hand.transform.position) < 0.05)
        {
            StopCoroutine(Hand());
            transform.position = hand.transform.position;
        }
    }

    void Move()
    {
        Vector3 targetdirection = target.transform.position - transform.position;

        if (Vector3.Distance(transform.position, target.transform.position) > 0.5)
        {
            Vector3 accelerationforce = Vector3.Normalize(targetdirection) * forceMult * Time.deltaTime;
            rb.AddForce(accelerationforce);
        }


        if (Vector3.Distance(transform.position, target.transform.position) < .05)
        {
            transform.position = target.transform.position;

        }
    }
}

