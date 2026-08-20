namespace Caissa;

public class King : Piece
{
    public King(int xPos, int yPos, bool isWhite) : base(xPos, yPos, isWhite)
    {
    }

    public override void move(int xMove, int yMove)
    {
        xPos = xMove;
        yPos = yMove;
    }

    public override bool isLegal(int xMove, int yMove, string[,] board)
    {
        // If the new position is out of bounds, return false
        if (xMove < 0 || xMove > 7 || yMove < 0 || yMove > 7)
        {
            return false;
        }

        // Holds whatever value is at the targetted square (w, b, n)
        var target = checkOccupied(xMove, yMove, board);

        // Cannot capture own piece
        if ((isWhite && target == 'w') || (!isWhite && target == 'b'))
        {
            return false;
        }

        // Check if the king can move diagonally forward / backward 1
        if ((yMove == yPos + 1 || yMove == yPos - 1) && (xMove == xPos + 1 || xMove == xPos - 1))
        {
            return true;
        }

        // Check if the king can move straight forward / backward 1
        if (yMove == yPos && (xMove == xPos + 1 || xMove == xPos - 1))
        {
            return true;
        }

        // Check if the king can move straight left / right 1
        if (xMove == xPos && (yMove == yPos + 1 || yMove == yPos - 1))
        {
            return true;
        }

        // Return illegal if all checks fail
        return false;
    }
}