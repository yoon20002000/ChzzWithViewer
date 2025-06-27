using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
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

        private RouletteDataManager rouletteDataManager = new();
        
        [Header("UI Buttons")] 
        [SerializeField] private Button channelConnectButton;
        [SerializeField] private Button startCountingButton;
        [SerializeField] private Button stopCountingButton;
        [SerializeField] private Button startSpinButton;
        [SerializeField] private Button addRouletteButton;
        [SerializeField] private Button removeRouletteButton;

        [Header("UI Inputs")]
        [SerializeField] private TMP_InputField channelInputField;
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
        
        private readonly List<RoulettePiece> roulettePieces = new();
        private readonly List<RectTransform> linePieces = new();

        private int countPerAmount = 1000;
        private bool isSpinning = false;
        private int selectedIndex = -1;

        private void Awake()
        {
            startCountingButton.onClick.AddListener(OnClickStartCounting);
            stopCountingButton.onClick.AddListener(OnClickStopCounting);
            addRouletteButton.onClick.AddListener(OnClickedAddRouletteData);
            removeRouletteButton.onClick.AddListener(OnClickedRemoveRouletteData);
            channelConnectButton.onClick.AddListener(OnClickedChannelConnect);
            startSpinButton.onClick.AddListener(OnClickedSpin);
            // 추후 제거 필요
            InitializeGame();
        }
        private void OnDestroy()
        {
            // 추후 제거 필요
            DeinitializeGame();
        }

        private void InitializeGame()
        {
            uiActiveSetting(false);
            rouletteDataManager.Clear();
            rouletteDataManager.OnPiecesDataUpdate.AddListener(UpdateRoulettePieceRotation);
        }

        private void DeinitializeGame()
        {
            rouletteDataManager.Clear();
            rouletteDataManager.OnPiecesDataUpdate.RemoveListener(UpdateRoulettePieceRotation);
            
            UnbindEvent();

            foreach (var piece in roulettePieces)
            {
                if (piece)
                {
                    Destroy(piece.gameObject);
                }
            }
            foreach (var line in linePieces)
            {
                if (line)
                {
                    Destroy(line.gameObject);
                }
            }

            roulettePieces.Clear();
            linePieces.Clear();
        }

        private void AddRouletteData(string key, int count)
        {
            rouletteDataManager.AddPiece(key, count);
        }

        private void RemoveRouletteData(string key, int count)
        {
            rouletteDataManager.RemovePiece(key,count);
        }
        
        private void UpdateRoulettePieceRotation()
        {
            int rouletteDataCount = rouletteDataManager.PiecesCount;
            while (roulettePieces.Count < rouletteDataCount)
            {
                var piece = Instantiate(piecePrefab, pieceParent.position, Quaternion.identity, pieceParent);
                roulettePieces.Add(piece.GetComponent<RoulettePiece>());
            }

            while (linePieces.Count < rouletteDataCount)
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

            for (int i = 0; i < rouletteDataCount; ++i)
            {
                var piece = roulettePieces[i];
                piece.Setup(rouletteDataManager[i]);

                if (piece.transform is RectTransform rectTransform)
                {
                    rectTransform.localRotation = Quaternion.identity;
                    if (roulettePieces.Count > 1)
                    {
                        rectTransform.localRotation = Quaternion.Euler(0, 0, GetPieceAngle(rouletteDataManager[i].Weight, rouletteDataManager[i].Chance));
                    }
                }

                piece.gameObject.SetActive(true);
            }

            if (rouletteDataCount > 1)
            {
                for (int i = 0; i < rouletteDataCount; ++i)
                {
                    var line = linePieces[i];
                    line.localRotation = Quaternion.identity;

                    float angle = GetPieceAngle(rouletteDataManager[i].Weight, rouletteDataManager[i].Chance);
                    float chanceAngle = 360f * rouletteDataManager[i].Chance / rouletteDataManager.TotalWeight;
                    line.localRotation = Quaternion.Euler(0, 0, angle + chanceAngle * 0.5f);

                    line.gameObject.SetActive(true);
                }
            }
        }

        private float GetPieceAngle(float weight, float chance)
        {
            float angle = 360f * weight / rouletteDataManager.TotalWeight;
            float chanceAngle = 360f * chance / rouletteDataManager.TotalWeight;
            return angle - chanceAngle * 0.5f;
        }
        public void Spin(UnityAction<RoulettePieceData> callback = null)
        {
            if (isSpinning)
            {
                return;
            }

            if (roulettePieces.Count == 0)
            {
                return;
            }

            spinningRoulette.rotation = Quaternion.identity;
            
            selectedIndex = GetRandomIndex();
            float angle = GetPieceAngle(rouletteDataManager[selectedIndex].Weight, rouletteDataManager[selectedIndex].Chance); 
            float change = 360f * rouletteDataManager[selectedIndex].Chance / rouletteDataManager.TotalWeight;
            float half = change * 0.5f;
            float padding = half * 0.25f;

            float randomAngle = Random.Range(angle - padding, angle + padding);
            float targetAngle = randomAngle + 360f * spinDuration * 2;

            Debug.Log($"angle : {angle}  random angle : {randomAngle}calc target angle {targetAngle}");
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
                spinningRoulette.rotation = Quaternion.Euler(0, 0, -z);
                yield return null;
            }

            isSpinning = false;
            callback?.Invoke(rouletteDataManager[selectedIndex]);
        }

        private int GetRandomIndex()
        {
            int rand = Random.Range(0, rouletteDataManager.TotalWeight);
            for (int i = 0; i < rouletteDataManager.PiecesCount; ++i)
            {
                if (rouletteDataManager[i].Weight > rand)
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
        private async void OnClickedChannelConnect()
        {
            string channelID = channelInputField.text;
            if (string.IsNullOrEmpty(channelID))
            {
                return;
            }

            await StartConnectChannel(channelID);
        }

        private async Task StartConnectChannel(string channelID)
        {
            await chzzkUnity.Connect(channelID);
        }

        private void uiActiveSetting(bool isCounting)
        {
            startCountingButton.gameObject.SetActive(!isCounting);
            stopCountingButton.gameObject.SetActive(isCounting);
            startSpinButton.gameObject.SetActive(!isCounting);
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

        private void OnClickedSpin()
        {
            Spin((data) =>
            {
                isSpinning = false;
                // 추가 연출 과 ui 출력
                
                Debug.Log($"선택된 제목 : {data.Description}, 가중치 : {data.Chance}");
            });
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