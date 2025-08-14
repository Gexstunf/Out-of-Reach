
namespace Characters.SystemAdaptations.Utils {
    public interface IVitalStates {
        bool IsUnconscious { get; }
        bool IsTired { get; }
        bool IsHeavy { get; }
        bool IsStarved { get; }
    }
}
