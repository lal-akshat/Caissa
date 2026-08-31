using Caissa.Chess_Rules;

namespace Caissa.Chess_Engine;

public class Engine
{
    private readonly Search _search;

    public Engine()
    {
        _search = new Search();
    }

    public Move? FindBestMove(
        Board board,
        PieceColor color,
        int depth)
    {
        return _search.FindBestMove(board, color, depth);
    }
}