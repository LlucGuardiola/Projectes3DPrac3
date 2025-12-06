using UnityEngine;

public class CameraController : MonoBehaviour
{
    public PlayerController m_Player;

    float m_Yaw = 0f;
    float m_Pitch = 0f;

    public float m_YawSpeed = 360f;
    public float m_PitchSpeed = 180f;
    public float m_MinPitch = 60f;
    public float m_MaxPitch = 80f;

    public float m_MinDistance = 3f;
    public float m_MaxDistance = 12f;

    public LayerMask m_LayerMask;
    public float m_OffsetDistance = 0.1f;

    public float m_TimeToResetCam = 5f;     
    public float m_ResetTime = 4f;          

    float idleTimer = 0f;
    bool resetting = false;
    float resetT = 0f;

    float targetYaw;
    float targetPitch;

    private void Start()
    {
        m_Yaw = transform.eulerAngles.y;
        m_Pitch = transform.eulerAngles.x;
    }

    private void LateUpdate()
    {
        HandleInput();
        HandleAutoReset();
        UpdateCamera(); 
    }

    void HandleInput()
    {
        float h = Input.GetAxis("Mouse X");
        float v = Input.GetAxis("Mouse Y");

        bool hasInput = Mathf.Abs(h) > 0.01f || Mathf.Abs(v) > 0.01f;

        if (hasInput)
        {
            idleTimer = 0f;
            resetting = false;

            m_Yaw += h * m_YawSpeed * Time.deltaTime;
            m_Pitch += v * m_PitchSpeed * Time.deltaTime;
            m_Pitch = Mathf.Clamp(m_Pitch, m_MinPitch, m_MaxPitch);

            return;
        }

        idleTimer += Time.deltaTime;
    }

    void HandleAutoReset()
    {
        if (!resetting && idleTimer >= m_TimeToResetCam)
        {
            resetting = true;
            resetT = 0f;

            targetYaw = m_Player.transform.eulerAngles.y;

            targetPitch = -40f;
        }

        if (resetting)
        {
            resetT += Time.deltaTime / m_ResetTime;

            m_Yaw = Mathf.LerpAngle(m_Yaw, targetYaw, resetT);
            m_Pitch = Mathf.Lerp(m_Pitch, targetPitch, resetT);

            if (resetT >= 1f)
            {
                resetting = false;
                idleTimer = 0f;
            }
        }
    }

    private void UpdateCamera()
    {
        Vector3 lookAt = m_Player.m_LookAt.transform.position;

        float pitchRad = m_Pitch * Mathf.Deg2Rad;
        float yawRad = m_Yaw * Mathf.Deg2Rad;

        Vector3 dir = new Vector3(
            Mathf.Cos(pitchRad) * Mathf.Sin(yawRad),
            Mathf.Sin(pitchRad),
            Mathf.Cos(pitchRad) * Mathf.Cos(yawRad)
        );

        float dist = 6f;

        Ray ray = new Ray(lookAt, -dir);
        Vector3 desiredPos = lookAt - dir * dist;

        if (Physics.Raycast(ray, out RaycastHit hit, dist, m_LayerMask.value))
        {
            desiredPos = hit.point + dir * m_OffsetDistance;
        }

        transform.position = desiredPos;
        transform.LookAt(lookAt);
    }
}
