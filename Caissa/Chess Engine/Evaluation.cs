namespace Caissa.Chess_Engine;

public class Evaluation
{
    public static int Evaluate(Board board)
    {
        int score = 0;

        for (int row = 0; row < 8; row++)
        {
            for (int column = 0; column < 8; column++)
            {
                PieceData piece = board.GetPiece(row, column);

                if (piece.Type == PieceType.None)
                {
                    continue;
                }

                int value = GetPieceValue(piece.Type);

                // Add positional value
                value += GetPositionValue(
                    piece.Type,
                    piece.Color,
                    row,
                    column);

                if (piece.Color == PieceColor.White)
                {
                    score += value;
                }
                else
                {
                    score -= value;
                }
            }
        }

        return score;
    }

    // ============================================================
    // MATERIAL VALUES
    // ============================================================

    public static int GetPieceValue(PieceType type)
    {
        return type switch
        {
            PieceType.Pawn => 100,
            PieceType.Knight => 320,
            PieceType.Bishop => 330,
            PieceType.Rook => 500,
            PieceType.Queen => 900,
            PieceType.King => 20000,
            PieceType.None => 0,
            _ => 0
        };
    }

    // ============================================================
    // POSITION VALUE
    // ============================================================

    private static int GetPositionValue(
        PieceType type,
        PieceColor color,
        int row,
        int column)
    {
        // Piece-square tables are written from White's perspective.
        //
        // White starts at the bottom of our board:
        //
        // 0 = black's back rank
        // 7 = white's back rank
        //
        // Therefore we flip the row for Black.

        int tableRow = color == PieceColor.White
            ? row
            : 7 - row;

        return type switch
        {
            PieceType.Pawn =>
                PawnTable[tableRow, column],

            PieceType.Knight =>
                KnightTable[tableRow, column],

            PieceType.Bishop =>
                BishopTable[tableRow, column],

            PieceType.Rook =>
                RookTable[tableRow, column],

            PieceType.Queen =>
                QueenTable[tableRow, column],

            PieceType.King =>
                KingTable[tableRow, column],

            _ => 0
        };
    }

    // ============================================================
    // PAWN
    // ============================================================

    private static readonly int[,] PawnTable =
    {
        {  0,   0,   0,   0,   0,   0,   0,   0 },
        { 50,  50,  50,  50,  50,  50,  50,  50 },
        { 10,  10,  20,  30,  30,  20,  10,  10 },
        {  5,   5,  10,  25,  25,  10,   5,   5 },
        {  0,   0,   0,  20,  20,   0,   0,   0 },
        {  5,  -5, -10,   0,   0, -10,  -5,   5 },
        {  5,  10,  10, -20, -20,  10,  10,   5 },
        {  0,   0,   0,   0,   0,   0,   0,   0 }
    };

    // ============================================================
    // KNIGHT
    // ============================================================

    private static readonly int[,] KnightTable =
    {
        { -50, -40, -30, -30, -30, -30, -40, -50 },
        { -40, -20,   0,   0,   0,   0, -20, -40 },
        { -30,   0,  10,  15,  15,  10,   0, -30 },
        { -30,   5,  15,  20,  20,  15,   5, -30 },
        { -30,   0,  15,  20,  20,  15,   0, -30 },
        { -30,   5,  10,  15,  15,  10,   5, -30 },
        { -40, -20,   0,   5,   5,   0, -20, -40 },
        { -50, -40, -30, -30, -30, -30, -40, -50 }
    };

    // ============================================================
    // BISHOP
    // ============================================================

    private static readonly int[,] BishopTable =
    {
        { -20, -10, -10, -10, -10, -10, -10, -20 },
        { -10,   0,   0,   0,   0,   0,   0, -10 },
        { -10,   0,   5,  10,  10,   5,   0, -10 },
        { -10,   5,   5,  10,  10,   5,   5, -10 },
        { -10,   0,  10,  10,  10,  10,   0, -10 },
        { -10,  10,  10,  10,  10,  10,  10, -10 },
        { -10,   5,   0,   0,   0,   0,   5, -10 },
        { -20, -10, -10, -10, -10, -10, -10, -20 }
    };

    // ============================================================
    // ROOK
    // ============================================================

    private static readonly int[,] RookTable =
    {
        {  0,   0,   0,   5,   5,   0,   0,   0 },
        { -5,   0,   0,   0,   0,   0,   0,  -5 },
        { -5,   0,   0,   0,   0,   0,   0,  -5 },
        { -5,   0,   0,   0,   0,   0,   0,  -5 },
        { -5,   0,   0,   0,   0,   0,   0,  -5 },
        { -5,   0,   0,   0,   0,   0,   0,  -5 },
        {  5,  10,  10,  10,  10,  10,  10,   5 },
        {  0,   0,   0,   0,   0,   0,   0,   0 }
    };

    // ============================================================
    // QUEEN
    // ============================================================

    private static readonly int[,] QueenTable =
    {
        { -20, -10, -10,  -5,  -5, -10, -10, -20 },
        { -10,   0,   0,   0,   0,   0,   0, -10 },
        { -10,   0,   5,   5,   5,   5,   0, -10 },
        {  -5,   0,   5,   5,   5,   5,   0,  -5 },
        {   0,   0,   5,   5,   5,   5,   0,  -5 },
        { -10,   5,   5,   5,   5,   5,   0, -10 },
        { -10,   0,   5,   0,   0,   0,   0, -10 },
        { -20, -10, -10,  -5,  -5, -10, -10, -20 }
    };

    // ============================================================
    // KING
    // ============================================================

    private static readonly int[,] KingTable =
    {
        { -30, -40, -40, -50, -50, -40, -40, -30 },
        { -30, -40, -40, -50, -50, -40, -40, -30 },
        { -30, -40, -40, -50, -50, -40, -40, -30 },
        { -30, -40, -40, -50, -50, -40, -40, -30 },
        { -20, -30, -30, -40, -40, -30, -30, -20 },
        { -10, -20, -20, -20, -20, -20, -20, -10 },
        {  20,  20,   0,   0,   0,   0,  20,  20 },
        {  20,  30,  10,   0,   0,  10,  30,  20 }
    };
}