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
    public float m_CurrentTime = 0f;
    public float m_TimeToResetCam = 2f;
    float m_ResetTime = 3f;
    float m_ResetTimeCounter = 0f;

    private void Start()
    {
        m_Yaw = transform.eulerAngles.y;
    }
    private void LateUpdate()
    {
        Vector3 l_StartPosition = transform.position;

        UpdateCamera();

        if (transform.position != l_StartPosition) 
            ResetTime();

        m_CurrentTime += Time.deltaTime;
        Debug.Log(m_CurrentTime);

        CheckCameraState();
        ResetCamera();
    }
    void CheckCameraState()
    {
        if (m_CurrentTime >= m_TimeToResetCam)
        {
            m_ResetTimeCounter = 0;
        }
    }
    public void ResetTime()
    {
        m_CurrentTime = 0f; 
    }
    public void ResetCamera()
    {
        if (m_ResetTimeCounter <= m_ResetTime)
            m_Yaw = Mathf.Lerp(m_Yaw, m_Player.transform.rotation.eulerAngles.y, m_ResetTimeCounter / m_ResetTime);
    }
    private void UpdateCamera()
    {
        Vector3 l_LookAt = m_Player.m_LookAt.transform.position;
        float l_Distance = Vector3.Distance(l_LookAt, transform.position);
        float l_HorizontalAxis = Input.GetAxis("Mouse X");
        float l_VerticalAxis = Input.GetAxis("Mouse Y");
        m_Yaw += l_HorizontalAxis * m_YawSpeed * Time.deltaTime;
        m_Pitch += l_VerticalAxis * m_PitchSpeed * Time.deltaTime;
        m_Pitch = Mathf.Clamp(m_Pitch, m_MinPitch, m_MaxPitch);

        float l_PitchRadians = m_Pitch * Mathf.Deg2Rad;
        float l_YawRadians = m_Yaw * Mathf.Deg2Rad;
        Vector3 l_Direction = new Vector3(Mathf.Cos(l_PitchRadians) * Mathf.Sin(l_YawRadians),
            Mathf.Sin(l_PitchRadians), Mathf.Cos(l_PitchRadians) * Mathf.Cos(l_YawRadians));

        l_Distance = Mathf.Clamp(l_Distance, m_MinDistance, m_MaxDistance);

        Ray l_Ray = new Ray(l_LookAt, -l_Direction);
        Vector3 l_DesiredPosition = l_LookAt - l_Direction * l_Distance;

        if (Physics.Raycast(l_Ray, out RaycastHit l_RaycastHit, l_Distance, m_LayerMask.value))
        {
            l_DesiredPosition = l_RaycastHit.point + l_Direction * m_OffsetDistance;
        }

        transform.position = l_DesiredPosition;
        transform.LookAt(l_LookAt);
    }
}
