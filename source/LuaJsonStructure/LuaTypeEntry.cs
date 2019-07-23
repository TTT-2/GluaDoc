using System;
using System.Collections.Generic;

namespace GluaDoc.LuaJsonStructure
{
	internal class LuaTypeEntry : IEquatable<LuaTypeEntry>
	{
		public string name { get; set; }

		public List<LuaSortable> entries { get; set; }

		public LuaTypeEntry(string name)
		{
			this.name = name;

			this.entries = new List<LuaSortable>();
		}

		public bool Equals(LuaTypeEntry other)
		{
			return this.name.Equals(other.name);
		}
	}
}
