namespace Caissa;

public abstract class Piece
{
    // Each piece has a coordinate position, and is either white or black
    protected bool IsWhite;
    public int XPos, YPos;

    // Constructor for the Piece class
    protected Piece(int xPos, int yPos, bool isWhite)
    {
        this.XPos = xPos;
        this.YPos = yPos;
        this.IsWhite = isWhite;
    }

    // Abstract move method to allow for each piece's custom movement
    public abstract void Move(int xMove, int yMove);

    // Abstract isLegal method to allow for each piece's custom rules
    public abstract bool IsLegal(int xMove, int yMove, string[,] board);

    // checkOccupied method checks if the selected square is occupied
    public char CheckOccupied(int xMove, int yMove, string[,] board)
    {
        // If the selected spot is empty, return 'n' to signal clear
        if (board[xMove, yMove] == ".")
        {
            return 'n';
        }

        // Holds the position the user wants to move to
        var piece = board[xMove, yMove][0];

        if (char.IsLower(piece))
        {
            return 'b'; // If lowercase, piece is black
        }

        if (char.IsUpper(piece))
        {
            return 'w'; // If uppercase, piece is white
        }

        // Return empty if all checks fail
        return 'n';
    }
}