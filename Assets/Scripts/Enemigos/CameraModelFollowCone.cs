using UnityEngine;

public class CameraModelFollowCone : MonoBehaviour
{
    public Transform visionCone;
    public Vector3 rotationOffset;

    void LateUpdate()
    {
        if (visionCone == null) return;

        Vector3 coneEuler = visionCone.rotation.eulerAngles;

        transform.rotation = Quaternion.Euler(
            transform.rotation.eulerAngles.x,
            coneEuler.y,
            transform.rotation.eulerAngles.z
        ) * Quaternion.Euler(rotationOffset);
    }
}