using UnityEngine;

public class CameraIdleRotation : MonoBehaviour
{
    public Transform visionCone;
    public DetectorCamara detector;

    [Header("Idle")]
    public float idleSpeed = 0.4f;
    public float maxAngle = 20f;
    public float returnSpeed = 2f;

    [Header("Offset")]
    public Vector3 rotationOffset;

    private Quaternion startRotation;
    private bool wasDetected = false;
    private bool returningCenter = false;
    private float idleTimer = 0f;

    void Start()
    {
        startRotation = transform.rotation;
    }

    void LateUpdate()
    {
        Quaternion centerRotation = startRotation * Quaternion.Euler(rotationOffset);

        if (detector != null && detector.PlayerDetected && visionCone != null)
        {
            wasDetected = true;
            returningCenter = false;

            Vector3 coneEuler = visionCone.rotation.eulerAngles;

            Quaternion targetRotation = Quaternion.Euler(
                startRotation.eulerAngles.x,
                coneEuler.y,
                startRotation.eulerAngles.z
            ) * Quaternion.Euler(rotationOffset);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                targetRotation,
                returnSpeed * Time.deltaTime
            );
        }
        else
        {
            if (wasDetected)
            {
                returningCenter = true;
                wasDetected = false;
                idleTimer = 0f;
            }

            if (returningCenter)
            {
                transform.rotation = Quaternion.Slerp(
                    transform.rotation,
                    centerRotation,
                    returnSpeed * Time.deltaTime
                );

                if (Quaternion.Angle(transform.rotation, centerRotation) < 1f)
                {
                    returningCenter = false;
                    idleTimer = 0f;
                }

                return;
            }

            idleTimer += Time.deltaTime;

            float angle = Mathf.Sin(idleTimer * idleSpeed) * maxAngle;

            Quaternion idleRotation =
                startRotation *
                Quaternion.Euler(0f, angle, 0f) *
                Quaternion.Euler(rotationOffset);

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                idleRotation,
                returnSpeed * Time.deltaTime
            );
        }
    }
}