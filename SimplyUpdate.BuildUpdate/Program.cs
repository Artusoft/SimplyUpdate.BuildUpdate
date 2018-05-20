using CommandLine;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Auth;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace SimplyUpdate.BuildUpdate
{
	class Program
	{
		public static int Main(string[] args)
		{
			return CommandLine.Parser.Default.ParseArguments<AzureOptions, PathOptions>(args)
							.MapResult(
								(AzureOptions opts) => SendToAzure(opts).Result,
								(PathOptions opts) => SendToPath(opts),
								errs => 1);
		}

		public static async Task<int> SendToAzure(AzureOptions opts)
		{
			var zipFile = CreateZipFile(opts.Source);

			Console.WriteLine("Hashing zip file.");
			var MD5Hash = ComputeHash(zipFile);

			Console.WriteLine("Upload zip file.");
			var storageCredentials = new StorageCredentials(opts.AccountName, opts.AccountKey);
			var cloudStorageAccount = new CloudStorageAccount(storageCredentials, true);
			var cloudBlobClient = cloudStorageAccount.CreateCloudBlobClient();

			var container = cloudBlobClient.GetContainerReference(opts.ContainerName);
			await container.CreateIfNotExistsAsync();
			
			var zipBlob = container.GetBlockBlobReference(opts.Destination + "/software.zip");
			await zipBlob.UploadFromFileAsync(zipFile);

			Console.WriteLine("Update version.");
			var xmlBlob = container.GetBlockBlobReference(opts.Destination + "/software.xml");

			XDocument doc;
			if (await xmlBlob.ExistsAsync())
				doc = XDocument.Parse(await xmlBlob.DownloadTextAsync());
			else
				doc = new XDocument(
								new XElement("Liveupdate",
								new XElement("Version", 0)
								));

			var nd = doc.Descendants("Version").FirstOrDefault();
			Int32 previousVer=0;
			Int32 currentVer=0;
			if (nd != null)
				nd.Value = (currentVer = (previousVer = Convert.ToInt32(nd.Value)) + 1).ToString();

			nd = doc.Descendants("MD5").FirstOrDefault();
			if (nd == null)
				doc.Element("Liveupdate").Add(new XElement("MD5", Convert.ToBase64String(MD5Hash)));
			else
				nd.Value = Convert.ToBase64String(MD5Hash);

			nd = doc.Descendants("FileLenght").FirstOrDefault();
			if (nd == null)
				doc.Element("Liveupdate").Add(new XElement("FileLenght", (new System.IO.FileInfo(zipFile)).Length));
			else
				nd.Value = (new System.IO.FileInfo(zipFile)).Length.ToString();

			await xmlBlob.UploadTextAsync(doc.ToString());

			File.Delete(zipFile);

			Console.WriteLine($"Published version {previousVer} -> {currentVer}");

			return 0;
		}

		private static String CreateZipFile(String source)
		{
			DirectoryInfo pathSource = new DirectoryInfo(source);

			if (!pathSource.Exists) return String.Empty;

			String pathTemp = Path.Combine(Path.GetTempPath(), "SimplyUpdate");
			if (!Directory.Exists(pathTemp)) Directory.CreateDirectory(pathTemp);

			String zipFile = Path.Combine(pathTemp, "software.zip");
			if (File.Exists(zipFile)) File.Delete(zipFile);

			List<String> files = new List<string>();

			files.AddRange(from f in pathSource.GetFiles("*.dll", SearchOption.AllDirectories)
										 select f.FullName);
			files.AddRange(from f in pathSource.GetFiles("*.config", SearchOption.AllDirectories)
										 select f.FullName);
			files.AddRange(from f in pathSource.GetFiles("*.exe", SearchOption.AllDirectories)
										 where !f.Name.Contains("vshost")
										 select f.FullName);

			Console.WriteLine("Zip files.");
			using (ZipArchive archive = ZipFile.Open(zipFile, ZipArchiveMode.Create))
				files.ForEach(f => archive.CreateEntryFromFile(f, f.Remove(0, pathSource.FullName.Length + 1), CompressionLevel.Optimal));

			return zipFile;
		}

		public static int SendToPath(PathOptions opts)
		{
			var zipFile = CreateZipFile(opts.Source);

			Console.WriteLine("Hashing zip file.");
			var MD5Hash = ComputeHash(zipFile);
			Console.WriteLine("Copy zip file.");

			File.Copy(zipFile, Path.Combine(opts.Destination, "software.zip"), true);

			Console.WriteLine("Update version.");
			var xmlFile = Path.Combine(opts.Destination, "software.xml");
			XDocument doc;
			if (File.Exists(xmlFile))
				doc = XDocument.Load(xmlFile);
			else
				doc = new XDocument(
					new XElement("Liveupdate",
					new XElement("Version", 0)
					));

			var nd = doc.Descendants("Version").FirstOrDefault();
			Int32 previousVer = 0;
			Int32 currentVer = 0;
			if (nd != null)
				nd.Value = (currentVer = (previousVer = Convert.ToInt32(nd.Value)) + 1).ToString();

			nd = doc.Descendants("MD5").FirstOrDefault();
			if (nd == null)
				doc.Element("Liveupdate").Add(new XElement("MD5", Convert.ToBase64String(MD5Hash)));
			else
				nd.Value = Convert.ToBase64String(MD5Hash);

			nd = doc.Descendants("FileLenght").FirstOrDefault();
			if (nd == null)
				doc.Element("Liveupdate").Add(new XElement("FileLenght", (new System.IO.FileInfo(zipFile)).Length));
			else
				nd.Value = (new System.IO.FileInfo(zipFile)).Length.ToString();

			doc.Save(xmlFile);

			System.IO.File.Delete(zipFile);

			Console.WriteLine($"Published version {previousVer} -> {currentVer}");

			return 0;
		}

		private static Byte[] ComputeHash(String filePath)
		{
			byte[] hashValue;

			using (MD5 myMD5 = MD5.Create())
			using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				hashValue = myMD5.ComputeHash(fileStream);
				fileStream.Close();
			}

			return hashValue;
		}
	}
}
