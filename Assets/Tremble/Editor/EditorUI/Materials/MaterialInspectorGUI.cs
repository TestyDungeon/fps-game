//
// This file is part of the Tremble package by Tiny Goose.
// Copyright (c) 2024-2026 TinyGoose Ltd., All Rights Reserved.
//

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace TinyGoose.Tremble.Editor
{
	[InitializeOnLoad]
	public class MaterialInspectorGUI
	{
		static MaterialInspectorGUI()
		{
			UnityEditor.Editor.finishedDefaultHeaderGUI -= OnPostHeaderGUI;
			UnityEditor.Editor.finishedDefaultHeaderGUI += OnPostHeaderGUI;
		}

		private static bool s_IsFoldoutOpen;

		static void OnPostHeaderGUI(UnityEditor.Editor editor)
		{
			if (editor is not MaterialEditor)
				return;

			GUIContent content = new("Material Settings for Tremble");
			Rect rect = GUILayoutUtility.GetRect(content, EditorStyles.foldoutHeader);
			rect.x += 20f;
			s_IsFoldoutOpen = EditorGUI.BeginFoldoutHeaderGroup(rect, s_IsFoldoutOpen, content, menuIcon: EditorStyles.foldoutHeaderIcon);

			if (s_IsFoldoutOpen)
			{
				EditorGUI.indentLevel++;

				TrembleSyncSettings trembleSyncSettings = TrembleSyncSettings.Get();

				// Get selected materials and their groups
				Material[] selectedMaterials = editor.targets.Cast<Material>().ToArray();
				HashSet<string> uniqueGroups = new(selectedMaterials.Select(m => GetMaterialGroupName(trembleSyncSettings, m)));

				// Gather list of material groups
				List<MaterialGroup> groups = trembleSyncSettings.MaterialGroups.ToList();

				List<string> titles = new() { "(none)" };
				titles.AddRange(groups.Select(g => g.Name));
				titles.Add(" + Create New Group");

				EditorGUI.showMixedValue = uniqueGroups.Count > 1;
				{
					int selectedIdx = uniqueGroups.Count > 1 ? -1 : titles.IndexOf(uniqueGroups.First());
					int newSelectedIdx = EditorGUILayout.Popup("Material Group", selectedIdx, titles.ToArray(), EditorStyles.miniPullDown);

					if (selectedIdx != newSelectedIdx)
					{
						if (newSelectedIdx > groups.Count)
						{
							Rect targetRect = new()
							{
								x = Event.current.mousePosition.x,
								y = Event.current.mousePosition.y,
								width = 400f,
								height = 50f
							};

							//Add new?
							NewMaterialGroupWindow newGroup = EditorWindow.GetWindow<NewMaterialGroupWindow>(true, "New Material Group Name", true);
							newGroup.Show(targetRect, groups.ToArray(), (groupName) => MaterialGroupUtil.MoveMaterialsToGroup(selectedMaterials, groupName, createIfNotExist: true));
							return;
						}

						// Shift from 1-based to 0-based, with -1 meaning none
						int index = newSelectedIdx - 1;
						if (index == -1)
						{
							MaterialGroupUtil.RemoveMaterialsFromGroups(selectedMaterials);
						}
						else
						{
							string groupName = groups[index].Name;
							MaterialGroupUtil.MoveMaterialsToGroup(selectedMaterials, groupName);
						}
					}
				}
				EditorGUI.showMixedValue = false;


				MaterialRenderData[] datas = selectedMaterials.Select(trembleSyncSettings.GetMaterialRenderData).ToArray();
				MaterialRenderData data = trembleSyncSettings.GetMaterialRenderData(selectedMaterials[0]);

				EditorGUI.showMixedValue = datas.Select(x => x.ExportMode).Distinct().Count() > 1;
				{
					int mode = (int)(data?.ExportMode ?? MaterialImportMode.FixedSize);
					EditorGUI.BeginChangeCheck();
					int newMode = EditorGUILayout.Popup("Render Mode", mode, Enum.GetNames(typeof(MaterialImportMode)), EditorStyles.miniPullDown);

					if (EditorGUI.EndChangeCheck())
					{
						foreach (Material mat in selectedMaterials)
						{
							mat.SetMaterialImportMode((MaterialImportMode)newMode);
						}
					}
				}

				if (datas.Any(x => x.ExportMode == MaterialImportMode.Dynamic))
				{
					EditorGUILayout.BeginHorizontal();
					{
						EditorGUILayout.LabelField(new GUIContent
						{
							text = "Resolution",
							tooltip = "Override this to force the texture to render at a certain resolution."
						}, GUILayout.MaxWidth(82));

						EditorGUI.BeginChangeCheck();

						GUIStyle miniLabel = EditorStyles.centeredGreyMiniLabel;
						miniLabel.alignment = TextAnchor.MiddleRight;

						EditorGUILayout.LabelField("Override", EditorStyles.centeredGreyMiniLabel);

						bool overrideRes = EditorGUILayout.Toggle(data?.IsResolutionOverridden ?? false, GUILayout.MinWidth(0));
						if (EditorGUI.EndChangeCheck())
						{
							foreach (Material mat in selectedMaterials)
							{
								mat.SetMaterialResolutionOverridden(overrideRes);
							}
						}

						string[] resolutionLabels = MaterialExportSizeUtils.Names;
						if (data?.IsResolutionOverridden ?? false)
						{
							MaterialExportSize sizeX = data?.ResolutionX ?? MaterialExportSizeUtils.GetDefault();
							MaterialExportSize sizeY = data?.ResolutionY ?? MaterialExportSizeUtils.GetDefault();

							// X Resolution
							EditorGUI.BeginChangeCheck();
							sizeX = (MaterialExportSize)EditorGUILayout.Popup((int)sizeX, resolutionLabels, EditorStyles.miniPullDown);
							if (EditorGUI.EndChangeCheck())
							{
								foreach (Material mat in selectedMaterials)
								{
									mat.SetMaterialResolutionX(sizeX);
								}
							}

							GUI.contentColor = Color.gray;
							GUILayout.Label(" x ");
							GUI.contentColor = Color.white;

							// Y Resolution
							EditorGUI.BeginChangeCheck();
							sizeY = (MaterialExportSize)EditorGUILayout.Popup((int)sizeY, resolutionLabels, EditorStyles.miniPullDown);
							if (EditorGUI.EndChangeCheck())
							{
								foreach (Material mat in selectedMaterials)
								{
									mat.SetMaterialResolutionY(sizeY);
								}
							}
						}
						else
						{
							int[] widths = selectedMaterials.Select(m => m.mainTexture?.width ?? 0).Where(w => w > 0).Distinct().ToArray();
							int[] heights = selectedMaterials.Select(m => m.mainTexture?.height ?? 0).Where(h => h > 0).Distinct().ToArray();

							string widthStr = widths.Length > 0 ? widths[0].ToString() : "(multiple)";
							string heightStr = heights.Length > 0 ? heights[0].ToString() : "(multiple)";

							GUI.contentColor = Color.gray;
							GUILayout.Label($"{widthStr} x {heightStr}");
							GUI.contentColor = Color.white;
						}
					}
					EditorGUILayout.EndHorizontal();
					EditorGUI.indentLevel--;
				}
			}
			EditorGUILayout.EndFoldoutHeaderGroup();
		}

		private static string GetMaterialGroupName(TrembleSyncSettings trembleSyncSettings, Material material)
		{
			foreach (MaterialGroup group in trembleSyncSettings.MaterialGroups)
			{
				if (group.Materials.Contains(material))
					return group.Name;
			}

			return "(none)";
		}
	}
}