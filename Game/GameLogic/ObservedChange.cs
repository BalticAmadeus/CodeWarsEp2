namespace GameLogic
{
    public class ObservedChange
    {
        public ObservedChangeType ChangeType;
        public int PlayerIndex;
        public int RatIndex;
        public Point? Target;
    }

    public enum ObservedChangeType
    {
        Move, Eat, Explode, Demolish, Casualty
    }
}