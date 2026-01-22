using UnityEngine;

public class J_PreviewAvatar : MonoBehaviour
{
    public Renderer bodyRenderer;
    public Renderer hatRenderer;

    public string[] colorProps = { "_BaseColor", "_Color" };
    MaterialPropertyBlock mpb;

    void Awake() => mpb = new MaterialPropertyBlock();

    public void SetBodyColor(Color c) => Apply(bodyRenderer, c);
    public void SetHatColor(Color c) => Apply(hatRenderer, c);

    void Apply(Renderer r, Color c)
    {
        if (r == null) return;

        string prop = "_BaseColor";
        foreach (var p in colorProps)
        {
            if (r.sharedMaterial != null && r.sharedMaterial.HasProperty(p))
            {
                prop = p; break;
            }
        }

        mpb.Clear();
        mpb.SetColor(prop, c);
        r.SetPropertyBlock(mpb);
    }
}
