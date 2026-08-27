using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDatabase", menuName = "Game/Character Database")]
public class CharacterDatabase : ScriptableObject
{
    [SerializeField]
    private List<CharacterData> characters = new();

    public IReadOnlyList<CharacterData> Characters => characters;

    public CharacterData GetCharacter(int id)
    {
        foreach (CharacterData character in characters)
        {
            if (character.id == id)
                return character;
        }

        Debug.LogError($"No existe ningún personaje con ID {id}");

        return null;
    }
}