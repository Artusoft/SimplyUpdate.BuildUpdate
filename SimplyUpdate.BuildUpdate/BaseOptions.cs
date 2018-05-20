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
	}
}
