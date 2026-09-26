//
// This file is part of the Tremble package by Tiny Goose.
// Copyright (c) 2024-2026 TinyGoose Ltd., All Rights Reserved.
//

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace TinyGoose.Tremble
{
	[Serializable]
	public class FgdFlagsField : FgdFieldBase
	{
		public string Name { get; init; }
		public string Description { get; init; }
		public string EnumClassName { get; init; }
		public string[] EnumNames { get; set; }

		public override string FieldName => Name;
		
		private string[] EnumNamesOrCached
		{
			get
			{
				if (EnumNames == null || EnumNames.Length == 0)
				{
					if (EnumClassName.ContainsInvariant("+"))
					{
						EnumClassName.Split('+', out string className, out string enumName);

						Debug.LogError($"Couldn't use {enumName} enum. Enums embedded in classes are "
											+ $"not supported by Tremble :(. Move your '{enumName}' enum "
											+ $"outside of {className} to use it!");
						return Array.Empty<string>();
					}

					Type enumType = TypeDatabase.GetType(EnumClassName);
					string[] names = Enum.GetNames(enumType);
					int[] values = Enum.GetValues(enumType).Cast<int>().ToArray();

					int maxValue = values.Max();
					EnumNames = new string[maxValue + 1];

					for (int i = 0; i <= maxValue; i++)
					{
						int nameIdx = Array.FindIndex(values, v => v == i);
						if (nameIdx == -1)
						{
							EnumNames[i] = "_";
						}
						else
						{
							EnumNames[i] = names[nameIdx];
						}
					}
				}

				return EnumNames;
			}
		}

		public override void Write(StreamWriter sw)
		{
			sw.WriteLine($"{Name}(flags) = ");
			sw.WriteLine("\t[");
			{
				for (int i = 0; i < EnumNamesOrCached.Length; i++)
				{
					string name = EnumNamesOrCached[i];
					if (name.EqualsInvariant("_"))
						continue;

					sw.WriteLine($"\t\t{i} : \"{name}\" : 0");
				}
			}
			sw.WriteLine("\t]");
		}

		public override void WriteCSharpClass(StreamWriter sw, int indent)
		{
			string enumTypeName = EnumClassName;
			int lastDot = enumTypeName.LastIndexOf('.');
			if (lastDot != -1)
			{
				enumTypeName = enumTypeName.Substring(lastDot + 1);
			}

			WriteClassField(sw, indent, enumTypeName, Name, Description, "default");
		}

		public override void WriteCSharpInterface(StreamWriter sw, int indent)
			=> WriteInterfaceProperty(sw, indent, EnumClassName, Name, Description, "Flags");
	}
}