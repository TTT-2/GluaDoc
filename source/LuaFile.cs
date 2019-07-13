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

		public string Type;
		public string TypeName;

		private List<string> AddOrCreateList(Dictionary<string, object> dict, string key)
		{
			List<string> list;

			if (dict.ContainsKey(key))
			{
				list = (List<string>)dict[key];
			}
			else
			{
				list = new List<string>();
				dict.Add(key, list);
			}

			return list;
		}

		private string GetLineParam(int i)
		{
			string param = this.Lines[i].TrimStart('-').TrimStart();

			if (!param.StartsWith("@"))
			{
				return "";
			}

			return param.TrimStart('@').Split(' ')[0];
		}

		private string GetWordParams(int i)
		{
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

			return key.TrimStart('@');
		}

		private Dictionary<string, object> GetParams(int i, bool local = false)
		{
			Dictionary<string, object> param = new Dictionary<string, object>();

			for (int y = 1; y < 10; y++) // run through 10 lines up to find params
			{
				if (i - y >= 0 && Regex.IsMatch(this.Lines[i - y], @"@\w*")) // if line has @word in it then process it, otherwise stop ALL search
				{
					string p = this.GetWordParams(i - y);

					if (p.Equals("param")) // support multiple @param
					{
						List<string> list = this.AddOrCreateList(param, p);

						list.Add(this.Lines[i - y]);
					}
					else if (!param.ContainsKey(p))
					{
						param.Add(p, this.Lines[i - y]);
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
			name = name.TrimEnd(')'); // remove ) that ends with hook.Run if there is just one param
			name = name.TrimEnd('"');

			return name;
		}

		public LuaFile(string path, string relPath)
		{
			this.Type = "";
			this.TypeName = "";
			this.Lines = File.ReadAllLines(path);

			List<LuaFunction> finds = new List<LuaFunction>();
			List<LuaHook> hfinds = new List<LuaHook>();

			for (int i = 0; i < this.Lines.Length; i++)
			{
				this.Lines[i] = this.Lines[i].TrimStart(); // clean up every space in front

				if (this.Lines[i].StartsWith("module")) // module("...", ...) support
				{
					this.Type = "module";
					this.TypeName = this.GetName(i, "module");

					break;
				}
				else if (this.GetLineParam(i).Equals("class")) // @class support
				{
					Dictionary<string, object> param = this.GetParams(i);

					this.Type = "class";
					this.Type = this.GetWordParams(i);

					break;
				}
			}

			string section = null;

			for (int i = 0; i < this.Lines.Length; i++)
			{
				bool local = this.Lines[i].StartsWith("local function");

				if (this.GetLineParam(i).Equals("section")) // @section support
				{
					section = this.GetWordParams(i);
				}

				if (this.Lines[i].StartsWith("function") || local) // find line that is supposedly a function
				{
					int trimLen = local ? 14 : 8;

					string name = this.Lines[i];
					name = name.Remove(0, trimLen); // remove function text
					name = name.TrimStart(); // remove spaces in front

					string stripArgs = Regex.Match(this.Lines[i], @"(\(.*)\)").Value;
					int pos = name.IndexOf(stripArgs);

					name = name.Remove(pos, name.Length - pos); // remove rest of the line

					Dictionary<string, object> param = this.GetParams(i, local);

					// add params of the function if not already inserted
					List<string> list = this.AddOrCreateList(param, "param");

					stripArgs = stripArgs.TrimStart('(').TrimEnd(')'); // remove ( and )

					if (stripArgs.Length > 0)
					{
						string[] split = stripArgs.Split(',');

						foreach (string part in split)
						{
							string p = part.Trim();

							if (!list.Contains(p)) // just insert, if not already documented
							{
								list.Add("UNDEFINED " + p);
							}
						}
					}

					string pre = (this.Type.Equals("module")) ? (this.TypeName + ".") : ""; // adding module name as prefix

					finds.Add(new LuaFunction(pre + name, param, section, this.Type, this.TypeName, relPath, i + 1));
				}
				else
				{
					string[] hookIdentifiers =
					{
						"hook.Run",
						"hook.Call"
					};

					foreach (string hookIdentifier in hookIdentifiers)
					{
						if (this.Lines[i].StartsWith(hookIdentifier))
						{
							string name = this.GetName(i, hookIdentifier);
							Dictionary<string, object> param = this.GetParams(i);

							hfinds.Add(new LuaHook(name, param, hookIdentifier, param.ContainsKey("type") ? param["type"].ToString() : "", relPath, i + 1)); // @type is used to set the typeName of an hook
						}
					}
				}

				this.Functions = finds.ToArray();
				this.Hooks = hfinds.ToArray();
			}
		}
	}
}
