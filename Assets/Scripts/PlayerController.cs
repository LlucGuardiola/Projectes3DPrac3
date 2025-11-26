using System.Diagnostics.CodeAnalysis;
using UnityEngine;

public class PlayerController : MonoBehaviour, IRestartElement
{
    public enum TPunchType
    {
        RIGHT_HAND = 0,
        LEFT_HAND,
        KICK
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

    [Header("Jump")]
    public float m_JumpSpeed = 12f;
    public float m_MaxAngleToKillGombaa = 50f;
    public float m_KillJumpSpeed = 4f; 

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

    [Header("Audio")]
    public AudioSource m_LeftFootStepAudioSource;
    public AudioSource m_RightFootStepAudioSource;

    int m_Life = 8;
    int m_Coins = 0;    

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
            m_VerticalSpeed = 0f;
        else if((l_CollisionFlags & CollisionFlags.CollidedAbove) != 0 && m_VerticalSpeed > 0f)
            m_VerticalSpeed = 0f;

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
        return true;
    }
    void Jump()
    {
        m_VerticalSpeed = m_JumpSpeed;
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
    }
    bool CanKillWithFeet(ControllerColliderHit hit)
    {
        float l_Dot = Vector3.Dot(hit.normal, Vector3.up);

        return m_VerticalSpeed < 0f && l_Dot > Mathf.Cos(m_MaxAngleToKillGombaa * Mathf.Deg2Rad);
    }
    public void RestartGame()
    {
        m_CharacterController.enabled = false;
        transform.position = m_StartPosition;
        transform.rotation = m_StartRotation;
        m_CharacterController.enabled = true;
    }
    public void Step (AnimationEvent _AnimationEvent)
    {
        AudioSource l_CurrentAudioSource = null;
        if (_AnimationEvent.stringParameter == "Left")
        {
            Debug.Log("Left");
        }
        else if(_AnimationEvent.stringParameter == "Right")
        {
            Debug.Log("Right");
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
        ++m_Coins;
        GameManager.GetGameManager().m_GameUI.SetCoins(m_Coins);
        GameManager.GetGameManager().m_GameUI.ShowUI();
    }
    public void Hit()
    {
        --m_Life;
        GameManager.GetGameManager().m_GameUI.SetLifeBar(m_Life/8f);
        GameManager.GetGameManager().m_GameUI.ShowUI();
    }
}
