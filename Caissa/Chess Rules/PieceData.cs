namespace Caissa.Chess_Engine;

public enum PieceType
{
    None,
    Pawn,
    Knight,
    Bishop,
    Rook,
    Queen,
    King
}

public enum PieceColor
{
    White,
    Black
}

public static class PieceColorExtensions
{
    public static PieceColor Opposite(this PieceColor color)
    {
        return color == PieceColor.White
            ? PieceColor.Black
            : PieceColor.White;
    }
}

public class PieceData
{
    public PieceType Type { get; }
    public PieceColor Color { get; }

    public PieceData(PieceType type, PieceColor color)
    {
        Type = type;
        Color = color;
    }
}