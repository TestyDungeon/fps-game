//
// This file is part of the Tremble package by Tiny Goose.
// Copyright (c) 2024-2026 TinyGoose Ltd., All Rights Reserved.
//

using UnityEditor;
using UnityEngine;

namespace TinyGoose.Tremble.Editor
{
	public static class TrembleEditorUtility
	{
		public delegate void OnHierarchyItemCallback(GameObject gameObject, Rect selectionRect);

		public static void RegisterOnHierarchyItem(OnHierarchyItemCallback callback)
		{
#if UNITY_6000_5_OR_NEWER
			EditorApplication.hierarchyWindowItemByEntityIdOnGUI += (id, rect) =>
			{
				GameObject go = EditorUtility.EntityIdToObject(id) as GameObject;
				if (!go)
					return;

				callback(go, rect);
			};
#else
			EditorApplication.hierarchyWindowItemOnGUI += (id, rect) =>
			{
				GameObject go = EditorUtility.InstanceIDToObject(id) as GameObject;
				if (!go)
					return;

				callback(go, rect);
			};
#endif
		}
	}
}