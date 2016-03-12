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
			return (raw_local_reserved & 0x01) == 1 ? true : false ;
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
			return false;
		}
		public TypeTag GetTag () {
			return TypeTag.VOID;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	struct SimpleTypeBlob {
		SimpleTypeBlobFlags flags;
		uint offset;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	struct ArgBlob {
		uint        name;

		/*guint          in                           : 1;
		guint          out                          : 1;
		guint          caller_allocates             : 1;
		guint          nullable                     : 1;
		guint          optional                     : 1;
		guint          transfer_ownership           : 1;
		guint          transfer_container_ownership : 1;
		guint          return_value                 : 1;
		guint          scope                        : 3;
		guint          skip                         : 1; */
		uint          reserved; /* 20 */
		byte          closure;
		byte          destroy;

		ushort        padding;

		SimpleTypeBlob arg_type;

		public bool IsIn () {
			return false;
		}
		public bool IsOut () {
			return false;
		}
		public bool CallerAllocates () {
			return false;
		}
		public bool Nullable () {
			return false;
		}
		public bool Optional () {
			return false;
		}
		public bool TransferOwnership () {
			return false;
		}
		public bool TransferContainerOwnership () {
			return false;
		}
		public bool IsReturnValue () {
			return false;
		}
		public ScopeType GetScope () {
			return ScopeType.INVALID;
		}
		public bool Skip () {
			return false;
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	struct Signatureblob {
		SimpleTypeBlob return_type;

		/*guint16        may_return_null              : 1;
		guint16        caller_owns_return_value     : 1;
		guint16        caller_owns_return_container : 1;
		guint16        skip_return                  : 1;
		guint16        instance_transfer_ownership  : 1;
		guint16        throws                       : 1; */
		ushort        reserved; /* :10 */
		ushort        n_arguments;
		/* ArgBlob[n_arguments] */

		public bool MayReturnNull () {
			return false;
		}
		public bool CallerOwnsReturnValue () {
			return false;
		}
		public bool CallerOwnsReturnContainer () {
			return false;
		}
		public bool SkipReturn () {
			return false;
		}
		public bool InstanceTransferOwnership () {
			return false;
		}
		public bool Throws () {
			return false;
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
			return false;
		}
		public bool IsSetter () {
			return false;
		}
		public bool IsGetter () {
			return false;
		}
		public bool IsConstructor () {
			return false;
		}
		public bool WrapsVFunc () {
			return false;
		}
		public bool Throws () {
			return false;
		}
		public bool GetIndex () {
			return false;
		}
		public bool IsStatic () {
			return false;
		}
	}
		
	public class Parser
	{
		private Header h;
		private string[] dependencies;
		private List<DirEntry> directory;
		public Parser ()
		{
			using (var reader = new BinaryReader (File.Open ("./Soup-2.4.typelib", FileMode.Open))) {
				var data = reader.ReadBytes (Marshal.SizeOf(typeof(Header)));
				h = (Header) Marshal.PtrToStructure (GCHandle.Alloc (data,
					                                                 GCHandleType.Pinned).AddrOfPinnedObject (),
					                                 typeof(Header));

				dependencies = GetDependencies (reader, (long)h.dependencies);
				directory = GetDirectoryEntries (reader, (long)h.directory, (ushort) h.n_entries);
			}
			

		}

		private static List<DirEntry> GetDirectoryEntries (BinaryReader reader, long offset, ushort entries) {
			var directory = new List<DirEntry> ();

			reader.BaseStream.Seek (offset, SeekOrigin.Begin);
			for (ushort i = 0; i < entries; i++) {
				var data = reader.ReadBytes (Marshal.SizeOf (typeof(DirEntry)));
				var entry = (DirEntry)Marshal.PtrToStructure (GCHandle.Alloc (data, GCHandleType.Pinned).AddrOfPinnedObject (),
					            typeof(DirEntry));
				directory.Add(entry);
			}

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

		public static string GetStringFromOffset (BinaryReader reader, long offset) {
			reader.BaseStream.Seek (offset, SeekOrigin.Begin);
			var data = "";
			for (byte c = reader.ReadByte (); c != 0; c = reader.ReadByte ()) {
				data += Convert.ToChar (c);
			}

			return data;
		}

		public static void Main (string[] args) {
			new Parser();
		}
	}
}