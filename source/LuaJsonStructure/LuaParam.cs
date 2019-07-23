using System;

namespace GluaDoc.LuaJsonStructure
{
	internal class LuaParam : IEquatable<LuaParam>
	{
		public string name { get; set; }
		public string type { get; set; }
		public string description { get; set; }
		public bool? optional { get; set; }

		public LuaParam(string name, string type = "_UDF_PRM_", string description = "", bool? optional = null)
		{
			this.name = name;
			this.type = type;
			this.description = description;
			this.optional = optional;
		}

		public bool Equals(LuaParam other)
		{
			return this.name.Equals(other.name);
		}
	}
}
