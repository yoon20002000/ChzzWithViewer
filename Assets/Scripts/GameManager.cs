using UnityEngine;

public class GameManager : MonoBehaviour
{
    private void Awake()
    {
        GameSettingManager.Load();
    }
}
