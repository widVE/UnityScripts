using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConstantSpeed : MonoBehaviour
{
    //<summary>
    //This is the script that allows the sprite to move to and from the target
    //in a straight line at a constant speed

    public enum State { START, HAND, MOVE, TARGET }
    public State state;
    public Transform hand;
    public Transform target;
    public float speed = 10f;
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
            StopAllCoroutines();
            state = State.HAND;
        }

        else if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) == 1)
        {
            StopAllCoroutines();
            state = State.MOVE;
        }

        else if (Vector3.Distance(transform.position, target.transform.position) == 0)
        {
            StopAllCoroutines();
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
                StartCoroutine(Move());
                break;

            case State.TARGET:
                //I haven't worked on this state yet
                break;
        }
    }
    IEnumerator Hand()
    {
        while (Vector3.Distance(transform.position, hand.transform.position) > 0.05)
        {
            Vector3 handdirection = hand.transform.position - transform.position;
            Vector3 handvector = Vector3.Normalize(handdirection) * speed * Time.deltaTime;
            //I normalized the vector so that we can set the speed in the Inspector
            rb.velocity = handvector;
            yield return null;
            //This should be yield return new WaitForFixedUpdate();, but I haven't tested
            //this script with that line in it yet. I don't think we will be using this script
            //in the final product, so I'm leaving it as is for now.
        }

    if (Vector3.Distance(transform.position, hand.transform.position) < 0.05)
            {
                StopCoroutine(Hand());
                {
                }
            transform.position = hand.transform.position;
            }    
    }

    IEnumerator Move()
    {
        while (Vector3.Distance(transform.position, target.transform.position) > 0.05)
        {
            Vector3 targetdirection = target.transform.position - transform.position;
            Vector3 targetvector = Vector3.Normalize(targetdirection) * speed * Time.deltaTime;
            rb.velocity = targetvector; 
            yield return null;
            //see line 73
        }

    if (Vector3.Distance(transform.position, target.transform.position) <.05)
            {
                StopCoroutine(Move());
                {
                }
            transform.position = target.transform.position;
            //This will transition the sprite into State.TARGET

        }  
    }
}
