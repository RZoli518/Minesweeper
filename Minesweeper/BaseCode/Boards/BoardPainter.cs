using System.Collections.Generic;
using System.Drawing;

namespace Minesweeper.BaseCode.Boards
{
    public class BoardPainter
    {
        public Board Board { get; set; }
        public Cell HoveredCell { get; set; }

        private Dictionary<int, SolidBrush> cellBackgroundColor;
        private readonly Font textStyle = new Font("Verdana", 16f, FontStyle.Bold);

        public BoardPainter()
        {
            SetupColours();
        }

        /// <summary>
        /// Configures the colours used to render the cell counts.
        /// </summary>
        private void SetupColours()
        {
            if (cellBackgroundColor == null)
            {
                cellBackgroundColor = new Dictionary<int, SolidBrush> {
                    { 0, new SolidBrush(ColorTranslator.FromHtml("0xffffff")) },
                    { 1, new SolidBrush(ColorTranslator.FromHtml("0x0000FE")) },
                    { 2, new SolidBrush(ColorTranslator.FromHtml("0x186900")) },
                    { 3, new SolidBrush(ColorTranslator.FromHtml("0xAE0107")) },
                    { 4, new SolidBrush(ColorTranslator.FromHtml("0x000177")) },
                    { 5, new SolidBrush(ColorTranslator.FromHtml("0x8D0107")) },
                    { 6, new SolidBrush(ColorTranslator.FromHtml("0x007A7C")) },
                    { 7, new SolidBrush(ColorTranslator.FromHtml("0x902E90")) },
                    { 8, new SolidBrush(ColorTranslator.FromHtml("0x000000")) }
                };
            }
        }

        /// <summary>
        /// Paints the game board.
        /// </summary>
        /// <param name="graphics"></param>
        public void Paint(Graphics graphics)
        {
            graphics.Clear(Color.White);
            graphics.TranslateTransform(Board.CellSize / 2, Board.CellSize * 2);

            for (int x = 0; x < Board.Width; x++)
            {
                for (int y = 0; y < Board.Height; y++)
                {
                    var cell = Board.Cells[x, y];
                    DrawInsideCell(cell, graphics);
                    graphics.DrawRectangle(Pens.DimGray, cell.Bounds);
                }
            }
        }

        /// <summary>
        /// Gets the brush colour to paint the background of the cell with.
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        private Brush GetBackgroundBrush(Cell cell)
        {
            if (HoveredCell != null && Board.ShowCellHighlights)
            {
                // The hovered cell itself:
                if (HoveredCell.XLoc == cell.XLoc && HoveredCell.YLoc == cell.YLoc)
                {
                    return Brushes.LightSteelBlue;
                }

                // Is one of the neighbor cells:
                if (HoveredCell.GetNeighborCells().Contains(cell))
                {
                    return Brushes.LightSkyBlue;
                }

                //// Is one of the constraints of this cell:
                //HoveredCell.UpdateConstraints();
                //if (HoveredCell.Constraint.Constraints.Contains(cell))
                //{
                //    return Brushes.LightSkyBlue;
                //}
            }

            // No cell being hovered over so return the default background colouring following the below logic:
            // Open Cell => Light Gray
            // Closed Cell => Dark Gray
            // 100% a mine => Salmon (red)
            // 100% safe => Pale Green
            if (cell.Opened)
                return Brushes.LightGray;
            else if (cell.MinePercentage == 0M && Board.ShowHints)
                return Brushes.PaleGreen;
            else if (cell.MinePercentage == 100M && Board.ShowHints)
                return Brushes.Salmon;

            return Brushes.DarkGray;
        }

        /// <summary>
        /// Renders one cell on the game board.
        /// </summary>
        /// <param name="cell"></param>
        /// <param name="graphics"></param>
        private void DrawInsideCell(Cell cell, Graphics graphics)
        {
            // Opened Cell
            if (cell.Opened)
            {
                graphics.FillRectangle(GetBackgroundBrush(cell), cell.Bounds);

                if (cell.NumMines > 0)
                {
                    graphics.DrawString(cell.NumMines.ToString(), textStyle, GetCellColour(cell), cell.CenterPos);
                }
            }

            // Flagged Cell
            if (cell.Flagged)
            {
                graphics.DrawString("⚑", textStyle, Brushes.Black, cell.CenterPos);
            }

            // Mine Cell
            if (cell.IsMine && (Board.ShowMines || Board.GameOver))
            {
                // This cell was the one that ended the game
                if (cell.Opened)
                {
                    graphics.FillRectangle(Brushes.Red, cell.Bounds);
                }

                // Reveal the locations of the mines that had not been flagged
                if (!cell.Flagged)
                {
                    var format = new StringFormat
                    {
                        Alignment = StringAlignment.Center,
                        LineAlignment = StringAlignment.Center
                    };
                    graphics.DrawString("X", textStyle, Brushes.Black, cell.Bounds, format);
                }
            }
        }

        /// <summary>
        /// Return the colour code associated with the number of surrounding mines
        /// </summary>
        /// <returns></returns>
        private SolidBrush GetCellColour(Cell cell)
        {
            return cellBackgroundColor[cell.NumMines];
        }
    }
}
