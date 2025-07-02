using System;
using System.Collections;
using ChzzAPI;
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
        RoulettePieceData pieceDataCopy = pieceData;
        UnityMainThreadDispatcher.Instance.Enqueue(showUIAfterSeconds(1, () =>
        {
            Setup(pieceDataCopy);
            this.gameObject.SetActive(true); 
        }));
    }
    public void Hide()
    {
        this.gameObject.SetActive(false);
    }

    private void Setup(RoulettePieceData pieceData)
    {
        ResultText.SetText(pieceData.Description);
    }

    private IEnumerator showUIAfterSeconds(float seconds,Action callback = null)
    {
        yield return Awaitable.WaitForSecondsAsync(seconds);
        
        callback?.Invoke();
    }
}
