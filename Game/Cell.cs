namespace SnakeWeb.Game
{
    public class Cell
    {
        public int Row { get; set; }
        public int Column { get; set; }

        public Cell(int row, int col)
        {
            Row = row;
            Column = col;
        }
    }
}