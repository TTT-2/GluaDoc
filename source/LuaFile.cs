using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace LuaDocIt
{
	internal class LuaFile
	{
		public string[] Lines;
		public LuaFunction[] Functions;
		public LuaHook[] Hooks;

		private Dictionary<string, object> GetWordParams(int i)
		{
			Dictionary<string, object> param = new Dictionary<string, object>();

			string key = Regex.Match(this.Lines[i], @"@\w*").Value;

			this.Lines[i] = this.Lines[i].Remove(0, this.Lines[i].IndexOf(key)); // remove whats before param key

			int end = key.Length + 1;

			if (end <= this.Lines[i].Length) // if there are params for the word
			{
				this.Lines[i] = this.Lines[i].Remove(this.Lines[i].IndexOf(key), end); // remove param key from line + one space
				this.Lines[i] = this.Lines[i].TrimEnd(';');
			}
			else // there are no params for this word
			{
				this.Lines[i] = "";
			}

			key = key.TrimStart('@');
			
			param.Add(key, this.Lines[i]);

			return param;
		}

		private Dictionary<string, object> GetParams(int i, bool local = false)
		{
			Dictionary<string, object> param = new Dictionary<string, object>();

			for (int y = 1; y < 10; y++) // run through 10 lines up to find params
			{
				if (i - y >= 0 && Regex.IsMatch(this.Lines[i - y], @"@\w*")) // if line has @word in it then process it, otherwise stop ALL search
				{
					Dictionary<string, object> dict = this.GetWordParams(i - y);

					foreach (string key in dict.Keys)
					{
						if (!param.ContainsKey(key))
						{
							param.Add(key, dict[key]);
						}
					}
				}
				else
				{
					break;
				}
			}

			if (local && !param.ContainsKey("local"))
			{
				param.Add("local", "");
			}

			return param;
		}

		private string GetName(int i, string prefix)
		{
			string name = this.Lines[i];
			name = name.Remove(0, prefix.Length);

			string[] args = name.Split(',');

			name = args[0];
			name = name.TrimStart('('); // remove ( that starts with each hook.Add, hook.Call
			name = name.TrimStart(); // remove eventual bonus space
			name = name.TrimStart('"');
			name = name.TrimEnd('"');

			return name;
		}

		public LuaFile(string path)
		{
			this.Lines = File.ReadAllLines(path);

			List<LuaFunction> finds = new List<LuaFunction>();
			List<LuaHook> hfinds = new List<LuaHook>();

			string module = "";
			string type = "";

			for (int i = 0; i < this.Lines.Length; i++)
			{
				if (this.Lines[i].StartsWith("module"))
				{
					module = this.GetName(i, "module") + ".";

					break;
				}
				else if(this.Lines[i].TrimStart('-').TrimStart().StartsWith("@type"))
				{
					Dictionary<string, object> param = GetParams(i);

					type = this.GetWordParams(i)["type"].ToString();

					break;
				}
			}

			for (int i = 0; i < this.Lines.Length; i++)
			{
				bool local = this.Lines[i].StartsWith("local function");

				if (this.Lines[i].StartsWith("function") || local) // find line that is supposedly a function
				{
					int trimLen = local ? 15 : 9;

					string name = this.Lines[i];
					name = name.Remove(0, trimLen); // Remove function text + one space

					string stripArgs = Regex.Match(this.Lines[i], @"(\(.*)\)").Value;

					name = name.Remove(name.IndexOf(stripArgs), stripArgs.Length);

					Dictionary<string, object> param = this.GetParams(i, local);

					finds.Add(new LuaFunction(module + name, param));
				}
				else if (this.Lines[i].StartsWith("hook.Add"))
				{
					string name = this.GetName(i, "hook.Add");
					Dictionary<string, object> param = this.GetParams(i);

					hfinds.Add(new LuaHook(name, param));
				}
			}

			this.Functions = finds.ToArray();
			this.Hooks = hfinds.ToArray();
		}
	}
}
