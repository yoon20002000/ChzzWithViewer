using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class Roulette : MonoBehaviour
{
    #region Logic

    [SerializeField] 
    private Transform piecePrefab;
    [SerializeField]
    private Transform linePrefab;
    [SerializeField]
    private Transform pieceParent;
    [SerializeField]
    private Transform lineParent;
    [SerializeField]
    private RoulettePieceData[] roulettePieceData;

    private float pieceAngle;
    private float halfPieceAngle;
    private float halfPieceAngleWithPaddings;

    private int accumulateWeight;
    private bool isSpinning = false;
    private int selectedIndex = 0;
    #endregion

    #region 
    [SerializeField]
    private int spinDuration;
    [SerializeField]
    private Transform spinningRoulette;
    [SerializeField]
    private AnimationCurve spinningCurve;
    #endregion

    private void Awake()
    {
        pieceAngle = 360 / roulettePieceData.Length;
        halfPieceAngle = pieceAngle * .5f;
        halfPieceAngleWithPaddings = halfPieceAngle - (halfPieceAngle * .25f);

        SpawnPiecesAndLines();
        CalculateWightAndIndices();
    }
    
    private void SpawnPiecesAndLines()
    {
        for (int i = 0; i < roulettePieceData.Length; ++i)
        {
            Transform piece = Instantiate(piecePrefab, pieceParent.position, quaternion.identity, pieceParent);
            piece.GetComponent<RoulettePiece>().Setup(roulettePieceData[i]);
            piece.RotateAround(pieceParent.position, Vector3.back, pieceAngle * i);
            
            Transform line = Instantiate(linePrefab, lineParent.position, quaternion.identity, lineParent);
            line.RotateAround(lineParent.position, Vector3.back, pieceAngle * i + halfPieceAngle);
        }
    }
    private void CalculateWightAndIndices()
    {
        for (int i = 0; i < roulettePieceData.Length; ++i)
        {
            roulettePieceData[i].Index = i;
            if (roulettePieceData[i].Chance <= 0)
            {
                roulettePieceData[i].Index = 1;
            }
            accumulateWeight += roulettePieceData[i].Chance;
            roulettePieceData[i].Weight = accumulateWeight;
        }
    }

    private int GetRandomIndex()
    {
        int weight = Random.Range(0, accumulateWeight);

        for (int i = 0; i < roulettePieceData.Length; ++i)
        {
            if (roulettePieceData[i].Weight > weight)
            {
                return i;
            }
        }
        
        return 0;
    }

    public void Spin(UnityAction<RoulettePieceData> action = null)
    {
        if (isSpinning)
        {
            return;
        }

        selectedIndex = GetRandomIndex();

        float angle = pieceAngle * selectedIndex;
        float leftOffset = (angle - halfPieceAngleWithPaddings) % 360;
        float rightOffset = (angle + halfPieceAngleWithPaddings) % 360;
        float randomAngle  = Random.Range(leftOffset,rightOffset);
        
        int rotateSpeed = 2;
        float targetAngle = (randomAngle + 360 * spinDuration * rotateSpeed);
        
        isSpinning = true;
        StartCoroutine(OnSpin(targetAngle, action));
    }

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
            action.Invoke(roulettePieceData[selectedIndex]);
        }
    }
}
