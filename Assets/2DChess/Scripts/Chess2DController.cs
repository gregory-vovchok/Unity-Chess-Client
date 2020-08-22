﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ChessCore;
using UnityEditor;

public class Chess2DController : MonoBehaviour
{
    public Chess Chess { get; private set; } = new Chess();
    public static Chess2DController Instance;
    [SerializeField] Transform HUDParent;
    [SerializeField] HighlightSquare squarePrefab;
    HighlightSquare[,] squares = new HighlightSquare[8, 8];

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        DragAndDropController.Instance.OnStartDrag += OnStartDragFigure;
        DragAndDropController.Instance.OnEndDrag += OnEndDragFigure;

        CreateSquares();
        ShowLegalFigures();
    }

    void ShowLegalFigures()
    {
        HideMoves();
        int x, y;

        foreach(string move in Chess.YieldValidMoves())
        {
            Chess.GetSquarePos(move.Substring(1, 2), out x, out y);
            ShowLegalSquare(x, y, true);
        }
    }
    public void ShowLegalMoves(Figure figure, int figureX, int figureY)
    {
        //Debug.Log($"{figure} in pos [{figureX}, {figureY}]");

        HideMoves();
        int x, y;

        foreach (string move in Chess.YieldValidMoves())
        {
            if ((char)figure == move.Substring(0, 1)[0])// the same figure
            {
                Chess.GetSquarePos(move.Substring(1, 2), out x, out y);
               
                if (figureX == x && figureY == y)// the same position
                {
                    Chess.GetSquarePos(move.Substring(3, 2), out x, out y);

                    if (Chess.FigureAt(x, y) != '.')// target square is not empty
                    {
                        ShowLegalSquare(x, y, false);
                    }
                    else
                    {
                        ShowLegalSquare(x, y, true);
                    }
                }
            }
        }
    }

    void CreateSquares()
    {
        HighlightSquare square;

        for (int x = 0; x < 8; x++)
        {
            for(int y = 0; y < 8; y++)
            {
                square = Instantiate(squarePrefab);
                square.transform.SetParent(HUDParent, false);

                square.Hide();

                Vector3 pos = Board2DBuilder.Instance.BoardStartPos + new Vector2(x * Board2DBuilder.Instance.SquareSize.x, y * Board2DBuilder.Instance.SquareSize.y);
                square.SetWorldPosition(pos);
                squares[x, y] = square;
            }
        }
    }

    void ShowLegalSquare(int x, int y, bool emptyOrEnemy)
    {
        if (emptyOrEnemy) 
            squares[x, y].ShowEmptySquare();
        else 
            squares[x, y].ShowEnemySquare();
    }

    void HideMoves()
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                squares[x, y].Hide();
            }
        }
    }

    void OnDestroy()
    {
        DragAndDropController.Instance.OnStartDrag -= OnStartDragFigure;
        DragAndDropController.Instance.OnEndDrag -= OnEndDragFigure;
    }

    public void Restart()
    {
        Chess = new Chess();
    }

    void OnStartDragFigure(object source, DragAndDropController.DragArgs args)
    {
        Figure2D figure = args.draggedObject.GetComponent<Figure2D>();
        ShowLegalMoves(figure.Id, figure.XBoardPos, figure.YBoardPos);
    }

    void OnEndDragFigure(object source, DragAndDropController.DragArgs args)
    {
        if (args.result)
        {
            Debug.Log($"End drag ({args.result }): { args.fenMove }");
            Chess = Chess.Move(args.fenMove);
            Board2DBuilder.Instance.UpdateBoard();  
        }

        ShowLegalFigures();
    }
}
