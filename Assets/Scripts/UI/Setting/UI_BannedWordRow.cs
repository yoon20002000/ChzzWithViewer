using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI_BannedWordRow : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI bannedWordText;
    [SerializeField]
    private Button removeButton;
    
    private Action<string> onClickedRemoveButton;

    private void Awake()
    {
        removeButton.onClick.AddListener(OnClickedRemoveButton);
    }

    private void OnClickedRemoveButton()
    {
        onClickedRemoveButton?.Invoke(bannedWordText.text);
    }

    public void Setup(string bannedText, Action<string> onRemove = null)
    {
        bannedWordText.SetText(bannedText);
        onClickedRemoveButton = onRemove;
    }
}
