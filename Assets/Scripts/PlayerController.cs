using System.Diagnostics.CodeAnalysis;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Progress;



public class PlayerController : MonoBehaviour, IRestartElement
{
    public enum TPunchType
    {
        RIGHT_HAND = 0,
        LEFT_HAND,
        KICK
    }

    public enum TJumpType
    {
        JUMP = 0,
        DOUBLE_JUMP,
        TRIPLE_JUMP,
        LONG_JUMP
    }

    public Camera m_Camera;
    CharacterController m_CharacterController;
    Animator m_Animator;
    Vector3 m_StartPosition;
    Quaternion m_StartRotation;
    public float m_RunSpeed;
    public float m_WalkSpeed;
    float m_VerticalSpeed = 0f;
    public Transform m_LookAt;
    public float m_DampTime = 0.2f;
    [Range(0f, 1f)] public float m_RotationLerpPct = 0.8f;
    CheckPoint m_CurrentCheckPoint;
    Vector3 m_KnockbackVelocity = Vector3.zero;

    [Header("Jump")]
    public float m_JumpSpeed = 12f;
    public float m_DoubleJumpSpeed = 16f;
    public float m_TripleJumpSpeed = 22f;
    public float m_LongJumpSpeed = 5f;
    public float m_MaxAngleToKillGombaa = 50f;
    public float m_KillJumpSpeed = 4f;
    public TJumpType m_JumpType = TJumpType.JUMP;

    public float m_CoyoteTime = 0.2f;
    private float m_CoyoteTimeCounter = 0;

    public float m_TimeBetweenJumps = 0.3f;
    private float m_TimeBetweenJumpsCounter;

    [Header("Punch")]
    public float m_MaxTimeToComboPunch = 0.8f;
    int m_CurrentPunchId;
    float m_LastPunchTime;
    public GameObject m_RightHandPunchCollider;
    public GameObject m_LeftHandPunchCollider;
    public GameObject m_KickPunchCollider;

    [Header("Input")]
    public int m_PunchMouseButton = 0;
    private KeyCode m_JumpKeyCode = KeyCode.Space;

    [Header("Elevator")]
    public float m_MaxAngleToAttachToElevator = 30f;
    Collider m_ElevatorCollider;

    [Header("Bridge")]
    public float m_BridgeHitForce = 10f;

    [Header("Audio")]
    public AudioSource m_FootStepR;
    public AudioSource m_FootStepL;

    public CoinsController m_CoinsController = new CoinsController();
    public LifeController m_LifeController = new LifeController();

    private void Awake()
    {
        m_CharacterController = GetComponent<CharacterController>();
        m_Animator = GetComponent<Animator>();
    }

    private void Start()
    {
        m_LastPunchTime = -m_MaxTimeToComboPunch;
        m_RightHandPunchCollider.SetActive(false);
        m_LeftHandPunchCollider.SetActive(false);
        m_KickPunchCollider.SetActive(false);
        m_StartPosition = transform.position;
        m_StartRotation = transform.rotation;
        GameManager.GetGameManager().AddRestartGameElements(this);
    }
    void Update()
    {
        Vector3 l_Right = m_Camera.transform.right;
        Vector3 l_Forward = m_Camera.transform.forward;
        Vector3 l_Movement = Vector3.zero;

        l_Right.y = 0;
        l_Right.Normalize();
        l_Forward.y = 0;
        l_Forward.Normalize();

        if(Input.GetKey(KeyCode.D))
        {
            l_Movement=l_Right;
        }
        else if(Input.GetKey(KeyCode.A))
        {
            l_Movement =- l_Right;
        }

        if (Input.GetKey(KeyCode.W))
        {
            l_Movement += l_Forward;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            l_Movement -= l_Forward;
        }

        l_Movement.Normalize();

        float l_SpeedAnimatorValue = 0.5f;
        float l_Speed = m_WalkSpeed;
        if (Input.GetKey(KeyCode.LeftShift))
        {
            l_Speed = m_RunSpeed;
            l_SpeedAnimatorValue = 1.0f;
        }

        if (l_Movement.sqrMagnitude == 0f)
            m_Animator.SetFloat("Speed", 0f, m_DampTime, Time.deltaTime);
        else
        {
            m_Animator.SetFloat("Speed", l_SpeedAnimatorValue, m_DampTime, Time.deltaTime);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.LookRotation(l_Movement), m_RotationLerpPct);
        }

