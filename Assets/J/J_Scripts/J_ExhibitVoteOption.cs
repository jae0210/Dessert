using UnityEngine;

public class J_ExhibitVoteOption : MonoBehaviour
{
    [Header("Unique ID (must be unique in scene)")]
    public string optionId;      // 예: yakgwa, ssanghwacha

    [Header("UI Display Name")]
    public string displayName;   // 예: 약과, 쌍화차

    [Header("UI Icon")]
    public Sprite iconSprite;    // 이미지
}