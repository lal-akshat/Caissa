namespace Caissa.Chess_Engine;

public class Engine
{
    private readonly Search search;

    public Engine()
    {
        search = new Search();
    }

    public Move? FindBestMove(
        Board board,
        PieceColor color,
        int depth)
    {
        return search.FindBestMove(board, color, depth);
    }
}