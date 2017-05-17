namespace Game.AdminClient.Models
{
    public class PlayerState
    {
        public string Condition { get; set; }
        public string Comment { get; set; }
        public int Score { get; set; }
        public RatPosition[] RatPositions { get; set; }
    }

    public class RatPosition
    {
        public int RatId { get; set; }
        public Point Position { get; set; }
    }
}