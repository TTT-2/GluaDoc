using Newtonsoft.Json;

namespace LuaDocIt
{
	public class LuaSortable
	{
		public string name { get; set; }
		public string section { get; set; }

		[JsonIgnore]
		public string type { get; set; }

		[JsonIgnore]
		public string typeName { get; set; }

		public string path { get; set; }
		public int line { get; set; }

		public LuaSortable(string name, string section, string type, string typeName, string path, int line)
		{
			this.name = name;
			this.section = section;
			this.type = type;
			this.typeName = typeName;
			this.path = path;
			this.line = line;
		}
	}
}
