using UnityEngine;
public class ArrowSpawnGizmo : MonoBehaviour
{
    public float dirLen = 0.4f;
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawSphere(transform.position, 0.02f);
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * dirLen);
    }
}
