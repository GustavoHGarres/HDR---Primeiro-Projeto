using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class CanvasBindMainCamera : MonoBehaviour
{
    void OnEnable()
    {
        var canvas = GetComponent<Canvas>();
        if (canvas.renderMode == RenderMode.ScreenSpaceCamera || canvas.renderMode == RenderMode.WorldSpace)
        {
            if (!canvas.worldCamera) canvas.worldCamera = Camera.main;
            if (!canvas.worldCamera)
            {
                // tenta de novo no próximo frame (cena aditiva pode demorar 1 frame)
                StartCoroutine(BindNextFrame(canvas));
            }
        }
    }

    System.Collections.IEnumerator BindNextFrame(Canvas canvas)
    {
        yield return null;
        if (!canvas.worldCamera) canvas.worldCamera = Camera.main;
    }
}
