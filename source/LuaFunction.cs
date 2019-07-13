using System.Collections.Generic;

namespace LuaDocIt
{
	internal class LuaFunction : LuaSortable
	{
		public Dictionary<string, object> param { get; set; } = new Dictionary<string, object>();
		public string module { get; set; }
		public string type { get; set; }

		public LuaFunction(string name, Dictionary<string, object> param, string module, string type, string section, string path, int line, string category) : base(name, section, path, line, category)
		{
			this.param = param;
			this.module = module;
			this.type = type;
		}
	}
}
