using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace GLib.Typelib
{
	internal enum BlobType : ushort {
		INVALID,
		FUNCTION,
		CALLBACK,
		STRUCT,
		BOXED,
		ENUM,
		FLAGS,
		OBJECT,
		INTERFACE,
		CONSTANT,
		INVALID_0,
		UNION
	}

	internal enum TypeTag : ushort {
		VOID,
		BOOLEAN,
		INT8,
		UINT8,
		INT16,
		UINT16,
		INT32,
		UINT32,
		INT64,
		UINT64,
		FLOAT,
		DOUBLE,
		GTYPE,
		UTF8,
		FILENAME,
		ARRAY,
		INTERFACE,
		GLIST,
		GSLIST,
		GHASH,
		ERROR,
		UNICHAR
	}

	internal enum ScopeType : ushort {
		INVALID,
		CALL,
		ASYNC,
		NOTIFIED
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	struct Header {
		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
		public byte[] magic;
		public byte  major_version;
		public byte  minor_version;

		public ushort reserved;

		public ushort n_entries;
		public ushort n_local_entries;
		public uint directory;

		public uint n_attributes;
		public uint attributes;

		public uint dependencies;

		public uint size;
		public uint @namespace;
		public uint nsversion;
		public uint shared_library;
		public uint c_prefix;
		public ushort entry_blob_size;
		public ushort function_blob_size;
		public ushort callback_blob_size;
		public ushort signal_blob_size;
		public ushort vfunc_blob_size;
		public ushort arg_blob_size;
		public ushort property_blob_size;
		public ushort field_blob_size;
		public ushort value_blob_size;
		public ushort attribute_blob_size;
		public ushort constant_blob_size;
		public ushort error_domain_blob_size;

		public ushort signature_blob_size;
		public ushort enum_blob_size;
		public ushort struct_blob_size;
		public ushort object_blob_size;
		public ushort interface_blob_size;
		public ushort union_blob_size;

		public uint sections;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)]
		public ushort[] padding;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	struct DirEntry {
		public BlobType type;
		internal ushort raw_local_reserved;
		public uint name;
		public uint offset;

		public bool IsLocal  () {
			return Parser.GetBoolFromField (raw_local_reserved, 16, 0);
		}
	}

	struct SimpleTypeBlobFlags {
		/* guint reserved   : 8;
		guint reserved2  :16;
		guint pointer    : 1;
		guint reserved3  : 2;
		guint tag        : 5; */
		uint reserved;

		public bool IsPointer () {
			return Parser.GetBoolFromField(reserved, 32, 24);
		}
		public TypeTag GetTag () {
			return (TypeTag) Parser.GetValueFromField(reserved, 32, 27, 5);;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	struct SimpleTypeBlob {
		SimpleTypeBlobFlags flags;
		uint offset;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	struct ArgBlob {
		uint          name;

		uint          reserved;
		byte          closure;
		byte          destroy;

		ushort        padding;

		SimpleTypeBlob arg_type;

		public bool IsIn () {
			return Parser.GetBoolFromField(reserved, 32, 0);
		}
		public bool IsOut () {
			return Parser.GetBoolFromField(reserved, 32, 1);
		}
		public bool CallerAllocates () {
			return Parser.GetBoolFromField(reserved, 32, 2);
		}
		public bool Nullable () {
			return Parser.GetBoolFromField(reserved, 32, 3);
		}
		public bool Optional () {
			return Parser.GetBoolFromField(reserved, 32, 4);
		}
		public bool TransferOwnership () {
			return Parser.GetBoolFromField(reserved, 32, 5);
		}
		public bool TransferContainerOwnership () {
			return Parser.GetBoolFromField(reserved, 32, 6);
		}
		public bool IsReturnValue () {
			return Parser.GetBoolFromField(reserved, 32, 7);
		}
		public ScopeType GetScope () {
			return (ScopeType) Parser.GetValueFromField(reserved, 32, 8, 3);
		}
		public bool Skip () {
			return Parser.GetBoolFromField(reserved, 32, 11);
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	struct Signatureblob {
		SimpleTypeBlob return_type;

		ushort        reserved;
		ushort        n_arguments;

		public bool MayReturnNull () {
			return Parser.GetBoolFromField((uint)reserved, 16, 0);
		}
		public bool CallerOwnsReturnValue () {
			return Parser.GetBoolFromField((uint)reserved, 16, 1);
		}
		public bool CallerOwnsReturnContainer () {
			return Parser.GetBoolFromField((uint)reserved, 16, 2);
		}
		public bool SkipReturn () {
			return Parser.GetBoolFromField((uint)reserved, 16, 3);
		}
		public bool InstanceTransferOwnership () {
			return Parser.GetBoolFromField((uint)reserved, 16, 4);
		}
		public bool Throws () {
			return Parser.GetBoolFromField((uint)reserved, 16, 5);
		}
		public ArgBlob[] GetArguments (BinaryReader reader, long offset) {
			return new ArgBlob[0];
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	struct FunctionBlob {
		ushort type;
		internal ushort flags;
		uint name;
		uint symbol;
		uint signature;
		ushort reserved;
		ushort reserved2;

		public bool IsDeprecated () {
			return Parser.GetBoolFromField((uint)flags, 16, 0);
		}
		public bool IsSetter () {
			return Parser.GetBoolFromField((uint)flags, 16, 1);
		}
		public bool IsGetter () {
			return Parser.GetBoolFromField((uint)flags, 16, 2);
		}
		public bool IsConstructor () {
			return Parser.GetBoolFromField((uint)flags, 16, 3);
		}
		public bool WrapsVFunc () {
			return Parser.GetBoolFromField((uint)flags, 16, 4);
		}
		public bool Throws () {
			return Parser.GetBoolFromField((uint)flags, 16, 5);
		}
		public bool GetIndex () {
			return Parser.GetBoolFromField((uint)flags, 16, 6);
		}
		public bool IsStatic () {
			return Parser.GetBoolFromField((uint)flags, 16, 7);
		}
	}
		
	public class Parser
	{
		private Header h;
		private string[] dependencies;
		private DirEntry[] directory;
		public Parser ()
		{
			using (var reader = new BinaryReader (File.Open ("./Soup-2.4.typelib", FileMode.Open))) {
				h = UnpackStruct<Header> (reader);
				dependencies = GetDependencies (reader, (long)h.dependencies);
				directory = GetDirectoryEntries (reader, (long)h.directory, (ushort) h.n_entries);
			}
		}

		private static T UnpackStruct<T> (BinaryReader reader, long offset = 0, bool seek = false) {
			if (seek)
				reader.BaseStream.Seek (offset, SeekOrigin.Begin);
			var data = reader.ReadBytes (Marshal.SizeOf (typeof(T)));
			var entry = (T)Marshal.PtrToStructure (GCHandle.Alloc (data, GCHandleType.Pinned).AddrOfPinnedObject (),
				typeof(T));
			return entry;
		}

		private static DirEntry[] GetDirectoryEntries (BinaryReader reader, long offset, ushort entries) {
			var directory = MarshalArray<DirEntry> (reader, offset, (uint) entries);

			foreach (var e in directory) {
				var pos = reader.BaseStream.Position;

				Console.WriteLine("{0} {1} {2}", e.type.ToString(), e.IsLocal(), GetStringFromOffset (reader, (long) e.name));

				reader.BaseStream.Position = pos;
			}
			return directory;
		}

		private static string[] GetDependencies (BinaryReader reader, long offset)
		{
			return GetStringFromOffset(reader, offset).Split ('|');
		}

		public static T[] MarshalArray<T> (BinaryReader reader, long offset, uint length) {
			var result = new T[length];
			reader.BaseStream.Seek (offset, SeekOrigin.Begin);
			for (uint i = 0; i < length; i++) {
				result[i] = UnpackStruct<T> (reader);
			}
			return result;
		}

		public static string GetStringFromOffset (BinaryReader reader, long offset) {
			reader.BaseStream.Seek (offset, SeekOrigin.Begin);
			var data = "";
			for (byte c = reader.ReadByte (); c != 0; c = reader.ReadByte ()) {
				data += Convert.ToChar (c);
			}

			return data;
		}

		public static bool GetBoolFromField (uint field, uint fieldLength, uint index) {
			return GetValueFromField (field, fieldLength, index, 1) == 1;
		}

		public static uint GetValueFromField (uint field, uint fieldLength, uint index, uint length) {
			uint value = 0;

			if (BitConverter.IsLittleEndian)
				index = index + length - 1;
			else
				index = fieldLength - 1 - index;

			while (length > 0) {
				if ((field & ((uint)0x1 << (int)index)) != 0x0)
					value = value | ((uint)0x1 << (int)(length - 1));

				length--;
			}

			return value;
		}

		public static void Main (string[] args) {
			new Parser();
		}
	}
}