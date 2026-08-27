namespace Caissa;

public class King : Piece
{
    public King(int xPos, int yPos, bool isWhite) : base(xPos, yPos, isWhite)
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

        // Holds whatever value is at the targetted square (w, b, n)
        var target = CheckOccupied(xMove, yMove, board);

        // Cannot capture own piece
        if ((IsWhite && target == 'w') || (!IsWhite && target == 'b'))
        {
            return false;
        }

        // Check if the king can move diagonally forward / backward 1
        if ((yMove == YPos + 1 || yMove == YPos - 1) && (xMove == XPos + 1 || xMove == XPos - 1))
        {
            return true;
        }

        // Check if the king can move straight forward / backward 1
        if (yMove == YPos && (xMove == XPos + 1 || xMove == XPos - 1))
        {
            return true;
        }

        // Check if the king can move straight left / right 1
        if (xMove == XPos && (yMove == YPos + 1 || yMove == YPos - 1))
        {
            return true;
        }

        // Return illegal if all checks fail
        return false;
    }
}