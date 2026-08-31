using Caissa.Chess_Rules;

namespace Caissa.Chess_Engine;

public class EngineAdapter
{
    private readonly Engine _engine;

    public EngineAdapter()
    {
        _engine = new Engine();
    }

    public Move? GetBestMove(Board board, PieceColor color, int depth = 2)
    {
        return _engine.FindBestMove(board, color, depth);
    }
}