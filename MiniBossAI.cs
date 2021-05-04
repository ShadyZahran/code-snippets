using UnityEngine;
using System.Collections;
using System;

public class MiniBossAI : MonoBehaviour
{

    
    public GameObject Player;
    float rangedChaseTimer;
    float rangedAttackTimer;
    Rigidbody2D myRigidBody;
    public Transform PlayerChecker;
    public Transform GroundChecker;
    public LayerMask GroundLayers;
    public GameObject TrailEffect;
    public bool grounded;
    public bool isPlayerInside;
    public enum States
    {
        Chase = 1,
        LeapAttack = 2,
        Airborne = 3,
        Melee = 4,
        Idle = 999,
    }
    public States currentState = States.Idle;
    int currentAttack;
    public float Speed = 3f;
    public float LeapForce;
    float LeapCooldownTimer;

    public float thresholdA, thresholdB, thresholdC, thresholdD;

    int rangedAttackCounter;

    public float distanceToTarget;
    float landingCooldown;
    Animator myAnimator;

    AudioSource[] sounds;
    public enum enemyFacing
    {
        Right,
        Left
    }
    public enemyFacing enemyDirection = enemyFacing.Left;

    // Use this for initialization
    void Start()
    {
        if (Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player");
        }
        myRigidBody = GetComponent<Rigidbody2D>();
        sounds = GetComponents<AudioSource>();
        myAnimator = transform.GetChild(0).GetComponent<Animator>();

    }
    void FixedUpdate()
    {
        if (Player == null)
        {
            Player = GameObject.FindGameObjectWithTag("Player");
        }
        else
        {
            distanceToTarget = Vector3.Distance(transform.position, Player.transform.position);
            //grounded checker
            grounded = Physics2D.OverlapCircle(GroundChecker.position, 0.08f, GroundLayers);

            //landing timer handler
            if (landingCooldown > 0)
            {
                landingCooldown -= Time.deltaTime;
            }

            #region enemy facing 
            if (enemyDirection == enemyFacing.Right)
            {
                this.transform.localScale = new Vector3(-1 * (Mathf.Abs(transform.localScale.x)), transform.localScale.y);
            }
            else if (enemyDirection == enemyFacing.Left)
            {
                this.transform.localScale = new Vector3((Mathf.Abs(transform.localScale.x)), transform.localScale.y);
            }
            #endregion

            #region trail effect handler
            if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("attack"))
            {
                TrailEffect.SetActive(true);
            }
            else
            {
                TrailEffect.SetActive(false);
            }
            #endregion


            switch (currentState)
            {
                #region Chase State
                case States.Chase:
                    if (myAnimator.GetCurrentAnimatorStateInfo(0).IsName("attack"))
                    {

                    }
                    else
                    {

                        #region chase logic
                        if (grounded && landingCooldown <= 0)
                        {
                            float directionToWalk = (Player.transform.position.x - transform.position.x) / (Mathf.Abs(Player.transform.position.x - transform.position.x));
                            myRigidBody.velocity = new Vector2(directionToWalk * Speed, myRigidBody.velocity.y);
                            myAnimator.SetFloat("Moving", Math.Abs(myRigidBody.velocity.x));
                            if (directionToWalk > 0)
                            {

                                enemyDirection = enemyFacing.Right;
                            }
                            else if (directionToWalk < 0)
                            {

                                enemyDirection = enemyFacing.Left;
                            }
                        }

                        #endregion

                        #region distance logic
                        if (Vector3.Distance(transform.position, Player.transform.position) < thresholdB && Vector3.Distance(transform.position, Player.transform.position) > thresholdC)
                        {
                            currentState = States.LeapAttack;
                        }
                        else if (Vector3.Distance(transform.position, Player.transform.position) < thresholdD)
                        {
                            currentState = States.Melee;
                        }
                        #endregion
                    }
                    break;
                #endregion

                #region LeapAttack State
                case States.LeapAttack:

                    #region attack logic
                    if (LeapCooldownTimer <= 0 && grounded)
                    {
                        Vector2 Direction = Player.transform.position - transform.position;
                        myRigidBody.AddForce(new Vector2(Direction.normalized.x * LeapForce, LeapForce));
                        LeapCooldownTimer = 0.7f;
                        currentState = States.Airborne;

                        if (Direction.x < 0)
                        {
                            this.transform.localScale = new Vector3((Mathf.Abs(transform.localScale.x)), transform.localScale.y);
                            enemyDirection = enemyFacing.Left;
                        }
                        else if (Direction.x > 0)
                        {
                            this.transform.localScale = new Vector3(-1 * (Mathf.Abs(transform.localScale.x)), transform.localScale.y);
                            enemyDirection = enemyFacing.Right;
                        }
                    }
                    if (LeapCooldownTimer > 0)
                    {
                        LeapCooldownTimer -= Time.deltaTime;
                    }
                    #endregion

                    break;
                #endregion

                #region Airborne State
                case States.Airborne:

                    if (grounded)
                    {
                        currentState = States.Chase;
                        landingCooldown = 0.5f;
                    }
                    break;
                #endregion

                #region Melee
                case States.Melee:
                    if (!myAnimator.GetCurrentAnimatorStateInfo(0).IsName("attack"))
                    {
                        myAnimator.SetTrigger("Attack");
                        AudioSource temp = sounds.GetValue(UnityEngine.Random.Range(0, 2)) as AudioSource;
                        temp.PlayDelayed(0.5f);
                    }




                    #region distance logic
                    if (Vector3.Distance(transform.position, Player.transform.position) > thresholdD)
                    {
                        currentState = States.Chase;
                    }
                    #endregion

                    break;



                #endregion

                #region Idle State
                case States.Idle:

                    #region distance logic
                    if (Vector3.Distance(transform.position, Player.transform.position) < thresholdA)
                    {
                        currentState = States.Chase;
                    }
                    #endregion

                    break;
                #endregion

                default:
                    break;
            }
        }
        

    }

    public void DamagePlayer()
    {
        if (isPlayerInside)
        {
            Debug.Log("Player Hit!");
        }
    }

}
