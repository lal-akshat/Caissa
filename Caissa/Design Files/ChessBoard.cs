using System.Drawing.Drawing2D;
using Caissa.Chess_Engine;
using Caissa.Chess_Rules;
using Caissa.Piece_Files;

namespace Caissa;

public partial class ChessBoard : Form
{
    // ── Board-specific palette (kept constant regardless of theme — a
    // wooden board reads as "chess", not as light/dark UI chrome) ──────────
    static readonly Color LightSquare = Color.FromArgb(236, 218, 185); // classic cream
    static readonly Color DarkSquare  = Color.FromArgb(86,  72,  50);  // walnut

    static readonly Color SelectedTint = Color.FromArgb(210, 190, 90);  // muted gold
    static readonly Color LastMoveTint = Color.FromArgb(175, 165, 80);
    static readonly Color LegalDotClr  = Color.FromArgb(30,  30,  30);
    static readonly Color LegalCapClr  = Color.FromArgb(160, 50,  40);
    static readonly Color CheckSquare  = Color.FromArgb(190, 48,  36);  // clear red, no glow

    static readonly Color WarningColor = Color.FromArgb(190, 48, 36);

    // ── Board Layout ──────────────────────────────────────────────────────
    const int SquareSize = 72;
    const int BoardOffset = 24;
    const int SidebarW = 260;

    // ── Game State ────────────────────────────────────────────────────────
    string[,] Board = new string[8, 8];

    int selectedRow = -1, selectedCol = -1;
    bool whiteTurn = true;
    List<(int r, int c)> legalMoves = new();
    List<string> moveHistory = new();
    GameMode currentMode;

    (int fr, int fc, int tr, int tc)? lastMove = null;

    // ── Animation ─────────────────────────────────────────────────────────
    string? animatingPiece = null;
    PointF animFrom, animTo, animCurrent;
    System.Windows.Forms.Timer animTimer;
    int animStep = 0;
    const int AnimSteps = 12;
    Action? onAnimDone;

    // ── Clock ─────────────────────────────────────────────────────────────
    System.Windows.Forms.Timer clockTimer;
    int whiteMs, blackMs; // milliseconds remaining
    bool clockRunning = false;
    bool gameOver = false;
    int initialMs = 5 * 60 * 1000; // default 5 min

    // ── AI ────────────────────────────────────────────────────────────────
    // Chosen up-front on the main menu now (Standard · vs Computer), so
    // there's no in-game toggle — just whether this game has one.
    bool aiEnabled = false;
    bool aiIsWhite = false;
    System.Windows.Forms.Timer aiTimer;

    EngineAdapter engineAdapter = new EngineAdapter();

    // ── Controls ─────────────────────────────────────────────────────────
    BoardPanel boardPanel = null!;
    ListBox historyList = null!;
    Label statusLabel = null!;
    Label whiteClock = null!;
    Label blackClock = null!;

    Panel _sidebar = null!;
    Label _headerLabel = null!;
    Panel _headerDivider = null!;
    ThemeToggle _themeToggle = null!;
    ToolTip _toggleTip = null!;

    Panel _blackClockPanel = null!;
    Panel _whiteClockPanel = null!;
    Label _blackNameLabel = null!;
    Label _whiteNameLabel = null!;

    Panel _hr1 = null!, _hr2 = null!, _hr3 = null!;
    Label _modeLabel = null!;
    Label _histTitle = null!;
    Button _menuBtn = null!;

