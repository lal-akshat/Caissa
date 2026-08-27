namespace Caissa.Chess_Engine;

public class PossibleMoves
{
    public List<Move> GenerateMoves(Board board, PieceColor color)
    {
        List<Move> moves = new List<Move>();

        for (int row = 0; row < 8; row++)
        {
            for (int column = 0; column < 8; column++)
            {
                PieceData piece =
                    board.GetPiece(row, column);

                if (piece.Type == PieceType.None)
                {
                    continue;
                }

                if (piece.Color != color)
                {
                    continue;
                }

                if (piece.Type == PieceType.Pawn)
                {
                    GeneratePawnMoves(board, row, column, color, moves);
                }
                else if (piece.Type == PieceType.Knight)
                {
                    GenerateKnightMoves(board, row, column, color, moves);
                }
                else if (piece.Type == PieceType.Bishop)
                {
                    GenerateBishopMoves(board, row, column, color, moves);
                }
                else if (piece.Type == PieceType.Rook)
                {
                    GenerateRookMoves(board, row, column, color, moves);
                }
                else if (piece.Type == PieceType.Queen)
                {
                    GenerateQueenMoves(board, row, column, color, moves);
                }
                else if (piece.Type == PieceType.King)
                {
                    GenerateKingMoves(board, row, column, color, moves);
                }
            }
        }
        return FilterLegalMoves(board, color, moves);
    }

    // ============================================================
    // LEGAL MOVE FILTERING
    // ============================================================

    private List<Move> FilterLegalMoves(Board board, PieceColor color, List<Move> moves)
    {
        List<Move> legalMoves = new List<Move>();

        foreach (Move move in moves)
        {
            board.MakeMove(move);

            bool kingInCheck = board.IsKingInCheck(color);

            board.UndoMove(move);

            if (!kingInCheck)
            {
                legalMoves.Add(move);
            }
        }
        return legalMoves;
    }

    // ============================================================
    // PAWN
    // ============================================================

    private void GeneratePawnMoves(Board board, int xPos, int yPos, PieceColor color, List<Move> moves)
    {
        int direction;

        if (color == PieceColor.White)
        {
            direction = -1;
        }
        else
        {
            direction = 1;
        }

        int xMove = xPos + direction;

        // Move forward
        if (xMove >= 0 && xMove <= 7)
        {
            PieceData target = board.GetPiece(xMove, yPos);

            if (target.Type == PieceType.None)
            {
                moves.Add(CreateMove(board, xPos, yPos, xMove, yPos));

                // Move forward two squares
                if ((color == PieceColor.White && xPos == 6) || (color == PieceColor.Black && xPos == 1))
                {
                    int twoSquares =
                        xPos + 2 * direction;

                    PieceData between =
                        board.GetPiece(
                            xPos + direction,
                            yPos);

                    PieceData targetTwo =
                        board.GetPiece(
                            twoSquares,
                            yPos);

                    if (between.Type == PieceType.None &&
                        targetTwo.Type == PieceType.None)
                    {
                        moves.Add(
                            CreateMove(
                                board,
                                xPos,
                                yPos,
                                twoSquares,
                                yPos));
                    }
                }
            }
        }

        // Diagonal captures
        int[] captureColumns =
        {
            yPos - 1,
            yPos + 1
        };

        foreach (int captureColumn in captureColumns)
        {
            if (xMove < 0 ||
                xMove > 7 ||
                captureColumn < 0 ||
                captureColumn > 7)
            {
                continue;
            }

            PieceData target =
                board.GetPiece(
                    xMove,
                    captureColumn);

            if (target.Type != PieceType.None &&
                target.Color != color)
            {
                moves.Add(
                    CreateMove(
                        board,
                        xPos,
                        yPos,
                        xMove,
                        captureColumn));
            }
        }
    }

    // ============================================================
    // KNIGHT
    // ============================================================

    private void GenerateKnightMoves(
        Board board,
        int xPos,
        int yPos,
        PieceColor color,
        List<Move> moves)
    {
        int[,] offsets =
        {
            { 2, 1 },
            { 2, -1 },
            { -2, 1 },
            { -2, -1 },
            { 1, 2 },
            { 1, -2 },
            { -1, 2 },
            { -1, -2 }
        };

        for (int i = 0; i < offsets.GetLength(0); i++)
        {
            int xMove =
                xPos + offsets[i, 0];

            int yMove =
                yPos + offsets[i, 1];

            if (IsLegalKnightMove(
                    board,
                    xMove,
                    yMove,
                    color))
            {
                moves.Add(
                    CreateMove(
                        board,
                        xPos,
                        yPos,
                        xMove,
                        yMove));
            }
        }
    }

