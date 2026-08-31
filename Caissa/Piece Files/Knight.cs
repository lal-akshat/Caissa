namespace Caissa.Piece_Files;

public class Knight : Piece
{
    public Knight(int xPos, int yPos, bool isWhite) : base(xPos, yPos, isWhite)
    {
    }

    public override void Move(int xMove, int yMove)
    {
        XPos = xMove;
        YPos = yMove;
    }

    public override bool IsLegal(int xMove, int yMove, string[,] board)
    {
        // If the new position is out of bounds, return false
        if (xMove < 0 || xMove > 7 || yMove < 0 || yMove > 7)
        {
            return false;
        }

        // Holds whatever value is at the targeted square (w, b, n)
        var target = CheckOccupied(xMove, yMove, board);

        // Cannot capture own piece
        if ((IsWhite && target == 'w') || (!IsWhite && target == 'b'))
        {
            return false;
        }

        // Knight moves: 2 forward / back + 1 up / down
        if ((xMove == XPos + 2 && yMove == YPos + 1) ||
            (xMove == XPos + 2 && yMove == YPos - 1) ||
            (xMove == XPos - 2 && yMove == YPos + 1) ||
            (xMove == XPos - 2 && yMove == YPos - 1) ||

            (xMove == XPos + 1 && yMove == YPos + 2) ||
            (xMove == XPos + 1 && yMove == YPos - 2) ||
            (xMove == XPos - 1 && yMove == YPos + 2) ||
            (xMove == XPos - 1 && yMove == YPos - 2))
        {
            return true;
        }

        // Return illegal if all checks fail
        return false;
    }
}