        if (Input.GetKey(m_JumpKeyCode))
        {
            if (CanJump())
                Jump();
        }

        l_Movement *= l_Speed*Time.deltaTime;
        m_VerticalSpeed += Physics.gravity.y * Time.deltaTime;
        l_Movement.y = m_VerticalSpeed * Time.deltaTime;

        CollisionFlags l_CollisionFlags = m_CharacterController.Move(l_Movement);   
        if((l_CollisionFlags & CollisionFlags.CollidedBelow) != 0 && m_VerticalSpeed < 0f)
        {
            m_VerticalSpeed = 0f;
            m_CoyoteTimeCounter = m_CoyoteTime;
            if (m_TimeBetweenJumpsCounter < 0f) m_TimeBetweenJumpsCounter = m_TimeBetweenJumps;
        }
        else if((l_CollisionFlags & CollisionFlags.CollidedAbove) != 0 && m_VerticalSpeed > 0f)
            m_VerticalSpeed = 0f;

        m_CoyoteTimeCounter -= Time.deltaTime;
        m_TimeBetweenJumpsCounter -=Time.deltaTime;
        m_VerticalSpeed += Physics.gravity.y * Time.deltaTime;
        l_Movement.y += m_VerticalSpeed * Time.deltaTime;

        m_CharacterController.Move(l_Movement);


