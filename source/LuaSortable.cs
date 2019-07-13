namespace LuaDocIt
{
	public class LuaSortable
	{
		public string name { get; set; }
		public string path { get; set; }
		public int line { get; set; }

		public LuaSortable(string name, string path, int line)
		{
			this.name = name;
			this.path = path;
			this.line = line;
		}
	}
}
