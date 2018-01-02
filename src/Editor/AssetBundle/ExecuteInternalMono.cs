using UnityEditor;
using UnityEngine;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;

namespace HouraiTeahouse.Loadables.AssetBundles {

	class MonoInstallationFinder {

		public static string GetFrameWorksFolder() {
			var editorAppPath = EditorApplication.applicationPath;
			if (Application.platform == RuntimePlatform.WindowsEditor)
				return Path.Combine(Path.GetDirectoryName(editorAppPath), "Data");
		    if (Application.platform == RuntimePlatform.OSXEditor)
		        return Path.Combine(editorAppPath, Path.Combine("Contents", "Frameworks"));
		    return Path.Combine(Path.GetDirectoryName(editorAppPath), "Data");
		}

		public static string GetProfileDirectory (BuildTarget target, string profile) {
			var monoprefix = GetMonoInstallation();
			return Path.Combine(monoprefix, Path.Combine("lib", Path.Combine("mono", profile)));
		}

		public static string GetMonoInstallation() {
			#if INCLUDE_MONO_2_12
			return GetMonoInstallation("MonoBleedingEdge");
			#else
			return GetMonoInstallation("Mono");
			#endif
		}

		public static string GetMonoInstallation(string monoName) {
			return Path.Combine(GetFrameWorksFolder(), monoName);
		}
	}

	class ExecuteInternalMono {

	    static readonly Regex UnsafeCharsWindows = new Regex("[^A-Za-z0-9\\_\\-\\.\\:\\,\\/\\@\\\\]");
	    static readonly Regex UnescapeableChars = new Regex("[\\x00-\\x08\\x10-\\x1a\\x1c-\\x1f\\x7f\\xff]");
	    static readonly Regex Quotes = new Regex("\"");

		public ProcessStartInfo processStartInfo = null;

		public static string PrepareFileName(string input) {
			if (Application.platform == RuntimePlatform.OSXEditor)
				return EscapeCharsQuote(input);
			return EscapeCharsWindows(input);
		}

		public static string EscapeCharsQuote(string input) {
			if (input.IndexOf('\'') == -1)
				return "'" + input + "'";
			if (input.IndexOf('"') == -1)
				return "\"" + input + "\"";
			return null;
		}

		public static string EscapeCharsWindows(string input) {
			if (input.Length == 0)
				return "\"\"";
			if (UnescapeableChars.IsMatch(input)) {
				UnityEngine.Debug.LogWarning("Cannot escape control characters in string");
				return "\"\"";
			}
			if (UnsafeCharsWindows.IsMatch(input))
				return "\"" + Quotes.Replace(input, "\"\"") + "\"";
			return input;
		}

		public static ProcessStartInfo GetProfileStartInfoForMono(string monodistribution, string profile, string executable, string arguments, bool setMonoEnvironmentVariables) {
			var monoexe = PathCombine(monodistribution, "bin", "mono");
			var profileAbspath = PathCombine(monodistribution, "lib", "mono", profile);
			if (Application.platform == RuntimePlatform.WindowsEditor)
				monoexe = PrepareFileName(monoexe + ".exe");

			var startInfo = new ProcessStartInfo {
				Arguments = PrepareFileName(executable) + " " + arguments,
				CreateNoWindow = true,
				FileName = monoexe,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				WorkingDirectory = Application.dataPath + "/..",
				UseShellExecute = false
			};

			if (setMonoEnvironmentVariables) {
				startInfo.EnvironmentVariables["MONO_PATH"] = profileAbspath;
				startInfo.EnvironmentVariables["MONO_CFG_DIR"] = PathCombine(monodistribution, "etc");
			}
			return startInfo;
		}

		static string PathCombine(params string[] parts) {
			var path = parts[0];
			for (var i = 1; i < parts.Length; ++i)
				path = Path.Combine(path, parts[i]);
			return path;
		}
	}
}
