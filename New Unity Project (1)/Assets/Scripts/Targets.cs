using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
//using System.Diagnostics;
//using System.Diagnostics;
using UnityEngine;

public class Targets : MonoBehaviour
{
    Camera viewCamera;

    public Transform startMarker;
    public Transform endMarker;
    private Transform targ;

    public float speed = 4.0F;
    float timeLeft;

    bool begin = true;

    Vector3 rotSpeed;

    static Transform callTarget;
    static bool call=false;
    public float callRadius = 20;

    public float TimeToStop = 10;
    float timeBeforeStop;
    public float rotatingTime = 3;
    float rotatingLeft;

    public float ChaseTime = 10;
    public float viewRadius;
    [Range(0, 360)]
    public float viewAngle;

    public LayerMask targetMask;
    public LayerMask obstacleMask;
    float distanceToStop=0.5F; 
    [HideInInspector]
    public List<Transform> visibleTargets = new List<Transform>();

    bool hasTarget = false;

    Rigidbody rigidbody;
    Vector3 velocity;

    void Patrol()
    {
        transform.LookAt(targ);
        if (begin)
        {
            targ = startMarker;
            rigidbody.MovePosition(rigidbody.position + velocity * Time.fixedDeltaTime);
            if (Vector3.Distance(transform.position, targ.position) < distanceToStop)
                begin = false;
            
        }
        else
        {
            targ = endMarker;
            rigidbody.MovePosition(rigidbody.position + velocity * Time.fixedDeltaTime);
            if (Vector3.Distance(transform.position, targ.position) < distanceToStop)
                begin = true;
        }
    }

    void Attack()
    {


        rigidbody.MovePosition(rigidbody.position + velocity * Time.fixedDeltaTime);
        transform.LookAt(targ);
        timeBeforeStop = TimeToStop;

    }
    void getCall()
    {
        if (Vector3.Distance(transform.position, callTarget.position) < callRadius)
        {
            hasTarget = true;
            targ = callTarget;
        }
    }
    void Start()
    {
        rigidbody = GetComponent<Rigidbody>();
        viewCamera = Camera.main;
        targ = endMarker;
        rotSpeed = new Vector3(0, 100, 0);
        StartCoroutine("FindTargetsWithDelay", .2f);
        timeBeforeStop = TimeToStop;
        rotatingLeft = rotatingTime;
    }
    
    void Update()
    {
        if(call)
            getCall();

        Vector3 dirToTarget = (targ.transform.position - transform.position).normalized;
        velocity = dirToTarget * speed;
        if (!hasTarget)
        {
            timeBeforeStop -= Time.deltaTime;
        }
        if (timeBeforeStop < 0)
        {
            rotatingLeft -= Time.deltaTime;
            if (rotatingLeft < 0) {
                timeBeforeStop = TimeToStop;
                rotatingLeft = rotatingTime;
            }
        }
            float dist = Vector3.Distance(transform.position, targ.position);
        if (dist > viewRadius)
        {
            call = false;
            if(hasTarget)
                timeLeft -= Time.deltaTime;
            if (timeLeft < 0)
            {
                hasTarget = false;
            }
        }
        else
            timeLeft = ChaseTime;
    }
    void FixedUpdate()
    {
        if (hasTarget)
        {
            Attack();
        }
        else
        {
            if (timeBeforeStop < 0)
                StopAndRotate();
            else
                Patrol();
        }
    }
    IEnumerator FindTargetsWithDelay(float delay)
    {
        while (true)
        {
            yield return new WaitForSeconds(delay);
            FindVisibleTargets();
        }
    }
    void StopAndRotate()
    {
        Quaternion deltaRotation = Quaternion.Euler(rotSpeed * Time.deltaTime);
        rigidbody.MoveRotation(rigidbody.rotation * deltaRotation);
    }

    void FindVisibleTargets()
    {
        visibleTargets.Clear();
        Collider[] targetsInViewRadius = Physics.OverlapSphere(transform.position, viewRadius, targetMask);

        for (int i = 0; i < targetsInViewRadius.Length; i++)
        {

            Transform target = targetsInViewRadius[i].transform;
            Vector3 dirToTarget = (target.position - transform.position).normalized;
            targ = target;
            callTarget = targ;
            if (Vector3.Angle(transform.forward, dirToTarget) < viewAngle / 2)
            {
                hasTarget = true;
                call = true;
                float dstToTarget = Vector3.Distance(transform.position, target.position);

                if (!Physics.Raycast(transform.position, dirToTarget, dstToTarget, obstacleMask))
                {
                    hasTarget = true;
                    visibleTargets.Add(target);
                }
            }

        }

    }
    public Vector3 DirFromAngle(float angleInDegrees, bool angleIsGlobal)
    {
        if (!angleIsGlobal)
        {
            angleInDegrees += transform.eulerAngles.y;
        }
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), 0, Mathf.Cos(angleInDegrees * Mathf.Deg2Rad));
    }
}