    private bool IsLegalKnightMove(
        Board board,
        int xMove,
        int yMove,
        PieceColor color)
    {
        if (xMove < 0 ||
            xMove > 7 ||
            yMove < 0 ||
            yMove > 7)
        {
            return false;
        }

        PieceData target =
            board.GetPiece(
                xMove,
                yMove);

        if (target.Type != PieceType.None &&
            target.Color == color)
        {
            return false;
        }

        return true;
    }

    // ============================================================
    // BISHOP
    // ============================================================

    private void GenerateBishopMoves(
        Board board,
        int xPos,
        int yPos,
        PieceColor color,
        List<Move> moves)
    {
        GenerateSlidingMoves(
            board,
            xPos,
            yPos,
            color,
            moves,
            1,
            1);

        GenerateSlidingMoves(
            board,
            xPos,
            yPos,
            color,
            moves,
            1,
            -1);

        GenerateSlidingMoves(
            board,
            xPos,
            yPos,
            color,
            moves,
            -1,
            1);

        GenerateSlidingMoves(
            board,
            xPos,
            yPos,
            color,
            moves,
            -1,
            -1);
    }

    // ============================================================
    // ROOK
    // ============================================================

    private void GenerateRookMoves(
        Board board,
        int xPos,
        int yPos,
        PieceColor color,
        List<Move> moves)
    {
        GenerateSlidingMoves(
            board,
            xPos,
            yPos,
            color,
            moves,
            1,
            0);

        GenerateSlidingMoves(
            board,
            xPos,
            yPos,
            color,
            moves,
            -1,
            0);

        GenerateSlidingMoves(
            board,
            xPos,
            yPos,
            color,
            moves,
            0,
            1);

        GenerateSlidingMoves(
            board,
            xPos,
            yPos,
            color,
            moves,
            0,
            -1);
    }

    // ============================================================
    // QUEEN
    // ============================================================

    private void GenerateQueenMoves(
        Board board,
        int xPos,
        int yPos,
        PieceColor color,
        List<Move> moves)
    {
        GenerateRookMoves(
            board,
            xPos,
            yPos,
            color,
            moves);

        GenerateBishopMoves(
            board,
            xPos,
            yPos,
            color,
            moves);
    }

    // ============================================================
    // KING
    // ============================================================

    private void GenerateKingMoves(
        Board board,
        int xPos,
        int yPos,
        PieceColor color,
        List<Move> moves)
    {
        for (int rowOffset = -1;
             rowOffset <= 1;
             rowOffset++)
        {
            for (int columnOffset = -1;
                 columnOffset <= 1;
                 columnOffset++)
            {
                if (rowOffset == 0 &&
                    columnOffset == 0)
                {
                    continue;
                }

                int xMove =
                    xPos + rowOffset;

                int yMove =
                    yPos + columnOffset;

                if (IsLegalKingMove(
                        board,
                        xMove,
                        yMove,
                        color))
                {
                    moves.Add(
                        CreateMove(
                            board,
                            xPos,
                            yPos,
                            xMove,
                            yMove));
                }
            }
        }
    }

    private bool IsLegalKingMove(
        Board board,
        int xMove,
        int yMove,
        PieceColor color)
    {
        if (xMove < 0 ||
            xMove > 7 ||
            yMove < 0 ||
            yMove > 7)
        {
            return false;
        }

        PieceData target =
            board.GetPiece(
                xMove,
                yMove);

        if (target.Type != PieceType.None &&
            target.Color == color)
        {
            return false;
        }

        return true;
    }

    // ============================================================
    // SLIDING PIECES
    // ============================================================

    private void GenerateSlidingMoves(
        Board board,
        int xPos,
        int yPos,
        PieceColor color,
        List<Move> moves,
        int rowDirection,
        int columnDirection)
    {
        for (int distance = 1;
             distance < 8;
             distance++)
        {
            int xMove =
                xPos + rowDirection * distance;

            int yMove =
                yPos + columnDirection * distance;

            if (xMove < 0 ||
                xMove > 7 ||
                yMove < 0 ||
                yMove > 7)
            {
                break;
            }

            PieceData target =
                board.GetPiece(
                    xMove,
                    yMove);

            if (target.Type != PieceType.None)
            {
                if (target.Color != color)
                {
                    moves.Add(
                        CreateMove(
                            board,
                            xPos,
                            yPos,
                            xMove,
                            yMove));
                }

                break;
            }

            moves.Add(
                CreateMove(
                    board,
                    xPos,
                    yPos,
                    xMove,
                    yMove));
        }
    }

    // ============================================================
    // MOVE CREATION
    // ============================================================

    private Move CreateMove(
        Board board,
        int fromRow,
        int fromColumn,
        int toRow,
        int toColumn)
    {
        PieceData capturedPiece =
            board.GetPiece(
                toRow,
                toColumn);

        return new Move(
            fromRow,
            fromColumn,
            toRow,
            toColumn,
            capturedPiece);
    }
}