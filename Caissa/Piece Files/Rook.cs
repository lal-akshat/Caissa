namespace Caissa;

public class Rook : Piece
{
    public Rook(int xPos, int yPos, bool isWhite) : base(xPos, yPos, isWhite)
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

        // If it moves horizontally and vertically in the same move, return false
        if (yMove != yPos && xMove != xPos)
        {
            return false;
        }
        
        if (yMove > yPos && xMove == xPos)
        {
            for (var i = yMove - yPos; i > 1; i--)
            {
                var newTarget = checkOccupied(xPos, yPos + i - 1, board);
                if (newTarget != 'n')
                {
                    return false;
                }
            }
        }

        else if (yMove < yPos && xMove == xPos)
        {
            for (var i = yPos - yMove; i > 1; i--)
            {
                var newTarget = checkOccupied(xPos, yPos - i + 1, board);
                if (newTarget != 'n')
                {
                    return false;
                }
            }
        }

        else if (yMove == yPos && xMove > xPos)
        {
            for (var i = xMove - xPos; i > 1; i--)
            {
                var newTarget = checkOccupied(xPos + i - 1, yPos, board);
                if (newTarget != 'n')
                {
                    return false;
                }
            }
        }

        else if (yMove == yPos && xMove < xPos)
        {
            for (var i = xPos - xMove; i > 1; i--)
            {
                var newTarget = checkOccupied(xPos - i + 1, yPos, board);
                if (newTarget != 'n')
                {
                    return false;
                }
            }
        }
        
        if ((isWhite && target == 'b') || (!isWhite && target == 'w') || target == 'n')
        {
            return true;
        }

        // Return illegal if all checks fail
        return false;
    }
}