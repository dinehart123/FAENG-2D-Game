using UnityEngine;

[CreateAssetMenu(fileName = "CharacterRoster", menuName = "FANG/Character Roster")]
public class CharacterRoster : ScriptableObject
{
    // Contains every character available in character select.
    public CharacterData[] characters;
}
