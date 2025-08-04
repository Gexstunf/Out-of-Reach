namespace Characters.SystemAdaptations.Utils {
    public struct VitalsStructScript : IVitalStates {
        
        public bool IsUnconscious { get; set; }
        public bool IsTired { get; set; }
        public bool IsHeavy { get; set; }
        public bool IsStarved { get; set; }
        
        public VitalsStructScript(bool isStarved, bool isUnconscious, bool isHeavy, bool isTired) {
            IsStarved = isStarved;
            IsUnconscious = isUnconscious;
            IsHeavy = isHeavy;
            IsTired = isTired;
        }
    }
}
