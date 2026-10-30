public static class PlayerStates
{
    public enum State
    {
        Idle,
        Move,
        // Jump covers both ascending and falling, including walking off an edge.
        Jump
    }
}
