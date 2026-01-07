using UnityEngine;

public class J_UISFXPlayer : MonoBehaviour
{
    public AudioSource source;
    public AudioClip clickClip;
    public AudioClip successClip;

    public void PlayClick()
    {
        if (source != null && clickClip != null)
            source.PlayOneShot(clickClip);
    }

    public void PlaySuccess()
    {
        if (source != null && successClip != null)
            source.PlayOneShot(successClip);
    }
}
