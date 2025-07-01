using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_BannedWord : MonoBehaviour
{
   [SerializeField]
   private Button bg;
   [SerializeField]
   private Button closeButton;

   private void Awake()
   {
      bg.onClick.AddListener(OnClickedClose);
      closeButton.onClick.AddListener(OnClickedClose);
   }

   private void OnClickedClose()
   {
      this.gameObject.SetActive(false);
   }
}
