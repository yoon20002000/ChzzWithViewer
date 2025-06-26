using TMPro;
using UnityEngine;

public class RoulettePiece : MonoBehaviour
{
   [SerializeField]
   private TextMeshProUGUI description;

   public void Setup(RoulettePieceData pieceData)
   {
      description.SetText(pieceData.Description);
   }
}
