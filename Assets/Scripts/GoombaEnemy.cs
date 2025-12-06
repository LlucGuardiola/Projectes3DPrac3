using UnityEngine;
using UnityEngine.AI;
using System.Collections.Generic;

public class GoombaEnemy : MonoBehaviour, IRestartElement
{
    public enum TState
    {
        PATROL,
        CHASE
    }

    private TState m_State = TState.PATROL;

    [Header("NavMesh")]
    private NavMeshAgent m_NavMeshAgent;
    public float m_PatrolSpeed = 1.5f;
    public float m_ChaseSpeed = 3f;

    [Header("Patrol Points")]
    public List<Transform> m_PatrolPositions = new List<Transform>();
    private int m_CurrentPatrolPositionId = 0;

    [Header("Sight")]
    public float m_ProximityDetection = 4f;
    public float m_SightAngle = 90f;
    public float m_EyesHeight = 0.5f;
    public LayerMask m_SightLayerMask;

    [Header("Chase")]
    public float m_MinDistanceToAttack = 1.5f;
    public float m_MaxChaseDistance = 10f;

    [Header("Damage")]
    public float m_DamagePerSecond = 1f;
    private bool m_TouchingPlayer = false;
    private float m_DamageCounter = 0f;
    public float m_KnockbackForce = 5f;

    Transform m_Player;

    Vector3 m_StartPos;
    Quaternion m_StartRot;
 


    private void Awake()
    {
        m_NavMeshAgent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        GameManager.GetGameManager().AddRestartGameElements(this);

        m_StartPos = transform.position;
        m_StartRot = transform.rotation;

        m_Player = GameManager.GetGameManager().m_Player.transform;

        SetPatrolState();
    }
  

    private void Update()
    {
        if (SeesPlayer())
            SetChaseState();

        switch (m_State)
        {
            case TState.PATROL:
                UpdatePatrol();
                break;

            case TState.CHASE:
                UpdateChase();
                break;
        }


        ApplyDamageIfTouching();
    }


    void SetPatrolState()
    {
        m_State = TState.PATROL;
        m_NavMeshAgent.speed = m_PatrolSpeed;

        m_NavMeshAgent.autoBraking = false;

        MoveToNextPatrolPosition();
    }

    void UpdatePatrol()
    {
        if (m_PatrolPositions.Count == 0) return;

        if (!m_NavMeshAgent.pathPending &&
            m_NavMeshAgent.remainingDistance <= m_NavMeshAgent.stoppingDistance + 0.2f)
        {
            MoveToNextPatrolPosition();
        }
    }

    void MoveToNextPatrolPosition()
    {
        if (m_PatrolPositions.Count == 0) return;

        Transform target = m_PatrolPositions[m_CurrentPatrolPositionId];

        m_NavMeshAgent.SetDestination(target.position);

        m_CurrentPatrolPositionId = (m_CurrentPatrolPositionId + 1) % m_PatrolPositions.Count;
    }


    void SetChaseState()
    {
        m_State = TState.CHASE;
        m_NavMeshAgent.speed = m_ChaseSpeed;
    }

    void UpdateChase()
    {
        SetNextChasePosition();
    }

    void SetNextChasePosition()
    {
        Vector3 l_PlayerPosition = m_Player.position;

        float l_Distance = Vector3.Distance(transform.position, l_PlayerPosition);

        if (l_Distance > m_MaxChaseDistance)
        {
            SetPatrolState();
            return;
        }

        Vector3 l_Direction = l_PlayerPosition - transform.position;
        l_Direction.Normalize();

        Vector3 l_Position =
            l_PlayerPosition - l_Direction * m_MinDistanceToAttack;

        m_NavMeshAgent.destination = l_Position;
    }

   
    bool SeesPlayer()
    {
        Vector3 l_PlayerPosition = m_Player.position;
        Vector3 l_Direction = l_PlayerPosition - transform.position;
        float l_Distance = l_Direction.magnitude;

        l_Direction /= l_Distance;

        if (l_Distance < m_ProximityDetection)
            return true;

        float l_DotValue = Vector3.Dot(l_Direction, transform.forward);
        if (l_DotValue >= Mathf.Cos(m_SightAngle * 0.5f * Mathf.Deg2Rad))
        {
            Ray l_Ray = new Ray(transform.position + Vector3.up * m_EyesHeight, l_Direction);

            if (!Physics.Raycast(l_Ray, l_Distance, m_SightLayerMask.value))
                return true;
        }

        return false;
    }

 

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            m_TouchingPlayer = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            m_TouchingPlayer = false;
    }

    void ApplyDamageIfTouching()
    {
        if (!m_TouchingPlayer) return;

        m_DamageCounter += Time.deltaTime;

        if (m_DamageCounter >= 1f / m_DamagePerSecond)
        {
            GameManager.GetGameManager().m_Player.Hit();

            Vector3 knockbackDir = (GameManager.GetGameManager().m_Player.transform.position - transform.position).normalized;
            //GameManager.GetGameManager().m_Player.ApplyKnockback(-knockbackDir, 5f); // fuerza ajustable

            m_DamageCounter = 0f;
        }
    }

    public void RestartGame()
    {
        m_NavMeshAgent.enabled = false;
        transform.position = m_StartPos;
        transform.rotation = m_StartRot;
        m_NavMeshAgent.enabled = true;

        m_TouchingPlayer = false;
        m_DamageCounter = 0f;

        SetPatrolState();
    }

    public void Kill()
    {
        gameObject.SetActive(false);
    }
}
