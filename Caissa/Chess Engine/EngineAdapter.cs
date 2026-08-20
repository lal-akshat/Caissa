namespace Caissa.Chess_Engine;

public class EngineAdapter
{
    private readonly Engine engine;

    public EngineAdapter()
    {
        engine = new Engine();
    }

    public Move? GetBestMove(Board board, PieceColor color, int depth = 2)
    {
        return engine.FindBestMove(board, color, depth);
    }
}