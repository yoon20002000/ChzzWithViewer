using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_RouletteSetting : MonoBehaviour
{
    [Header("UI Buttons")]
    [SerializeField]
    private Button bannedWordSettingButton;
    [SerializeField]
    private Button closeButton;

    [Header("UI Scripts")]
    [SerializeField]
    private UI_BannedWord bannedWordSetting;
    private void Awake()
    {
        bannedWordSettingButton.onClick.AddListener(OnClickedBannedWordSettingButton);
        closeButton.onClick.AddListener(OnClickedCloseButton);
    }

    private void OnClickedCloseButton()
    {
        this.gameObject.SetActive(false);
    }

    private void OnClickedBannedWordSettingButton()
    {
        bannedWordSetting.gameObject.SetActive(true);
    }
}
