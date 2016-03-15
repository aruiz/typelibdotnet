using System.IO;
using System.Runtime.InteropServices;
using System;
using System.Collections.Generic;

namespace GLib.Typelib {
	public class Parser
	{
		private Header h;
		private string[] dependencies;
		private DirEntry[] directory;
		private Section[] sections;
		public Parser (string file)
		{
			using (var reader = new BinaryReader (File.Open (file, FileMode.Open))) {
				h = UnpackStruct<Header> (reader);
				dependencies = GetDependencies (reader, (long)h.dependencies);
				directory = GetDirectoryEntries (reader, (long)h.directory, (ushort) h.n_entries);
				sections = GetSections (reader, (long)h.sections);
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
			var directory = MarshalArray<DirEntry> (reader, (uint) entries, offset, true);

			foreach (var e in directory) {
				var pos = reader.BaseStream.Position;

				//Console.WriteLine("{0} {1} {2}", e.type.ToString(), e.IsLocal(), GetStringFromOffset (reader, (long) e.name));

				reader.BaseStream.Position = pos;
			}
			return directory;
		}

		private static string[] GetDependencies (BinaryReader reader, long offset)
		{
			return GetStringFromOffset(reader, offset).Split ('|');
		}

		private static Section[] GetSections (BinaryReader reader, long offset)
		{
			var sections = new List<Section> ();

			reader.BaseStream.Seek (offset, SeekOrigin.Begin);
			while (true) {
				var section = UnpackStruct<Section> (reader);

				if (section.id == SectionType.END)
					break;

				sections.Add (section);
			}

			return sections.ToArray ();
		}

		public static T[] MarshalArray<T> (BinaryReader reader, uint length, long offset = 0, bool seek = false) {
			var result = new T[length];
			if (seek)
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

			/* In here we place index at the most significant bit of each value depending on
			 * the endianness of the architecture.
			 *
			 * We've followed this doc to understand how alignment works: http://mjfrazer.org/mjfrazer/bitfields/
			 */

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
	}
}