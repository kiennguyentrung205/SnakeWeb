using System;
using System.Linq;
using System.Timers;

namespace SnakeWeb.Game
{
    public class GameViewModel
    {
        public int Rows { get; } = 20;
        public int Columns { get; } = 20;

        public GameState State { get; private set; } = new();
        public int Score { get; private set; }
        public string StatusText { get; private set; } = "Sẵn sàng";
        public string RankTitle { get; private set; } = ""; // Danh hiệu cuối game
        public Direction CurrentDirection { get; private set; } = Direction.Right;

        // Thông báo bay lên (Toast effect)
        public string LastFloatingMsg { get; private set; } = "";
        public bool ShowFloatingMsg { get; private set; } = false;

        private readonly System.Timers.Timer _timer;
        private bool _isRunning;
        private readonly Random _random = new Random();
        private int _inkDurationCounter = 0; // Đếm ngược thời gian bị phun mực

        public Action? OnStateChanged { get; set; }

        public GameViewModel()
        {
            _timer = new System.Timers.Timer(150); // Tốc độ mặc định
            _timer.Elapsed += (_, __) => GameLoop();
            InitGame();
        }

        public void InitGame()
        {
            State = new GameState();
            Score = 0;
            StatusText = "Sẵn sàng du xuân";
            RankTitle = "";
            CurrentDirection = Direction.Right;
            LastFloatingMsg = "";
            ShowFloatingMsg = false;
            _inkDurationCounter = 0;

            // Khởi tạo rắn ở giữa
            int midRow = Rows / 2;
            int midCol = Columns / 2;
            State.SnakeParts.Add(new Cell(midRow, midCol));     // Đầu
            State.SnakeParts.Add(new Cell(midRow, midCol - 1)); // Thân
            State.SnakeParts.Add(new Cell(midRow, midCol - 2)); // Đuôi

            SpawnItem();
            NotifyChanged();
        }

        public void Start()
        {
            if (_isRunning) return;
            _isRunning = true;
            StatusText = "Đang đi chợ...";
            _timer.Start();
            NotifyChanged();
        }

        public void Pause()
        {
            if (!_isRunning) return;
            _isRunning = false;
            StatusText = "Đang nghỉ uống trà";
            _timer.Stop();
            NotifyChanged();
        }

        public void Restart()
        {
            _timer.Stop();
            _isRunning = false;
            InitGame();
            Start();
        }

        public void ChangeDirection(Direction dir)
        {
            // Không cho quay đầu 180 độ
            if ((CurrentDirection == Direction.Up && dir == Direction.Down) ||
                (CurrentDirection == Direction.Down && dir == Direction.Up) ||
                (CurrentDirection == Direction.Left && dir == Direction.Right) ||
                (CurrentDirection == Direction.Right && dir == Direction.Left))
                return;

            CurrentDirection = dir;
        }

        private void GameLoop()
        {
            MoveSnake();

            // Xử lý đếm ngược hiệu ứng phun mực
            if (State.IsInked)
            {
                _inkDurationCounter--;
                if (_inkDurationCounter <= 0)
                {
                    State.IsInked = false;
                }
            }
            NotifyChanged();
        }

        private void MoveSnake()
        {
            var head = State.SnakeParts[0];
            int newRow = head.Row;
            int newCol = head.Column;

            switch (CurrentDirection)
            {
                case Direction.Up: newRow--; break;
                case Direction.Down: newRow++; break;
                case Direction.Left: newCol--; break;
                case Direction.Right: newCol++; break;
            }

            // 1. Kiểm tra va chạm tường -> Chết
            if (newRow < 0 || newRow >= Rows || newCol < 0 || newCol >= Columns)
            {
                GameOver();
                return;
            }

            // 2. Kiểm tra cắn vào thân -> Chết
            if (State.SnakeParts.Any(p => p.Row == newRow && p.Column == newCol))
            {
                GameOver();
                return;
            }

            // Di chuyển đầu rắn
            State.SnakeParts.Insert(0, new Cell(newRow, newCol));

            // 3. Kiểm tra ăn vật phẩm
            if (State.CurrentItem.Row == newRow && State.CurrentItem.Column == newCol)
            {
                EatItem();
            }
            else
            {
                // Nếu không ăn thì cắt đuôi
                State.SnakeParts.RemoveAt(State.SnakeParts.Count - 1);
            }
        }

        private void EatItem()
        {
            var item = State.CurrentItem;
            Score += item.ScoreValue;
            if (Score < 0) Score = 0; // Không âm điểm

            // Hiệu ứng Toast message
            TriggerFloatingMsg(item.Message);

            // Xử lý đặc biệt
            if (item.Type == ItemType.Squid)
            {
                State.IsInked = true;
                _inkDurationCounter = 15; // Mù trong khoảng 15 tick (khoảng 2-3 giây)
            }
            else if (item.Type == ItemType.GoldenPig)
            {
                // Ăn heo vàng rắn dài thêm 1 khúc nữa (bonus length)
                var tail = State.SnakeParts.Last();
                State.SnakeParts.Add(new Cell(tail.Row, tail.Column));
            }

            // Tăng tốc độ nhẹ mỗi khi ăn (trừ khi ăn mực)
            if (item.Type != ItemType.Squid && _timer.Interval > 50)
            {
                _timer.Interval -= 2;
            }

            SpawnItem();
        }

        private void TriggerFloatingMsg(string msg)
        {
            LastFloatingMsg = msg;
            ShowFloatingMsg = true;
            // Reset message sau 1s (UI xử lý animation)
            System.Threading.Tasks.Task.Delay(1000).ContinueWith(_ =>
            {
                ShowFloatingMsg = false;
                NotifyChanged();
            });
        }

        private void SpawnItem()
        {
            while (true)
            {
                int r = _random.Next(0, Rows);
                int c = _random.Next(0, Columns);

                // Không spawn trùng lên rắn
                if (State.SnakeParts.Any(p => p.Row == r && p.Column == c)) continue;

                // Tỷ lệ spawn
                double roll = _random.NextDouble();
                ItemType type;

                if (roll < 0.1) type = ItemType.Squid;       // 10% ra Mực
                else if (roll < 0.15) type = ItemType.GoldenPig; // 5% ra Heo Vàng
                else if (roll < 0.3) type = ItemType.Watermelon;
                else if (roll < 0.5) type = ItemType.BanhChung;
                else type = ItemType.RedEnvelope;            // 50% ra Lì Xì

                // Logic chặn: Nếu điểm thấp quá thì đừng ra Mực tội nghiệp người chơi
                if (Score < 10 && type == ItemType.Squid) type = ItemType.RedEnvelope;

                State.CurrentItem = new GameItem(r, c, type);
                break;
            }
        }

        private void GameOver()
        {
            _isRunning = false;
            _timer.Stop();
            StatusText = "Hết Tết Rồi!";

            // Phong danh hiệu
            if (Score < 10) RankTitle = "Còn cái nịt";
            else if (Score < 30) RankTitle = "Thánh Nhọ";
            else if (Score < 60) RankTitle = "Cháu Ngoan Bác Hồ";
            else RankTitle = "Đại Gia Chân Đất 🏆";

            NotifyChanged();
        }

        private void NotifyChanged() => OnStateChanged?.Invoke();
    }
}