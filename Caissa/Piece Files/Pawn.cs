namespace Caissa;

public class Pawn : Piece
{
    public Pawn(int xPos, int yPos, bool isWhite) : base(xPos, yPos, isWhite)
    {
    }

    public override void move(int xMove, int yMove)
    {
        xPos = xMove;
        yPos = yMove;
    }

    public override bool isLegal(int xMove, int yMove, string[,] board)
    {
        // Allows us to differ the movement between white and black pawns
        int direction;
        if (isWhite)
        {
            direction = -1;
        }
        else
        {
            direction = 1;
        }

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

        // Check if the pawn can move forward 1
        if (yMove == yPos && xMove == xPos + direction)
        {
            // If the targetted square is empty
            if (target == 'n')
            {
                return true;
            }
        }

        // Check if the pawn can move forward 2
        if (yMove == yPos && xMove == xPos + 2 * direction)
        {
            // If the pawn is on its starting square
            if ((isWhite && xPos == 6) || (!isWhite && xPos == 1))
            {
                // If the targetted and in between squares are empty
                if (board[xPos + direction, yPos] == "." && target == 'n')
                {
                    return true;
                }
            }
        }

        // Check if the pawn can move diagonally to capture a piece
        if ((yMove == yPos + 1 || yMove == yPos - 1) && xMove == xPos + direction)
        {
            if (target != 'n')
            {
                return true;
            }
        }

        // Return illegal if all checks fail
        return false;
    }
}