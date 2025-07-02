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
    public class RouletteGame : MonoBehaviour
    {
        [Header("ChzzAPI")] [SerializeField] private ChzzkUnity chzzkUnity;
        private const string ROULETTE_COMMAND = "[룰렛]";

        private RouletteDataManager rouletteDataManager = new();
        
        [Header("UI Buttons")] 
        [SerializeField] private Button startCountingButton;
        [SerializeField] private Button stopCountingButton;
        [SerializeField] private Button startSpinButton;
        [SerializeField] private Button addRouletteButton;
        [SerializeField] private Button removeRouletteButton;
        [SerializeField] private Button settingButton;
        [SerializeField] private Button resetPieceDataButton;
        
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

        [HideInInspector]
        public UnityEvent<IReadOnlyList<RoulettePieceData>> OnUpdateRoulettePieces = new();

        [Header("UI")]
        [SerializeField] private UI_ResultPopup resultPopup;
        [SerializeField] private UI_RouletteSetting rouletteSetting;
        private void Awake()
        {
            InitializeGame();
            chzzkUnity.onOpen.AddListener(OnOpen);
            chzzkUnity.onClose.AddListener(OnClose);
            chzzkUnity.onDonation.AddListener(OnDonation);
            chzzkUnity.onMessage.AddListener(OnMessage);
            
            startSpinButton.gameObject.SetActive(false);
        }

        private void OnDestroy()
        {
            chzzkUnity.onOpen.RemoveListener(OnOpen);
            chzzkUnity.onClose.RemoveListener(OnClose);
            chzzkUnity.onDonation.RemoveListener(OnDonation);

            DeinitializeGame();
        }

        private void InitializeGame()
        {
            startCountingButton.onClick.AddListener(OnClickStartCounting);
            stopCountingButton.onClick.AddListener(OnClickStopCounting);
            addRouletteButton.onClick.AddListener(OnClickedAddRouletteData);
            removeRouletteButton.onClick.AddListener(OnClickedRemoveRouletteData);
            startSpinButton.onClick.AddListener(OnClickedSpin);
            settingButton.onClick.AddListener(OnClickedSetting);
            resetPieceDataButton.onClick.AddListener(OnClickedResetPiece);
            
            uiActiveSetting(false);
            rouletteDataManager.Clear();
        }
        private void DeinitializeGame()
        {
            foreach (var piece in roulettePieces)
            {
                if (piece != null)
                {
                    Destroy(piece.gameObject);
                }
            }
            roulettePieces.Clear();
            
            foreach (var line in linePieces)
            {
                if (line != null)
                {
                    Destroy(line.gameObject);
                }
            }
            linePieces.Clear();
            chzzkUnity.StopListening();
        }

        private void AddRouletteData(string key, int count)
        {
            rouletteDataManager.AddPiece(key, count);
            // UI 업데이트를 메인 스레드에서 실행
            UnityMainThreadDispatcher.Instance.Enqueue(UpdateRoulettePieceUI);
        }

        private void RemoveRouletteData(string key, int count)
        {
            rouletteDataManager.RemovePiece(key, count);
            // UI 업데이트를 메인 스레드에서 실행
            UnityMainThreadDispatcher.Instance.Enqueue(UpdateRoulettePieceUI);
        }

        private void UpdateRoulettePieceUI()
        {
            UpdateRoulettePieceRotation();
            OnUpdateRoulettePieces?.Invoke(rouletteDataManager.Pieces);
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
            float chance = 360f * rouletteDataManager[selectedIndex].Chance / rouletteDataManager.TotalWeight;
            float half = chance * 0.5f;
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
                {
                    return i;
                }
            }
            return 0;
        }

        private async void OnClickStartCounting()
        {
            string channelID = channelInputField.text;
            if (string.IsNullOrEmpty(channelID))
            {
                return;
            }

            await StartConnectChannel(channelID);
            
        }

        private void OnClickStopCounting()
        {
            uiActiveSetting(false);
            chzzkUnity.StopListening();
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

        private async Task StartConnectChannel(string channelID)
        {
            await chzzkUnity.Connect(channelID);
        }

        private void uiActiveSetting(bool isCounting)
        {
            startCountingButton.gameObject.SetActive(!isCounting);
            stopCountingButton.gameObject.SetActive(isCounting);
            startSpinButton.gameObject.SetActive(!isCounting);
            startSpinButton.interactable = !isCounting;
            resetPieceDataButton.interactable = !isCounting;
        }
        private void OnClickedSpin()
        {
            startSpinButton.interactable = false;
            Spin((data) =>
            {
                isSpinning = false;
                // 추가 연출 과 ui 출력
                resultPopup.Show(data);
                startSpinButton.interactable = true;
                Debug.Log($"선택된 제목 : {data.Description}, 가중치 : {data.Chance}");
            });
        }
        
        private void OnClickedSetting()
        {
            rouletteSetting.gameObject.SetActive(true);
        }
        
        private void OnClickedResetPiece()
        {
            rouletteDataManager.Clear();
            UpdateRoulettePieceUI();
        }
        
        private void OnMessage(Profile profile, string message)
        {
            // if (string.IsNullOrWhiteSpace(message) || !message.Contains(ROULETTE_COMMAND))
            // {
            //     return;
            // }
            //
            // int firstQuote = message.IndexOf('"');
            // int secondQuote = message.IndexOf('"', firstQuote + 1);
            //
            // if (firstQuote != -1 && secondQuote != -1)
            // {
            //     string result = message.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
            //     
            //     int count = 1;
            //     AddRouletteData(result, count);
            //     Debug.Log($"룰렛 키 추가 : {result}, {count}");
            // }
            // else
            // {
            //     Console.WriteLine("큰따옴표가 없습니다.");
            //     return;
            // }
        }
        public void OnDonation(Profile profile, string message, DonationExtras donation)
        {
            if (string.IsNullOrWhiteSpace(message) || !message.Contains(ROULETTE_COMMAND))
            {
                return;
            }

            int firstQuote = message.IndexOf('"');
            int secondQuote = message.IndexOf('"', firstQuote + 1);

            if (firstQuote != -1 && secondQuote != -1)
            {
                string result = message.Substring(firstQuote + 1, secondQuote - firstQuote - 1);
                
                int count = donation.payAmount / countPerAmount;
                AddRouletteData(result, count);
                Debug.Log($"룰렛 키 추가 : {result}, {count}");
            }
            else
            {
                Debug.LogWarning($"큰따옴표가 없습니다. : {message}");
                return;
            }
        }

        public void OnOpen()
        {
            UnityMainThreadDispatcher.Instance.Enqueue(uiActive);
            
            Debug.Log("연결 완료");
        }
        public void OnClose()
        {
            UnityMainThreadDispatcher.Instance.Enqueue(uiDeactive);
            
            Debug.Log("연결 실패");
        }
        private void uiActive()
        {
            uiActiveSetting(true);
        }

        private void uiDeactive()
        {
            uiActiveSetting(false);
        }
    }
}