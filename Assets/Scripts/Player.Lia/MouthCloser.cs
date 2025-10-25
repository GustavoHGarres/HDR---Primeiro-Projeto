using UnityEngine;

public class MouthCloser : MonoBehaviour
{
    public SkinnedMeshRenderer faceRenderer;
    [Range(0,100)] public float mouthCloseValue = 60f;

    void Start()
    {
        if (!faceRenderer) return;
        // procura blendshape chamado "Mouth Close" ou parecido
        for (int i = 0; i < faceRenderer.sharedMesh.blendShapeCount; i++)
        {
            string shapeName = faceRenderer.sharedMesh.GetBlendShapeName(i);
            if (shapeName.ToLower().Contains("mouthclose") || shapeName.ToLower().Contains("jawclose"))
            {
                faceRenderer.SetBlendShapeWeight(i, mouthCloseValue);
            }
        }
    }
}
