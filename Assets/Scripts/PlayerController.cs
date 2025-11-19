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

    [Header("Punch")]
    public float m_MaxTimeToComboPunch = 0.8f;
    int m_CurrentPunchId;
    float m_LastPunchTime;
    public GameObject m_RightHandPunchCollider;
    public GameObject m_LeftHandPunchCollider;
    public GameObject m_KickPunchCollider;

    [Header("Input")]
    public int m_PunchMouseButton = 0;


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

        l_Movement *= l_Speed*Time.deltaTime;
        m_VerticalSpeed += Physics.gravity.y * Time.deltaTime;
        l_Movement.y = m_VerticalSpeed * Time.deltaTime;
        CollisionFlags l_CollisionFlags = m_CharacterController.Move(l_Movement);   
        if((l_CollisionFlags & CollisionFlags.CollidedBelow) != 0)
            m_VerticalSpeed = 0f;
        else if((l_CollisionFlags & CollisionFlags.CollidedAbove) != 0 && m_VerticalSpeed > 0f)
            m_VerticalSpeed = 0f;

        UpdatePunch();
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
    public void RestartGame()
    {
        m_CharacterController.enabled = false;
        transform.position = m_StartPosition;
        transform.rotation = m_StartRotation;
        m_CharacterController.enabled = true;
    }
}
