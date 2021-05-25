using Minesweeper.BaseCode;
using Minesweeper.BaseCode.Boards;
using System;
using System.Windows.Forms;

namespace Minesweeper
{
    public partial class Minesweeper : Form
    {
        public Board gameBoard { get; private set; }
        public Difficulty diff { get; private set; }
        public Random rnd { get; private set; }
        public int BOARD_WIDTH { get; private set; }
        public int BOARD_HEIGHT { get; private set; }
        public int MINE_NUMBER { get; private set; }

        public Minesweeper()
        {
            InitializeComponent();
            DoubleBuffered = true;

            rnd = new Random();

            diff = Difficulty.Intermediate;
            StartGame();
        }

        /// <summary>
        /// Starts a new game.
        /// </summary>
        private void StartGame()
        {
            GetBoardSize();

            gameBoard = new Board(this, BOARD_WIDTH, BOARD_HEIGHT, MINE_NUMBER);
            gameBoard.SetupBoard();
            gameBoard.PlaceMines();

            // Dynamically resize the board to fit
            Width = (BOARD_WIDTH * Board.CellSize) + (int)(Board.CellSize * 1.5);
            Height = (BOARD_HEIGHT * Board.CellSize) + Board.CellSize * 4;

            UpdateMinesRemaining();
        }

        /// <summary>
        /// Updates the number of mines remaining on the UI.
        /// </summary>
        /// <param name="numberOfMines"></param>
        public void UpdateMinesRemaining()
        {
            lblMinesLeft.Text = $"Mines Left: {gameBoard.NumMinesRemaining}";
        }

        /// <summary>
        /// Size of the board is determined by the GameMode.
        /// </summary>
        /// <returns></returns>
        private void GetBoardSize()
        {
            switch (diff)
            {
                case Difficulty.Beginner:
                    BOARD_WIDTH = 8;
                    BOARD_HEIGHT = 8;
                    MINE_NUMBER = 10;
                    break;
                case Difficulty.Intermediate:
                    BOARD_WIDTH = 16;
                    BOARD_HEIGHT = 16;
                    MINE_NUMBER = 40;
                    break;
                case Difficulty.Expert:
                    BOARD_WIDTH = 30;
                    BOARD_HEIGHT = 16;
                    MINE_NUMBER = 99;
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// Returns the cell that was clicked on. Or null if the click was outside of the bounds of the 
        /// game board.
        /// </summary>
        /// <param name="mouseArgs"></param>
        /// <returns></returns>
        private Cell GetCellFromMouseEvent(MouseEventArgs mouseArgs)
        {
            // Game is in an over state - do not register clicks on the board.
            if (gameBoard != null && gameBoard.GameOver)
            {
                return null;
            }

            var clickedX = mouseArgs.X - (Board.CellSize / 2);
            var clickedY = mouseArgs.Y - (Board.CellSize * 2);

            var cellX = clickedX / Board.CellSize;
            var cellY = clickedY / Board.CellSize;

            // Check for out of bounds:
            if (clickedX < 0 || clickedY < 0 || cellX < 0 || cellY < 0 || cellX >= gameBoard.Width || cellY >= gameBoard.Height)
            {
                return null;
            }

            return gameBoard.Cells[cellX, cellY];
        }

        /// <summary>
        /// Handles click events on the form for opening/flagging cells.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Minesweeper_Click(object sender, EventArgs e)
        {
            var mouseArgs = (MouseEventArgs)e;

            var cell = GetCellFromMouseEvent(mouseArgs);
            if (cell == null)
                return;

            switch (mouseArgs.Button)
            {
                case MouseButtons.Left:
                    // Left click opens the cell:
                    cell.OnClick();
                    AfterClick();
                    break;
                case MouseButtons.Right:
                    // Right click places a flag:
                    if (cell.Closed)
                    {
                        cell.OnFlag();
                        AfterClick();
                    }
                    break;
                default: 
                    break;
            }
        }

        /// <summary>
        /// Updates the UI after the user has clicked
        /// </summary>
        private void AfterClick()
        {
            gameBoard.UpdateCells();
            gameBoard.CheckForWin();

            Invalidate();
        }

        /// <summary>
        /// Paints the game board.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Minesweeper_Paint(object sender, PaintEventArgs e)
        {
            if (gameBoard != null)
            {
                gameBoard.Painter?.Paint(e.Graphics);
            }
        }

        /// <summary>
        /// Handles the user clicking exit.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void quitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var response = MessageBox.Show("Do you really want to quit the game?", "Quit?", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if (response == DialogResult.Yes)
            {
                Application.Exit();
            }
        }

        /// <summary>
        /// Starts a new beginner game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void beginnerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfirmNewGame(Difficulty.Beginner);
        }

        /// <summary>
        /// Starts a new intermediate game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void intermediateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfirmNewGame(Difficulty.Intermediate);
        }

        /// <summary>
        /// Starts a new expert game.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void expertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConfirmNewGame(Difficulty.Expert);
        }

        /// <summary>
        /// Confirms that the user wants to start a new game.
        /// </summary>
        /// <returns></returns>
        private void ConfirmNewGame(Difficulty mode)
        {
            var response = MessageBox.Show("Do you really want to start a new game?\nYou will lose any progress in your current game.", "Start New Game", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);

            if (response == DialogResult.Yes)
            {
                diff = mode;
                StartGame();
            }
        }

        /// <summary>
        /// Event handler for when the mouse is moved.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Minesweeper_MouseMove(object sender, MouseEventArgs e)
        {
            var cell = GetCellFromMouseEvent(e);
            gameBoard.Painter.HoveredCell = cell;
            Cursor = cell != null && cell.Closed ? Cursors.Hand : Cursors.Default;

            if (cell != null)
            {
                gameBoard.UpdateCells();
            }

            Invalidate();
        }
    }
}
