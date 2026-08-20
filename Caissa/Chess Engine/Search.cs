namespace Caissa.Chess_Engine;

public class Search
{
    private readonly PossibleMoves moveGenerator = new PossibleMoves();

    public Move? FindBestMove(Board board, PieceColor color, int depth)
    {
        List<Move> moves =
            moveGenerator.GenerateMoves(board, color);

        if (moves.Count == 0)
        {
            return null;
        }

        OrderMoves(moves);

        Move? bestMove = null;

        int bestScore = color == PieceColor.White
            ? int.MinValue
            : int.MaxValue;

        foreach (Move move in moves)
        {
            board.MakeMove(move);

            int score = Minimax(
                board,
                depth - 1,
                color.Opposite(),
                int.MinValue,
                int.MaxValue);

            board.UndoMove(move);

            if (color == PieceColor.White)
            {
                if (score > bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }
            else
            {
                if (score < bestScore)
                {
                    bestScore = score;
                    bestMove = move;
                }
            }
        }

        return bestMove;
    }

    // ============================================================
    // MINIMAX + ALPHA-BETA
    // ============================================================

    private int Minimax(
        Board board,
        int depth,
        PieceColor color,
        int alpha,
        int beta)
    {
        if (depth == 0)
        {
            return Quiescence(
                board,
                color,
                alpha,
                beta);
        }

        List<Move> moves =
            moveGenerator.GenerateMoves(board, color);

        if (moves.Count == 0)
        {
            return Evaluation.Evaluate(board);
        }

        OrderMoves(moves);

        // ========================================================
        // WHITE — MAXIMIZING
        // ========================================================

        if (color == PieceColor.White)
        {
            int bestScore = int.MinValue;

            foreach (Move move in moves)
            {
                board.MakeMove(move);

                int score = Minimax(
                    board,
                    depth - 1,
                    color.Opposite(),
                    alpha,
                    beta);

                board.UndoMove(move);

                if (score > bestScore)
                {
                    bestScore = score;
                }

                if (bestScore > alpha)
                {
                    alpha = bestScore;
                }

                if (beta <= alpha)
                {
                    break;
                }
            }

            return bestScore;
        }

        // ========================================================
        // BLACK — MINIMIZING
        // ========================================================

        int blackBestScore = int.MaxValue;

        foreach (Move move in moves)
        {
            board.MakeMove(move);

            int score = Minimax(
                board,
                depth - 1,
                color.Opposite(),
                alpha,
                beta);

            board.UndoMove(move);

            if (score < blackBestScore)
            {
                blackBestScore = score;
            }

            if (blackBestScore < beta)
            {
                beta = blackBestScore;
            }

            if (beta <= alpha)
            {
                break;
            }
        }

        return blackBestScore;
    }

    // ============================================================
    // QUIESCENCE SEARCH
    // ============================================================

    private int Quiescence(
        Board board,
        PieceColor color,
        int alpha,
        int beta)
    {
        int standPat = Evaluation.Evaluate(board);

        // ========================================================
        // WHITE — MAXIMIZING
        // ========================================================

        if (color == PieceColor.White)
        {
            if (standPat >= beta)
            {
                return beta;
            }

            if (standPat > alpha)
            {
                alpha = standPat;
            }
        }

        // ========================================================
        // BLACK — MINIMIZING
        // ========================================================

        else
        {
            if (standPat <= alpha)
            {
                return alpha;
            }

            if (standPat < beta)
            {
                beta = standPat;
            }
        }

        // Generate all legal moves.
        List<Move> moves =
            moveGenerator.GenerateMoves(board, color);

        // Only investigate captures.
        List<Move> captures = new List<Move>();

        foreach (Move move in moves)
        {
            if (move.CapturedPiece.Type != PieceType.None)
            {
                captures.Add(move);
            }
        }

        OrderMoves(captures);

        // ========================================================
        // SEARCH CAPTURES
        // ========================================================

        foreach (Move move in captures)
        {
            board.MakeMove(move);

            int score = Quiescence(
                board,
                color.Opposite(),
                alpha,
                beta);

            board.UndoMove(move);

            if (color == PieceColor.White)
            {
                if (score > alpha)
                {
                    alpha = score;
                }

                if (alpha >= beta)
                {
                    break;
                }
            }
            else
            {
                if (score < beta)
                {
                    beta = score;
                }

                if (beta <= alpha)
                {
                    break;
                }
            }
        }

        return color == PieceColor.White
            ? alpha
            : beta;
    }

    // ============================================================
    // MOVE ORDERING
    // ============================================================

    private void OrderMoves(List<Move> moves)
    {
        moves.Sort((a, b) =>
        {
            int scoreA = GetMoveScore(a);
            int scoreB = GetMoveScore(b);

            return scoreB.CompareTo(scoreA);
        });
    }

    private int GetMoveScore(Move move)
    {
        int score = 0;

        if (move.CapturedPiece.Type != PieceType.None)
        {
            score += 1000;

            score += Evaluation.GetPieceValue(
                move.CapturedPiece.Type);
        }

        return score;
    }
}