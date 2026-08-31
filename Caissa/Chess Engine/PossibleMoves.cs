using Caissa.Chess_Rules;

namespace Caissa.Chess_Engine;

public class PossibleMoves
{
    // Generates all the possible moves for a specified color
    public List<Move> GenerateMoves(Board board, PieceColor color)
    {
        // A list to store all possible moves
        List<Move> moves = new List<Move>();

        for (int row = 0; row < 8; row++)
        {
            for (int column = 0; column < 8; column++)
            {
                PieceData piece = board.GetPiece(row, column);

                // If the square is empty, no possible move
                if (piece.Type == PieceType.None)
                {
                    continue;
                }

                // If it is the wrong color, no possible move
                if (piece.Color != color)
                {
                    continue;
                }

                // Generate all possible moves depending on the piece type
                if (piece.Type == PieceType.Pawn)
                {
                    GeneratePawnMoves(board, row, column, color, moves);
                }
                else if (piece.Type == PieceType.Knight)
                {
                    GenerateKnightMoves(board, row, column, color, moves);
                }
                else if (piece.Type == PieceType.Bishop)
                {
                    GenerateBishopMoves(board, row, column, color, moves);
                }
                else if (piece.Type == PieceType.Rook)
                {
                    GenerateRookMoves(board, row, column, color, moves);
                }
                else if (piece.Type == PieceType.Queen)
                {
                    GenerateQueenMoves(board, row, column, color, moves);
                }
                else if (piece.Type == PieceType.King)
                {
                    GenerateKingMoves(board, row, column, color, moves);
                }
            }
        }

        // Remove any moves that endanger the king
        return FilterLegalMoves(board, color, moves);
    }

    // Removes all moves that leave the king under check
    private List<Move> FilterLegalMoves(Board board, PieceColor color, List<Move> moves)
    {
        List<Move> legalMoves = new List<Move>();

        // Iterate through all the moves
        foreach (Move move in moves)
        {
            // Simulate the move
            board.MakeMove(move);

            // Check the king's status
            bool kingInCheck = board.IsKingInCheck(color);

            // Undo the move
            board.UndoMove(move);

            // If the king was safe, it's a legal move
            if (!kingInCheck)
            {
                legalMoves.Add(move);
            }
        }
        return legalMoves;
    }

    // Generate all possible pawn moves
    private void GeneratePawnMoves(Board board, int xPos, int yPos, PieceColor color, List<Move> moves)
    {
        int direction;

        // Determine the direction the pawn is moving in
        if (color == PieceColor.White)
        {
            direction = -1;
        }
        else
        {
            direction = 1;
        }

        // Calculate the square in front of the pawn
        int xMove = xPos + direction;

        // Move forward
        if (xMove >= 0 && xMove <= 7)
        {
            PieceData target = board.GetPiece(xMove, yPos);

            // A pawn can only move forward if the square is empty
            if (target.Type == PieceType.None)
            {
                moves.Add(CreateMove(board, xPos, yPos, xMove, yPos));

                // Move forward two squares
                if ((color == PieceColor.White && xPos == 6) || (color == PieceColor.Black && xPos == 1))
                {
                    int twoSquares = xPos + 2 * direction;

                    PieceData between = board.GetPiece(xPos + direction, yPos);

                    PieceData targetTwo = board.GetPiece(twoSquares, yPos);

                    // Ensure the target square and the square between are empty
                    if (between.Type == PieceType.None && targetTwo.Type == PieceType.None)
                    {
                        moves.Add(CreateMove(board, xPos, yPos, twoSquares, yPos));
                    }
                }
            }
        }

        // Calculate the possible columns for diagonal captures
        int[] captureColumns =
        {
            yPos - 1,
            yPos + 1
        };

        // Check both squares
        foreach (int captureColumn in captureColumns)
        {
            // Ensure they are inside the board
            if (xMove < 0 || xMove > 7 || captureColumn < 0 || captureColumn > 7)
            {
                continue;
            }

            // Get the piece on the target square
            PieceData target = board.GetPiece(xMove, captureColumn);

            // If the piece is the opposite color, add the move
            if (target.Type != PieceType.None && target.Color != color)
            {
                moves.Add(CreateMove(board, xPos, yPos, xMove, captureColumn));
            }
        }
    }

    // Generate all possible knight moves
    private void GenerateKnightMoves(Board board, int xPos, int yPos, PieceColor color, List<Move> moves)
    {
        // All possible 'L' shapes on a board
        int[,] offsets =
        {
            { 2, 1 },
            { 2, -1 },
            { -2, 1 },
            { -2, -1 },
            { 1, 2 },
            { 1, -2 },
            { -1, 2 },
            { -1, -2 }
        };

        // Check all eight possible moves
        for (int i = 0; i < offsets.GetLength(0); i++)
        {
            // Calculate the target row
            int xMove = xPos + offsets[i, 0];
            
            // Calculate the target column
            int yMove = yPos + offsets[i, 1];

            // Ensure the move is legal
            if (IsLegalKnightMove(board, xMove, yMove, color))
            {
                moves.Add(CreateMove(board, xPos, yPos, xMove, yMove));
            }
        }
    }

