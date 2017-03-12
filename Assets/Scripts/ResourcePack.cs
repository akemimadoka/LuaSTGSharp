using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.IO.Compression;
using Ionic.Zip;

namespace Assets.Scripts
{
	public sealed class ResourcePack : IResourceDataProvider
	{
		private readonly ZipFile _zipFile;
		
		public ResourcePack(string path, string password = null)
		{
			_zipFile = ZipFile.Read(path);
			if (password != null)
			{
				_zipFile.Password = password;
			}
		}

		public ResourcePack(Stream stream, string password = null)
		{
			_zipFile = ZipFile.Read(stream);
			if (password != null)
			{
				_zipFile.Password = password;
			}
		}

		public void Dispose()
		{
			_zipFile.Dispose();
		}

		public Stream GetResource(string path, object additionalData = null)
		{
			return GetFileData(path, additionalData as string);
		}

		public Stream GetFileData(string path, string password = null)
		{
			var entry = _zipFile[path];

			if (entry == null)
			{
				return null;
			}

			if (password != null)
			{
				entry.Password = password;
			}

			var stream = new MemoryStream((int) entry.UncompressedSize);
			entry.Extract(stream);
			return stream;
		}
	}
}
