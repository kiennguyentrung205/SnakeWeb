namespace SnakeWeb.Game
{
    public enum Direction { Up, Down, Left, Right }

    public enum ItemType
    {
        RedEnvelope, // Lì xì (Bình thường)
        BanhChung,   // Bánh chưng (Bình thường)
        Watermelon,  // Dưa hấu (Bình thường)
        Squid,       // Mực (Bẫy - trừ điểm, phun mực)
        GoldenPig    // Heo vàng (Bonus - điểm cao)
    }
}