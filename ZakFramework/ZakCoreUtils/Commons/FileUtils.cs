﻿using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ZakCore.Utils.Commons
{
	public class FileUtils
	{
		public static String BaseRoot = null;


		public static void CreateFolderRecursive(string path)
		{
			if (Directory.Exists(path))
			{
				return;
			}
			string[] current = path.Split(Path.DirectorySeparatorChar);

			string pathToCheck = current[0];
			if (pathToCheck.IndexOf(':') == 1)
			{
				pathToCheck += Path.DirectorySeparatorChar;
			}

			for (int i = 1; i < current.Length; i++)
			{
				pathToCheck = Path.Combine(pathToCheck, current[i]);

				if (!Directory.Exists(pathToCheck))
				{
					Directory.CreateDirectory(pathToCheck);
				}
			}
		}

		public static String FindFile(string path, string availableRoot = null)
		{
			if (path == null) return string.Empty;
			if (string.IsNullOrEmpty(path)) return path;
			if (Path.IsPathRooted(path) && File.Exists(path)) return path;

			string npath = string.Empty;
			if (Assembly.GetExecutingAssembly().GetName().CodeBase != null)
			{
				string pddn = Path.GetDirectoryName(Assembly.GetExecutingAssembly().GetName().CodeBase);
				if(pddn!=null) npath = Path.Combine(pddn, path);
			}
			if (npath.IndexOf("file:", StringComparison.Ordinal) == 0)
			{
				npath = npath.Substring("file:x".Length);
			}
			if (File.Exists(npath)) return npath;

			if (!string.IsNullOrEmpty(availableRoot))
			{
				npath = Path.Combine(availableRoot, path);
				if (File.Exists(npath)) return npath;
			}
			if (!string.IsNullOrEmpty(BaseRoot))
			{
				npath = Path.Combine(BaseRoot, path);
				if (File.Exists(npath)) return npath;
			}
			npath = Path.Combine(Environment.CurrentDirectory, path);
			if (File.Exists(npath)) return npath;
			npath = Path.Combine(Environment.SystemDirectory, path);
			if (File.Exists(npath)) return npath;


			return null;
		}

		public static Type LoadClassFromAssemblies(string className, Assembly asm = null)
		{
			// Type toret = Activator.CreateInstance("IOSPlugin", className).GetType();
			// return toret;
			if (asm != null)
			{
				Type toret = asm.GetType(className, false);
				if (toret != null)
				{
					return toret;
				}
			}
			foreach (var ex in AppDomain.CurrentDomain.GetAssemblies())
			{
				Type toret = ex.GetType(className, false);
				if (toret != null) return toret;
			}
			return null;
		}

		public static Assembly LoadAssembly(String path, string availableRoot = null)
		{
			try
			{
				//Something already present
				if (path == null) return null;
				path = FindFile(path, availableRoot);
				String fileName = Path.GetFileName(path);
				foreach (var ex in AppDomain.CurrentDomain.GetAssemblies())
				{
					if (!ex.IsDynamic && !string.IsNullOrEmpty(ex.CodeBase))
					{
						if (String.Compare(Path.GetFileName(ex.CodeBase), fileName, StringComparison.OrdinalIgnoreCase) == 0)
						{
							return ex;
						}
					}
				}

				/*byte[] asmToLoad = File.ReadAllBytes(path);
				AppDomain.CurrentDomain.Load(asmToLoad);*/
				return Assembly.LoadFrom(path);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
				//Logger.Log("FileUtils", ex);
			}
			return null;
		}

		public static void InitializeRoot(CommandLineParser commandLineParser = null)
		{
			//Nothing set
			if (CommandLineParser.GetEnv("ROOT") != null)
			{
				BaseRoot = CommandLineParser.GetEnv("ROOT");
			}
			else if (commandLineParser == null || !commandLineParser.IsSet("root"))
			{
				BaseRoot = Environment.CurrentDirectory;
			}
			else
			{
				BaseRoot = commandLineParser["root"] == null ? CommandLineParser.GetEnv("ROOT") : commandLineParser["root"];
			}

			BaseRoot = BaseRoot.Replace('\\', Path.DirectorySeparatorChar);
			BaseRoot = BaseRoot.Replace('/', Path.DirectorySeparatorChar);
		}
	}
}