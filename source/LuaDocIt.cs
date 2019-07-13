using Newtonsoft.Json;
using StylishForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace LuaDocIt
{
	internal static class LuaDocIt
	{
		private static string JsonASC(Dictionary<string, Dictionary<string, List<LuaSortable>>> listOb)
		{
			foreach (string key in listOb.Keys)
			{
				foreach (string key2 in listOb[key].Keys)
				{
					listOb[key][key2].Sort((x, y) => string.Compare(x.name, y.name));
				}
			}

			return JsonConvert.SerializeObject(listOb,
				new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				}
			);
		}

		private static string GetType(LuaSortable file)
		{
			return file.type ?? "";
		}

		private static string GetTypeName(LuaSortable file)
		{
			return file.typeName ?? "";
		}

		private static void GenerateDocumentation(LuaFile[] files, string path)
		{
			Dictionary<string, Dictionary<string, List<LuaSortable>>> list = new Dictionary<string, Dictionary<string, List<LuaSortable>>>();

			for (int n = 0; n < files.Length; n++)
			{
				LuaSortable[][] loopList =
				{
					files[n].Functions,
					files[n].Hooks
				};

				foreach (LuaSortable[] loop in loopList)
				{
					for (int f = 0; f < loop.Length; f++)
					{
						Dictionary<string, List<LuaSortable>> subList;
						List<LuaSortable> luaSortables;

						string type = GetType(loop[f]);

						// get the sub list (dict)
						if (list.ContainsKey(type))
						{
							subList = list[type];
						}
						else
						{
							subList = new Dictionary<string, List<LuaSortable>>();
							list.Add(type, subList);
						}

						string typeName = GetTypeName(loop[f]);

						// get the list
						if (subList.ContainsKey(typeName))
						{
							luaSortables = subList[typeName];
						}
						else
						{
							luaSortables = new List<LuaSortable>();
							subList.Add(typeName, luaSortables);
						}

						// finally add the file
						luaSortables.Add(loop[f]);
					}
				}
			}

			Directory.CreateDirectory($"output/{path}");
			File.WriteAllText($"output/{path}/documentation.json", JsonASC(list));

			//copy web files
			foreach (string s in Directory.GetFiles("webfiles/"))
			{
				File.Copy(s, Path.Combine($"output/{path}/", Path.GetFileName(s)), true);
			}

			MessageBox.Show($"Your documentation has been generated succesfully in 'output/{path}/'\n\nWeb files have also been copied to your folder, you can upload it all to your website.");
		}

		private static LuaFile[] PrepLuaFiles(string[] files, string folder)
		{
			List<LuaFile> generated = new List<LuaFile>();

			for (int n = 0; n < files.Length; n++)
			{
				string relPath = files[n].Remove(0, folder.Length);
				relPath = relPath.TrimStart('\\');
				relPath = relPath.Replace('\\', '/');

				LuaFile file = new LuaFile(files[n], relPath);

				if (!file.Ignored)
				{
					generated.Add(file);
				}
			}

			return generated.ToArray();
		}

		private static string[] BuildFileTree(string path)
		{
			return Directory.GetFiles(path, "*.lua", SearchOption.AllDirectories);
		}

		[STAThread]
		private static void Main()
		{
			StylishForm mainMenu = new StylishForm
			{
				Text = "LuaDocIt"
			};
			mainMenu.SetBounds(0, 0, 800, 600);
			mainMenu.Icon = new Icon("content/icon.ico");

			Panel fileTreePanel = new Panel
			{
				Parent = mainMenu
			};
			fileTreePanel.SetBounds(230, 5, 550, 552);
			fileTreePanel.BackColor = Color.FromArgb(238, 238, 255);
			fileTreePanel.AutoScroll = false;
			fileTreePanel.HorizontalScroll.Enabled = false;
			fileTreePanel.HorizontalScroll.Visible = false;
			fileTreePanel.HorizontalScroll.Maximum = 0;
			fileTreePanel.AutoScroll = true;

			StylishButton addonFolder = new StylishButton
			{
				Text = "Select Folder",
				Parent = mainMenu
			};
			addonFolder.SetBounds(5, 5, 220, 35);
			addonFolder.TextFont = new Font("Arial", 12);
			addonFolder.Background = Color.FromArgb(255, 193, 7);
			addonFolder.SideGround = Color.FromArgb(194, 146, 0);
			addonFolder.TextColor = Color.White;

			FolderBrowserDialog fileSelection = new FolderBrowserDialog();

			addonFolder.Click += (sender, e) => fileSelection.ShowDialog();

			StylishButton generateTree = new StylishButton
			{
				Text = "List Files",
				Parent = mainMenu
			};
			generateTree.SetBounds(5, 42, 220, 35);
			generateTree.TextFont = new Font("Arial", 12);
			generateTree.Background = Color.FromArgb(139, 195, 74);
			generateTree.SideGround = Color.FromArgb(104, 151, 50);
			generateTree.TextColor = Color.White;
			generateTree.Click += (sender, e) =>
			{
				for (int c = 0; c < fileTreePanel.Controls.Count; c++)
				{
					fileTreePanel.Controls[c].Dispose();
					fileTreePanel.Invalidate();
				}

				try
				{
					string[] files = BuildFileTree(fileSelection.SelectedPath);

					LuaFile[] luaFileObj = PrepLuaFiles(files, fileSelection.SelectedPath);

					for (int i = 0; i < files.Length; i++)
					{
						PictureBox icon = new PictureBox
						{
							Image = Image.FromFile("content/lua_icon.png"),
							Size = new Size(24, 24),
							SizeMode = PictureBoxSizeMode.StretchImage,
							Parent = fileTreePanel,
							Left = 5,
							Top = 5 + (29 * i)
						};

						Label name = new Label
						{
							AutoSize = true,
							Text = Path.GetFileName(files[i]),
							Parent = fileTreePanel,
							Left = 34,
							Top = 10 + (29 * i)
						};

						Label total = new Label
						{
							AutoSize = true,
							Text = $"Elements: {luaFileObj[i].Functions.Length + luaFileObj[i].Hooks.Length}",
							ForeColor = Color.Blue,
							Parent = fileTreePanel,
							Top = 10 + (29 * i),
							Left = name.Left + name.Width + 5
						};

						int goodf = 0;

						for (int f = 0; f < luaFileObj[i].Functions.Length; f++)
						{
							if (luaFileObj[i].Functions[f].param.Count > 0)
							{
								goodf++;
							}
						}

						for (int f = 0; f < luaFileObj[i].Hooks.Length; f++)
						{
							if (luaFileObj[i].Hooks[f].param.Count > 0)
							{
								goodf++;
							}
						}

						int badf = luaFileObj[i].Functions.Length + luaFileObj[i].Hooks.Length - goodf;

						Label good = new Label
						{
							AutoSize = true,
							Text = goodf > 0 ? $"To-Document: {goodf}" : "",
							ForeColor = Color.Green,
							Parent = fileTreePanel,
							Top = 10 + (29 * i),
							Left = total.Left + total.Width + 2
						};

						Label bad = new Label
						{
							AutoSize = true,
							Text = badf > 0 ? $"To-Skip: {badf}" : "",
							ForeColor = Color.Red,
							Parent = fileTreePanel,
							Top = 10 + (29 * i),
							Left = good.Left + good.Width + 2
						};
					}
				}
				catch
				{
					// error handling
				}
			};

			StylishTextEntry docName = new StylishTextEntry
			{
				Parent = mainMenu
			};
			docName.SetBounds(5, 495, 220, 35);
			docName.Text = "Project name";

			StylishButton generateDoc = new StylishButton
			{
				Text = "Generate Documentation",
				Parent = mainMenu
			};
			generateDoc.SetBounds(5, 522, 220, 35);
			generateDoc.TextFont = new Font("Arial", 12);
			generateDoc.Background = Color.FromArgb(233, 30, 99);
			generateDoc.SideGround = Color.FromArgb(178, 17, 72);
			generateDoc.TextColor = Color.White;
			generateDoc.Click += (sender, e) =>
			{
				string[] files = BuildFileTree(fileSelection.SelectedPath);
				LuaFile[] luaFileObj = PrepLuaFiles(files, fileSelection.SelectedPath);

				GenerateDocumentation(luaFileObj, docName.Text);
			};

			mainMenu.ShowDialog();
		}
	}
}
