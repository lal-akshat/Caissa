namespace Caissa.Chess_Rules;

public class Board
{
    // Creates a 2D 8x8 array with elements of type PieceData
    private readonly PieceData[,] _squares = new PieceData[8, 8];

    public Board()
    {
        // Initialize every square as empty
        for (int row = 0; row < 8; row++)
        {
            for (int column = 0; column < 8; column++)
            {
                _squares[row, column] = new PieceData(PieceType.None, PieceColor.None);
            }
        }
        SetupStartingPosition();
    }

    // Method to return the piece on a specific row, column
    public PieceData GetPiece(int row, int column)
    {
        return _squares[row, column];   
    }

    // Method to set a piece on a specific row, column
    public void SetPiece(int row, int column, PieceData piece)
    {
        _squares[row, column] = piece;
    }
    
    // Method to allow a piece to move from its current square to another square
    public void MakeMove(Move move)
    {
        // Get the piece at the starting position
        PieceData piece = _squares[move.FromRow, move.FromColumn];

        // Place that piece at the ending position
        _squares[move.ToRow, move.ToColumn] = piece;

        // Empty the starting position
        _squares[move.FromRow, move.FromColumn] = new PieceData(PieceType.None, PieceColor.None);
    }

    // Method that allows a piece to move back to its original square
    public void UndoMove(Move move)
    {
        // Get the piece at the ending position
        PieceData piece = _squares[move.ToRow, move.ToColumn];

        // Place that piece at the starting position
        _squares[move.FromRow, move.FromColumn] = piece;

        // Replace the ending position with the captured piece if any
        _squares[move.ToRow, move.ToColumn] = move.CapturedPiece;
    }

    // Checks whether the specified king is currently in check
    public bool IsKingInCheck(PieceColor color)
    {
        int kingRow = -1;
        int kingColumn = -1;

        // Find the king by iterating through the board
        for (int row = 0; row < 8; row++)
        {
            for (int column = 0; column < 8; column++)
            {
                PieceData isKing = _squares[row, column];

                // Check whether the current square contains the correct king
                if (isKing.Type == PieceType.King && isKing.Color == color)
                {
                    kingRow = row;
                    kingColumn = column;
                    break;
                }
            }

            // Stop searching once the king is found
            if (kingRow != -1)
            {
                break;
            }
        }

        // Failsafe, should never execute
        if (kingRow == -1)
        {
            return true;
        }

        // If color is white, make enemy color black. Vice Versa
        PieceColor enemyColor = color == PieceColor.White
                ? PieceColor.Black
                : PieceColor.White;

        // Check whether any enemy piece attacks the king
        for (int row = 0; row < 8; row++)
        {
            for (int column = 0; column < 8; column++)
            {
                PieceData piece = _squares[row, column];

                if (piece.Type == PieceType.None)
                {
                    continue;
                }

                if (piece.Color != enemyColor)
                {
                    continue;
                }

                // If a piece is attacking the king, the king is in check
                if (PieceAttacksSquare(row, column, kingRow, kingColumn))
                {
                    return true;
                }
            }
        }
        return false;
    }
    
