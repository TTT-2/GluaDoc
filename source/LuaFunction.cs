using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace LuaDocIt
{
	internal class LuaFunction
	{
		public string name;
		public Dictionary<string, object> param = new Dictionary<string, object>();
		public string module;
		public string type;
		public string section;

		public LuaFunction(string name, Dictionary<string, object> param, string module, string type, string section)
		{
			this.name = name;
			this.param = param;
			this.module = module;
			this.type = type;
			this.section = section;
		}

		public string GenerateJson()
		{
			return new JavaScriptSerializer().Serialize(this);
		}
	}
}
