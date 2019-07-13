using System.Collections.Generic;

namespace LuaDocIt
{
	internal class LuaHook : LuaSortable
	{
		public Dictionary<string, object> param { get; set; } = new Dictionary<string, object>();

		public LuaHook(string name, Dictionary<string, object> param, string section, string typeName, string path, int line) : base(name, section, "hooks", typeName ?? "", path, line)
		{
			this.param = param;
		}
	}
}
