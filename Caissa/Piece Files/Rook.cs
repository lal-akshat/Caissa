namespace Caissa;

public class Rook : Piece
{
    public Rook(int xPos, int yPos, bool isWhite) : base(xPos, yPos, isWhite)
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

        // If it moves horizontally and vertically in the same move, return false
        if (yMove != YPos && xMove != XPos)
        {
            return false;
        }
        
        if (yMove > YPos && xMove == XPos)
        {
            for (var i = yMove - YPos; i > 1; i--)
            {
                var newTarget = CheckOccupied(XPos, YPos + i - 1, board);
                if (newTarget != 'n')
                {
                    return false;
                }
            }
        }

        else if (yMove < YPos && xMove == XPos)
        {
            for (var i = YPos - yMove; i > 1; i--)
            {
                var newTarget = CheckOccupied(XPos, YPos - i + 1, board);
                if (newTarget != 'n')
                {
                    return false;
                }
            }
        }

        else if (yMove == YPos && xMove > XPos)
        {
            for (var i = xMove - XPos; i > 1; i--)
            {
                var newTarget = CheckOccupied(XPos + i - 1, YPos, board);
                if (newTarget != 'n')
                {
                    return false;
                }
            }
        }

        else if (yMove == YPos && xMove < XPos)
        {
            for (var i = XPos - xMove; i > 1; i--)
            {
                var newTarget = CheckOccupied(XPos - i + 1, YPos, board);
                if (newTarget != 'n')
                {
                    return false;
                }
            }
        }
        
        if ((IsWhite && target == 'b') || (!IsWhite && target == 'w') || target == 'n')
        {
            return true;
        }

        // Return illegal if all checks fail
        return false;
    }
}