using System;
using System.Collections.Generic;
using System.Text;

namespace DaramRenamer
{
	public enum ErrorCode
	{
		NoError,
		Unknown,
		UnauthorizedAccess,
		PathTooLong,
		DirectoryNotFound,
		IOError,
		FailedOverwrite,
		FileNotFound,
	}
}
