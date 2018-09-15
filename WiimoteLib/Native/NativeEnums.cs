using System;

namespace WiimoteLib.Native {
	[Flags]
	internal enum LookupFlags : uint {
		None = 0,
		Containers = 0x0002,
		ReturnName = 0x0010,
		ReturnAddr = 0x0100,
		ReturnBlob = 0x0200,
		FlushCache = 0x1000,
		ResService = 0x8000,
	}

	[Flags]
	internal enum EFileAttributes : uint {
		Readonly = 0x00000001,
		Hidden = 0x00000002,
		System = 0x00000004,
		Directory = 0x00000010,
		Archive = 0x00000020,
		Device = 0x00000040,
		Normal = 0x00000080,
		Temporary = 0x00000100,
		SparseFile = 0x00000200,
		ReparsePoint = 0x00000400,
		Compressed = 0x00000800,
		Offline = 0x00001000,
		NotContentIndexed = 0x00002000,
		Encrypted = 0x00004000,
		Write_Through = 0x80000000,
		Overlapped = 0x40000000,
		NoBuffering = 0x20000000,
		RandomAccess = 0x10000000,
		SequentialScan = 0x08000000,
		DeleteOnClose = 0x04000000,
		BackupSemantics = 0x02000000,
		PosixSemantics = 0x01000000,
		OpenReparsePoint = 0x00200000,
		OpenNoRecall = 0x00100000,
		FirstPipeInstance = 0x00080000,
	}

	[Flags]
	internal enum DeviceInfoFlags : uint {
		Default = 0x00000001, // only valid with DIGCF_DEVICEINTERFACE
		Present = 0x00000002,
		AllClasses = 0x00000004,
		Profile = 0x00000008,
		DeviceInterface = 0x00000010,
	}
	[Flags]
	internal enum BluetoothServiceFlags : int {
		Disable = 0,
		Enable = 1,
	}
}
