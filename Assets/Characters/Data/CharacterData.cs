using UnityEngine;

[CreateAssetMenu(fileName = "CharacterData", menuName = "Game/Character Data")]
public class CharacterData : ScriptableObject
{
    [Header("Información")]
    public int id;
    public string characterName;

    [Header("Estadísticas")]
    public float moveSpeed = 5f;
    public int maxHealth = 100;

    // [Header("Gráficos")]
    // public RuntimeAnimatorController animatorController;
    // public Sprite idleSprite;

    [Header("Visual")]
    public AnimatorOverrideController animatorOverride;
}