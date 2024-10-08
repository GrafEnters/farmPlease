﻿using UnityEditor.Tilemaps;

namespace UnityEditor {
    internal static class CustomRuleTileMenu {
        [MenuItem("Assets/Create/2D/Tiles/Custom Rule Tile Script", false, (int) ETilesMenuItemOrder.CustomRuleTile)]
        private static void CreateCustomRuleTile() {
            ProjectWindowUtil.CreateScriptAssetFromTemplateFile(
                "Packages/com.unity.2d.tilemap.extras/Editor/Tiles/RuleTile/ScriptTemplates/NewCustomRuleTile.cs.txt",
                "NewCustomRuleTile.cs");
        }
    }
}