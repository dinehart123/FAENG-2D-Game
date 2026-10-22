using UnityEngine;

[CreateAssetMenu(fileName = "CharacterRoster", menuName = "FANG/Character Roster")]
public class CharacterRoster : ScriptableObject
{
    public CharacterData[] characters;
}