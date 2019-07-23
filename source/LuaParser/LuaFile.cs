using GluaDoc.LuaJsonStructure;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

/**
 * TODO
 * multi type support (@param nil|Table|Player)
 * opt  support (@param[opt])
 * optchain support (@param[optchain])
 * GLOBAL var support
 * _G var support
 */

namespace GluaDoc
{
	internal class LuaFile
	{
		public const string REGEX = @"^\s*-{2,}\s*@\w+";

		public string[] Lines { get; set; }
		public Dictionary<string, object> FoundEntries { get; set; }

		public string Type { get; set; }
		public string TypeName { get; set; }

		public bool Ignored { get; set; }

		public string[] MultipleParams { get; set; } =
		{
			"param",
			"return",
			"usage"
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

		private string GetLineParam(int i)
		{
			string param = this.Lines[i].TrimStart('-').TrimStart();

			if (!param.StartsWith("@"))
			{
				return "";
			}

			return param.TrimStart('@').Split(' ')[0];
		}

		private Match MatchParams(int i)
		{
			return Regex.Match(this.Lines[i], REGEX);
		}

		private string GetWord(int i)
		{
			string key = this.MatchParams(i).Value;

			return key.Trim().TrimStart('-').TrimStart().TrimStart('@');
		}

		private string GetWordParam(int i)
		{
			string line = this.Lines[i];
			Match match = this.MatchParams(i);
			string word = match.Value;
			int index = match.Index;
			string val = line.Remove(0, index); // remove whats before param key

			if (word.Length + 1 <= line.Length) // if there are params for the word
			{
				// remove param key from line and following spaces
				return val.Remove(0, word.Length).Trim().TrimEnd(';');
			}
			else
			{
				return "";
			}
		}

		private List<T> AddOrCreateList<T>(Dictionary<string, object> dict, string key)
		{
			List<T> list;

			if (dict.ContainsKey(key))
			{
				list = (List<T>)dict[key];
			}
			else
			{
				list = new List<T>();
				dict.Add(key, list);
			}

			return list;
		}

		private void AddParam(Dictionary<string, object> param, string name, string text)
		{
			// support multiple params
			if (this.IsMultipleParam(name))
			{
				if (name.Equals("param") || name.Equals("return"))
				{
					int startIndex = name.Equals("param") ? 2 : 1;
					bool? optional = null;
					string type = "_UDF_PRM_";
					string[] arr = text.Trim().Split(' ');
					string prm = "";

					if (arr.Length > 1)
					{
						type = arr[0];
					}

					if (startIndex == 2 && arr.Length > 2)
					{
						prm = arr[1];
					}

					if (type.Equals("[opt]"))
					{
						optional = true;

						type = arr[++startIndex];
					}

					string desc = "";

					for (int i = startIndex; i < arr.Length; i++)
					{
						if (i != 1)
						{
							desc += " ";
						}

						desc += arr[i];
					}

					desc = desc.Trim();

					List<LuaParam> list = this.AddOrCreateList<LuaParam>(param, name);

					list.Add(new LuaParam(prm, type, desc, optional));
				}
				else
				{
					List<object> list = this.AddOrCreateList<object>(param, name);

					list.Add(text);
				}
			}
			else if (this.IsConcatenateParam(name) || this.IsConcatenateCommaParam(name)) // concatenate with space or comma
			{
				string p = "";

				if (param.ContainsKey(name))
				{
					p = param[name].ToString();
				}

				p += (this.IsConcatenateCommaParam(name) ? ", " : " ") + text;

				param[name] = p.Trim();
			}
			else if (!param.ContainsKey(name)) // otherwise just allow a single param
			{
				param.Add(name, text);
			}
		}

		private bool IsParam(int line)
		{
			return this.Lines[line].Trim().StartsWith("--"); // min two '-'?
		}

		private bool IsStartingParam(int line)
		{
			return this.Lines[line].Trim().StartsWith("---"); // more than two '-'?
		}

		private Dictionary<string, object> AddToParams(int i, Dictionary<string, object> param)
		{
			string line = this.Lines[i];

			if (line.Trim().StartsWith("--"))
			{
				if (Regex.IsMatch(line, REGEX)) // if line has @word in it then process it
				{
					string p = this.GetWord(i);
					string val = this.GetWordParam(i);

					this.AddParam(param, p, val);
				}
				else // if this is a simple comment, it's used as @desc
				{
					this.AddParam(param, "desc", line.TrimStart('-').Trim()); // clear '-' and spaces
				}
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
			this.FoundEntries = new Dictionary<string, object>();

			for (int i = 0; i < this.Lines.Length; i++)
			{
				this.Lines[i] = this.Lines[i].TrimStart(); // clear every space in front
			}

			for (int i = 0; i < this.Lines.Length; i++)
			{
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
					this.Type = "class";
					this.TypeName = this.GetWordParam(i);
				}
				else if (!this.Lines[i].StartsWith("--") && !this.Lines[i].Equals("")) // if this line is no comment and not empty
				{
					break;
				}
			}

			string section = null;
			Dictionary<string, object> cachedParams = new Dictionary<string, object>();
			bool cacheActivated = false;

			for (int i = 0; i < this.Lines.Length; i++)
			{
				if (this.Lines[i].Trim().Equals("")) // empty line
				{
					continue;
				}
				else if (this.IsStartingParam(i)) // exclude commented but documented functions, you need to start with min. 3x '-' ("---")
				{
					cachedParams.Clear(); // clear cached dict

					cacheActivated = true; // activate caching
				}

				bool local = this.Lines[i].StartsWith("local function");

				if (this.GetLineParam(i).Equals("section")) // @section support
				{
					section = this.GetWordParam(i);
				}
				else if (cacheActivated && this.IsParam(i)) //  just process if caching is activated
				{
					this.AddToParams(i, cachedParams);
				}
				else if (this.Lines[i].StartsWith("function") || local) // find line that is supposedly a function
				{
					int trimLen = local ? 14 : 8;

					string name = this.Lines[i];
					name = name.Remove(0, trimLen); // remove function text
					name = name.TrimStart(); // remove spaces in front

					string stripArgs = Regex.Match(this.Lines[i], @"(\(.*)\)").Value;
					int pos = name.IndexOf(stripArgs);

					name = name.Remove(pos, name.Length - pos); // remove rest of the line

					// copy cached dict
					Dictionary<string, object> param = new Dictionary<string, object>(cachedParams);

					// clear cached dict
					cachedParams.Clear();

					// deactivate caching
					cacheActivated = false;

					// add local param if not already set
					if (local && !param.ContainsKey("local"))
					{
						param.Add("local", "");
					}

					// add params of the function if not already inserted
					List<LuaParam> tmpList = this.AddOrCreateList<LuaParam>(param, "param");
					List<LuaParam> list = new List<LuaParam>();

					stripArgs = stripArgs.TrimStart('(').TrimEnd(')'); // remove ( and )

					if (stripArgs.Length > 0)
					{
						int found = 0;

						foreach (string part in stripArgs.Split(','))
						{
							string p = part.Trim();
							LuaParam prm = new LuaParam(p);

							foreach (LuaParam item in tmpList)
							{
								if (item.Equals(prm))
								{
									found++;
									prm = item;

									break;
								}
							}

							if (!list.Contains(prm)) // just insert, if not already documented
							{
								list.Add(prm);
							}
						}

						if (found != tmpList.Count)
						{
							System.Console.WriteLine("Param mismatch in '" + relPath + "', Line " + i);
						}

						param["param"] = list;
					}

					this.AddOrCreateList<LuaSortable>(this.FoundEntries, "functions").Add(new LuaSortable(name, param, section, this.Type, this.TypeName, relPath, i + 1));
				}
				else
				{
					// search for ConVars
					{
						string[] conVarIdentifiers =
						{
							"CreateConVar",
							"CreateClientConVar"
						};

						foreach (string conVarIdentifier in conVarIdentifiers)
						{
							if (this.Lines[i].StartsWith(conVarIdentifier))
							{
								string stripArgs = Regex.Match(this.Lines[i], @"(\(.*)\)").Value;
								string name = this.GetName(i, conVarIdentifier);

								// copy cached dict
								Dictionary<string, object> param = new Dictionary<string, object>(cachedParams);

								// clear cached dict
								cachedParams.Clear();

								// deactivate caching
								cacheActivated = false;

								stripArgs = stripArgs.TrimStart('(').TrimEnd(')'); // remove ( and )

								string def = "";

								if (stripArgs.Length > 0)
								{
									string[] splits = stripArgs.Split(',');

									def = splits[1] ?? "";
								}

								// add default value as a param
								// @default is used to set the default value of a ConVar
								if (!param.ContainsKey("default"))
								{
									param.Add("default", def);
								}

								// @name is used to set the name of a ConVar
								// @type is used to set the typeName of a ConVar
								this.AddOrCreateList<LuaSortable>(this.FoundEntries, "convars").Add(new LuaSortable(param.ContainsKey("name") ? param["name"].ToString() : name, param, conVarIdentifier, "hooks", param.ContainsKey("type") ? param["type"].ToString() : "", relPath, i + 1)); // @type is used to set the typeName of a ConVar
							}
						}
					}

					// search for hooks
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

								// copy cached dict
								Dictionary<string, object> param = new Dictionary<string, object>(cachedParams);

								// clear cached dict
								cachedParams.Clear();

								// deactivate caching
								cacheActivated = false;

								// add params of the function if not already inserted
								List<LuaParam> tmpList = this.AddOrCreateList<LuaParam>(param, "param");
								List<LuaParam> list = new List<LuaParam>();

								stripArgs = stripArgs.TrimStart('(').TrimEnd(')'); // remove ( and )

								string _access = hookIdentifier.Equals("hook.Call") ? null : "GLOBAL";
								bool _jump = false;

								if (stripArgs.Length > 0)
								{
									int found = 0;

									foreach (string part in stripArgs.Split(','))
									{
										if (!_jump)
										{
											_jump = true;

											continue;
										}

										string p = part.Trim();

										if (_access == null)
										{
											_access = p.Equals("nil") ? "GLOBAL" : p;

											continue;
										}

										LuaParam prm = new LuaParam(p);

										foreach (LuaParam item in tmpList)
										{
											if (item.Equals(prm))
											{
												found++;
												prm = item;

												break;
											}
										}

										if (!list.Contains(prm)) // just insert, if not already documented
										{
											list.Add(prm);
										}
									}

									if (found != tmpList.Count)
									{
										System.Console.WriteLine("Param mismatch in '" + relPath + "', Line " + i);
									}

									param["param"] = list;
								}

								this.AddOrCreateList<LuaSortable>(this.FoundEntries, "hooks").Add(new LuaSortable(name, param, hookIdentifier, "hooks", param.ContainsKey("type") ? param["type"].ToString() : "", relPath, i + 1)); // @type is used to set the typeName of an hook
							}
						}
					}
				}
			}
		}
	}
}
