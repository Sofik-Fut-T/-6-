using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace TetrisWindowsForms
{
    public class TetrisForm : Form
    {
        private const int BoardWidth = 10;
        private const int BoardHeight = 20;
        private const int CellSize = 30;

        private TetrisGame game;
        private Timer gameTimer;

        public TetrisForm()
        {
            // Налаштування форми
            this.Width = BoardWidth * CellSize + 16; // Додано ширину рамки
            this.Height = BoardHeight * CellSize + 39; // Додано висоту заголовка
            this.Text = "Tetris";
            this.DoubleBuffered = true; // Включення подвійного буферизації для уникнення мерехтіння

            // Ініціалізація гри
            game = new TetrisGame(BoardWidth, BoardHeight);

            // Ініціалізація таймера
            gameTimer = new Timer { Interval = 500 }; // Затримка 500 мс
            gameTimer.Tick += GameTimer_Tick;
            gameTimer.Start();

            // Обробники подій
            this.KeyDown += TetrisForm_KeyDown;
            this.Paint += TetrisForm_Paint;
        }

        private void GameTimer_Tick(object sender, EventArgs e)
        {
            game.MovePieceDown();
            this.Invalidate(); // Оновлення відображення форми
        }

        private void TetrisForm_KeyDown(object sender, KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Left:
                    game.MovePieceLeft();
                    break;
                case Keys.Right:
                    game.MovePieceRight();
                    break;
                case Keys.Down:
                    game.MovePieceDown();
                    break;
            }
            this.Invalidate(); // Оновлення відображення форми після руху
        }

        private void TetrisForm_Paint(object sender, PaintEventArgs e)
        {
            Graphics g = e.Graphics;

            var board = game.GetBoardRepresentation();
            for (int y = 0; y < BoardHeight; y++)
            {
                for (int x = 0; x < BoardWidth; x++)
                {
                    if (board[y, x] != 0)
                    {
                        g.FillRectangle(Brushes.Blue, x * CellSize, y * CellSize, CellSize, CellSize);
                        g.DrawRectangle(Pens.Black, x * CellSize, y * CellSize, CellSize, CellSize);
                    }
                    else
                    {
                        g.FillRectangle(Brushes.White, x * CellSize, y * CellSize, CellSize, CellSize);
                        g.DrawRectangle(Pens.Gray, x * CellSize, y * CellSize, CellSize, CellSize);
                    }
                }
            }
        }
    }

    // Логіка гри
    public class TetrisGame
    {
        private TetrisBoard board;
        private Tetromino currentPiece;

        public TetrisGame(int width, int height)
        {
            board = new TetrisBoard(width, height);
            SpawnNewPiece();
        }

        private void SpawnNewPiece()
        {
            currentPiece = Tetromino.GetRandomPiece();
            if (!board.CanPlacePiece(currentPiece))
            {
                MessageBox.Show("Game Over!");
                Application.Exit();
            }
            board.PlacePiece(currentPiece);
        }

        public void MovePieceDown()
        {
            if (board.CanMovePiece(currentPiece, 1, 0))
            {
                board.MovePiece(currentPiece, 1, 0);
            }
            else
            {
                board.LockPiece(currentPiece);
                board.ClearFullLines();
                SpawnNewPiece();
            }
        }

        public void MovePieceLeft()
        {
            if (board.CanMovePiece(currentPiece, 0, -1))
            {
                board.MovePiece(currentPiece, 0, -1);
            }
        }

        public void MovePieceRight()
        {
            if (board.CanMovePiece(currentPiece, 0, 1))
            {
                board.MovePiece(currentPiece, 0, 1);
            }
        }

        public int[,] GetBoardRepresentation()
        {
            return board.GetBoardRepresentation();
        }
    }

    public class TetrisBoard
    {
        private int[,] grid;

        public TetrisBoard(int width, int height)
        {
            grid = new int[height, width];
        }

        public bool CanPlacePiece(Tetromino piece)
        {
            foreach (var (y, x) in piece.GetCoordinates())
            {
                if (y >= grid.GetLength(0) || x >= grid.GetLength(1) || y < 0 || x < 0 || grid[y, x] != 0)
                {
                    return false;
                }
            }
            return true;
        }

        public bool CanMovePiece(Tetromino piece, int deltaY, int deltaX)
        {
            foreach (var (y, x) in piece.GetCoordinates())
            {
                int newY = y + deltaY;
                int newX = x + deltaX;
                if (newY >= grid.GetLength(0) || newX >= grid.GetLength(1) || newY < 0 || newX < 0 || grid[newY, newX] != 0)
                {
                    return false;
                }
            }
            return true;
        }

        public void PlacePiece(Tetromino piece)
        {
            foreach (var (y, x) in piece.GetCoordinates())
            {
                grid[y, x] = piece.Color;
            }
        }

        public void LockPiece(Tetromino piece)
        {
            PlacePiece(piece);
        }

        public void ClearFullLines()
        {
            for (int y = 0; y < grid.GetLength(0); y++)
            {
                bool fullLine = true;
                for (int x = 0; x < grid.GetLength(1); x++)
                {
                    if (grid[y, x] == 0)
                    {
                        fullLine = false;
                        break;
                    }
                }
                if (fullLine)
                {
                    for (int row = y; row > 0; row--)
                    {
                        for (int col = 0; col < grid.GetLength(1); col++)
                        {
                            grid[row, col] = grid[row - 1, col];
                        }
                    }
                    for (int col = 0; col < grid.GetLength(1); col++)
                    {
                        grid[0, col] = 0;
                    }
                }
            }
        }

        public int[,] GetBoardRepresentation()
        {
            return (int[,])grid.Clone();
        }

        internal void MovePiece(Tetromino currentPiece, int v1, int v2)
        {
            throw new NotImplementedException();
        }
    }

    public abstract class Tetromino
    {
        protected List<(int, int)> coordinates;
        public int Color { get; protected set; }

        public List<(int, int)> GetCoordinates() => coordinates;

        public static Tetromino GetRandomPiece()
        {
            var rand = new Random();
            switch (rand.Next(3))
            {
                case 0: return new TetrominoL();
                case 1: return new TetrominoT();
                case 2: return new TetrominoI();
                default: return new TetrominoL();
            }
        }
    }

    public class TetrominoL : Tetromino
    {
        public TetrominoL()
        {
            coordinates = new List<(int, int)> { (0, 4), (1, 4), (2, 4), (2, 5) };
            Color = 1;
        }
    }

    public class TetrominoT : Tetromino
    {
        public TetrominoT()
        {
            coordinates = new List<(int, int)> { (0, 4), (1, 3), (1, 4), (1, 5) };
            Color = 2;
        }
    }

    public class TetrominoI : Tetromino
    {
        public TetrominoI()
        {
            coordinates = new List<(int, int)> { (0, 3), (0, 4), (0, 5), (0, 6) };
            Color = 3;
        }
    }

    // Запуск програми
    public static class Program
    {
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TetrisForm());
        }
    }
}










