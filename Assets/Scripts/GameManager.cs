using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    [SerializeField] BoardManager boardManager;
    [SerializeField] GameObject finishUI;
    [SerializeField] GameObject retryUI;
    [SerializeField] GameObject wturnUI;
    [SerializeField] GameObject bturnUI;
    [SerializeField] TextMeshProUGUI winText;
    [SerializeField] TextMeshProUGUI blackTerritoryText;
    [SerializeField] TextMeshProUGUI whiteTerritoryText;

    bool isBlackTurn = true;
    bool isEnded = false;

    private void Awake()
    {
        if (boardManager == null)
        {
            boardManager = FindObjectOfType<BoardManager>();
        }

        bturnUI.SetActive(true);
        finishUI.SetActive(false);
        retryUI.SetActive(false);
    }

    private void Start()
    {
        UpdateTerritoryUI();
    }

    private void Update()
    {
        if (isEnded) return;

        if (CheckGameOver())
        {
            EndGame();
        }
    }

    bool CheckGameOver()
    {
        return boardManager.CheckWin();
    }

    void ChangeTurn()
    {
        isBlackTurn = !isBlackTurn;
        UpdateTurnUI();
    }

    void UpdateTurnUI()
    {
        if (isBlackTurn)
        {
            wturnUI.SetActive(false);
            bturnUI.SetActive(true);
        }
        else
        {
            wturnUI.SetActive(true);
            bturnUI.SetActive(false);
        }
    }

    void UpdateTerritoryUI()
    {
        int[] territories = boardManager.CalculateTerritory();
        blackTerritoryText.text = "Black Territory: " + territories[0];
        whiteTerritoryText.text = "White Territory: " + territories[1];
    }

    void EndGame()
    {
        isEnded = true;
        UpdateTerritoryUI();

        string winner = CalculateWinner();
        Debug.Log(winner + " wins!");
        finishUI.SetActive(true);
        winText.text = (winner + " wins!");
        retryUI.SetActive(true);
        Time.timeScale = 0;
    }

    public void OnCellClicked(BoardCell cell)
    {
        if (cell.HasPiece() || boardManager.IsForbiddenPosition(cell.x, cell.y))
        {
            Debug.Log("Invalid move");
            return;
        }

        boardManager.MakeMove(cell.x, cell.y, isBlackTurn);
        UpdateTerritoryUI();

        if (CheckGameOver())
        {
            EndGame();
        }
        else
        {
            ChangeTurn();
        }
    }

    public void RestartGame()
    {
        isEnded = false;
        Time.timeScale = 1;
        boardManager.ResetBoard();
        finishUI.SetActive(false);
        retryUI.SetActive(false);
        isBlackTurn = true;
        UpdateTurnUI();
        UpdateTerritoryUI();
    }

    string CalculateWinner()
    {
        int[] territories = boardManager.CalculateTerritory();
        int blackScore = territories[0];
        int whiteScore = territories[1];

        return blackScore > whiteScore ? "Black" : "White";
    }
}
