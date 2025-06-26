using System;
using System.Collections.Generic;
using ChzzAPI;
using UnityEngine;

public class RouletteGame : MonoBehaviour, IChzzAPIEvents
{
    private const string ROULETTE_COMMAND = "[룰렛]";
    private Dictionary<string, int> rouletteData = new Dictionary<string, int>();

    public void addRouletteDataOnce(string key)
    {
        addRouletteData(key, 1);
    }
    public void removeRouletteDataOnce(string key)
    {
        removeRouletteData(key, 1);
    }

    private void initializeGame()
    {
        rouletteData.Clear();
        // chzz api 초기화
        // bind 추가
    }

    private void  deinitializeGame()
    {
        rouletteData.Clear();
        // chzz api 종료
        // bind 제거
    }

    private void addRouletteData(string key, int count)
    {
        if(rouletteData.ContainsKey(key))
        {
            rouletteData[key] += count;
        }
        else
        {
            rouletteData[key] = count;
        }
    }

    private void removeRouletteData(string key, int count)
    {
        if(rouletteData.ContainsKey(key))
        {
            rouletteData[key] -= count;
        }
    }

    #region IChzzAPIEvents

    public void OnMessage(Profile profile, string message)
    {
        if(string.IsNullOrEmpty(message))
        {
            return;
        }
            
        string[] words = message.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        
        if(words.Length < 2)
        {
            return;
        }
        
        if(words[0] != ROULETTE_COMMAND)
        {
            return;
        }
        
        string key = words[1];

        if(string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        addRouletteData(key, 1);
        Debug.Log($"룰렛 키 추가 : {key} : {rouletteData[key]}");
    }

    public void OnDonation(Profile profile, string message, DonationExtras donation)
    {
        
    }

    public void OnSubscription(Profile profile, SubscriptionExtras subscription)
    {
        
    }

    public void OnClose()
    {
        deinitializeGame();
    }

    public void OnOpen()
    {
        initializeGame();
    }

    #endregion
}
