using UnityEngine.InputSystem;

public static class PlayerSelectionData
{
    // Holds character and controller choices while changing scenes.
    public static CharacterData Player1Character;
    public static CharacterData Player2Character;

    public static FighterController.ControlMode Player1ControlMode;
    public static FighterController.ControlMode Player2ControlMode;

    public static Gamepad Player1Gamepad;
    public static Gamepad Player2Gamepad;

    public static void ResetSelections()
    {
        // Clear selections before starting a new character-select session.
        Player1Character = null;
        Player2Character = null;

        Player1Gamepad = null;
        Player2Gamepad = null;
    }
}
