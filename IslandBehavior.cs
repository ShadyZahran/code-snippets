using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IslandBehavior : MonoBehaviour
{
    private Vector3 targetPoint;
    private int currentIndex = 0;
    [Tooltip("Assign reference to the desired orbit")]
    public Orbit DE_myOrbit;
    private Vector3[] myPath;
    [Tooltip("Island orbiting speed")]
    public float DE_orbitSpeed = 2f;
    public float DE_rotationSpeed = 2f;
    [Tooltip("Where along the path should the orbit start")]
    [Range(0f, 1f)]
    public float DE_startProgress = 0f;
    [Tooltip("If the island should move in the opposite direction")]
    public bool DE_counterClockwise = false;
    public CharacterController targetPlayer;
    Rigidbody myRigidbody;
    public Transform DE_centerOfUniverse;
    public Vector3 angularV;
    bool startMoving = false;

    private void Awake()
    {
        myRigidbody = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        Setup();
    }

    private void FixedUpdate()
    {
        if (!startMoving)
        {
            return;
        }
        if (targetPlayer != null)
        {
            targetPlayer.gameObject.GetComponent<PlayerBehavior>().UpdateIslandMotionVector(myRigidbody.velocity, myRigidbody.angularVelocity);
        }
        else
        {
            Debug.Log("Target player is null");
        }

        if (DE_counterClockwise)
        {
            if (currentIndex > 0)
            {
                if (Vector3.Distance(transform.position, myPath[currentIndex]) > 0.1f)
                {
                    myRigidbody.MovePosition(Vector3.MoveTowards(transform.position, myPath[currentIndex], DE_orbitSpeed * Time.fixedDeltaTime));
                }
                else
                {
                    currentIndex--;
                }
            }
            else
            {
                currentIndex = myPath.Length - 1;
            }
        }
        else
        {
            if (currentIndex < myPath.Length - 1)
            {
                if (Vector3.Distance(transform.position, myPath[currentIndex]) > 0.1f)
                {
                    myRigidbody.MovePosition(Vector3.MoveTowards(transform.position, myPath[currentIndex], DE_orbitSpeed * Time.fixedDeltaTime));
                }
                else
                {
                    currentIndex++;
                }
            }
            else
            {
                currentIndex = 0;
            }
        }

        // rotate towards center of universe
        Vector3 tempTarget = transform.InverseTransformPoint(DE_centerOfUniverse.transform.position);

        float angle = Mathf.Atan2(tempTarget.x, tempTarget.z) * Mathf.Rad2Deg;

        Vector3 eulerAngleVelocity = new Vector3(0, angle, 0);
        Quaternion deltaRotation = Quaternion.Euler(eulerAngleVelocity * Time.deltaTime * DE_rotationSpeed);
        myRigidbody.MoveRotation(myRigidbody.rotation * deltaRotation);
        angularV = myRigidbody.angularVelocity;

    }
    /// <summary>
    /// Sets up the path of the island according to the referenced orbit.
    /// </summary>
    public void Setup()
    {
        myPath = new Vector3[DE_myOrbit.myLineRenderer.positionCount];
        myPath = DE_myOrbit.GetPathLocalSpace();

        if (DE_counterClockwise)
        {
            int calculatedIndex = (myPath.Length - 1) - Mathf.FloorToInt(DE_startProgress * (myPath.Length - 1));
            currentIndex = calculatedIndex;
            transform.position = myPath[calculatedIndex];
        }
        else
        {
            int calculatedIndex = Mathf.FloorToInt(DE_startProgress * (myPath.Length - 1));
            currentIndex = calculatedIndex;
            transform.position = myPath[calculatedIndex];
        }


    }
    /// <summary>
    /// sets the target player so the island's velocity is applied to that player
    /// </summary>
    /// <param name="fTargetPlayer"></param>
    public void SetTarget(CharacterController fTargetPlayer)
    {
        targetPlayer = fTargetPlayer;
        Debug.Log(targetPlayer.gameObject.GetComponent<PlayerBehavior>().playerName);
    }

    public void StartMoving(bool fFlag)
    {
        startMoving = fFlag;
    }
}
