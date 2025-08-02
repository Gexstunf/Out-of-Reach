namespace Characters.SystemAdaptations.Utils {
    public struct VitalsStructScript : IVitalStates {
        
        public bool IsUnconscious { get; }
        public bool IsTired { get;  }
        public bool IsHeavy { get; }
        public bool IsStarved { get; }
        
        public VitalsStructScript(bool isStarved, bool isUnconscious, bool isHeavy, bool isTired) {
            IsStarved = isStarved;
            IsUnconscious = isUnconscious;
            IsHeavy = isHeavy;
            IsTired = isTired;
        }
    }
}
