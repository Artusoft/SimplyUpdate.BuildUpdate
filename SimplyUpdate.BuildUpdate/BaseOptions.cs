using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimplyUpdate.BuildUpdate
{
	class BaseOptions
	{
		[Option('s', "source", Required = true, HelpText = "The source path")]
		public string Source { get; set; }

		[Option('d', "destination", Required = true, HelpText = "The destination path")]
		public string Destination { get; set; }

		[Option('v', "version", Required = false, HelpText = "The version of release. If missing the version is in automatic increment")]
		public Int32 Version { get; set; }

		[Option('x', "exclude", Required = false, HelpText = "Files to exclude")]
		public IEnumerable<string> ExcludeFiles { get; set; }

		[Option('i', "include", Required = false, HelpText = "Files to exclude", Default = new String[] { "*.config", "*.exe", "*.dll" })]
		public IEnumerable<string> IncludeFiles { get; set; }

		[Option('f', "fileversion", Required = false, HelpText = "The version of the primary compiled assembly")]
		public String FileVersion { get; set; }
	}
}
