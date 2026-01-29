using System;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

public class FinishLine : MonoBehaviour
{
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TextMeshProUGUI winText;
    private bool raceFinished = false;

    private void Awake()
    {
        MoveAgent.PlayerTouchedFinishLine += OnPlayerTouchedFinishLine;
    }

    private void OnPlayerTouchedFinishLine(GameObject player)
    {
        if (raceFinished)
            return;
        if (!player.CompareTag("Player"))
            return;
        raceFinished = true;
        string winnerName = player.name.Replace("(Clone)", "");
        ShowWinUI(winnerName);
    }
    
    
    void ShowWinUI(string winnerName)
    {
        winPanel.SetActive(true);
        winText.text = $"{winnerName} \nWon the race!";
        Time.timeScale = 0f;
    }
}
