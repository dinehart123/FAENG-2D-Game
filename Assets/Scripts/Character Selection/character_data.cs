using UnityEngine;

[CreateAssetMenu(fileName = "NewCharacter", menuName = "FANG/Character")]
public class CharacterData : ScriptableObject
{
    [Header("Character Info")]
    public string characterName;

    [Header("Character Select")]
    public Sprite characterIcon;

    [Header("Gameplay")]
    public GameObject fighterPrefab;

    [Header("Character Select Animation")]
    public GameObject selectionPreviewPrefab;
    public string previewIdleStateName = "MonkeyIdle";
}
