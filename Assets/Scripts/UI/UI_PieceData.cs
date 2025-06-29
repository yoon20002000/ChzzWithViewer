using System;
using System.Collections.Generic;
using ChzzAPI;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UIElements;

public class UI_PieceData : MonoBehaviour
{
    [SerializeField] private RouletteGame rouletteGame;
    [SerializeField] private ScrollView scrollView;
    [SerializeField] private Transform contentParent; 
    [SerializeField] private GameObject rowPrefab;
    
    private ObjectPool<UI_PieceDataRow> rowPool;
    private readonly List<UI_PieceDataRow> activeRows = new();
    private void Awake()
    {
        rouletteGame.OnUpdateRoulettePieces.AddListener(UpdatePieceListUI);   
        rowPool = new ObjectPool<UI_PieceDataRow>(
            createFunc: () =>
            {
                var go = Instantiate(rowPrefab, contentParent);
                return go.GetComponent<UI_PieceDataRow>();
            },
            actionOnGet: row => row.gameObject.SetActive(true),
            actionOnRelease: row => row.gameObject.SetActive(false),
            actionOnDestroy: row => Destroy(row.gameObject),
            collectionCheck: false,
            defaultCapacity: 10
        );
    }

    private void OnDestroy()
    {
        rouletteGame.OnUpdateRoulettePieces.RemoveListener(UpdatePieceListUI);
    }

    private void UpdatePieceListUI(IReadOnlyList<RoulettePieceData> pieces)
    {
        // 기존 Row 반환
        foreach (var row in activeRows)
        {
            rowPool.Release(row);
        }
        activeRows.Clear();

        // 새 Row 할당
        foreach (var piece in pieces)
        {
            var row = rowPool.Get();
            row.Setup(piece.Description, piece.Chance);
            activeRows.Add(row);
        }
    }
}
