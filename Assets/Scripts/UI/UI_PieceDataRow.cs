using TMPro;
using UnityEngine;

public class UI_PieceDataRow : MonoBehaviour
{
    
    [SerializeField]
    private TextMeshProUGUI descriptionText;
    [SerializeField]
    private TextMeshProUGUI chanceText;

    public void Setup(string description, int chance)
    {
        if (descriptionText != null)
        {
            descriptionText.text = description;
        }
        if (chanceText != null)
        {
            chanceText.text = chance.ToString();
        }
    }
}