    public ChessBoard(GameMode mode, bool vsComputer = false)
    {
        currentMode = mode;
        aiEnabled = vsComputer;
        aiIsWhite = false;

        Text = $"Chess — {mode}" + (vsComputer ? " · vs Computer" : "");
        BackColor = Theme.BgColor;
        StartPosition = FormStartPosition.CenterScreen;
        FormBorderStyle = FormBorderStyle.FixedSingle;
        MaximizeBox = false;
        DoubleBuffered = true;
        KeyPreview = true;

        int boardPx = SquareSize * 8 + BoardOffset * 2;
        ClientSize = new Size(boardPx + SidebarW, boardPx);

        whiteMs = initialMs;
        blackMs = initialMs;

        SetupBoard();
        BuildUI(boardPx);

        animTimer = new System.Windows.Forms.Timer { Interval = 12 };
        animTimer.Tick += OnAnimTick;

        clockTimer = new System.Windows.Forms.Timer { Interval = 100 };
        clockTimer.Tick += OnClockTick;

        aiTimer = new System.Windows.Forms.Timer { Interval = 600 };
        aiTimer.Tick += OnAiTick;

        KeyDown += (s, e) =>
        {
            if (e.KeyCode == Keys.Escape) Close();
        };

        // Repaint whenever the theme changes — whether that happened from
        // this window's own toggle or from the one on the main menu.
        // Unsubscribe on close since a new ChessBoard is created per game
        // and we don't want closed instances lingering on a static event.
        Theme.ThemeChanged += ApplyTheme;
        FormClosed += (_, __) => Theme.ThemeChanged -= ApplyTheme;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // UI Layout
    // ═══════════════════════════════════════════════════════════════════════
    private void BuildUI(int boardPx)
    {
        boardPanel = new BoardPanel(this)
        {
            Left = 0,
            Top = 0,
            Width = boardPx,
            Height = boardPx,
            BackColor = Theme.BgColor,
        };
        boardPanel.MouseClick += OnBoardClick;
        Controls.Add(boardPanel);

        // ── Sidebar shell ────────────────────────────────────────────────
        _sidebar = new Panel
        {
            Left = boardPx,
            Top = 0,
            Width = SidebarW,
            Height = boardPx,
            BackColor = Theme.BgColor,
        };
        _sidebar.Paint += (s, e) =>
        {
            using var pen = new Pen(Theme.BorderColor, 1);
            e.Graphics.DrawLine(pen, 0, 0, 0, boardPx);
        };
        Controls.Add(_sidebar);

        int cx = SidebarW; // content width
        int px = 20; // horizontal padding

        // ── Header (mirrors MainMenu: title + theme toggle) ────────────────
        _headerLabel = new Label
        {
            Text = "Caissa",
            Font = new Font("Segoe UI Semibold", 13F, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            Left = px,
            Top = 18,
            Width = 160,
            Height = 24,
            BackColor = Color.Transparent,
        };
        _sidebar.Controls.Add(_headerLabel);

        const int toggleSize = 32;

        _themeToggle = new ThemeToggle
        {
            Left = cx - toggleSize - px + 4,
            Top = 12,
            Width = toggleSize,
            Height = toggleSize,
        };

        _sidebar.Controls.Add(_themeToggle);
        _themeToggle.BringToFront();

        _toggleTip = new ToolTip();
        _toggleTip.SetToolTip(_themeToggle, Theme.IsDarkMode ? "Switch to light mode" : "Switch to dark mode");

        _headerDivider = HRule(0, 54, cx);
        _sidebar.Controls.Add(_headerDivider);

        // ── Clocks ───────────────────────────────────────────────────────
        _blackClockPanel = BuildClockPanel(false, px, 70, cx - px * 2, out _blackNameLabel);
        _sidebar.Controls.Add(_blackClockPanel);

        _whiteClockPanel = BuildClockPanel(true, px, 156, cx - px * 2, out _whiteNameLabel);
        _sidebar.Controls.Add(_whiteClockPanel);

        // ── Divider ──────────────────────────────────────────────────────
        _hr1 = HRule(px, 240, cx - px * 2);
        _sidebar.Controls.Add(_hr1);

        // ── Status / turn label ──────────────────────────────────────────
        statusLabel = new Label
        {
            Text = "White to move",
            Font = new Font("Segoe UI", 11F, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            Left = px,
            Top = 252,
            Width = cx - px * 2,
            Height = 24,
            BackColor = Color.Transparent,
        };
        _sidebar.Controls.Add(statusLabel);

        // Mode label — small, factual
        _modeLabel = new Label
        {
            Text = ModeLabelText(),
            Font = new Font("Segoe UI", 8.5F),
            ForeColor = Theme.TextTertiary,
            Left = px,
            Top = 280,
            Width = cx - px * 2,
            Height = 18,
            BackColor = Color.Transparent,
        };
        _sidebar.Controls.Add(_modeLabel);

        // ── Divider ──────────────────────────────────────────────────────
        _hr2 = HRule(px, 310, cx - px * 2);
        _sidebar.Controls.Add(_hr2);

        // ── Move history ─────────────────────────────────────────────────
        _histTitle = new Label
        {
            Text = "MOVES",
            Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
            ForeColor = Theme.TextTertiary,
            Left = px,
            Top = 322,
            Width = cx - px * 2,
            Height = 16,
            BackColor = Color.Transparent,
        };
        _sidebar.Controls.Add(_histTitle);

        historyList = new ListBox
        {
            Left = px,
            Top = 344,
            Width = cx - px * 2,
            Height = boardPx - 344 - 96,
            BackColor = Theme.CardBg,
            ForeColor = Theme.TextSecondary,
            BorderStyle = BorderStyle.None,
            Font = new Font("Consolas", 9.5F),
            IntegralHeight = false,
        };
        _sidebar.Controls.Add(historyList);

        // ── Divider ──────────────────────────────────────────────────────
        _hr3 = HRule(px, boardPx - 88, cx - px * 2);
        _sidebar.Controls.Add(_hr3);

        // ── Main menu button ─────────────────────────────────────────────
        _menuBtn = MakeButton("Main Menu", px, boardPx - 72, cx - px * 2, 56);
        _menuBtn.Click += (s, e) => Close();
        _sidebar.Controls.Add(_menuBtn);

        UpdateClockDisplay();
    }

    private string ModeLabelText()
    {
        string baseText = currentMode switch
        {
            GameMode.Chess960 => "Chess960 — Fischer Random",
            GameMode.AtomicChess => "Atomic Chess — exploding captures",
            _ => "Standard Chess",
        };

        return baseText + (aiEnabled ? "  ·  vs Computer" : "  ·  vs Player");
    }

    // ── Clock panel builder ──────────────────────────────────────────────
    private Panel BuildClockPanel(bool isWhite, int left, int top, int width, out Label nameLabel)
    {
        var panel = new Panel
        {
            Left = left,
            Top = top,
            Width = width,
            Height = 68,
            BackColor = Color.Transparent,
        };

        panel.Paint += (s, e) =>
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            var rect = new RectangleF(0, 0, panel.Width - 1, panel.Height - 1);

            using var path = Theme.RoundedRect(rect, 8f);

            using (var bg = new SolidBrush(Theme.CardBg))
                g.FillPath(bg, path);

            using var pen = new Pen(Theme.BorderColor, 1f);
            g.DrawPath(pen, path);
        };

        var nl = new Label
        {
            Text = isWhite ? "White" : "Black",
            Font = new Font("Segoe UI", 8F),
            ForeColor = Theme.TextTertiary,
            Left = 12,
            Top = 8,
            Width = 120,
            Height = 16,
            BackColor = Color.Transparent,
        };
        panel.Controls.Add(nl);
        nameLabel = nl;

        var clockLabel = new Label
        {
            Text = FormatMs(initialMs),
            Font = new Font("Consolas", 20F, FontStyle.Bold),
            ForeColor = Theme.TextPrimary,
            Left = 10,
            Top = 26,
            Width = width - 20,
            Height = 32,
            BackColor = Color.Transparent,
        };
        panel.Controls.Add(clockLabel);

        if (isWhite) whiteClock = clockLabel;
        else blackClock = clockLabel;

        return panel;
    }

    private static Button MakeButton(string text, int left, int top, int width, int height)
    {
        var btn = new Button
        {
            Text = text,
            Left = left,
            Top = top,
            Width = width,
            Height = height,
            FlatStyle = FlatStyle.Flat,
            BackColor = Theme.CardBg,
            ForeColor = Theme.TextPrimary,
            Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            Cursor = Cursors.Hand,
            TextAlign = ContentAlignment.MiddleCenter,
        };
        btn.FlatAppearance.BorderColor = Theme.BorderColor;
        btn.FlatAppearance.BorderSize = 1;
        btn.FlatAppearance.MouseOverBackColor = Theme.HoverTint;
        return btn;
    }

    private static Panel HRule(int left, int top, int width)
    {
        return new Panel
        {
            Left = left,
            Top = top,
            Width = width,
            Height = 1,
            BackColor = Theme.BorderColor,
        };
    }

    // ── Theme Application ────────────────────────────────────────────────
    private void ApplyTheme()
    {
        BackColor = Theme.BgColor;
        boardPanel.BackColor = Theme.BgColor;

        _sidebar.BackColor = Theme.BgColor;
        _sidebar.Invalidate();

        _headerLabel.ForeColor = Theme.TextPrimary;
        _headerDivider.BackColor = Theme.BorderColor;

        _hr1.BackColor = Theme.BorderColor;
        _hr2.BackColor = Theme.BorderColor;
        _hr3.BackColor = Theme.BorderColor;

        _blackClockPanel.Invalidate();
        _whiteClockPanel.Invalidate();
        _blackNameLabel.ForeColor = Theme.TextTertiary;
        _whiteNameLabel.ForeColor = Theme.TextTertiary;

        _modeLabel.ForeColor = Theme.TextTertiary;
        _modeLabel.Text = ModeLabelText();

        _histTitle.ForeColor = Theme.TextTertiary;

        historyList.BackColor = Theme.CardBg;
        historyList.ForeColor = Theme.TextSecondary;

        _menuBtn.BackColor = Theme.CardBg;
        _menuBtn.ForeColor = Theme.TextPrimary;
        _menuBtn.FlatAppearance.BorderColor = Theme.BorderColor;
        _menuBtn.FlatAppearance.MouseOverBackColor = Theme.HoverTint;

        _themeToggle.BackColor = Theme.BgColor;
        _themeToggle.Invalidate();
        _toggleTip.SetToolTip(_themeToggle, Theme.IsDarkMode ? "Switch to light mode" : "Switch to dark mode");

        UpdateClockDisplay();

        if (!gameOver) UpdateStatus();

        boardPanel.Invalidate();
        Invalidate(true);
        _themeToggle.BringToFront();
        Update();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Board Initialisation
    // ═══════════════════════════════════════════════════════════════════════
    private void SetupBoard()
    {
        if (currentMode == GameMode.Standard || currentMode == GameMode.AtomicChess)
            SetupStandardBoard();
        else
            SetupChess960Board();
    }

    private void SetupStandardBoard()
    {
        Board = new string[,]
        {
            { "r", "n", "b", "q", "k", "b", "n", "r" },
            { "p", "p", "p", "p", "p", "p", "p", "p" },
            { ".", ".", ".", ".", ".", ".", ".", "." },
            { ".", ".", ".", ".", ".", ".", ".", "." },
            { ".", ".", ".", ".", ".", ".", ".", "." },
            { ".", ".", ".", ".", ".", ".", ".", "." },
            { "P", "P", "P", "P", "P", "P", "P", "P" },
            { "R", "N", "B", "Q", "K", "B", "N", "R" }
        };
    }

    private void SetupChess960Board()
    {
        for (int r = 0; r < 8; r++)
        for (int c = 0; c < 8; c++)
            Board[r, c] = ".";

        for (int i = 0; i < 8; i++)
        {
            Board[1, i] = "p";
            Board[6, i] = "P";
        }

        char[] pieces = { 'R', 'N', 'B', 'Q', 'K', 'B', 'N', 'R' };
        var rand = new Random();
        do
        {
            for (int i = pieces.Length - 1; i > 0; i--)
            {
                int j = rand.Next(i + 1);
                (pieces[i], pieces[j]) = (pieces[j], pieces[i]);
            }
        } while (!Is960Valid(pieces));

        for (int i = 0; i < 8; i++)
        {
            Board[7, i] = pieces[i].ToString();
            Board[0, i] = char.ToLower(pieces[i]).ToString();
        }
    }

    static bool Is960Valid(char[] a)
    {
        int kIdx = Array.IndexOf(a, 'K');
        int r1 = Array.IndexOf(a, 'R');
        int r2 = Array.LastIndexOf(a, 'R');
        if (!(r1 < kIdx && kIdx < r2)) return false;

        int bLight = -1, bDark = -1;
        for (int i = 0; i < a.Length; i++)
            if (a[i] == 'B')
            {
                if (i % 2 == 0) bLight = i;
                else bDark = i;
            }

        return bLight != -1 && bDark != -1;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Clock
    // ═══════════════════════════════════════════════════════════════════════
    private void OnClockTick(object? sender, EventArgs e)
    {
        if (gameOver) return;

        if (whiteTurn) whiteMs -= 100;
        else blackMs -= 100;

        whiteMs = Math.Max(0, whiteMs);
        blackMs = Math.Max(0, blackMs);

        UpdateClockDisplay();

        if (whiteMs == 0 || blackMs == 0)
        {
            clockTimer.Stop();
            gameOver = true;
            string loser = whiteMs == 0 ? "White" : "Black";
            statusLabel.Text = $"{loser} — out of time";
            statusLabel.ForeColor = WarningColor;
        }
    }

    private void UpdateClockDisplay()
    {
        if (whiteClock != null) whiteClock.Text = FormatMs(whiteMs);
        if (blackClock != null) blackClock.Text = FormatMs(blackMs);

        // Dim the inactive clock, brighten the active one
        var activeColor = Theme.TextPrimary;
        var inactiveColor = Theme.TextTertiary;
        if (whiteClock != null) whiteClock.ForeColor = whiteTurn ? activeColor : inactiveColor;
        if (blackClock != null) blackClock.ForeColor = whiteTurn ? inactiveColor : activeColor;
    }

    private static string FormatMs(int ms)
    {
        int total = ms / 1000;
        int m = total / 60;
        int s = total % 60;
        return $"{m:D1}:{s:D2}";
    }

    // ═══════════════════════════════════════════════════════════════════════
    // AI (engine move)
    // ═══════════════════════════════════════════════════════════════════════
    private void OnAiTick(object? sender, EventArgs e)
    {
        aiTimer.Stop();
        if (!aiEnabled || gameOver || animTimer.Enabled) return;
        if (whiteTurn == !aiIsWhite) return; // not AI's turn

        MakeEngineMove();
    }

    private void MakeEngineMove()
    {
        PieceColor aiColor = aiIsWhite
            ? PieceColor.White
            : PieceColor.Black;

        Board engineBoard = ConvertToEngineBoard();

        Move? bestMove = engineAdapter.GetBestMove(
            engineBoard,
            aiColor,
            2);

        if (bestMove == null)
        {
            gameOver = true;
            clockTimer.Stop();

            statusLabel.Text = "No legal moves";
            return;
        }

        ExecuteMove(
            bestMove.FromRow,
            bestMove.FromColumn,
            bestMove.ToRow,
            bestMove.ToColumn);
    }
    
    private Board ConvertToEngineBoard()
    {
        var engineBoard = new Board();

        for (int row = 0; row < 8; row++)
        {
            for (int column = 0; column < 8; column++)
            {
                string piece = Board[row, column];

                if (piece == ".")
                {
                    engineBoard.SetPiece(
                        row,
                        column,
                        new PieceData(
                            PieceType.None,
                            PieceColor.White));

                    continue;
                }

                bool isWhite = char.IsUpper(piece[0]);

                PieceType type = char.ToLower(piece[0]) switch
                {
                    'p' => PieceType.Pawn,
                    'n' => PieceType.Knight,
                    'b' => PieceType.Bishop,
                    'r' => PieceType.Rook,
                    'q' => PieceType.Queen,
                    'k' => PieceType.King,
                    _ => PieceType.None
                };

                PieceColor color = isWhite
                    ? PieceColor.White
                    : PieceColor.Black;

                engineBoard.SetPiece(
                    row,
                    column,
                    new PieceData(type, color));
            }
        }

        return engineBoard;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Input
    // ═══════════════════════════════════════════════════════════════════════
    private void OnBoardClick(object? sender, MouseEventArgs e)
    {
        if (animTimer.Enabled || gameOver) return;
        if (aiEnabled && whiteTurn == aiIsWhite) return; // block clicks on AI turn

        int c = (e.X - BoardOffset) / SquareSize;
        int r = (e.Y - BoardOffset) / SquareSize;
        if (r < 0 || r > 7 || c < 0 || c > 7) return;

        if (selectedRow == -1)
        {
            if (Board[r, c] == ".") return;
            bool isWhite = char.IsUpper(Board[r, c][0]);
            if (isWhite != whiteTurn) return;

            selectedRow = r;
            selectedCol = c;
            legalMoves = GetLegalMoves(r, c);
            boardPanel.Invalidate();
        }
        else
        {
            if (legalMoves.Contains((r, c)))
            {
                int fr = selectedRow, fc = selectedCol;
                selectedRow = -1;
                selectedCol = -1;
                legalMoves.Clear();
                ExecuteMove(fr, fc, r, c);
            }
            else
            {
                selectedRow = -1;
                selectedCol = -1;
                legalMoves.Clear();
                boardPanel.Invalidate();
            }
        }
    }

    private void CheckAtomicWinCondition()
    {
        bool whiteKingExists = false;
        bool blackKingExists = false;

        for (int r = 0; r < 8; r++)
        {
            for (int c = 0; c < 8; c++)
            {
                if (Board[r, c] == "K") whiteKingExists = true;
                if (Board[r, c] == "k") blackKingExists = true;
            }
        }

        if (!whiteKingExists || !blackKingExists)
        {
            gameOver = true;
            clockTimer.Stop();

            if (whiteKingExists && !blackKingExists)
                statusLabel.Text = "White wins!";
            else if (!whiteKingExists && blackKingExists)
                statusLabel.Text = "Black wins!";
            else
                statusLabel.Text = "Draw";

            statusLabel.ForeColor = WarningColor;
        }
    }

    private void ExecuteMove(int fr, int fc, int tr, int tc)
    {
        string piece = Board[fr, fc];
        bool isCapture = Board[tr, tc] != ".";

        animatingPiece = piece;
        Board[fr, fc] = ".";
        animFrom = new PointF(BoardOffset + fc * SquareSize, BoardOffset + fr * SquareSize);
        animTo = new PointF(BoardOffset + tc * SquareSize, BoardOffset + tr * SquareSize);
        animCurrent = animFrom;
        animStep = 0;

        onAnimDone = () =>
        {
            // Atomic chess explosion
            if (currentMode == GameMode.AtomicChess && isCapture)
            {
                Board[tr, tc] = ".";
                for (int dr = -1; dr <= 1; dr++)
                for (int dc = -1; dc <= 1; dc++)
                {
                    int nr = tr + dr, nc = tc + dc;
                    if (nr < 0 || nr > 7 || nc < 0 || nc > 7) continue;
                    if (Board[nr, nc] != "." && char.ToLower(Board[nr, nc][0]) != 'p')
                        Board[nr, nc] = ".";
                }

                CheckAtomicWinCondition();

                if (gameOver)
                {
                    animatingPiece = null;
                    boardPanel.Invalidate();
                    return;
                }
            }
            else
            {
                // Pawn promotion: auto-queen
                char type = char.ToLower(piece[0]);
                bool isWh = char.IsUpper(piece[0]);
                if (type == 'p' && (tr == 0 || tr == 7))
                    Board[tr, tc] = isWh ? "Q" : "q";
                else
                    Board[tr, tc] = piece;
            }

            // Record move
            string notation = ChessNotation(fr, fc, tr, tc, piece, isCapture);
            moveHistory.Add(notation);
            int moveNum = (moveHistory.Count + 1) / 2;
            string prefix = whiteTurn ? $"{moveNum}." : "  ";
            historyList.Items.Add($"{prefix} {notation}");
            historyList.TopIndex = historyList.Items.Count - 1;

            lastMove = (fr, fc, tr, tc);
            whiteTurn = !whiteTurn;
            animatingPiece = null;

            // Start clock on first move
            if (!clockRunning && moveHistory.Count >= 1)
            {
                clockRunning = true;
                clockTimer.Start();
            }

            UpdateStatus();
            UpdateClockDisplay();
            boardPanel.Invalidate();

            // Queue AI move if applicable
            if (aiEnabled && !gameOver)
            {
                bool aiTurn = whiteTurn == aiIsWhite;
                if (aiTurn) aiTimer.Start();
            }
        };

        animTimer.Start();
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Animation
    // ═══════════════════════════════════════════════════════════════════════
    private void OnAnimTick(object? sender, EventArgs e)
    {
        animStep++;
        float t = (float)animStep / AnimSteps;
        float eased = 1 - (float)Math.Pow(1 - t, 3);
        animCurrent = new PointF(
            animFrom.X + (animTo.X - animFrom.X) * eased,
            animFrom.Y + (animTo.Y - animFrom.Y) * eased
        );
        boardPanel.Invalidate();

        if (animStep >= AnimSteps)
        {
            animTimer.Stop();
            onAnimDone?.Invoke();
        }
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Status
    // ═══════════════════════════════════════════════════════════════════════
    private void UpdateStatus()
    {
        if (gameOver) return;

        bool inCheck = IsKingInCheck(whiteTurn);

        // Check for checkmate / stalemate
        bool hasAnyMove = false;
        for (int r = 0; r < 8 && !hasAnyMove; r++)
        for (int c = 0; c < 8 && !hasAnyMove; c++)
        {
            if (Board[r, c] == ".") continue;
            if (char.IsUpper(Board[r, c][0]) != whiteTurn) continue;
            if (GetLegalMoves(r, c).Count > 0) hasAnyMove = true;
        }

        if (!hasAnyMove)
        {
            gameOver = true;
            clockTimer.Stop();
            statusLabel.Text = inCheck
                ? $"{(whiteTurn ? "White" : "Black")} — checkmate"
                : "Stalemate";
            statusLabel.ForeColor = inCheck ? WarningColor : Theme.TextSecondary;
            return;
        }

        string side = whiteTurn ? "White to move" : "Black to move";
        statusLabel.Text = inCheck ? $"{side} — check" : side;
        statusLabel.ForeColor = inCheck ? WarningColor : Theme.TextPrimary;
        boardPanel.Invalidate(); // repaint to show/clear check highlight
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Notation
    // ═══════════════════════════════════════════════════════════════════════
    private string ChessNotation(int fr, int fc, int tr, int tc, string piece, bool isCapture)
    {
        string n = "";
        char type = char.ToLower(piece[0]);
        if (type != 'p') n += char.ToUpper(type);
        else if (isCapture) n += (char)(fc + 'a');
        if (isCapture) n += "x";
        n += (char)(tc + 'a');
        n += (8 - tr).ToString();
        return n;
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Move Generation
    // ═══════════════════════════════════════════════════════════════════════
    private List<(int r, int c)> GetLegalMoves(int r, int c)
    {
        var moves = new List<(int, int)>();
        var piece = Board[r, c];
        bool isWhite = char.IsUpper(piece[0]);
        char type = char.ToLower(piece[0]);

        for (int i = 0; i < 8; i++)
        for (int j = 0; j < 8; j++)
        {
            if (r == i && c == j) continue;
            if (Board[i, j] != "." && char.IsUpper(Board[i, j][0]) == isWhite) continue;

            bool basic = type switch
            {
                'p' => new Pawn(r, c, isWhite).IsLegal(i, j, Board),
                'r' => new Rook(r, c, isWhite).IsLegal(i, j, Board),
                'n' => new Knight(r, c, isWhite).IsLegal(i, j, Board),
                'b' => new Bishop(r, c, isWhite).IsLegal(i, j, Board),
                'q' => new Queen(r, c, isWhite).IsLegal(i, j, Board),
                'k' => new King(r, c, isWhite).IsLegal(i, j, Board),
                _ => false
            };
            if (basic && WouldBeSafeMove(r, c, i, j, isWhite))
                moves.Add((i, j));
        }

        return moves;
    }

    private bool WouldBeSafeMove(int r, int c, int nr, int nc, bool isWhite)
    {
        string from = Board[r, c], to = Board[nr, nc];
        Board[nr, nc] = from;
        Board[r, c] = ".";
        bool safe = !IsKingInCheck(isWhite);
        Board[r, c] = from;
        Board[nr, nc] = to;
        return safe;
    }

    private bool IsKingInCheck(bool isWhite)
    {
        int kr = -1, kc = -1;
        for (int r = 0; r < 8; r++)
        for (int c = 0; c < 8; c++)
            if (Board[r, c] != "." && char.ToLower(Board[r, c][0]) == 'k'
                                   && char.IsUpper(Board[r, c][0]) == isWhite)
            {
                kr = r;
                kc = c;
            }

        if (kr == -1) return false;

        for (int r = 0; r < 8; r++)
        for (int c = 0; c < 8; c++)
        {
            if (Board[r, c] == ".") continue;
            if (char.IsUpper(Board[r, c][0]) == isWhite) continue;

            char type = char.ToLower(Board[r, c][0]);
            bool attack = type switch
            {
                'p' => new Pawn(r, c, !isWhite).IsLegal(kr, kc, Board),
                'r' => new Rook(r, c, !isWhite).IsLegal(kr, kc, Board),
                'n' => new Knight(r, c, !isWhite).IsLegal(kr, kc, Board),
                'b' => new Bishop(r, c, !isWhite).IsLegal(kr, kc, Board),
                'q' => new Queen(r, c, !isWhite).IsLegal(kr, kc, Board),
                'k' => new King(r, c, !isWhite).IsLegal(kr, kc, Board),
                _ => false
            };
            if (attack) return true;
        }

        return false;
    }

    // Helper: find king position for check highlight
    private (int r, int c) FindKing(bool isWhite)
    {
        for (int r = 0; r < 8; r++)
        for (int c = 0; c < 8; c++)
            if (Board[r, c] != "." && char.ToLower(Board[r, c][0]) == 'k'
                                   && char.IsUpper(Board[r, c][0]) == isWhite)
                return (r, c);
        return (-1, -1);
    }

    // ═══════════════════════════════════════════════════════════════════════
    // Board Renderer
    // ═══════════════════════════════════════════════════════════════════════
    public sealed class BoardPanel : Panel
    {
        readonly ChessBoard _g;

        public BoardPanel(ChessBoard g)
        {
            _g = g;
            DoubleBuffered = true;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;

            // Determine check state for highlight
            bool whiteInCheck = _g.IsKingInCheck(true);
            bool blackInCheck = _g.IsKingInCheck(false);
            var (wkr, wkc) = _g.FindKing(true);
            var (bkr, bkc) = _g.FindKing(false);

            // ── Squares ──────────────────────────────────────────────────
            for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
            {
                var rect = new Rectangle(
                    BoardOffset + c * SquareSize,
                    BoardOffset + r * SquareSize,
                    SquareSize, SquareSize);

                Color sq = (r + c) % 2 == 0 ? LightSquare : DarkSquare;

                // Last-move tint — subtle, not shouted
                if (_g.lastMove.HasValue &&
                    ((_g.lastMove.Value.fr == r && _g.lastMove.Value.fc == c) ||
                     (_g.lastMove.Value.tr == r && _g.lastMove.Value.tc == c)))
                    sq = Blend(sq, LastMoveTint, 0.42f);

                // Selected square
                if (r == _g.selectedRow && c == _g.selectedCol)
                    sq = Blend(sq, SelectedTint, 0.60f);

                // King in check — override to red
                if (whiteInCheck && r == wkr && c == wkc)
                    sq = Blend(sq, CheckSquare, 0.72f);
                if (blackInCheck && r == bkr && c == bkc)
                    sq = Blend(sq, CheckSquare, 0.72f);

                using (var br = new SolidBrush(sq))
                    g.FillRectangle(br, rect);

                // Coordinates — printed once per edge square, muted
                if (c == 0)
                {
                    using var br = new SolidBrush(Color.FromArgb(120, (r + c) % 2 == 0 ? DarkSquare : LightSquare));
                    using var fnt = new Font("Segoe UI", 7.5F, FontStyle.Bold);
                    g.DrawString((8 - r).ToString(), fnt, br, rect.X + 3, rect.Y + 2);
                }

                if (r == 7)
                {
                    using var br = new SolidBrush(Color.FromArgb(120, (r + c) % 2 == 0 ? DarkSquare : LightSquare));
                    using var fnt = new Font("Segoe UI", 7.5F, FontStyle.Bold);
                    g.DrawString(((char)('a' + c)).ToString(), fnt, br, rect.Right - 11, rect.Bottom - 14);
                }
            }

            // ── Legal move indicators ─────────────────────────────────────
            foreach (var (lr, lc) in _g.legalMoves)
            {
                var cx = BoardOffset + lc * SquareSize + SquareSize / 2f;
                var cy = BoardOffset + lr * SquareSize + SquareSize / 2f;
                bool capture = _g.Board[lr, lc] != ".";

                if (capture)
                {
                    using var pen = new Pen(Color.FromArgb(160, LegalCapClr), 4);
                    g.DrawEllipse(pen,
                        cx - SquareSize / 2f + 5,
                        cy - SquareSize / 2f + 5,
                        SquareSize - 10,
                        SquareSize - 10);
                }
                else
                {
                    // Subtle filled dot
                    using var br = new SolidBrush(Color.FromArgb(90, LegalDotClr));
                    g.FillEllipse(br, cx - 8, cy - 8, 16, 16);
                }
            }

            // ── Pieces ────────────────────────────────────────────────────
            for (int r = 0; r < 8; r++)
            for (int c = 0; c < 8; c++)
            {
                if (_g.Board[r, c] == ".") continue;
                DrawPiece(g, _g.Board[r, c],
                    BoardOffset + c * SquareSize,
                    BoardOffset + r * SquareSize);
            }

            // Animating piece drawn last (on top)
            if (_g.animatingPiece != null)
                DrawPiece(g, _g.animatingPiece, _g.animCurrent.X, _g.animCurrent.Y);

            // ── Board border — 1 px, clean ────────────────────────────────
            using (var pen = new Pen(Color.FromArgb(55, 52, 46), 1))
                g.DrawRectangle(pen,
                    BoardOffset - 1, BoardOffset - 1,
                    SquareSize * 8 + 1, SquareSize * 8 + 1);
        }

        static Color Blend(Color a, Color b, float t) => Color.FromArgb(
            Math.Clamp((int)(a.R + (b.R - a.R) * t), 0, 255),
            Math.Clamp((int)(a.G + (b.G - a.G) * t), 0, 255),
            Math.Clamp((int)(a.B + (b.B - a.B) * t), 0, 255));

        void DrawPiece(Graphics g, string piece, float x, float y)
        {
            var img = LoadImage(piece);
            if (img != null)
            {
                g.DrawImage(img, x + 4, y + 4, SquareSize - 8, SquareSize - 8);
            }
            else
            {
                // Unicode fallback — clean, no double-shadow
                string glyph = piece switch
                {
                    "K" => "♔", "Q" => "♕", "R" => "♖",
                    "B" => "♗", "N" => "♘", "P" => "♙",
                    "k" => "♚", "q" => "♛", "r" => "♜",
                    "b" => "♝", "n" => "♞", "p" => "♟",
                    _ => ""
                };
                bool isWhite = char.IsUpper(piece[0]);
                using var fnt = new Font("Segoe UI Symbol", 42F);
                using var shBr = new SolidBrush(Color.FromArgb(55, 0, 0, 0));
                using var br = new SolidBrush(isWhite
                    ? Color.FromArgb(238, 232, 216)
                    : Color.FromArgb(22, 20, 16));
                var sf = new StringFormat
                {
                    Alignment = StringAlignment.Center,
                    LineAlignment = StringAlignment.Center,
                };
                var rect = new RectangleF(x, y, SquareSize, SquareSize);
                // Single, tight shadow — not a floating blob
                g.DrawString(glyph, fnt, shBr,
                    new RectangleF(x + 1, y + 2, SquareSize, SquareSize), sf);
                g.DrawString(glyph, fnt, br, rect, sf);
            }
        }

        static Image? LoadImage(string piece)
        {
            if (piece == ".") return null;

            string type = char.ToLower(piece[0]).ToString();
            string color = char.IsUpper(piece[0]) ? "l" : "d";
            string fileName = $"Chess_{type}{color}t60.png";

            string current = AppDomain.CurrentDomain.BaseDirectory;

            // Search upward until we find the Piece Textures folder
            DirectoryInfo? directory = new DirectoryInfo(current);

            while (directory != null)
            {
                string path = Path.Combine(
                    directory.FullName,
                    "Piece Textures",
                    fileName
                );

                if (File.Exists(path))
                    return Image.FromFile(path);

                directory = directory.Parent;
            }

            return null;
        }
    }
}