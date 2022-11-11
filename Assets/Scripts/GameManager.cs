using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    
    private static GameManager instance;

    public static GameManager Instance
    {
        get
        {
            if (instance == null) instance = FindObjectOfType<GameManager>();
            return instance;
        }
    }

    [SerializeField] private TMPro.TextMeshProUGUI scoreTMPro;
    [SerializeField] private string scorePreface = "Points: ";
    [SerializeField , Range(0, 6)] private int scoreDigits = 6;
    private int score = 0;

    private void Awake()
    {
        if (instance == null) instance = FindObjectOfType<GameManager>();
    }

    private void Start()
    {
        UpdatePointsText();
    }

    public void AddPoints(int pointsToAdd)
    {
        if ((score += pointsToAdd) >= (Math.Pow(10,scoreDigits))) score = (int)(Math.Pow(10, scoreDigits));
        UpdatePointsText();
    }

    private void UpdatePointsText()
    {
        scoreTMPro.text = scorePreface + score.ToString("D" + scoreDigits.ToString());
    }
    private void SetScore(int scoreToSet)
    {
        score = scoreToSet;
        if (score >= (Math.Pow(10, scoreDigits))) score = (int)(Math.Pow(10, scoreDigits));
        UpdatePointsText();
    }

    public void SaveGlobalData(ref GlobalSaveData globalSaveData)
    {
        globalSaveData.score = score;
    }

    public void LoadGlobalData(GlobalSaveData globalSaveData)
    {
        SetScore(globalSaveData.score);
    }
}
