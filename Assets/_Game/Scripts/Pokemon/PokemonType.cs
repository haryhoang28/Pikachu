using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "NewPokemonType", menuName = "Match2/PokemonType")]
public class PokemonType : ScriptableObject
{
    public string typeId;
    public string typeName;
    public Sprite sprite;
}
