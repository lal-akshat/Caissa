namespace Caissa;

public class Knight : Piece
{
    public Knight(int xPos, int yPos, bool isWhite) : base(xPos, yPos, isWhite)
    {
    }

    public override void move(int xMove, int yMove)
    {
        this.xPos = xMove;
        this.yPos = yMove;
    }

    public override bool isLegal(int xMove, int yMove, string[,] board)
    {
        // If the new position is out of bounds, return false
        if (xMove < 0 || xMove > 7 || yMove < 0 || yMove > 7)
        {
            return false;
        }

        // Holds whatever value is at the targeted square (w, b, n)
        var target = checkOccupied(xMove, yMove, board);

        // Cannot capture own piece
        if ((isWhite && target == 'w') || (!isWhite && target == 'b'))
        {
            return false;
        }

        // Knight moves: 2 forward / back + 1 up / down
        if ((xMove == xPos + 2 && yMove == yPos + 1) ||
            (xMove == xPos + 2 && yMove == yPos - 1) ||
            (xMove == xPos - 2 && yMove == yPos + 1) ||
            (xMove == xPos - 2 && yMove == yPos - 1) ||

            (xMove == xPos + 1 && yMove == yPos + 2) ||
            (xMove == xPos + 1 && yMove == yPos - 2) ||
            (xMove == xPos - 1 && yMove == yPos + 2) ||
            (xMove == xPos - 1 && yMove == yPos - 2))
        {
            return true;
        }

        // Return illegal if all checks fail
        return false;
    }
}