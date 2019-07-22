using System.Collections.Generic;

namespace LuaDocIt
{
	internal class LuaConVar : LuaSortable
	{
		public Dictionary<string, object> param { get; set; } = new Dictionary<string, object>();

		public LuaConVar(string name, Dictionary<string, object> param, string section, string typeName, string path, int line) : base(name, section, "convars", typeName ?? "", path, line)
		{
			this.param = param;
		}
	}
}
