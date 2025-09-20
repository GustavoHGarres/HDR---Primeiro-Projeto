using UnityEngine;

public class EntryPoint : MonoBehaviour
{
    [SerializeField] private string entryId; // ex: "fromFase1", "fromPrologo"
    public string EntryId => entryId;

#if UNITY_EDITOR
private void OnDrawGizmos()
{
    Gizmos.color = Color.green;
    Gizmos.DrawWireSphere(transform.position, 0.5f);
    UnityEditor.Handles.Label(transform.position + Vector3.up * 0.6f, $"Entry: {entryId}\n{transform.position}");
}
#endif

}
