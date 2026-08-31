namespace Caissa.Chess_Rules;

// Stores information about a single chess move
public class Move
{
    // Starting position, read only
    public int FromRow { get; }
    public int FromColumn { get; }

    // Ending position, read only
    public int ToRow { get; }
    public int ToColumn { get; }
    
    // Any captured pieces, read only
    public PieceData CapturedPiece { get; }

    // Creates a move with all the above information
    public Move(int fromRow, int fromColumn, int toRow, int toColumn, PieceData capturedPiece)
    {
        FromRow = fromRow;
        FromColumn = fromColumn;

        ToRow = toRow;
        ToColumn = toColumn;
        
        CapturedPiece = capturedPiece;
    }
}