    // Checks if a specified knight can move to a specified square
    private bool IsLegalKnightMove(Board board, int xMove, int yMove, PieceColor color)
    {
        // Ensure the move is inside the board
        if (xMove < 0 || xMove > 7 || yMove < 0 || yMove > 7)
        {
            return false;
        }

        PieceData target = board.GetPiece(xMove, yMove);

        // Ensure the target piece is the opposite color
        if (target.Type != PieceType.None && target.Color == color)
        {
            return false;
        }
        return true;
    }
    
    // Generate all possible bishop moves
    private void GenerateBishopMoves(Board board, int xPos, int yPos, PieceColor color, List<Move> moves)
    {
        // Down right diagonal
        GenerateSlidingMoves(board, xPos, yPos, color, moves, 1, 1);
        
        // Down left diagonal
        GenerateSlidingMoves(board, xPos, yPos, color, moves, 1, -1);
        
        // Up right diagonal
        GenerateSlidingMoves(board, xPos, yPos, color, moves, -1, 1);
        
        // Up left diagonal
        GenerateSlidingMoves(board, xPos, yPos, color, moves, -1, -1);
    }

    // Generate all possible rook moves
    private void GenerateRookMoves(Board board, int xPos, int yPos, PieceColor color, List<Move> moves)
    {
        // Down straight
        GenerateSlidingMoves(board, xPos, yPos, color, moves, 1, 0);
        
        // Up straight
        GenerateSlidingMoves(board, xPos, yPos, color, moves, -1, 0);
        
        // Right straight
        GenerateSlidingMoves(board, xPos, yPos, color, moves, 0, 1);
        
        // Left straight
        GenerateSlidingMoves(board, xPos, yPos, color, moves, 0, -1);
    }

    // Generate all possible queen moves
    private void GenerateQueenMoves(Board board, int xPos, int yPos, PieceColor color, List<Move> moves)
    {
        // A queen moves like a rook + bishop
        GenerateRookMoves(board, xPos, yPos, color, moves);
        GenerateBishopMoves(board, xPos, yPos, color, moves);
    }

    // Generate all possible king moves
    private void GenerateKingMoves(Board board, int xPos, int yPos, PieceColor color, List<Move> moves)
    {
        // Iterate through the 3x3 square the king can move in
        for (int rowOffset = -1; rowOffset <= 1; rowOffset++)
        {
            for (int columnOffset = -1; columnOffset <= 1; columnOffset++)
            {
                // King can't move to the square it is already on
                if (rowOffset == 0 && columnOffset == 0)
                {
                    continue;
                }

                // Calculate the target row and column
                int xMove = xPos + rowOffset;
                int yMove = yPos + columnOffset;
                
                // Ensure the taget square is valid
                if (IsLegalKingMove(board, xMove, yMove, color))
                {
                    moves.Add(CreateMove(board, xPos, yPos, xMove, yMove));
                }
            }
        }
    }

    // Checks if a specified king can move to a specified square
    private bool IsLegalKingMove(Board board, int xMove, int yMove, PieceColor color)
    {
        // Ensure the move is inside the board
        if (xMove < 0 || xMove > 7 || yMove < 0 || yMove > 7)
        {
            return false;
        }

        PieceData target = board.GetPiece(xMove, yMove);

        // Ensure the target piece is the opposite color
        if (target.Type != PieceType.None && target.Color == color)
        {
            return false;
        }
        return true;
    }

    // Generates all possible moves for pieces that slide
    private void GenerateSlidingMoves(Board board, int xPos, int yPos, PieceColor color, List<Move> moves, int rowDirection, int columnDirection)
    {
        // Move one square at a time
        for (int distance = 1; distance < 8; distance++)
        {
            // Calculate the destination row and column
            int xMove = xPos + rowDirection * distance;
            int yMove = yPos + columnDirection * distance;

            // Ensure the move is inside the board
            if (xMove < 0 || xMove > 7 || yMove < 0 || yMove > 7)
            {
                break;
            }

            PieceData target = board.GetPiece(xMove, yMove);

            // Ensure the target piece is the opposite color
            if (target.Type != PieceType.None)
            {
                if (target.Color != color)
                {
                    moves.Add(CreateMove(board, xPos, yPos, xMove, yMove));
                }

                // Cannot move through another piece
                break;
            }

            // If the square is empty, the piece can move there
            moves.Add(CreateMove(board, xPos, yPos, xMove, yMove));
        }
    }

    // Creates a move object containing the starting and target squares
    private Move CreateMove(Board board, int fromRow, int fromColumn, int toRow, int toColumn)
    {
        // Stores the piece at the target square
        PieceData capturedPiece = board.GetPiece(toRow, toColumn);

        // Create and return the move
        return new Move(fromRow, fromColumn, toRow, toColumn, capturedPiece);
    }
}