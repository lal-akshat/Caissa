namespace Caissa.Piece_Files;

public class Pawn : Piece
{
    public Pawn(int xPos, int yPos, bool isWhite) : base(xPos, yPos, isWhite)
    {
    }

    public override void Move(int xMove, int yMove)
    {
        XPos = xMove;
        YPos = yMove;
    }

    public override bool IsLegal(int xMove, int yMove, string[,] board)
    {
        // Allows us to differ the movement between white and black pawns
        int direction;
        if (IsWhite)
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

        // Holds whatever value is at the targeted square (w, b, n)
        var target = CheckOccupied(xMove, yMove, board);

        // Cannot capture own piece
        if ((IsWhite && target == 'w') || (!IsWhite && target == 'b'))
        {
            return false;
        }

        // Check if the pawn can move forward 1
        if (yMove == YPos && xMove == XPos + direction)
        {
            // If the targeted square is empty
            if (target == 'n')
            {
                return true;
            }
        }

        // Check if the pawn can move forward 2
        if (yMove == YPos && xMove == XPos + 2 * direction)
        {
            // If the pawn is on its starting square
            if ((IsWhite && XPos == 6) || (!IsWhite && XPos == 1))
            {
                // If the targeted and in between squares are empty
                if (board[XPos + direction, YPos] == "." && target == 'n')
                {
                    return true;
                }
            }
        }

        // Check if the pawn can move diagonally to capture a piece
        if ((yMove == YPos + 1 || yMove == YPos - 1) && xMove == XPos + direction)
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