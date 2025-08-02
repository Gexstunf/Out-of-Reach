
namespace Characters.SystemAdaptations.Utils {
    public interface IMovementStates {
        bool IsWalking { get; }
        bool IsJumping { get; }
        bool IsRunning { get; }
        bool IsClimbing { get; }
        bool IsIdle { get; }
    }
}
