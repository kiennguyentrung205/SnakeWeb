namespace SnakeWeb.Game
{
    public class GameItem : Cell
    {
        public ItemType Type { get; set; }
        public int ScoreValue { get; set; }
        public string Icon { get; set; } = "";
        public string Message { get; set; } = ""; // Tin nhắn khi ăn (VD: "Đen quá!")

        public GameItem(int row, int col, ItemType type) : base(row, col)
        {
            Type = type;
            SetupItem();
        }

        private void SetupItem()
        {
            switch (Type)
            {
                case ItemType.RedEnvelope:
                    ScoreValue = 1; Icon = "🧧"; Message = "Lộc lá!";
                    break;
                case ItemType.BanhChung:
                    ScoreValue = 2; Icon = "🥮"; Message = "No bụng!";
                    break;
                case ItemType.Watermelon:
                    ScoreValue = 2; Icon = "🍉"; Message = "Đỏ thắm!";
                    break;
                case ItemType.Squid:
                    ScoreValue = -5; Icon = "🦑"; Message = "Đen như mực!";
                    break;
                case ItemType.GoldenPig:
                    ScoreValue = 10; Icon = "🐖"; Message = "TRÚNG SỐ!";
                    break;
            }
        }
    }
}