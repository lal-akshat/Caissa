namespace Caissa.Chess_Engine;

public class Move
{
    public int FromRow { get; }
    public int FromColumn { get; }

    public int ToRow { get; }
    public int ToColumn { get; }
    
    public PieceData CapturedPiece { get; }

    public Move(int fromRow, int fromColumn, int toRow, int toColumn, PieceData capturedPiece)
    {
        FromRow = fromRow;
        FromColumn = fromColumn;

        ToRow = toRow;
        ToColumn = toColumn;
        
        CapturedPiece = capturedPiece;
    }
}