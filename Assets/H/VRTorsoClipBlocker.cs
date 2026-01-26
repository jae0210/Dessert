using System;
using UnityEngine;

[DisallowMultipleComponent]
public class VRTorsoClipBlocker : MonoBehaviour
{
    [Header("필수")]
    [Tooltip("HMD 카메라(OVRCameraRig의 CenterEyeAnchor Camera 등)")]
    public Camera xrCamera;

    [Tooltip("몸통(또는 아바타 전체 메쉬)을 움직일 루트 Transform (예: bobusang_b 또는 mixamorig:Hips 상위)")]
    public Transform bodyRoot;

    [Header("1) Near Clip 보정(권장)")]
    public bool forceNearClip = true;

    [Range(0.001f, 0.1f)]
    public float nearClip = 0.01f;

    [Header("2) 몸통을 HMD 기준으로 따라오게(권장)")]
    public bool followHmdYaw = true;

    [Tooltip("HMD 기준 몸통 위치 오프셋(대략 y는 -1.0~-1.3 정도)")]
    public Vector3 bodyOffsetFromHmd = new Vector3(0f, -1.15f, 0.05f);

    [Tooltip("따라오는 부드러움(클수록 즉각 반응)")]
    public float followSmooth = 20f;

    [Header("3) 겹치면 몸통 페이드(선택)")]
    public bool fadeWhenTooClose = false;

    [Tooltip("페이드 대상으로 삼을 Renderer들(비우면 bodyRoot 아래 Renderer 자동 수집)")]
    public Renderer[] torsoRenderers;

    [Tooltip("HMD와 몸통 중심 거리(이 이하로 가까우면 페이드 시작)")]
    public float fadeStartDistance = 0.20f;

    [Tooltip("이 거리 이하로 오면 거의 완전 투명(=안 보이게)")]
    public float fadeEndDistance = 0.10f;

    [Tooltip("몸통 중심 기준점(비우면 bodyRoot 사용)")]
    public Transform torsoCenter;

    public float fadeSpeed = 15f;

    static readonly int ColorId = Shader.PropertyToID("_Color");
    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");

    MaterialPropertyBlock _mpb;
    float _currentAlpha = 1f;

    void Reset()
    {
        if (!xrCamera) xrCamera = Camera.main;
        if (!torsoCenter) torsoCenter = bodyRoot;
    }

    void Awake()
    {
        if (!torsoCenter) torsoCenter = bodyRoot;

        if (fadeWhenTooClose)
        {
            if (torsoRenderers == null || torsoRenderers.Length == 0)
                torsoRenderers = bodyRoot ? bodyRoot.GetComponentsInChildren<Renderer>(true) : Array.Empty<Renderer>();

            _mpb = new MaterialPropertyBlock();
        }
    }

    void LateUpdate()
    {
        if (!xrCamera) return;

        // 1) Near clip 강제
        if (forceNearClip)
        {
            if (Mathf.Abs(xrCamera.nearClipPlane - nearClip) > 0.0001f)
                xrCamera.nearClipPlane = nearClip;
        }

        // 2) 몸통을 HMD 기준으로 배치(겹침 최소화)
        if (bodyRoot)
        {
            var camT = xrCamera.transform;

            Quaternion yawRot = followHmdYaw
                ? Quaternion.Euler(0f, camT.eulerAngles.y, 0f)
                : camT.rotation;

            Vector3 targetPos = camT.position + yawRot * bodyOffsetFromHmd;
            Quaternion targetRot = yawRot;

            float t = 1f - Mathf.Exp(-followSmooth * Time.deltaTime);
            bodyRoot.position = Vector3.Lerp(bodyRoot.position, targetPos, t);
            bodyRoot.rotation = Quaternion.Slerp(bodyRoot.rotation, targetRot, t);
        }

        // 3) 그래도 가까워지면 페이드(선택)
        if (fadeWhenTooClose && torsoRenderers != null && torsoRenderers.Length > 0)
        {
            Transform centerT = torsoCenter ? torsoCenter : bodyRoot;
            if (!centerT) return;

            float d = Vector3.Distance(xrCamera.transform.position, centerT.position);

            float targetAlpha;
            if (d >= fadeStartDistance) targetAlpha = 1f;
            else if (d <= fadeEndDistance) targetAlpha = 0f;
            else
            {
                // start~end 사이를 1->0으로 보간
                targetAlpha = Mathf.InverseLerp(fadeEndDistance, fadeStartDistance, d);
            }

            float ft = 1f - Mathf.Exp(-fadeSpeed * Time.deltaTime);
            _currentAlpha = Mathf.Lerp(_currentAlpha, targetAlpha, ft);

            ApplyAlphaToRenderers(_currentAlpha);
        }
    }

    void ApplyAlphaToRenderers(float a)
    {
        for (int i = 0; i < torsoRenderers.Length; i++)
        {
            var r = torsoRenderers[i];
            if (!r) continue;

            r.GetPropertyBlock(_mpb);

            // URP/HDRP: _BaseColor, Built-in Standard: _Color 둘 다 시도
            if (r.sharedMaterial)
            {
                if (r.sharedMaterial.HasProperty(BaseColorId))
                {
                    Color c = _mpb.GetColor(BaseColorId);
                    if (c == default) c = r.sharedMaterial.GetColor(BaseColorId);
                    c.a = a;
                    _mpb.SetColor(BaseColorId, c);
                }

                if (r.sharedMaterial.HasProperty(ColorId))
                {
                    Color c = _mpb.GetColor(ColorId);
                    if (c == default) c = r.sharedMaterial.GetColor(ColorId);
                    c.a = a;
                    _mpb.SetColor(ColorId, c);
                }
            }

            r.SetPropertyBlock(_mpb);
        }
    }
}
