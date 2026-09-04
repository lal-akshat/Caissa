using Caissa.Chess_Rules;

namespace Caissa.Chess_Engine;

public class Engine
{
    // Store the search object used to actually find the best move
    private readonly Search _search;

    public Engine()
    {
        // Creates a new search object and stores it in _search
        _search = new Search();
    }

    // Finds the best move given the board and player
    public Move? FindBestMove(
        Board board,
        PieceColor color,
        int depth)
    {
        // Returns the best move for the given board, color, and depth
        return _search.FindBestMove(board, color, depth);
    }
}