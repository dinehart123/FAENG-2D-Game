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
}