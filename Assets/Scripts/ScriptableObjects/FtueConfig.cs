﻿using UnityEngine;

namespace DefaultNamespace.Tables {
    [CreateAssetMenu(fileName = "FtueConfig", menuName = "ScriptableObjects/FtueConfig", order = 6)]
    public class FtueConfig : ScriptableObject {
        public string StartHint;
        public SpotlightAnimConfig ScytheHint;
        public SpotlightAnimConfig GainSeedsHint;
        public SpotlightAnimConfig HoeHint;
        public SpotlightAnimConfig SeedHint;
        public SpotlightAnimConfig WaterHint;
        public SpotlightAnimConfig ClockHint;
        public SpotlightAnimConfig EndHint;
    }
}