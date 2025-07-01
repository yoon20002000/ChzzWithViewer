using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

public class UI_BannedWord : MonoBehaviour
{
   [SerializeField]
   private Button bg;
   [SerializeField]
   private Button closeButton;
   [SerializeField]
   private Button addBannedWordButton;

   [SerializeField]
   private TMP_InputField bannedWord;
   [SerializeField]
   private Transform content;
   
   [SerializeField]
   private Transform bannedWordRow;
   
   private ObjectPool<UI_BannedWordRow> rowPool;
   private readonly List<UI_BannedWordRow> activeRows = new();
   
   private void Awake()
   {
      bg.onClick.AddListener(OnClickedClose);
      closeButton.onClick.AddListener(OnClickedClose);
      addBannedWordButton.onClick.AddListener(OnClickedAddBannedWord);
      rowPool = new ObjectPool<UI_BannedWordRow>(
         createFunc: () =>
         {
            var go = Instantiate(bannedWordRow, content);
            return go.GetComponent<UI_BannedWordRow>();
         },
         actionOnGet: row => row.gameObject.SetActive(true),
         actionOnRelease: row => row.gameObject.SetActive(false),
         actionOnDestroy: row => Destroy(row.gameObject),
         collectionCheck: false,
         defaultCapacity: 10
      );
      
      updateScrollView(GameSettingManager.GetBannedWorlds());
   }
   
   private void updateScrollView(HashSet<string> getBannedWorlds)
   {
      foreach (var row in activeRows)
      {
         rowPool.Release(row);
      }
      activeRows.Clear();

      // 새 Row 할당
      foreach (var piece in getBannedWorlds)
      {
         var row = rowPool.Get();
         row.Setup(piece, RemoveBannedWord);
         activeRows.Add(row);
      }
   }

   private void OnClickedClose()
   {
      this.gameObject.SetActive(false);
   }

   private void RemoveBannedWord(string word)
   {
      GameSettingManager.RemoveBannedWord(word);
      updateScrollView(GameSettingManager.GetBannedWorlds());
   }
   
   private void OnClickedAddBannedWord()
   {
      GameSettingManager.AddBannedWord(bannedWord.text);
      updateScrollView(GameSettingManager.GetBannedWorlds());
   }
}
