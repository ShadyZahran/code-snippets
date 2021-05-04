using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Freak : MonoBehaviour
{
    [Header("Stats")]
    public string Name;
    public int maxHealth;
    [SerializeField]private int currentHealth;
    public int maxCharge = 100;
    [SerializeField]private int currentCharge; 
    public float speed;

    [Header("Damage")]
    public int basicAttackDamage = 1;
    public int mutationAttackDamage = 2;

    public CharacterController2D m_characterController;
    float horizontalMove = 0f;
    bool jump = false;


    public PlayerController m_playercontroller;
    [SerializeField] int m_index;
    [SerializeField] Facing myFacing;
    [SerializeField] float stunLaunchPower;
    [SerializeField] float hitLaunchPower;
    public Image ui_health;
    public Image ui_charge;
    public Text debugText;

    [SerializeField] Animator m_myEffectsAnimator;
    public GameObject immuneIndicator, stunIndicator;
    public int Health
    {
        get
        { return currentHealth; }
        set
        {
            if (currentHealth == value) return;
            else
            {
                currentHealth = value;
                if (OnHealthChange != null)
                    OnHealthChange(currentHealth);
            }

        }
    }

    public int Charge
    {
        get
        { return currentCharge; }
        set
        {
            if (currentCharge == value) return;
            else
            {
                currentCharge = value;
                if (OnChargeChange != null) OnChargeChange(currentCharge);
            }
        }
    }

    public delegate void OnVariableChangeDelegate(int newVal);
    public event OnVariableChangeDelegate OnHealthChange;
    public event OnVariableChangeDelegate OnChargeChange;

    private void Awake()
    {
        m_characterController = GetComponent<CharacterController2D>();
        OnHealthChange += Freak_OnHealthChange;
        OnChargeChange += Freak_OnChargeChange;
        Charge = 0;
        Health = maxHealth;
    }

    private void Freak_OnHealthChange(int newVal)
    {
        //update health ui
        ui_health.fillAmount = (newVal * 1.0f) / (maxHealth * 1.0f);
    }

    private void Freak_OnChargeChange(int newVal)
    {
        ui_charge.fillAmount = (newVal * 1.0f) / (maxCharge * 1.0f);
    }

    private void Update()
    {

    }

    private void FixedUpdate()
    {
        // Move/Jump our character
        if (m_characterController != null)
        {
            m_characterController.Move(horizontalMove * Time.fixedDeltaTime, jump);
            jump = false;
        }
    }


    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="horizontalMovementValue">Horizontal movement value from Gamepad</param>
    public void Move(float horizontalMovementValue)
    {
        horizontalMove = horizontalMovementValue * speed;
        m_characterController.Animation_Movement(Mathf.Abs(horizontalMove));
    }

    public void Jump()
    {
        jump = true;
    }


    public void OnHit(int damage, int direction)
    {
        if ((m_playercontroller.Currentstate == PlayerController.PlayerState.Active ||
            m_playercontroller.Currentstate == PlayerController.PlayerState.InHitAnimation ||
            m_playercontroller.Currentstate == PlayerController.PlayerState.InJumpAnimation ||
            m_playercontroller.Currentstate == PlayerController.PlayerState.Stunned)
            &&
            !m_playercontroller.isInvulnerable)

        {
            Health -= damage;
            Charge += 10;
            m_characterController.Animation_GetHit();
            Move(0f);
            m_characterController.Launch(direction, stunLaunchPower);



            if (Health <= 0)
            {
                KillCharacter();
            }
        }
        else if (m_playercontroller.Currentstate == PlayerController.PlayerState.Blocking && !m_playercontroller.IsBlockingSameDirectionAsHit(direction))
        {
            Health -= damage;
            Charge += 10;

            m_characterController.Animation_GetHit();
            Move(0f);
            m_characterController.Launch(direction, stunLaunchPower);
            Block(false);


            if (Health <= 0)
            {
                KillCharacter();
            }
        }

        else if (m_playercontroller.Currentstate == PlayerController.PlayerState.Blocking && m_playercontroller.IsBlockingSameDirectionAsHit(direction))
        {
            m_characterController.Launch(direction, hitLaunchPower);
            Charge += 5;
        }

    }


    public void BasicAttack()
    {
        m_characterController.Animation_BasicAttack();
        Move(0f);
        if (m_playercontroller.MyFacing == Facing.Right)
        {
            m_characterController.Launch(1, hitLaunchPower);
        }
        else if (m_playercontroller.MyFacing == Facing.Left)
        {
            m_characterController.Launch(-1, hitLaunchPower);
        }

    }


    //should tell the player controller to handle this character's death and activate the next one if exists
    void KillCharacter()
    {
        m_playercontroller.KillCharacter(this.gameObject);
    }


    public void InitFreak(int index, PlayerController playercontroller)
    {
        m_index = index;
        m_playercontroller = playercontroller;
    }


    public void FlipCharacter()
    {
        m_characterController.Flip();
    }


    public void Block(bool value)
    {
        m_characterController.Animation_Block(value);
    }

    public void MutationAttack()
    {
        if (Charge < maxCharge) return;
        m_characterController.Animation_MutationAttack();
        Move(0f);
        if (m_playercontroller.MyFacing == Facing.Right)
        {
            m_characterController.Launch(1, hitLaunchPower);
        }
        else if (m_playercontroller.MyFacing == Facing.Left)
        {
            m_characterController.Launch(-1, hitLaunchPower);
        }
        Charge = 0;
    }

    public void Invulnerable(bool value)
    {
        m_characterController.Animation_Invulnerable(value);
    }

    public enum Facing
    {
        Right,
        Left
    }

    public void UpdateState(string state)
    {
        //handles stopping the character when presses block
        //or any transition for that matter
        Move(0f);



        switch (state)
        {
            case "active":
                //if we go back from stunned to active, then reset the hit counter
                if (m_playercontroller.Currentstate == PlayerController.PlayerState.Stunned)
                {
                    m_playercontroller.ResetStunCounter();
                }
                m_playercontroller.Player_UpdateState(PlayerController.PlayerState.Active);
                break;
            case "dead":
                m_playercontroller.Player_UpdateState(PlayerController.PlayerState.Dead);
                break;
            case "stunned":
                //start counting for immune status
                m_playercontroller.UpdateHitCounter();
                m_playercontroller.Player_UpdateState(PlayerController.PlayerState.Stunned);
                break;
            case "inhitanimation":
                //if we go back from stunned to active, then reset the hit counter
                if (m_playercontroller.Currentstate == PlayerController.PlayerState.Stunned)
                {
                    m_playercontroller.ResetStunCounter();
                }
                m_playercontroller.Player_UpdateState(PlayerController.PlayerState.InHitAnimation);
                break;
            case "injumpanimation":
                //if we go back from stunned to active, then reset the hit counter
                if (m_playercontroller.Currentstate == PlayerController.PlayerState.Stunned)
                {
                    m_playercontroller.ResetStunCounter();
                }
                m_playercontroller.Player_UpdateState(PlayerController.PlayerState.InJumpAnimation);
                break;
            case "blocking":
                //if we go back from stunned to active, then reset the hit counter
                if (m_playercontroller.Currentstate == PlayerController.PlayerState.Stunned)
                {
                    m_playercontroller.ResetStunCounter();
                }
                m_playercontroller.Player_UpdateState(PlayerController.PlayerState.Blocking);
                break;
            default:
                break;
        }
    }

    public void DebugTextDisplay(string message)
    {
        debugText.text = message;
    }

    public void PlayHitEffectAnimation()
    {
        m_myEffectsAnimator.SetTrigger("EffectHit");
    }
}
