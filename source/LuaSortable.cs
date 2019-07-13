namespace LuaDocIt
{
	public class LuaSortable
	{
		public string name { get; set; }
		public string section { get; set; }
		public string path { get; set; }
		public int line { get; set; }
		public string category { get; set; }

		public LuaSortable(string name, string section, string path, int line, string category)
		{
			this.name = name;
			this.section = section;
			this.path = path;
			this.line = line;
			this.category = category;
		}
	}
}
