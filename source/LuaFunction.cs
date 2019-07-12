using System.Collections.Generic;
using System.Web.Script.Serialization;

namespace LuaDocIt
{
	internal class LuaFunction
	{
		public string name;
		public Dictionary<string, object> param = new Dictionary<string, object>();

		public LuaFunction(string name, Dictionary<string, object> param)
		{
			this.name = name;
			this.param = param;
		}

		public string GenerateJson()
		{
			return new JavaScriptSerializer().Serialize(this);
		}
	}
}
