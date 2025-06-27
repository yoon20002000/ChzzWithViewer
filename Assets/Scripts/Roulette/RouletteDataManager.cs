using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace ChzzAPI
{
    public class RouletteDataManager
    {
        private readonly List<RoulettePieceData> pieces = new(100);
        public IReadOnlyList<RoulettePieceData> Pieces => pieces;
        public int TotalWeight { get; private set; } = 0;
        public int PiecesCount => pieces.Count;

        public RoulettePieceData this[int index]
        {
            get
            {
                if (index < 0 || index >= pieces.Count)
                {
                    throw new IndexOutOfRangeException($"Index is out of range {index}");
                }
                return pieces[index];
            }
        }

        public UnityEvent OnPiecesDataUpdate = new();
        
        // 조각 추가
        public void AddPiece(string key, int weight)
        {
            if (string.IsNullOrWhiteSpace(key) || weight <= 0)
            {
                return;
            }

            var piece = GetRouletteData(key); 
            
            if (piece == null)
            {
                pieces.Add(new RoulettePieceData(key, weight));
            }
            else
            {
                piece.Chance += weight;
            }
            UpdateWeight();
            OnPiecesDataUpdate?.Invoke();
        }

        // 조각 가중치 감소/삭제
        public void RemovePiece(string key, int weight)
        {
            if (string.IsNullOrWhiteSpace(key) || weight <= 0)
            {
                return;
            }
            var piece = GetRouletteData(key);
            if (piece != null)
            {
                piece.Chance -= weight;
                if (piece.Chance <= 0)
                {
                    pieces.Remove(piece);
                }
                UpdateWeight();
                OnPiecesDataUpdate?.Invoke();
            }
        }

        // 전체 초기화
        public void Clear()
        {
            pieces.Clear();
            UpdateWeight();
        }

        // 가중치 합계 갱신
        private void UpdateWeight()
        {
            TotalWeight = 0;
            foreach (var p in pieces)
            {
                TotalWeight += p.Chance;
                p.Weight = TotalWeight;
            }
        }

        // 랜덤 인덱스 추출 (가중치 기반)
        public int GetRandomIndex()
        {
            if (TotalWeight == 0)
            {
                return -1;
            }
            int weight = UnityEngine.Random.Range(0, TotalWeight);
            int sum = 0;
            for (int i = 0; i < pieces.Count; ++i)
            {
                sum += pieces[i].Chance;
                if (weight < sum)
                {
                    return i;
                }
            }
            return -1;
        }

        // 특정 키의 현재 가중치 반환
        public int GetWeight(string key)
        {
            var piece = GetRouletteData(key);
            return piece != null ? piece.Chance : 0;
        }

        public RoulettePieceData GetRouletteData(string key)
        {
            return pieces.Find(x => x.Description == key);
        }
    }
}
