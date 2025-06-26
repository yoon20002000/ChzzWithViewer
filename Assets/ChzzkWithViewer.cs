using ChzzAPI;
using UnityEngine;
using UnityEngine.Assertions;

public class ChzzkWithViewer : MonoBehaviour
{
    [SerializeField]
    private string channelId; 

    [SerializeField]
    private ChzzkUnity chzzk;
    private async void Start()
    {
        Assert.IsNotNull(chzzk);
        chzzk.channel = channelId;

        // Connect 호출 (내부적으로 GetLiveStatus 호출)
        await chzzk.Connect();

        // 현재 LiveStatus 정보 출력
        var liveStatus = await chzzk.GetLiveStatus(channelId);
        if (liveStatus != null && liveStatus.content != null)
        {
            Debug.Log($"방송 제목: {liveStatus.content.liveTitle}");
            Debug.Log($"상태: {liveStatus.content.status}");
            Debug.Log($"동시 시청자: {liveStatus.content.concurrentUserCount}");
            Debug.Log($"누적 시청자: {liveStatus.content.accumulateCount}");
        }
        else
        {
            Debug.Log("LiveStatus 정보를 가져오지 못했습니다.");
        }
    }
} 