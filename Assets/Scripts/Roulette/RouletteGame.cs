using System;
using System.Collections;
using System.Collections.Generic;
using ChzzAPI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class RouletteGame : MonoBehaviour, IChzzAPIEvents
{
    [SerializeField] 
    private ChzzkUnity chzzkUnity;
    private const string ROULETTE_COMMAND = "[룰렛]";
    private List<RoulettePieceData> rouletteData = new (100);

    [SerializeField] 
    private Button startCounting;
    [SerializeField] 
    private Button stopCounting;
    [SerializeField]
    private Button startSpin;
    
    private int countPerAmount = 1000;
    private int totalWeight = 0;
    
    
    private void Awake()
    {
        startCounting.onClick.AddListener(OnClick_StartCounting);
        stopCounting.onClick.AddListener(OnClick_StopCounting);
    }

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
        uiActiveSetting(false);
    }

    private void deinitializeGame()
    {
        rouletteData.Clear();
        // chzz api 종료
        unbindEvent();
    }

    private void addRouletteData(string key, int count)
    {
        RoulettePieceData roulettePieceData = rouletteData.Find(x => x.Description.Equals(key));
        if (roulettePieceData == null)
        {
            rouletteData.Add(new RoulettePieceData(key, count));    
        }
        else
        {
            roulettePieceData.Chance += count;
        }
        onPieceDataChanged();
    }

    private void removeRouletteData(string key, int count)
    {
        RoulettePieceData roulettePieceData = rouletteData.Find(x => x.Description.Equals(key));
        if (roulettePieceData != null)
        {
            roulettePieceData.Chance -= count;
            onPieceDataChanged();
        }
    }

    private void bindEvent()
    {
        chzzkUnity.onMessage.AddListener(OnMessage);
        chzzkUnity.onDonation.AddListener(OnDonation);
        chzzkUnity.onOpen.AddListener(OnOpen);
        chzzkUnity.onClose.AddListener(OnClose);
        chzzkUnity.onSubscription.AddListener(OnSubscription);
    }

    private void unbindEvent()
    {
        chzzkUnity.onMessage.RemoveListener(OnMessage);
        chzzkUnity.onDonation.RemoveListener(OnDonation);
        chzzkUnity.onOpen.RemoveListener(OnOpen);
        chzzkUnity.onClose.RemoveListener(OnClose);
        chzzkUnity.onSubscription.RemoveListener(OnSubscription);
    }
    [SerializeField] 
    private Transform piecePrefab;
    [SerializeField]
    private Transform linePrefab;
    [SerializeField]
    private Transform pieceParent;
    [SerializeField]
    private Transform lineParent;
    
    private int GetRandomIndex()
    {
        int weight = Random.Range(0, totalWeight);

        for (int i = 0; i < rouletteData.Count; ++i)
        {
            if (rouletteData[i].Weight > weight)
            {
                return i;
            }
        }
        
        return 0;
    }
    #region IChzzAPIEvents

    public void OnMessage(ChzzkUnity.Profile profile, string message)
    {
    }

    public void OnDonation(ChzzkUnity.Profile profile, string message, ChzzkUnity.DonationExtras donation)
    {
        if (string.IsNullOrEmpty(message))
        {
            return;
        }

        string[] words = message.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (words.Length < 2)
        {
            return;
        }

        if (words[0] != ROULETTE_COMMAND)
        {
            return;
        }

        string key = words[1];

        if (string.IsNullOrWhiteSpace(key))
        {
            return;
        }

        addRouletteData(key, donation.payAmount / countPerAmount);
        Debug.Log($"룰렛 키 추가 : {key}, {donation.payAmount / countPerAmount}");
    }

    public void OnSubscription(ChzzkUnity.Profile profile, ChzzkUnity.SubscriptionExtras subscription)
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

    void updateWeight()
    {
        foreach (var pieceData in rouletteData)
        {
            totalWeight += pieceData.Chance;
            pieceData.Weight = totalWeight;
        }
    }
    void onPieceDataChanged()
    {
        updateWeight();

        // Piece UI 위치 조정
    }
    #region UI

    private void OnClick_StartCounting()
    {
        bindEvent();
        uiActiveSetting(true);
    }

    private void OnClick_StopCounting()
    {
        unbindEvent();
        uiActiveSetting(false);
    }

    private void uiActiveSetting(bool isCounting)
    {
        startCounting.gameObject.SetActive(!isCounting);
        stopCounting.gameObject.SetActive(isCounting);
        spin.gameObject.SetActive(!isCounting);
    }
  
    [SerializeField] 
    private Button spin;
    [SerializeField]
    private int spinDuration;
    [SerializeField]
    private Transform spinningRoulette;
    [SerializeField]
    private AnimationCurve spinningCurve;
    private bool isSpinning = false;
    private int selectedIndex = 0;
    private IEnumerator OnSpin(float targetAngle, UnityAction<RoulettePieceData> action)
    {
        float current = 0;
        float percent = 0;
        while (percent < 1)
        {
            current += Time.deltaTime;
            percent = current / spinDuration;

            float z = Mathf.Lerp(0, targetAngle, spinningCurve.Evaluate(percent));
            spinningRoulette.rotation = Quaternion.Euler(0, 0, z);
            
            yield return null;
        }

        isSpinning = false;
        if (action != null)
        {
            action.Invoke(rouletteData[selectedIndex]);
        }
    }
    #endregion
}