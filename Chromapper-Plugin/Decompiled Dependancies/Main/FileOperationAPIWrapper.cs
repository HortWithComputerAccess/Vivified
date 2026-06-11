using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

public class FileOperationAPIWrapper
{
	[Flags]
	public enum FileOperationFlags : ushort
	{
		FOF_SILENT = 4,
		FOF_NOCONFIRMATION = 0x10,
		FOF_ALLOWUNDO = 0x40,
		FOF_SIMPLEPROGRESS = 0x100,
		FOF_NOERRORUI = 0x400,
		FOF_WANTNUKEWARNING = 0x4000
	}

	public enum FileOperationType : uint
	{
		FO_MOVE = 1u,
		FO_COPY,
		FO_DELETE,
		FO_RENAME
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	private struct SHFILEOPSTRUCT
	{
		public readonly IntPtr hwnd;

		[MarshalAs(UnmanagedType.U4)]
		public FileOperationType wFunc;

		public string pFrom;

		public readonly string pTo;

		public FileOperationFlags fFlags;

		[MarshalAs(UnmanagedType.Bool)]
		public readonly bool fAnyOperationsAborted;

		public readonly IntPtr hNameMappings;

		public readonly string lpszProgressTitle;
	}

	[DllImport("shell32.dll", CharSet = CharSet.Auto)]
	private static extern int SHFileOperation(ref SHFILEOPSTRUCT FileOp);

	[DllImport("/System/Library/Frameworks/CoreServices.framework/CoreServices", CharSet = CharSet.Auto)]
	private static extern int FSPathMoveObjectToTrashSync(string sourcePath, out string targetPath, uint options);

	public static bool Send(string path, FileOperationFlags flags)
	{
		try
		{
			SHFILEOPSTRUCT FileOp = new SHFILEOPSTRUCT
			{
				wFunc = FileOperationType.FO_DELETE,
				pFrom = path + "\0\0",
				fFlags = (FileOperationFlags.FOF_ALLOWUNDO | flags)
			};
			SHFileOperation(ref FileOp);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static bool Send(string path)
	{
		return Send(path, FileOperationFlags.FOF_NOCONFIRMATION | FileOperationFlags.FOF_WANTNUKEWARNING);
	}

	public static void MoveToRecycleBin(string path)
	{
		if (!TryWindows(path) && !TryMac(path) && !TryLinux(path))
		{
			if (File.Exists(path))
			{
				File.Delete(path);
			}
			else if (Directory.Exists(path))
			{
				Directory.Delete(path, recursive: true);
			}
		}
	}

	private static bool TryMac(string path)
	{
		try
		{
			string targetPath;
			return FSPathMoveObjectToTrashSync(path, out targetPath, 0u) == 0;
		}
		catch
		{
		}
		return false;
	}

	private static bool TryWindows(string path)
	{
		try
		{
			return Send(path, FileOperationFlags.FOF_SILENT | FileOperationFlags.FOF_NOCONFIRMATION | FileOperationFlags.FOF_NOERRORUI);
		}
		catch
		{
		}
		return false;
	}

	private static bool TryLinux(string path)
	{
		ProcessStartInfo startInfo = new ProcessStartInfo("gio", "trash \"" + path + "\"")
		{
			UseShellExecute = false
		};
		try
		{
			return Process.Start(startInfo) != null;
		}
		catch
		{
		}
		return false;
	}

	private static bool DeleteFile(string path, FileOperationFlags flags)
	{
		try
		{
			SHFILEOPSTRUCT FileOp = new SHFILEOPSTRUCT
			{
				wFunc = FileOperationType.FO_DELETE,
				pFrom = path + "\0\0",
				fFlags = flags
			};
			SHFileOperation(ref FileOp);
			return true;
		}
		catch (Exception)
		{
			return false;
		}
	}

	public static bool DeleteCompletelySilent(string path)
	{
		return DeleteFile(path, FileOperationFlags.FOF_SILENT | FileOperationFlags.FOF_NOCONFIRMATION | FileOperationFlags.FOF_NOERRORUI);
	}
}
