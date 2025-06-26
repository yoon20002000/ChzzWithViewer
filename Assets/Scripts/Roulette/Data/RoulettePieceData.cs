using System;
using UnityEngine;

[Serializable]
public class RoulettePieceData
{
    public string description;
    
    [Range(1,100)]
    public int chance = 100;
    
    [HideInInspector]
    public int index;
    [HideInInspector] 
    public int weight;
}
