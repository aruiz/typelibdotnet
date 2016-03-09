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

		public bool GetIsLocal  () {
			return (raw_local_reserved & 0x01) == 1 ? true : false ;
		}
	}
		
	public class Parser
	{
		private Header h;
		private string[] dependencies;
		private List<DirEntry> directory;
		public Parser ()
		{
			using (var reader = new BinaryReader (File.Open ("./Gst-1.0.typelib", FileMode.Open))) {
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

				Console.WriteLine("{0} {1} {2}", e.type.ToString(), e.GetIsLocal(), GetStringFromOffset (reader, (long) e.name));

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