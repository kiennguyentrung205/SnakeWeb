using System.Collections.Generic;

namespace SnakeWeb.Game
{
    public class GameState
    {
        public List<Cell> SnakeParts { get; set; } = new();
        public GameItem CurrentItem { get; set; } = null!;
        public bool IsInked { get; set; } = false; // Trạng thái bị phun mực
    }
}