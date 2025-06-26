using System;
using UnityEngine;
using UnityEngine.UI;

public class RouletteDemo : MonoBehaviour
{
    [SerializeField]
    private Roulette roulette;
    [SerializeField]
    private Button spinButton;

    private void Awake()
    {
        spinButton.onClick.AddListener(() =>
        {
            spinButton.interactable = false;
            roulette.Spin(EndOfSpin);
        });
    }

    private void EndOfSpin(RoulettePieceData selectedPiece)
    {
        spinButton.interactable = true;
        Debug.Log($"Selected index : {selectedPiece.index} : {selectedPiece.description}");
    }
}
