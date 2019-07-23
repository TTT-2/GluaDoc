using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace GluaDoc.LuaJsonStructure
{
	internal class LuaSortable : IEquatable<LuaSortable>
	{
		public string name { get; set; }
		public string section { get; set; }

		[JsonIgnore]
		public string type { get; set; }

		[JsonIgnore]
		public string typeName { get; set; }

		public string path { get; set; }
		public int line { get; set; }

		public Dictionary<string, object> param { get; set; }

		public LuaSortable(string name, Dictionary<string, object> param, string section, string type, string typeName, string path, int line)
		{
			this.name = name;
			this.param = param;
			this.section = section;
			this.type = type;
			this.typeName = typeName;
			this.path = path;
			this.line = line;
		}

		public bool Equals(LuaSortable other)
		{
			return this.name.Equals(other.name) && this.section.Equals(other.section);
		}
	}
}