    // Checks whether a piece can attack a specific square
    private bool PieceAttacksSquare(int fromRow, int fromColumn, int toRow, int toColumn)
    {
        PieceData piece = _squares[fromRow, fromColumn];

        // Figure out the distance between the piece and the specific square
        int rowDifference = toRow - fromRow;

        int columnDifference = toColumn - fromColumn;

        int absRow = Math.Abs(rowDifference);

        int absColumn = Math.Abs(columnDifference);

        switch (piece.Type)
        {
            case PieceType.Pawn:
            {
                // Determine which way the pawn moves
                int direction = piece.Color == PieceColor.White
                        ? -1
                        : 1;

                return rowDifference == direction && absColumn == 1;
            }

            case PieceType.Knight:
            {
                // The knight moves in an L shape pattern
                return (absRow == 2 && absColumn == 1) || (absRow == 1 && absColumn == 2);
            }

            case PieceType.Bishop:
            {
                // The bishop moves diagonally
                if (absRow != absColumn)
                {
                    return false;
                }
                
                // Ensure the bishop isn't blocked by another piece
                return IsPathClear(fromRow, fromColumn, toRow, toColumn);
            }

            case PieceType.Rook:
            {
                // Ensure the rook only moves in straight lines
                if (fromRow != toRow && fromColumn != toColumn)
                {
                    return false;
                }

                // Ensure the rook isn't blocked by another piece
                return IsPathClear(fromRow, fromColumn, toRow, toColumn);
            }

            case PieceType.Queen:
            {
                bool diagonal = absRow == absColumn;

                bool straight = fromRow == toRow || fromColumn == toColumn;

                // Ensure the queen either moved straight or moved diagonally
                if (!diagonal && !straight)
                {
                    return false;
                }

                // Ensure the queen isn't blocked by another piece
                return IsPathClear(fromRow, fromColumn, toRow, toColumn);
            }

            // Ensure the king only moved one square
            case PieceType.King:
            {
                return absRow <= 1 && absColumn <= 1 && (absRow != 0 || absColumn != 0);
            }

            // Return false for all other moves
            default:
                return false;
        }
    }

    // Checks if there are pieces in between the starting and ending squares
    private bool IsPathClear(int fromRow, int fromColumn, int toRow, int toColumn)
    {
        // Determine the direction the piece must move in
        int rowDirection = Math.Sign(toRow - fromRow);
        int columnDirection = Math.Sign(toColumn - fromColumn);

        // Start checking from the square right after the starting square
        int row = fromRow + rowDirection;
        int column = fromColumn + columnDirection;

        while (row != toRow || column != toColumn)
        {
            // If the piece is found, the path is blocked
            if (_squares[row, column].Type != PieceType.None)
            {
                return false;
            }

            // Move to the next square along the path
            row += rowDirection;
            column += columnDirection;
        }
        return true;
    }

    // Set up the chessboards starting position
    private void SetupStartingPosition()
    {
        // Black pieces
        _squares[0, 0] = new PieceData(PieceType.Rook, PieceColor.Black);

        _squares[0, 1] = new PieceData(PieceType.Knight, PieceColor.Black);

        _squares[0, 2] = new PieceData(PieceType.Bishop, PieceColor.Black);

        _squares[0, 3] = new PieceData(PieceType.Queen, PieceColor.Black);

        _squares[0, 4] = new PieceData(PieceType.King, PieceColor.Black);

        _squares[0, 5] = new PieceData(PieceType.Bishop, PieceColor.Black);

        _squares[0, 6] = new PieceData(PieceType.Knight, PieceColor.Black);

        _squares[0, 7] = new PieceData(PieceType.Rook, PieceColor.Black);

        // Black pawns
        for (int column = 0; column < 8; column++)
        {
            _squares[1, column] = new PieceData(PieceType.Pawn, PieceColor.Black);
        }

        // White pieces
        _squares[7, 0] = new PieceData(PieceType.Rook, PieceColor.White);

        _squares[7, 1] = new PieceData(PieceType.Knight, PieceColor.White);

        _squares[7, 2] = new PieceData(PieceType.Bishop, PieceColor.White);

        _squares[7, 3] = new PieceData(PieceType.Queen, PieceColor.White);

        _squares[7, 4] = new PieceData(PieceType.King, PieceColor.White);

        _squares[7, 5] = new PieceData(PieceType.Bishop, PieceColor.White);

        _squares[7, 6] = new PieceData(PieceType.Knight, PieceColor.White);

        _squares[7, 7] = new PieceData(PieceType.Rook, PieceColor.White);

        // White pawns
        for (int column = 0; column < 8; column++)
        {
            _squares[6, column] = new PieceData(PieceType.Pawn, PieceColor.White);
        }
    }
}