namespace Caissa.Piece_Files;

public class Queen : Piece
{
    public Queen(int xPos, int yPos, bool isWhite) : base(xPos, yPos, isWhite)
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

        // Checks if the piece is moving upwards
        if (yMove > YPos && xMove == XPos)
        {
            // Checks if there is a piece blocking path
            for (var i = yMove - YPos; i > 1; i--)
            {
                var newTarget = CheckOccupied(XPos, YPos + i - 1, board);
                if (newTarget != 'n')
                {
                    return false;
                }
            }
        }

        // Checks if the piece is moving downwards
        else if (yMove < YPos && xMove == XPos)
        {
            // Checks if there is a piece blocking path
            for (var i = YPos - yMove; i > 1; i--)
            {
                var newTarget = CheckOccupied(XPos, YPos - i + 1, board);
                if (newTarget != 'n')
                {
                    return false;
                }
            }
        }

        // Checks if the piece is moving right
        else if (yMove == YPos && xMove > XPos)
        {
            // Checks if there is a piece blocking path
            for (var i = xMove - XPos; i > 1; i--)
            {
                var newTarget = CheckOccupied(XPos + i - 1, YPos, board);
                if (newTarget != 'n')
                {
                    return false;
                }
            }
        }

        // Checks if the piece is moving left
        else if (yMove == YPos && xMove < XPos)
        {
            // Checks if there is a piece blocking path
            for (var i = XPos - xMove; i > 1; i--)
            {
                var newTarget = CheckOccupied(XPos - i + 1, YPos, board);
                if (newTarget != 'n')
                {
                    return false;
                }
            }
        }

        // Checks the square diagonally right upwards
        else if (yMove > YPos && xMove > XPos && yMove - YPos == xMove - XPos)
        {
            // Checks if there is a piece blocking path
            for (var i = yMove - YPos; i > 1; i--)
            {
                var newTarget = CheckOccupied(XPos + i - 1, YPos + i - 1, board);
                if (newTarget != 'n')
                {
                    return false;
                }
            }
        }

        // Checks the square diagonally left upwards
        else if (yMove < YPos && xMove > XPos && YPos - yMove == xMove - XPos)
        {
            // Checks if there is a piece blocking path
            for (var i = YPos - yMove; i > 1; i--)
            {
                var newTarget = CheckOccupied(XPos + i - 1, YPos - i + 1, board);
                if (newTarget != 'n')
                {
                    return false;
                }
            }
        }

        // Checks the square diagonally right downwards
        else if (yMove > YPos && xMove < XPos && yMove - YPos == XPos - xMove)
        {
            // Checks if there is a piece blocking path
            for (var i = yMove - YPos; i > 1; i--)
            {
                var newTarget = CheckOccupied(XPos - i + 1, YPos + i - 1, board);
                if (newTarget != 'n')
                {
                    return false;
                }
            }
        }

        // Checks the square diagonally left downwards
        else if (yMove < YPos && xMove < XPos && YPos - yMove == XPos - xMove)
        {
            // Checks if there is a piece blocking path
            for (var i = YPos - yMove; i > 1; i--)
            {
                var newTarget = CheckOccupied(XPos - i + 1, YPos - i + 1, board);
                if (newTarget != 'n')
                {
                    return false;
                }
            }
        }

        else
        {
            return false;
        }

        if ((IsWhite && target == 'b') || (!IsWhite && target == 'w') || target == 'n')
        {
            return true;
        }

        // Return illegal if all checks fail
        return false;
    }
}