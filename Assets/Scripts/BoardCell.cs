using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoardCell : MonoBehaviour
{
    public int x, y;
    public GameObject black;
    public GameObject white;
    [HideInInspector] public GameObject currentPiece; // 이 부분을 public에서 HideInInspector로 변경
    private Vector2 offset = new Vector2(0.5f, -0.5f); // 바둑판의 교차점 중심을 맞추기 위한 오프셋 조정

    public void PlacePiece(bool isBlack)
    {
        if (currentPiece != null) Destroy(currentPiece);
        Vector2 position = (Vector2)transform.position + offset;
        currentPiece = Instantiate(isBlack ? black : white, position, Quaternion.identity, transform);
    }

    public bool HasPiece()
    {
        return currentPiece != null;
    }

    public bool IsBlack()
    {
        if (currentPiece == null) return false;
        return currentPiece.GetComponent<SpriteRenderer>().color == Color.black;
    }
}
