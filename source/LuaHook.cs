using System.Collections.Generic;

namespace LuaDocIt
{
	internal class LuaHook : LuaSortable
	{
		public Dictionary<string, object> param { get; set; } = new Dictionary<string, object>();

		public LuaHook(string name, Dictionary<string, object> param, string path, int line) : base(name, path, line)
		{
			this.param = param;
		}
	}
}
