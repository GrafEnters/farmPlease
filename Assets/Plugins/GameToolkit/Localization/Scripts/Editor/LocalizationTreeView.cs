﻿// Copyright (c) H. Ibrahim Penekli. All rights reserved.
// Licensed under the MIT License. See LICENSE in the project root for license information.

using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;
using UnityEngine.Assertions;
using Object = UnityEngine.Object;

namespace GameToolkit.Localization.Editor {
    public class LocalizationTreeView : TreeView {
        private const float RowHeight = 20f;
        private const float ToggleWidth = 18f;
        private const float MaxTextAreaHeight = 100f;
        private const int FirstElementId = 1;

        private Rect m_CellRect;

        private int m_ElementId = FirstElementId;
        private readonly GUIStyle m_TextAreaStyle;
        private float m_ValueColumnWidth;

        public LocalizationTreeView(TreeViewState state, MultiColumnHeader multiColumnHeader) : base(state,
            multiColumnHeader) {
            m_TextAreaStyle = new GUIStyle(EditorStyles.textArea);
            m_TextAreaStyle.wordWrap = true;

            rowHeight = RowHeight;
            columnIndexForTreeFoldouts = 1;
            showAlternatingRowBackgrounds = true;
            showBorder = true;

            // Center foldout in the row since we also center content. See RowGUI.
            customFoldoutYOffset = (RowHeight - EditorGUIUtility.singleLineHeight) * 0.5f;
            multiColumnHeader.canSort = false;
            Reload();
        }

        protected override TreeViewItem BuildRoot() {
            m_ElementId = FirstElementId;

            // This section illustrates that IDs should be unique. The root item is required to 
            // have a depth of -1, and the rest of the items increment from that.
            TreeViewItem root = new TreeViewItem {id = 0, depth = -1, displayName = "Root"};
            LocalizedAssetBase[] localizedAssets = Localization.FindAllLocalizedAssets();
            List<TreeViewItem> allItems = new List<TreeViewItem>();

            // Add localized assets.
            foreach (LocalizedAssetBase localizedAsset in localizedAssets) {
                AssetTreeViewItem assetItem = new AssetTreeViewItem(0, localizedAsset);
                allItems.Add(assetItem);

                // Add locale items.
                LocaleItemBase[] localItems = localizedAsset.LocaleItems;
                for (int i = 0; i < localItems.Length; i++)
                    allItems.Add(new LocaleTreeViewItem(m_ElementId++, 1, localItems[i], assetItem));
            }

            // Utility method that initializes the TreeViewItem.children and .parent for all items.
            SetupParentsAndChildrenFromDepths(root, allItems);
            return root;
        }

        protected override void RowGUI(RowGUIArgs args) {
            for (int i = 0; i < args.GetNumVisibleColumns(); ++i) {
                ColumnType columnType = (ColumnType) args.GetColumn(i);
                Rect cellRect = args.GetCellRect(i);
                CellGUI(cellRect, args.item, columnType, ref args);

                // Refresh row heights if the Value column width is changed.
                if (columnType == ColumnType.Value && cellRect.width != m_ValueColumnWidth) {
                    m_ValueColumnWidth = cellRect.width;
                    RefreshCustomRowHeights();
                }
            }
        }

        /// <summary>
        ///     Make TextArea as expandable as possible.
        /// </summary>
        protected override float GetCustomRowHeight(int row, TreeViewItem item) {
            float rowHeight = base.GetCustomRowHeight(row, item);
            LocaleTreeViewItem localeItem = item as LocaleTreeViewItem;
            if (localeItem != null) {
                AssetTreeViewItem assetItem = localeItem.Parent;
                if (assetItem.LocalizedAsset.ValueType == typeof(string)) {
                    MultiColumnHeaderState.Column column = multiColumnHeader.GetColumn(3);
                    if (column != null) {
                        string stringValue = (string) localeItem.LocaleItem.ObjectValue;
                        float calculatedRowHeight =
                            m_TextAreaStyle.CalcHeight(new GUIContent(stringValue), column.width) + 4;
                        rowHeight = Mathf.Clamp(calculatedRowHeight, rowHeight, MaxTextAreaHeight);
                    }
                }
            }

            return rowHeight;
        }

        private void CellGUI(Rect cellRect, TreeViewItem item, ColumnType column, ref RowGUIArgs args) {
            switch (column) {
                case ColumnType.Type:
                    DrawTypeCell(cellRect, item);
                    break;

                case ColumnType.Name:
                    DrawNameCell(cellRect, item, ref args);
                    break;

                case ColumnType.Language:
                    DrawLanguageCell(cellRect, item);
                    break;

                case ColumnType.Value:
                    DrawValueCell(cellRect, item);
                    break;
            }
        }

        private void DrawTypeCell(Rect cellRect, TreeViewItem item) {
            CenterRectUsingSingleLineHeight(ref cellRect);
            AssetTreeViewItem treeViewItem = item as AssetTreeViewItem;
            if (treeViewItem != null) {
                Texture icon;

                // Set icon by localized asset value type.
                Type valueType = treeViewItem.LocalizedAsset.ValueType;
                if (valueType == typeof(string))
                    icon = EditorGUIUtility.ObjectContent(null, typeof(TextAsset)).image;
                else
                    icon = EditorGUIUtility.ObjectContent(null, valueType).image;

                // Set default icon if not exist.
                if (!icon) icon = EditorGUIUtility.ObjectContent(null, typeof(ScriptableObject)).image;
                if (icon) GUI.DrawTexture(cellRect, icon, ScaleMode.ScaleToFit);
            }
        }

