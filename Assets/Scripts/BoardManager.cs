using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardManager : MonoBehaviour
{
    public static BoardManager Instance { get; private set; }
    public GameObject boardCellPrefab;
    public int boardSize = 19; // 바둑은 19x19 크기로 설정합니다.
    public BoardCell[,] boardCells;
    private HashSet<Vector2Int> forbiddenPositions = new HashSet<Vector2Int>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        InitializeBoard();
    }

    void InitializeBoard()
    {
        boardCells = new BoardCell[boardSize, boardSize];

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                Vector2 position = new Vector2(x - boardSize / 2, y - boardSize / 2);
                GameObject cellObj = Instantiate(boardCellPrefab, position, Quaternion.identity);
                BoardCell cell = cellObj.GetComponent<BoardCell>();
                cell.x = x;
                cell.y = y;
                boardCells[x, y] = cell;
            }
        }
    }

    public void MakeMove(int x, int y, bool isBlack)
    {
        if (IsForbiddenPosition(x, y))
        {
            Debug.Log("Cannot place piece in forbidden position");
            return;
        }

        if (boardCells[x, y].HasPiece())
        {
            Debug.Log("Cell is already occupied");
            return;
        }

        boardCells[x, y].PlacePiece(isBlack);
        CaptureStones(x, y, isBlack);
    }

    void CaptureStones(int x, int y, bool isBlack)
    {
        int[][] directions = { new int[] { -1, 0 }, new int[] { 1, 0 }, new int[] { 0, -1 }, new int[] { 0, 1 } };

        foreach (var dir in directions)
        {
            int nx = x + dir[0];
            int ny = y + dir[1];

            if (nx < 0 || nx >= boardSize || ny < 0 || ny >= boardSize) continue;

            if (boardCells[nx, ny].HasPiece() && boardCells[nx, ny].IsBlack() != isBlack)
            {
                List<BoardCell> group = new List<BoardCell>();
                HashSet<BoardCell> visited = new HashSet<BoardCell>();

                if (IsGroupCaptured(nx, ny, !isBlack, group, visited))
                {
                    foreach (var cell in group)
                    {
                        Destroy(cell.currentPiece);
                        cell.currentPiece = null;
                        forbiddenPositions.Add(new Vector2Int(cell.x, cell.y));
                    }
                }
            }
        }
    }

    bool IsGroupCaptured(int x, int y, bool isBlack, List<BoardCell> group, HashSet<BoardCell> visited)
    {
        if (x < 0 || x >= boardSize || y < 0 || y >= boardSize) return false;
        BoardCell cell = boardCells[x, y];

        if (visited.Contains(cell)) return true;

        visited.Add(cell);

        if (!cell.HasPiece())
        {
            return false;
        }

        if (cell.IsBlack() != isBlack)
        {
            return true;
        }

        group.Add(cell);

        int[][] directions = { new int[] { -1, 0 }, new int[] { 1, 0 }, new int[] { 0, -1 }, new int[] { 0, 1 } };

        foreach (var dir in directions)
        {
            int nx = x + dir[0];
            int ny = y + dir[1];

            if (!IsGroupCaptured(nx, ny, isBlack, group, visited))
            {
                return false;
            }
        }

        return true;
    }

    public bool CheckWin()
    {
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (!boardCells[x, y].HasPiece())
                {
                    return false;
                }
            }
        }
        return true;
    }

    public void ResetBoard()
    {
        forbiddenPositions.Clear();
        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (boardCells[x, y].HasPiece())
                {
                    Destroy(boardCells[x, y].currentPiece);
                    boardCells[x, y].currentPiece = null;
                }
            }
        }
    }

    public bool IsForbiddenPosition(int x, int y)
    {
        return forbiddenPositions.Contains(new Vector2Int(x, y));
    }

    public BoardCell GetCellAt(int x, int y)
    {
        if (x < 0 || x >= boardSize || y < 0 || y >= boardSize) return null;
        return boardCells[x, y];
    }

    public int[] CalculateTerritory()
    {
        bool[,] visited = new bool[boardSize, boardSize];
        int blackTerritory = 0;
        int whiteTerritory = 0;

        for (int x = 0; x < boardSize; x++)
        {
            for (int y = 0; y < boardSize; y++)
            {
                if (boardCells[x, y] == null) // Null 체크 추가
                {
                    Debug.LogWarning($"Board cell at ({x}, {y}) is null");
                    continue;
                }

                if (!visited[x, y] && !boardCells[x, y].HasPiece())
                {
                    int territorySize;
                    bool isBlackTerritory;
                    if (IsTerritory(x, y, visited, out territorySize, out isBlackTerritory))
                    {
                        if (isBlackTerritory)
                        {
                            blackTerritory += territorySize;
                        }
                        else
                        {
                            whiteTerritory += territorySize;
                        }
                    }
                }
            }
        }

        return new int[] { blackTerritory, whiteTerritory };
    }

    bool IsTerritory(int x, int y, bool[,] visited, out int territorySize, out bool isBlackTerritory)
    {
        List<BoardCell> emptyGroup = new List<BoardCell>();
        Queue<BoardCell> queue = new Queue<BoardCell>();
        queue.Enqueue(boardCells[x, y]);
        visited[x, y] = true;

        bool hasBlackNeighbor = false;
        bool hasWhiteNeighbor = false;
        territorySize = 0;

        int[][] directions = { new int[] { -1, 0 }, new int[] { 1, 0 }, new int[] { 0, -1 }, new int[] { 0, 1 } };

        while (queue.Count > 0)
        {
            BoardCell cell = queue.Dequeue();
            emptyGroup.Add(cell);
            territorySize++;

            foreach (var dir in directions)
            {
                int nx = cell.x + dir[0];
                int ny = cell.y + dir[1];

                if (nx >= 0 && nx < boardSize && ny >= 0 && ny < boardSize && !visited[nx, ny])
                {
                    visited[nx, ny] = true;
                    BoardCell neighbor = boardCells[nx, ny];
                    if (neighbor.HasPiece())
                    {
                        if (neighbor.IsBlack())
                        {
                            hasBlackNeighbor = true;
                        }
                        else
                        {
                            hasWhiteNeighbor = true;
                        }
                    }
                    else
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }
        }

        isBlackTerritory = hasBlackNeighbor && !hasWhiteNeighbor;
        return hasBlackNeighbor != hasWhiteNeighbor;
    }
}
