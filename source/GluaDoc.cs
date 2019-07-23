using GluaDoc.LuaJsonStructure;
using Newtonsoft.Json;
using StylishForms;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace GluaDoc
{
	internal static class GluaDoc
	{
		private static string JsonASC(List<LuaType> listOb)
		{
			for (int l = 0; l < listOb.Count; l++)
			{
				for (int i = 0; i < listOb[l].entries.Count; i++)
				{
					listOb[l].entries[i].entries.Sort((x, y) => string.Compare(x.name, y.name));
				}
			}

			return JsonConvert.SerializeObject(listOb,
				new JsonSerializerSettings
				{
					NullValueHandling = NullValueHandling.Ignore
				}
			);
		}

		private static void GenerateDocumentation(LuaFile[] files, string path)
		{
			List<LuaType> list = new List<LuaType>();

			for (int n = 0; n < files.Length; n++)
			{
				foreach (string key in files[n].FoundEntries.Keys)
				{
					foreach (LuaSortable item in (List<LuaSortable>)files[n].FoundEntries[key])
					{
						LuaType luaType = null;
						LuaTypeEntry luaTypeEntry = null;

						foreach (LuaType lt in list)
						{
							if (lt.name.Equals(item.type))
							{
								luaType = lt;

								break;
							}
						}

						if (luaType == null)
						{
							luaType = new LuaType(item.type);

							list.Add(luaType);
						}

						foreach (LuaTypeEntry lte in luaType.entries)
						{
							if (lte.name.Equals(item.typeName))
							{
								luaTypeEntry = lte;

								break;
							}
						}

						if (luaTypeEntry == null)
						{
							luaTypeEntry = new LuaTypeEntry(item.typeName);

							luaType.entries.Add(luaTypeEntry);
						}

						luaTypeEntry.entries.Add(item);
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
				Text = "GluaDoc"
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
