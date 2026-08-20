namespace Caissa.Chess_Engine;

public class Board
{
    private readonly PieceData[,] squares = new PieceData[8, 8];

    public Board()
    {
        // Initialize every square as empty
        for (int row = 0; row < 8; row++)
        {
            for (int column = 0; column < 8; column++)
            {
                squares[row, column] =
                    new PieceData(PieceType.None, PieceColor.White);
            }
        }

        SetupStartingPosition();
    }

    public PieceData GetPiece(int row, int column)
    {
        return squares[row, column];
    }

    public void SetPiece(int row, int column, PieceData piece)
    {
        squares[row, column] = piece;
    }

    // ============================================================
    // MAKE MOVE
    // ============================================================

    public void MakeMove(Move move)
    {
        PieceData piece =
            squares[move.FromRow, move.FromColumn];

        squares[move.ToRow, move.ToColumn] = piece;

        squares[move.FromRow, move.FromColumn] =
            new PieceData(
                PieceType.None,
                PieceColor.White);
    }

    // ============================================================
    // UNDO MOVE
    // ============================================================

    public void UndoMove(Move move)
    {
        PieceData piece =
            squares[move.ToRow, move.ToColumn];

        squares[move.FromRow, move.FromColumn] = piece;

        squares[move.ToRow, move.ToColumn] =
            move.CapturedPiece;
    }

    // ============================================================
    // CHECK DETECTION
    // ============================================================

    public bool IsKingInCheck(PieceColor color)
    {
        int kingRow = -1;
        int kingColumn = -1;

        // Find the king
        for (int row = 0; row < 8; row++)
        {
            for (int column = 0; column < 8; column++)
            {
                PieceData piece =
                    squares[row, column];

                if (piece.Type == PieceType.King &&
                    piece.Color == color)
                {
                    kingRow = row;
                    kingColumn = column;
                    break;
                }
            }

            if (kingRow != -1)
            {
                break;
            }
        }

        // No king found
        if (kingRow == -1)
        {
            return true;
        }

        PieceColor enemyColor =
            color == PieceColor.White
                ? PieceColor.Black
                : PieceColor.White;

        // Check every enemy piece
        for (int row = 0; row < 8; row++)
        {
            for (int column = 0; column < 8; column++)
            {
                PieceData piece =
                    squares[row, column];

                if (piece.Type == PieceType.None)
                {
                    continue;
                }

                if (piece.Color != enemyColor)
                {
                    continue;
                }

                if (PieceAttacksSquare(
                        row,
                        column,
                        kingRow,
                        kingColumn))
                {
                    return true;
                }
            }
        }

        return false;
    }

    // ============================================================
    // PIECE ATTACK DETECTION
    // ============================================================

    private bool PieceAttacksSquare(
        int fromRow,
        int fromColumn,
        int toRow,
        int toColumn)
    {
        PieceData piece =
            squares[fromRow, fromColumn];

        int rowDifference =
            toRow - fromRow;

        int columnDifference =
            toColumn - fromColumn;

        int absRow =
            Math.Abs(rowDifference);

        int absColumn =
            Math.Abs(columnDifference);

        switch (piece.Type)
        {
            case PieceType.Pawn:
            {
                int direction =
                    piece.Color == PieceColor.White
                        ? -1
                        : 1;

                return rowDifference == direction &&
                       absColumn == 1;
            }

            case PieceType.Knight:
            {
                return
                    (absRow == 2 && absColumn == 1) ||
                    (absRow == 1 && absColumn == 2);
            }

            case PieceType.Bishop:
            {
                if (absRow != absColumn)
                {
                    return false;
                }

                return IsPathClear(
                    fromRow,
                    fromColumn,
                    toRow,
                    toColumn);
            }

            case PieceType.Rook:
            {
                if (fromRow != toRow &&
                    fromColumn != toColumn)
                {
                    return false;
                }

                return IsPathClear(
                    fromRow,
                    fromColumn,
                    toRow,
                    toColumn);
            }

            case PieceType.Queen:
            {
                bool diagonal =
                    absRow == absColumn;

                bool straight =
                    fromRow == toRow ||
                    fromColumn == toColumn;

                if (!diagonal && !straight)
                {
                    return false;
                }

                return IsPathClear(
                    fromRow,
                    fromColumn,
                    toRow,
                    toColumn);
            }

            case PieceType.King:
            {
                return absRow <= 1 &&
                       absColumn <= 1 &&
                       (absRow != 0 || absColumn != 0);
            }

            default:
                return false;
        }
    }

    // ============================================================
    // PATH CHECK
    // ============================================================

    private bool IsPathClear(
        int fromRow,
        int fromColumn,
        int toRow,
        int toColumn)
    {
        int rowDirection =
            Math.Sign(toRow - fromRow);

        int columnDirection =
            Math.Sign(toColumn - fromColumn);

        int row =
            fromRow + rowDirection;

        int column =
            fromColumn + columnDirection;

        while (row != toRow ||
               column != toColumn)
        {
            if (squares[row, column].Type != PieceType.None)
            {
                return false;
            }

            row += rowDirection;
            column += columnDirection;
        }

        return true;
    }

    // ============================================================
    // STARTING POSITION
    // ============================================================

    private void SetupStartingPosition()
    {
        // Black pieces
        squares[0, 0] =
            new PieceData(PieceType.Rook, PieceColor.Black);

        squares[0, 1] =
            new PieceData(PieceType.Knight, PieceColor.Black);

        squares[0, 2] =
            new PieceData(PieceType.Bishop, PieceColor.Black);

        squares[0, 3] =
            new PieceData(PieceType.Queen, PieceColor.Black);

        squares[0, 4] =
            new PieceData(PieceType.King, PieceColor.Black);

        squares[0, 5] =
            new PieceData(PieceType.Bishop, PieceColor.Black);

        squares[0, 6] =
            new PieceData(PieceType.Knight, PieceColor.Black);

        squares[0, 7] =
            new PieceData(PieceType.Rook, PieceColor.Black);

        // Black pawns
        for (int column = 0; column < 8; column++)
        {
            squares[1, column] =
                new PieceData(
                    PieceType.Pawn,
                    PieceColor.Black);
        }

        // White pieces
        squares[7, 0] =
            new PieceData(PieceType.Rook, PieceColor.White);

        squares[7, 1] =
            new PieceData(PieceType.Knight, PieceColor.White);

        squares[7, 2] =
            new PieceData(PieceType.Bishop, PieceColor.White);

        squares[7, 3] =
            new PieceData(PieceType.Queen, PieceColor.White);

        squares[7, 4] =
            new PieceData(PieceType.King, PieceColor.White);

        squares[7, 5] =
            new PieceData(PieceType.Bishop, PieceColor.White);

        squares[7, 6] =
            new PieceData(PieceType.Knight, PieceColor.White);

        squares[7, 7] =
            new PieceData(PieceType.Rook, PieceColor.White);

        // White pawns
        for (int column = 0; column < 8; column++)
        {
            squares[6, column] =
                new PieceData(
                    PieceType.Pawn,
                    PieceColor.White);
        }
    }
}