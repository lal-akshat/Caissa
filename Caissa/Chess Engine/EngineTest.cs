using Caissa.Chess_Rules;

namespace Caissa.Chess_Engine;

public static class EngineTest
{
    public static void Run()
    {
        Board board = new Board();

        Search search = new Search();

        Move? move = search.FindBestMove(
            board,
            PieceColor.White,
            2);

        if (move == null)
        {
            Console.WriteLine("No move found.");
            return;
        }

        Console.WriteLine(
            $"Best move: {move.FromRow},{move.FromColumn} -> " +
            $"{move.ToRow},{move.ToColumn}");
    }
}