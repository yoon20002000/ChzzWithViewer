using System;
using UnityEngine;
using UnityEngine.Serialization;

[Serializable]
public class RoulettePieceData
{
    public RoulettePieceData(string description, int chance)
    {
        Description = description;
        Chance = chance;
    }
    public string Description;

    public int Chance;
    
    [HideInInspector]
    public int Index;
    [HideInInspector] 
    public int Weight;
}