        private void DrawNameCell(Rect cellRect, TreeViewItem item, ref RowGUIArgs args) {
            CenterRectUsingSingleLineHeight(ref cellRect);
            args.rowRect = cellRect;
            base.RowGUI(args);
        }

        private void DrawLanguageCell(Rect cellRect, TreeViewItem item) {
            cellRect.y += 2;
            cellRect.height -= 4;
            LocaleTreeViewItem localeItem = item as LocaleTreeViewItem;
            if (localeItem != null)
                localeItem.LocaleItem.Language =
                    (SystemLanguage) EditorGUI.EnumPopup(cellRect, localeItem.LocaleItem.Language);
        }

        private void DrawValueCell(Rect cellRect, TreeViewItem item) {
            cellRect.y += 2;
            cellRect.height -= 4;
            LocaleTreeViewItem treeViewItem = item as LocaleTreeViewItem;
            if (treeViewItem != null) {
                LocaleItemBase localeItem = treeViewItem.LocaleItem;
                Type valueType = treeViewItem.Parent.LocalizedAsset.ValueType;

                EditorGUI.BeginChangeCheck();
                if (valueType.IsSubclassOf(typeof(Object))) {
                    localeItem.ObjectValue = EditorGUI.ObjectField(cellRect, (Object) localeItem.ObjectValue,
                        localeItem.ObjectValue.GetType(), false);
                } else if (valueType == typeof(string)) {
                    EditorGUI.BeginChangeCheck();
                    localeItem.ObjectValue =
                        EditorGUI.TextArea(cellRect, (string) localeItem.ObjectValue, m_TextAreaStyle);
                    if (EditorGUI.EndChangeCheck()) RefreshCustomRowHeights();
                } else {
                    EditorGUI.LabelField(cellRect, valueType + " value type not supported.");
                }

                if (EditorGUI.EndChangeCheck()) {
                    treeViewItem.Parent.IsDirty = true;
                    EditorUtility.SetDirty(treeViewItem.Parent.LocalizedAsset);
                }
            }
        }

        protected override bool CanRename(TreeViewItem item) {
            if (item is AssetTreeViewItem) {
                // Only allow rename if we can show the rename overlay with a certain width (label might be clipped
                // by other columns).
                Rect renameRect = GetRenameRect(treeViewRect, 0, item);
                return renameRect.width > 30;
            }

            return false;
        }

        protected override void RenameEnded(RenameEndedArgs args) {
            // Set the backend name and reload the tree to reflect the new model
            if (args.acceptedRename) {
                AssetTreeViewItem item = FindItem(args.itemID, rootItem) as AssetTreeViewItem;
                if (item != null) {
                    string assetPath = AssetDatabase.GetAssetPath(item.LocalizedAsset.GetInstanceID());
                    AssetDatabase.RenameAsset(assetPath, args.newName);
                    AssetDatabase.SaveAssets();
                    Reload();
                }
            }
        }

        public TreeViewItem GetSelectedItem() {
            IList<int> selection = GetSelection();
            if (selection.Count > 0) return FindItem(selection[0], rootItem);
            return null;
        }

        protected override Rect GetRenameRect(Rect rowRect, int row, TreeViewItem item) {
            Rect cellRect = GetCellRectForTreeFoldouts(rowRect);
            CenterRectUsingSingleLineHeight(ref cellRect);
            return base.GetRenameRect(cellRect, row, item);
        }

        public static MultiColumnHeaderState CreateDefaultMultiColumnHeaderState(float treeViewWidth) {
            int typeColumnWidth = 30;
            int nameColumnWidth = 150;
            int languageColumnWidth = 110;
            float valueColumnWidth = treeViewWidth - typeColumnWidth - nameColumnWidth - languageColumnWidth;
            MultiColumnHeaderState.Column[] columns = new[] {
                new MultiColumnHeaderState.Column {
                    headerContent = new GUIContent(EditorGUIUtility.FindTexture("FilterByType"),
                        "Localized asset type."),
                    contextMenuText = "Type",
                    headerTextAlignment = TextAlignment.Center,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = typeColumnWidth,
                    minWidth = 30,
                    maxWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = true
                },
                new MultiColumnHeaderState.Column {
                    headerContent = new GUIContent("Name", "Localized asset name."),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = nameColumnWidth,
                    minWidth = 60,
                    autoResize = false,
                    allowToggleVisibility = false
                },
                new MultiColumnHeaderState.Column {
                    headerContent = new GUIContent("Language", "Locale language."),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Center,
                    width = languageColumnWidth,
                    minWidth = 35,
                    autoResize = false
                },
                new MultiColumnHeaderState.Column {
                    headerContent = new GUIContent("Value", "Locale value."),
                    headerTextAlignment = TextAlignment.Left,
                    sortedAscending = true,
                    sortingArrowAlignment = TextAlignment.Left,
                    width = valueColumnWidth,
                    minWidth = 60,
                    autoResize = true
                }
            };

            Assert.AreEqual(columns.Length, Enum.GetValues(typeof(ColumnType)).Length,
                "Number of columns should match number of enum values: You probably forgot to update one of them.");

            MultiColumnHeaderState state = new MultiColumnHeaderState(columns);
            return state;
        }

        // TreeView column types.
        private enum ColumnType {
            Type,
            Name,
            Language,
            Value
        }
    }
}