using System;
using System.Collections.Generic;

namespace GluaDoc.LuaJsonStructure
{
	internal class LuaType : IEquatable<LuaType>
	{
		public string name { get; set; }

		public List<LuaTypeEntry> entries { get; set; }

		public LuaType(string name)
		{
			this.name = name;

			this.entries = new List<LuaTypeEntry>();
		}

		public bool Equals(LuaType other)
		{
			return this.name.Equals(other.name);
		}
	}
}
