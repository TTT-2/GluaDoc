using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace LuaDocIt
{
	internal class LuaFile
	{
		public string[] Lines { get; set; }
		public LuaFunction[] Functions { get; set; }
		public LuaHook[] Hooks { get; set; }

		public string Type { get; set; }
		public string TypeName { get; set; }

		public bool Ignored { get; set; }

		public string[] MultipleParams { get; set; } =
		{
			"param"
		};

		public string[] ConcatenateCommaParams { get; set; } =
		{
			"author" // TODO add a file option too
		};

		public string[] ConcatenateParams { get; set; } =
		{
			"desc"
		};

		private bool IsMultipleParam(string param)
		{
			foreach (string p in this.MultipleParams)
			{
				if (p.Equals(param))
				{
					return true;
				}
			}

			return false;
		}

		private bool IsConcatenateParam(string param)
		{
			foreach (string p in this.ConcatenateParams)
			{
				if (p.Equals(param))
				{
					return true;
				}
			}

			return false;
		}

		private bool IsConcatenateCommaParam(string param)
		{
			foreach (string p in this.ConcatenateCommaParams)
			{
				if (p.Equals(param))
				{
					return true;
				}
			}

			return false;
		}

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

		private void AddParam(Dictionary<string, object> param, string key, string val)
		{
			// support multiple params

			if (this.IsMultipleParam(key)) // use as list
			{
				List<string> list = this.AddOrCreateList(param, key);

				list.Add(val);
			}
			else if (this.IsConcatenateParam(key)) // concatenate with space
			{
				string p = "";

				if (param.ContainsKey(key))
				{
					p = param[key].ToString();
				}

				p += " " + val;
			}
			else if (this.IsConcatenateCommaParam(key)) // concatenate with comma
			{
				string p = "";

				if (param.ContainsKey(key))
				{
					p = param[key].ToString();
				}

				p += ", " + val;
			}
			else if (!param.ContainsKey(key)) // otherwise just allow a single param
			{
				param.Add(key, val);
			}
		}

		private Dictionary<string, object> GetParams(int i, bool local = false)
		{
			Dictionary<string, object> param = new Dictionary<string, object>();

			for (int y = 1; y < 100; y++) // run through 100 (max) lines up to find params
			{
				if (i - y >= 0 && !this.Lines[i - y].Trim().Equals("")) // if line is valid, otherwise stop ALL search
				{
					string line = this.Lines[i - y];

					if (Regex.IsMatch(line, @"@\w*")) // if line has @word in it then process it
					{
						string p = this.GetWordParams(i - y);

						this.AddParam(param, p, line);
					}
					else if (line.Trim().StartsWith("--")) // if this is a simple comment, it's used as @desc
					{
						string trimmed = line.TrimStart('-');
						string p = trimmed.Trim(); // clear spaces

						this.AddParam(param, "desc", p);

						// if there are more than two '-', stop search
						if (line.Length - trimmed.Length > 2)
						{
							break;
						}
					}
					else // otherwise stop ALL search
					{
						break;
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
			this.Ignored = false;
			this.Type = "";
			this.TypeName = "";
			this.Lines = File.ReadAllLines(path);

			List<LuaFunction> finds = new List<LuaFunction>();
			List<LuaHook> hfinds = new List<LuaHook>();

			for (int i = 0; i < this.Lines.Length; i++)
			{
				this.Lines[i] = this.Lines[i].TrimStart(); // clear every space in front

				if (this.GetLineParam(i).Equals("ignore")) // @ignore support
				{
					this.Ignored = true;

					return;
				}
				else if (this.Lines[i].StartsWith("module")) // module("...", ...) support
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
				}
				else if (!this.Lines[i].StartsWith("--") && !this.Lines[i].Equals("")) // if this line is no comment and not empty
				{
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
						foreach (string part in stripArgs.Split(','))
						{
							string p = part.Trim();

							if (!list.Contains(p)) // just insert, if not already documented
							{
								list.Add("_UDF_PRM_ " + p);
							}
						}
					}

					finds.Add(new LuaFunction(name, param, section, this.Type, this.TypeName, relPath, i + 1));
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
							string stripArgs = Regex.Match(this.Lines[i], @"(\(.*)\)").Value;
							string name = this.GetName(i, hookIdentifier);
							Dictionary<string, object> param = this.GetParams(i);

							// add params of the function if not already inserted
							List<string> list = this.AddOrCreateList(param, "param");

							stripArgs = stripArgs.TrimStart('(').TrimEnd(')'); // remove ( and )

							string access = hookIdentifier.Equals("hook.Call") ? null : "GLOBAL";
							bool jump = false;

							if (stripArgs.Length > 0)
							{
								foreach (string part in stripArgs.Split(','))
								{
									if (!jump)
									{
										jump = true;

										continue;
									}

									string p = part.Trim();

									if (access == null)
									{
										access = p.Equals("nil") ? "GLOBAL" : p;

										continue;
									}

									if (!list.Contains(p)) // just insert, if not already documented
									{
										list.Add("_UDF_PRM_ " + p);
									}
								}
							}

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
