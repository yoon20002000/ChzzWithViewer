using System;
using System.Collections;
using System.Collections.Generic;
using ChzzAPI;
using TMPro;
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
        startCounting.onClick.AddListener(onClick_StartCounting);
        stopCounting.onClick.AddListener(onClick_StopCounting);
        
        addRouletteButton.onClick.AddListener(onClicked_AddRouletteData);
        removeRouletteButton.onClick.AddListener(onClicked_RemoveRouletteData);
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
        totalWeight = 0;
        rouletteData.Clear();
        // chzz api 초기화
        uiActiveSetting(false);
    }

    private void deinitializeGame()
    {
        rouletteData.Clear();
        // chzz api 종료
        unbindEvent();
        // 제거
        for (int i = roulettePieces.Count - 1; i > 0 ; --i)
        {
            Destroy(roulettePieces[i].gameObject);
        }
        for (int i = linePieces.Count - 1; i > 0 ; --i)
        {
            Destroy(linePieces[i].gameObject);
        }
        
        roulettePieces.Clear();
        linePieces.Clear();
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
            if (roulettePieceData.Chance <= 0)
            {
                rouletteData.Remove(roulettePieceData);
            }
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
        totalWeight = 0;
        foreach (var pieceData in rouletteData)
        {
            totalWeight += pieceData.Chance;
            pieceData.Weight = totalWeight;
        }
    }
    
    List<RoulettePiece> roulettePieces = new List<RoulettePiece>();
    List<RectTransform> linePieces = new List<RectTransform>();
    void updateRoulettPieceRotatioin()
    {
        if (roulettePieces.Count < rouletteData.Count)
        {
            int makeCount = rouletteData.Count - roulettePieces.Count;
            
            for (int count = 0; count < makeCount; count++)
            {
                Transform piece = Instantiate(piecePrefab, pieceParent.position, Quaternion.identity, pieceParent);
                roulettePieces.Add(piece.GetComponent<RoulettePiece>());
            }
        }

        if (linePieces.Count < rouletteData.Count)
        {
            int makeCount = rouletteData.Count - linePieces.Count;
            
            for (int count = 0; count < makeCount; ++count)
            {
                Transform line = Instantiate(linePrefab, lineParent.position, Quaternion.identity, lineParent);
                linePieces.Add(line as RectTransform);
            }
        }

        for (int index = 0; index < roulettePieces.Count; ++index)
        {
            roulettePieces[index].gameObject.SetActive(false);
            
        }

        for (int index = 0; index < linePieces.Count; ++index)
        {
            linePieces[index].gameObject.SetActive(false);   
        }
        

        for (int index = 0; index < rouletteData.Count; ++index)
        {
            roulettePieces[index].Setup(rouletteData[index]);
            
            RectTransform rectTran = (roulettePieces[index].gameObject.transform as RectTransform);
            if (rectTran == null)
            {
                continue;
            }
            rectTran.localRotation = Quaternion.identity;
            
            if (roulettePieces.Count > 1)
            {
                rectTran.localRotation = Quaternion.Euler(0, 0, getPieceAngle(rouletteData[index].Weight, rouletteData[index].Chance));    
            }
            roulettePieces[index].gameObject.SetActive(true);
        }

        if (rouletteData.Count > 1)
        {
            for (int index = 0; index < linePieces.Count; ++index)
            {
                linePieces[index].localRotation =  Quaternion.identity;
                float chanceAngle = 360 * ((float)rouletteData[index].Chance / totalWeight);
                float targetAngle = getPieceAngle(rouletteData[index].Weight, rouletteData[index].Chance);
                
                linePieces[index].localRotation = Quaternion.Euler(0, 0, targetAngle + chanceAngle * .5f);
                linePieces[index].gameObject.SetActive(true);
            }    
        }
    }
    private float getPieceAngle(float weight, float change)
    {
        float angle = 360 * (weight / totalWeight);
        float chanceAngle = 360 * (change / totalWeight);
        
        return angle - chanceAngle * .5f;
    }
    void onPieceDataChanged()
    {
        updateWeight();

        updateRoulettPieceRotatioin();
    }
    public void Spin(UnityAction<RoulettePieceData> action = null)
    {
        if (isSpinning)
        {
            return;
        }

        selectedIndex = GetRandomIndex();

        float angle = 360 / ((float)rouletteData[selectedIndex].Weight / totalWeight);
        float halfPieceAngle = angle * .5f;
        float halfPieceAngleWithPaddings = halfPieceAngle - (halfPieceAngle * .25f);
        float leftOffset = (angle - halfPieceAngleWithPaddings) % 360;
        float rightOffset = (angle + halfPieceAngleWithPaddings) % 360;
        float randomAngle  = Random.Range(leftOffset,rightOffset);
        
        int rotateSpeed = 2;
        float targetAngle = (randomAngle + 360 * spinDuration * rotateSpeed);
        
        isSpinning = true;
        StartCoroutine(OnSpin(targetAngle, action));
    }
    #region UI

    private void onClick_StartCounting()
    {
        bindEvent();
        uiActiveSetting(true);
    }

    private void onClick_StopCounting()
    {
        unbindEvent();
        uiActiveSetting(false);
    }

    [Space(10)]
    [SerializeField]
    private TMP_InputField keyInputField;
    [SerializeField]
    private TMP_InputField countInputField;
    [SerializeField]
    private Button addRouletteButton;
    [SerializeField]
    private Button removeRouletteButton;
    private void onClicked_AddRouletteData()
    {
        string key = keyInputField.text;
        string strCount = countInputField.text;
        
        addRouletteData(key, int.Parse(strCount));
    }

    private void onClicked_RemoveRouletteData()
    {
        string key = keyInputField.text;
        string strCount = countInputField.text;
        
        removeRouletteData(key, int.Parse(strCount));
    }

    private void uiActiveSetting(bool isCounting)
    {
        startCounting.gameObject.SetActive(!isCounting);
        stopCounting.gameObject.SetActive(isCounting);
        startSpin.gameObject.SetActive(!isCounting);
    }
  

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