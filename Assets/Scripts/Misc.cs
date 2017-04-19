using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

public static partial class ExtendClass
{
	public static void CopyTo(this Stream from, Stream to, int bufferSize = 1024)
	{
		var buffer = new byte[bufferSize];
		var highwaterMark = 0;
		int read;
		while ((read = from.Read(buffer, 0, buffer.Length)) != 0)
		{
			if (read > highwaterMark) highwaterMark = read;
			to.Write(buffer, 0, read);
		}
	}
}
