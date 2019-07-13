using System.Collections.Generic;

namespace LuaDocIt
{
	internal class LuaHook : LuaSortable
	{
		public Dictionary<string, object> param { get; set; } = new Dictionary<string, object>();

		public LuaHook(string name, Dictionary<string, object> param, string section, string path, int line, string category) : base(name, section, path, line, category)
		{
			this.param = param;
		}
	}
}
