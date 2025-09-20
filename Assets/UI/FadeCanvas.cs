using UnityEngine;
using UnityEngine.UI;
using System;
public class FadeCanvas : MonoBehaviour
{
    public Image blocker; // Image preta inteira
    public void FadeIn (float t, Action onEnd=null)  => StartCoroutine(Fade(1,0,t,onEnd));
    public void FadeOut(float t, Action onEnd=null)  => StartCoroutine(Fade(0,1,t,onEnd));

    System.Collections.IEnumerator Fade(float from, float to, float t, Action onEnd)
    {
        var c = blocker.color;
        float e=0;
        while (e < t){ c.a = Mathf.Lerp(from,to,e/t); blocker.color = c; e += Time.deltaTime; yield return null; }
        c.a = to; blocker.color = c;
        onEnd?.Invoke();
    }
}
