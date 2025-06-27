using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace ChzzAPI
{
    public class RouletteGame : MonoBehaviour, IChzzAPIEvents
    {
        [Header("ChzzAPI")] [SerializeField] private ChzzkUnity chzzkUnity;
        private const string ROULETTE_COMMAND = "[룰렛]";

        [Header("UI Buttons")] 
        [SerializeField] private Button startCounting;
        [SerializeField] private Button stopCounting;
        [SerializeField] private Button startSpin;
        [SerializeField] private Button addRouletteButton;
        [SerializeField] private Button removeRouletteButton;

        [Header("UI Inputs")]
        [SerializeField] private TMP_InputField keyInputField;
        [SerializeField] private TMP_InputField countInputField;

        [Header("Roulette Piece Prefabs")]
        [SerializeField] private Transform piecePrefab;
        [SerializeField] private Transform linePrefab;
        [SerializeField] private Transform pieceParent;
        [SerializeField] private Transform lineParent;

        [Header("Spinning")]
        [SerializeField] private Transform spinningRoulette;
        [SerializeField] private AnimationCurve spinningCurve;
        [SerializeField] private int spinDuration;

        private readonly List<RoulettePieceData> rouletteData = new(100);
        private readonly List<RoulettePiece> roulettePieces = new();
        private readonly List<RectTransform> linePieces = new();

        private int totalWeight = 0;
        private int selectedIndex = 0;
        private int countPerAmount = 1000;
        private bool isSpinning = false;

        private void Awake()
        {
            startCounting.onClick.AddListener(OnClickStartCounting);
            stopCounting.onClick.AddListener(OnClickStopCounting);
            addRouletteButton.onClick.AddListener(OnClickedAddRouletteData);
            removeRouletteButton.onClick.AddListener(OnClickedRemoveRouletteData);
        }

        private void InitializeGame()
        {
            totalWeight = 0;
            rouletteData.Clear();
            uiActiveSetting(false);
        }

        private void DeinitializeGame()
        {
            rouletteData.Clear();
            UnbindEvent();

            foreach (var piece in roulettePieces)
            {
                Destroy(piece.gameObject);
            }
            foreach (var line in linePieces)
            {
                Destroy(line.gameObject);
            }

            roulettePieces.Clear();
            linePieces.Clear();
        }

        private void AddRouletteData(string key, int count)
        {
            var pieceData = rouletteData.Find(x => x.Description == key);
            if (pieceData == null)
            {
                rouletteData.Add(new RoulettePieceData(key, count));
            }
            else
            {
                pieceData.Chance += count;
            }

            OnPieceDataChanged();
        }

        private void RemoveRouletteData(string key, int count)
        {
            var pieceData = rouletteData.Find(x => x.Description == key);
            if (pieceData == null)
            {
                return;
            }

            pieceData.Chance -= count;
            if (pieceData.Chance <= 0)
            {
                rouletteData.Remove(pieceData);
            }

            OnPieceDataChanged();
        }

        private void UpdateWeight()
        {
            totalWeight = 0;
            foreach (var pieceData in rouletteData)
            {
                totalWeight += pieceData.Chance;
                pieceData.Weight = totalWeight;
            }
        }

        private void UpdateRoulettePieceRotation()
        {
            while (roulettePieces.Count < rouletteData.Count)
            {
                var piece = Instantiate(piecePrefab, pieceParent.position, Quaternion.identity, pieceParent);
                roulettePieces.Add(piece.GetComponent<RoulettePiece>());
            }

            while (linePieces.Count < rouletteData.Count)
            {
                var line = Instantiate(linePrefab, lineParent.position, Quaternion.identity, lineParent);
                linePieces.Add(line as RectTransform);
            }

            foreach (var piece in roulettePieces)
            {
                piece.gameObject.SetActive(false);
            }
            foreach (var line in linePieces)
            {
                line.gameObject.SetActive(false);
            }

            for (int i = 0; i < rouletteData.Count; ++i)
            {
                var piece = roulettePieces[i];
                piece.Setup(rouletteData[i]);

                if (piece.transform is RectTransform rectTransform)
                {
                    rectTransform.localRotation = Quaternion.identity;
                    if (roulettePieces.Count > 1)
                    {
                        rectTransform.localRotation = Quaternion.Euler(0, 0, GetPieceAngle(rouletteData[i].Weight, rouletteData[i].Chance));
                    }
                }

                piece.gameObject.SetActive(true);
            }

            if (rouletteData.Count > 1)
            {
                for (int i = 0; i < rouletteData.Count; ++i)
                {
                    var line = linePieces[i];
                    line.localRotation = Quaternion.identity;

                    float angle = GetPieceAngle(rouletteData[i].Weight, rouletteData[i].Chance);
                    float chanceAngle = 360f * rouletteData[i].Chance / totalWeight;
                    line.localRotation = Quaternion.Euler(0, 0, angle + chanceAngle * 0.5f);

                    line.gameObject.SetActive(true);
                }
            }
        }

        private float GetPieceAngle(float weight, float chance)
        {
            float angle = 360f * weight / totalWeight;
            float chanceAngle = 360f * chance / totalWeight;
            return angle - chanceAngle * 0.5f;
        }

        private void OnPieceDataChanged()
        {
            UpdateWeight();
            UpdateRoulettePieceRotation();
        }

        public void Spin(UnityAction<RoulettePieceData> callback = null)
        {
            if (isSpinning) return;

            selectedIndex = GetRandomIndex();
            float angle = 360f * rouletteData[selectedIndex].Chance / totalWeight;
            float half = angle * 0.5f;
            float padding = half * 0.25f;

            float randomAngle = Random.Range(angle - padding, angle + padding);
            float targetAngle = randomAngle + 360f * spinDuration * 2;

            isSpinning = true;
            StartCoroutine(SpinCoroutine(targetAngle, callback));
        }

        private IEnumerator SpinCoroutine(float targetAngle, UnityAction<RoulettePieceData> callback)
        {
            float time = 0;
            while (time < spinDuration)
            {
                time += Time.deltaTime;
                float percent = time / spinDuration;
                float z = Mathf.Lerp(0, targetAngle, spinningCurve.Evaluate(percent));
                spinningRoulette.rotation = Quaternion.Euler(0, 0, z);
                yield return null;
            }

            isSpinning = false;
            callback?.Invoke(rouletteData[selectedIndex]);
        }

        private int GetRandomIndex()
        {
            int rand = Random.Range(0, totalWeight);
            for (int i = 0; i < rouletteData.Count; ++i)
            {
                if (rouletteData[i].Weight > rand)
                    return i;
            }
            return 0;
        }

        private void OnClickStartCounting()
        {
            BindEvent();
            uiActiveSetting(true);
        }

        private void OnClickStopCounting()
        {
            UnbindEvent();
            uiActiveSetting(false);
        }

        private void OnClickedAddRouletteData()
        {
            if (!int.TryParse(countInputField.text, out int count) || count <= 0) return;
            AddRouletteData(keyInputField.text, count);
        }

        private void OnClickedRemoveRouletteData()
        {
            if (!int.TryParse(countInputField.text, out int count) || count <= 0) return;
            RemoveRouletteData(keyInputField.text, count);
        }

        private void uiActiveSetting(bool isCounting)
        {
            startCounting.gameObject.SetActive(!isCounting);
            stopCounting.gameObject.SetActive(isCounting);
            startSpin.gameObject.SetActive(!isCounting);
        }

        private void BindEvent()
        {
            chzzkUnity.onMessage.AddListener(OnMessage);
            chzzkUnity.onDonation.AddListener(OnDonation);
            chzzkUnity.onOpen.AddListener(OnOpen);
            chzzkUnity.onClose.AddListener(OnClose);
            chzzkUnity.onSubscription.AddListener(OnSubscription);
        }

        private void UnbindEvent()
        {
            chzzkUnity.onMessage.RemoveListener(OnMessage);
            chzzkUnity.onDonation.RemoveListener(OnDonation);
            chzzkUnity.onOpen.RemoveListener(OnOpen);
            chzzkUnity.onClose.RemoveListener(OnClose);
            chzzkUnity.onSubscription.RemoveListener(OnSubscription);
        }

        #region IChzzAPIEvents

        public void OnMessage(Profile profile, string message) { }
        public void OnSubscription(Profile profile, SubscriptionExtras subscription) { }

        public void OnDonation(Profile profile, string message, DonationExtras donation)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                return;
            }

            var words = message.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (words.Length < 2 || words[0] != ROULETTE_COMMAND)
            {
                return;
            }

            string key = words[1];
            if (string.IsNullOrWhiteSpace(key))
            {
                return;
            }

            int count = donation.payAmount / countPerAmount;
            AddRouletteData(key, count);
            Debug.Log($"룰렛 키 추가 : {key}, {count}");
        }

        public void OnOpen() => InitializeGame();
        public void OnClose() => DeinitializeGame();

        #endregion
    }
}