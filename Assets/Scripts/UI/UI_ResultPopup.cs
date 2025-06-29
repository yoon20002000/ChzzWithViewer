using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_ResultPopup : MonoBehaviour
{
    [SerializeField]
    private Button CloseButton;
    [SerializeField]
    private TextMeshProUGUI ResultText;

    private void Awake()
    {
        CloseButton.onClick.AddListener(Hide);
    }

    public void Show(RoulettePieceData pieceData)
    {
        Setup(pieceData);
        this.gameObject.SetActive(true);
    }
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    private void Setup(RoulettePieceData pieceData)
    {
        ResultText.SetText(pieceData.Description);
    }
}
