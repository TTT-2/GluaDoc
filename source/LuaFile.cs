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

        private Dictionary<string, object> GetParams(int i, bool local = false)
        {
            Dictionary<string, object> param = new Dictionary<string, object>();

            for (int y = 1; y < 10; y++) // run through 10 lines up to find params
            {
                if (i - y >= 0 && Regex.IsMatch(this.Lines[i - y], @"@\w*")) // if line has @word in it then process it, otherwise stop ALL search
                {
                    string key = Regex.Match(this.Lines[i - y], @"@\w*").Value;

                    this.Lines[i - y] = this.Lines[i - y].Remove(0, this.Lines[i - y].IndexOf(key)); // remove whats before param key
                    
                    int end = key.Length + 1;

                    if (end <= this.Lines[i - y].Length) // if there are params for the word
                    {
                        this.Lines[i - y] = this.Lines[i - y].Remove(this.Lines[i - y].IndexOf(key), end); // remove param key from line + one space
                        this.Lines[i - y] = this.Lines[i - y].TrimEnd(';');
                    }
                    else // there are no params for this word
                    {
                        this.Lines[i - y] = "";
                    }

                    key = key.TrimStart('@');

                    if (!param.ContainsKey(key))
                    {
                        param.Add(key, this.Lines[i - y]);
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

        public LuaFile(string path)
        {
            this.Lines = File.ReadAllLines(path);

            List<LuaFunction> finds = new List<LuaFunction>();
            List<LuaHook> hfinds = new List<LuaHook>();

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

                    finds.Add(new LuaFunction(name, param));
                }
                else if (this.Lines[i].StartsWith("hook.Add"))
                {
                    string name = this.Lines[i];
                    name = name.Remove(0, 8);

                    string[] args = name.Split(',');

                    name = args[0];
                    name = name.TrimStart('('); // remove ( that starts with each hook.Add, hook.Call
                    name = name.TrimStart();    // remove eventual bonus space
                    name = name.TrimStart('"');
                    name = name.TrimEnd('"');

                    Dictionary<string, object> param = this.GetParams(i);

                    hfinds.Add(new LuaHook(name, param));
                }
            }

            this.Functions = finds.ToArray();
            this.Hooks = hfinds.ToArray();
        }
    }
}
