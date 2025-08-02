


namespace Characters.SystemAdaptations.Utils {
    public struct MovementStatesStructScript : IMovementStates
    {
        public bool IsWalking { get; set; }
        public bool IsJumping { get; set; }
        public bool IsRunning { get; set; }
        public bool IsClimbing { get; set; }
        public bool IsIdle { get; set; }
        
        public MovementStatesStructScript(bool isWalking, bool isRunning, bool isJumping, bool isClimbing, bool isIdle) {
            IsWalking = isWalking;
            IsRunning = isRunning;
            IsJumping = isJumping;
            IsClimbing = isClimbing;
            IsIdle = isIdle;
        }
    }
}
