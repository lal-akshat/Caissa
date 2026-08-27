namespace Caissa;

// An enum to contain the possible pieces on a square
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

// An enum to contain the possible colors a square can be
public enum PieceColor
{
    None,
    White,
    Black
}

// A class that contains a method to determine the opposite color of a piece
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