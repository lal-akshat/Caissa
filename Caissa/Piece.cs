namespace Caissa;

public abstract class Piece
{
    // Each piece has a coordinate position, and is either white or black
    public bool isWhite;
    public int xPos, yPos;

    // Constructor for the Piece class
    public Piece(int xPos, int yPos, bool isWhite)
    {
        this.xPos = xPos;
        this.yPos = yPos;
        this.isWhite = isWhite;
    }

    // Abstract move method to allow for each piece's custom movement
    public abstract void move(int xMove, int yMove);

    // Abstract isLegal method to allow for each piece's custom rules
    public abstract bool isLegal(int xMove, int yMove, string[,] board);

    // checkOccupied method checks if the selected square is occupied
    public char checkOccupied(int xMove, int yMove, string[,] board)
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