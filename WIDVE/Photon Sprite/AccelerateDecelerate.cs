using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AccelerateDecelerate : MonoBehaviour
{
    //<summary>
    //This script allows the sprite to accelerate and then decelerate towards
    //the target with a projectile-like motion

    public enum State { START, HAND, MOVE, TARGET }
    public State state;
    public Transform hand;
    public Transform target;
    public float speed = 125f;
    public Rigidbody rb;
    //I set the mass of the sprite to (mass value)


    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        state = State.START;
    }

    public void Update()
    {
        OVRInput.Update();

        if (OVRInput.Get(OVRInput.Axis1D.PrimaryHandTrigger) == 1)
        {
            StopAllCoroutines();
            rb.drag = 0f; 
            state = State.HAND;
        }

        else if (OVRInput.Get(OVRInput.Axis1D.PrimaryIndexTrigger) == 1)
        {
            StopAllCoroutines();
            state = State.MOVE;
        }

        switch (state)
        {
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

        IEnumerator Hand()
        {
            while (Vector3.Distance(transform.position, hand.transform.position) > 0.05)
            {
                Vector3 handdirection = hand.transform.position - transform.position;
                Vector3 handvector = Vector3.Normalize(handdirection) * speed * Time.fixedDeltaTime;
                //I normalized this vector so we can control velocity in the Inspector
                rb.velocity = handvector;
                yield return new WaitForFixedUpdate();
            }

            if (Vector3.Distance(transform.position, hand.transform.position) < .05)
            {
                transform.position = hand.transform.position;
                StopCoroutine(Hand());
                
            }
        } 

        IEnumerator Move()
        {
            //The drag and add force values (eg .15f on line 86) in this coroutine
            //may need to change based on the expected distance between the
            //participant and the target.
            //The largest distance I tested was about 7 meters. 


            if (Vector3.Distance(target.transform.position, transform.position) > .1f)
                //the value .1f is also adjustable
            {
                while (Vector3.Distance(target.transform.position, transform.position) >.1f)
                {
                    Vector3 targetdirection = target.transform.position - transform.position;
                    rb.AddForce(targetdirection.x*.15f, targetdirection.y*.15f, targetdirection.z*.15f, ForceMode.VelocityChange);
                    yield return new WaitForFixedUpdate(); 

                    if (Vector3.Distance(target.transform.position, transform.position) <3f && Vector3.Distance(target.transform.position, transform.position) >.1f)
                    //I put this range in so that the sprite accelerates for a few seconds before slowing to a stop

                    {
                        Vector3 targetdirection1 = target.transform.position - transform.position;
                        rb.drag = 90f;
                        //I determined this value from trial and error
                        yield return new WaitForFixedUpdate();
                    }
                }
            }
            rb.velocity = new Vector3(0f, 0f, 0f);
            //When I tested the script, the above line wasn't necessary.
            //I'm keeping it so that the sprite will still stop at the target
            //regardless of drag.

            transform.position = target.transform.position;
            //This will transition the sprite into State.TARGET 

                StopCoroutine(Move());
        }
    }
}
    
