using System;
using System.Text;
using ChzzAPI;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using WebSocketSharp;

public class ChzzkWithViewer : MonoBehaviour
{
    [SerializeField]
    private ChzzkUnity chzzk;
    private string channelIdFilePath = 
    #if UNITY_EDITOR
    "Scripts/ChzzAPI/ChannelId.txt"; // 불러올 txt 파일 경로
    #elif UNITY_STANDALONE
    "ChannelId.txt"; // 불러올 txt 파일 경로
    #endif
    
    [SerializeField] 
    private TextMeshProUGUI temp;
    private async void Start()
    {
        Assert.IsNotNull(chzzk, "chzzk is null");
        string channelId = LoadChannelIdFromFile();
        Assert.IsTrue(!channelId.IsNullOrEmpty(), "Channel id load failed");
        
        // Connect 호출 (내부적으로 GetLiveStatus 호출)
        await chzzk.Connect(channelId);

        // 현재 LiveStatus 정보 출력
        var liveStatus = await chzzk.GetLiveStatus(channelId);
        if (liveStatus != null && liveStatus.content != null)
        {
            StringBuilder sb = new();
            sb.Append($"방송 제목: {liveStatus.content.liveTitle}");
            sb.Append($"상태: {liveStatus.content.status}");
            sb.Append($"동시 시청자: {liveStatus.content.concurrentUserCount}");
            sb.Append($"누적 시청자: {liveStatus.content.accumulateCount}");
            temp.SetText(sb.ToString());
        }
        else
        {
            Debug.Log("LiveStatus 정보를 가져오지 못했습니다.");
        }
    }
    private string LoadChannelIdFromFile()
    {
        temp.SetText(channelIdFilePath);
        
        if (string.IsNullOrEmpty(channelIdFilePath))
        {
            Debug.LogError("channelIdFilePath가 지정되지 않았습니다.");
            return string.Empty;
        }
        try
        {
            string fullPath = System.IO.Path.Combine(Application.dataPath, channelIdFilePath);
            temp.SetText(fullPath);
            if (!System.IO.File.Exists(fullPath))
            {
                Debug.LogError($"ChannelID 파일이 존재하지 않습니다: {fullPath}");
                return string.Empty;
            }
            string[] lines = System.IO.File.ReadAllLines(fullPath);
            if (lines.Length > 0)
            {
                Debug.Log($"ChannelID를 파일에서 불러왔습니다: {lines[0].Trim()}");
                return lines[0].Trim();
            }
            else
            {
                Debug.LogError("ChannelID 파일이 비어 있습니다.");
                return string.Empty;
            }
        }
        catch (Exception ex)
        {
            Debug.LogError($"ChannelID 파일 읽기 오류: {ex.Message}");
            return string.Empty;
        }
    }
} 