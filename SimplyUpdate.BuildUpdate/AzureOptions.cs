using CommandLine;
using System;
using System.Collections.Generic;
using System.Text;

namespace SimplyUpdate.BuildUpdate
{
	[Verb("azure", HelpText = "Publish update to azure storage")]
	class AzureOptions:BaseOptions
    {
		[Option('c', "container", Required = true)]
		public string ContainerName { get; set; }

		[Option('a', "accountname", Required = true)]
		public string AccountName { get; set; }

		[Option('k', "accountkey", Required = true)]
		public string AccountKey { get; set; }

	}
}
