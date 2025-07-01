using System.Collections.Generic;

public class GameSettingData
{
    public HashSet<string> bannedWords = new HashSet<string>();
    public HashSet<string> defaultBannedWords = new HashSet<string>();
    public bool enableDevChat = false;
}