        UpdatePunch();
    }
    private void LateUpdate()
    {
        UpdateElevator();
    }
    
    void UpdatePunch()
    {
        if (CanPunch() && Input.GetMouseButtonDown(m_PunchMouseButton))
        {
            Punch();
        }
    }
    bool CanPunch()
    {
        return !m_Animator.IsInTransition(0) &&
            m_Animator.GetCurrentAnimatorStateInfo(0).shortNameHash == Animator.StringToHash("Movement");
    }
    void Punch()
    {
        float l_DiffPunchTime = Time.time - m_LastPunchTime;
        if (l_DiffPunchTime < m_MaxTimeToComboPunch)
            m_CurrentPunchId = (m_CurrentPunchId + 1) % 3;
        else
            m_CurrentPunchId = 0;

        m_LastPunchTime = Time.time;
        m_Animator.SetTrigger("Punch");
        m_Animator.SetInteger("PunchId", m_CurrentPunchId);
    }
    public void SetActivePunch(TPunchType PunchType, bool Active)
    {
        if (PunchType == TPunchType.RIGHT_HAND)
            m_RightHandPunchCollider.SetActive(Active);
        else if (PunchType == TPunchType.LEFT_HAND)
            m_LeftHandPunchCollider.SetActive(Active);
        else if (PunchType == TPunchType.KICK)
            m_KickPunchCollider.SetActive(Active);
    }
    bool CanJump()
    {
        return m_CoyoteTimeCounter > 0f;
    }
    void Jump()
    {
        if(m_CoyoteTimeCounter <= 0f)
        {
            m_VerticalSpeed = m_JumpSpeed;
            m_JumpType = TJumpType.JUMP;
        }
        else
        {
            if (m_JumpType == TJumpType.JUMP)
            {
                m_VerticalSpeed = m_JumpSpeed;
                m_JumpType = TJumpType.DOUBLE_JUMP;
            }
            else if (m_JumpType == TJumpType.DOUBLE_JUMP)
            {
                m_VerticalSpeed = m_DoubleJumpSpeed;
                m_JumpType = TJumpType.TRIPLE_JUMP;
            }
            else if (m_JumpType == TJumpType.TRIPLE_JUMP)
            {
                m_VerticalSpeed = m_TripleJumpSpeed;
                m_JumpType = TJumpType.JUMP;
            }
        }
        
        m_CoyoteTimeCounter = 0f;
    }
    void JumpOverEnemy()
    {
        m_VerticalSpeed = m_KillJumpSpeed;
    }
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if(hit.collider.CompareTag("Goomba"))
        {
            GoombaEnemy l_GoombaEnemy = hit.collider.GetComponent<GoombaEnemy>();

            if (CanKillWithFeet(hit))
            {
                l_GoombaEnemy.Kill();
                JumpOverEnemy();
            }
        }
        else if(hit.collider.CompareTag("Bridge"))
        {
            hit.rigidbody.AddForceAtPosition(-hit.normal * m_BridgeHitForce, hit.point);
        }
    }
    bool CanKillWithFeet(ControllerColliderHit hit)
    {
        float l_Dot = Vector3.Dot(hit.normal, Vector3.up);

        return m_VerticalSpeed < 0f && l_Dot > Mathf.Cos(m_MaxAngleToKillGombaa * Mathf.Deg2Rad);
    }
    public void RestartGame()
    {
        if(m_CurrentCheckPoint != null)
        {
            m_StartPosition = m_CurrentCheckPoint.m_RestartPosition.position;
            m_StartRotation = m_CurrentCheckPoint.m_RestartPosition.rotation;
        }

        m_CharacterController.enabled = false;
        transform.position = m_StartPosition;
        transform.rotation = m_StartRotation;
        m_CharacterController.enabled = true;
        m_LifeController.ResetLife();

    }

    public void Step (AnimationEvent _AnimationEvent)
    {
        AudioSource l_CurrentAudioSource = null;
        if (_AnimationEvent.stringParameter == "Left")
        {
            l_CurrentAudioSource = m_FootStepL;
        }
        else if(_AnimationEvent.stringParameter == "Right")
        {
            l_CurrentAudioSource = m_FootStepR;
        }

        AudioClip l_AudioClip = (AudioClip)_AnimationEvent.objectReferenceParameter;
        l_CurrentAudioSource.clip = l_AudioClip;
        l_CurrentAudioSource.Play();
    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Elevator"))
        {
            if (CanAttachToElevator(other))
            {
                AttachToElevator(other);
            }
        }
        else if (other.CompareTag("CheckPoint"))
        {
            m_CurrentCheckPoint = other.GetComponent<CheckPoint>();
        }
        else if (other.CompareTag("Item"))
        {
            Item l_Item = other.GetComponent<Item>();

            if (l_Item.CanPick())
            {
                l_Item.Pick();
            }
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if(other.CompareTag("Elevator"))
        {
            DetachFromElevator();
        }
    }
    bool CanAttachToElevator(Collider ElevatorCollider)
    {
        return Vector3.Dot(ElevatorCollider.transform.up, Vector3.up) > Mathf.Cos(m_MaxAngleToAttachToElevator * Mathf.Deg2Rad);
    }
    void AttachToElevator(Collider ElevatorCollider)
    {
        transform.SetParent(ElevatorCollider.transform.parent);
        m_ElevatorCollider = ElevatorCollider;
    }
    void DetachFromElevator()
    {
        transform.SetParent(null);
        UpdateUpElevator();
        m_ElevatorCollider = null;
    }
    void UpdateUpElevator()
    {
        Vector3 l_Direction = transform.forward;
        l_Direction.y = 0f;
        l_Direction.Normalize();
        transform.rotation = Quaternion.LookRotation(l_Direction, Vector3.up);
    }
    void UpdateElevator()
    {
        if (m_ElevatorCollider != null)
        {
            UpdateUpElevator();
        }
    }
    public void AddCoin()
    {
        m_CoinsController.AddCoins(1);
    }
    public void Hit()
    {
        m_LifeController.AddLife(-1);
    }
    public void Heal()
    {
        m_LifeController.AddLife(1);
    }

    public void ApplyKnockback(Vector3 force)
    {
        m_KnockbackVelocity = force;
    }
    public void Die()
    {
      
        GameManager.GetGameManager().RestartGame();
    }
}
