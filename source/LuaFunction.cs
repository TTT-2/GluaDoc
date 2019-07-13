using System.Collections.Generic;

namespace LuaDocIt
{
	internal class LuaFunction : LuaSortable
	{
		public Dictionary<string, object> param { get; set; } = new Dictionary<string, object>();

		public LuaFunction(string name, Dictionary<string, object> param, string section, string type, string typeName, string path, int line) : base(name, section, type ?? "", typeName ?? "", path, line)
		{
			this.param = param;
		}
	}
}
