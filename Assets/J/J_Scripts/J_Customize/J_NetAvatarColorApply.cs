using Photon.Pun;
using UnityEngine;

public class J_NetAvatarColorApply : MonoBehaviourPun, IPunInstantiateMagicCallback
{
    public Renderer bodyRenderer;
    public Renderer hatRenderer;

    // URP Lit: _BaseColor, Standard: _Color
    public string[] colorProps = { "_BaseColor", "_Color" };
    private MaterialPropertyBlock mpb;

    void Awake()
    {
        mpb = new MaterialPropertyBlock();
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = photonView.InstantiationData;
        if (data == null || data.Length < 2) return;

        if (data[0] is int bodyPacked)
            Apply(bodyRenderer, J_ColorPack.Unpack(bodyPacked));

        if (data[1] is int hatPacked)
            Apply(hatRenderer, J_ColorPack.Unpack(hatPacked));
    }

    void Apply(Renderer r, Color c)
    {
        if (!r) return;

        string prop = "_BaseColor";
        foreach (var p in colorProps)
        {
            if (r.sharedMaterial != null && r.sharedMaterial.HasProperty(p))
            {
                prop = p;
                break;
            }
        }

        mpb.Clear();
        c.a = 1f;
        mpb.SetColor(prop, c);
        r.SetPropertyBlock(mpb);
    }
}
