using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace GLib.Introspection
{
	public enum RepositoryLoadFlags : uint {
		LAZY
	}

	public enum InfoType : uint {
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
		UNION,
		VALUE,
		SIGNAL,
		VFUNC,
		PROPERTY,
		FIELD,
		ARG,
		TYPE,
		UNRESOLVED
	}

	public enum Direction : uint {
		IN,
		OUT,
		INOUT
	}

	public enum ScopeType : uint {
		INVALID,
		CALL,
		ASYNC,
		NOTIFIED
	}

	public enum Transfer : uint {
		NOTHING,
		CONTAINER,
		EVERYTHING
	}

	public enum FunctionInfoFlags : uint {
		IS_METHOD,
		IS_CONSTRUCTOR,
		IS_GETTER,
		IS_SETTER,
		WRAPS_VFUNC,
		THROWS
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct AttributeIter {
		IntPtr a;
		IntPtr b;
		IntPtr c;
		IntPtr d;

		public AttributeIter () {
			a = IntPtr.Zero;
			b = IntPtr.Zero;
			c = IntPtr.Zero;
			d = IntPtr.Zero;
		}
	}

	internal class Consts {
		internal const string GISO = "libgirepository-1.0.so";
	}

	class Typelib {
		internal IntPtr _handle;

		internal Typelib (IntPtr handle) {
			_handle = handle;
		}
	}

	class BaseInfo : IDisposable {
		internal IntPtr _handle = IntPtr.Zero;
		private bool disposed = false;

		internal BaseInfo (IntPtr handle) {
			_handle = handle;
		}

		internal BaseInfo () {}

		[DllImport(Consts.GISO)]
		private static extern IntPtr g_base_info_ref (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern void g_base_info_unref (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern bool	g_base_info_equal (IntPtr a, IntPtr b);
		[DllImport(Consts.GISO)]
		private static extern InfoType	g_base_info_get_type (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern IntPtr g_base_info_get_typelib (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern IntPtr g_base_info_get_namespace (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern IntPtr g_base_info_get_name (IntPtr info);
		[DllImport(Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern IntPtr g_base_info_get_attribute (IntPtr info, string name);
		[DllImport(Consts.GISO)]
		private static extern bool g_base_info_iterate_attributes (IntPtr info, ref AttributeIter iter, out string name, out string value);
		[DllImport(Consts.GISO)]
		private static extern IntPtr g_base_info_get_container (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern bool g_base_info_is_deprecated (IntPtr info);

		public delegate void AttributeDelegate (string name, string value);

		public InfoType Type { get { return g_base_info_get_type (_handle); } }
		public Typelib Typelib { get { return new Typelib (g_base_info_get_typelib (_handle)); } }
		public string Namespace { get { return Marshal.PtrToStringAnsi (g_base_info_get_namespace (_handle)); } }
		public string Name { get { return Marshal.PtrToStringAnsi (g_base_info_get_name (_handle)); } }
		public BaseInfo Container { get { return new BaseInfo (g_base_info_get_container (_handle)); } }

		public void IterateAttributes (AttributeDelegate cb) {
			var iter = new AttributeIter ();
			string name, value;

			while (g_base_info_iterate_attributes (_handle, ref iter, out name, out value)) {
				cb (name, value);
			}
		}

		public void Dispose () {
			Dispose (true);
			GC.SuppressFinalize (this);
		}

		public virtual void Dispose(bool disposing) {
			if (this.disposed)
				return;

			g_base_info_unref (_handle);
			_handle = IntPtr.Zero;
		}
	}

	class ArgInfo : BaseInfo {
		[DllImport(Consts.GISO)]
		private static extern int g_arg_info_get_closure (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern int g_arg_info_get_destroy (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern Direction	g_arg_info_get_direction (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern Transfer g_arg_info_get_ownership_transfer (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern ScopeType g_arg_info_get_scope (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern IntPtr g_arg_info_get_type (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern void g_arg_info_load_type (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern bool g_arg_info_may_be_null (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern bool g_arg_info_is_caller_allocates (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern bool g_arg_info_is_optional (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern bool g_arg_info_is_return_value (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern bool g_arg_info_is_skip (IntPtr info);

		internal ArgInfo (IntPtr handle)
		{
			_handle = handle;
		}

		public TypeInfo GetTypeInfo () {
			return new TypeInfo (g_arg_info_get_type (_handle));
		}

		public bool IsSkip () {
			return g_arg_info_is_skip (_handle);
		}

		public bool IsReturnValue () {
			return g_arg_info_is_return_value (_handle);
		}

		public bool IsOptional () {
			return g_arg_info_is_optional (_handle);
		}

		public bool IsCallerAllocates () {
			return g_arg_info_is_caller_allocates (_handle);
		}

		public bool MayBeNull () {
			return g_arg_info_may_be_null (_handle);
		}

		public ScopeType GetScope () {
			return g_arg_info_get_scope (_handle);
		}

		public Transfer GetOwnershipTransfer () {
			return g_arg_info_get_ownership_transfer (_handle);
		}

		public Direction GetDirection () {
			return g_arg_info_get_direction (_handle);
		}

		public int GetDestroyIndex () {
			return g_arg_info_get_destroy (_handle);
		}

		public int GetClosureIndex () {
			return g_arg_info_get_closure (_handle);
		}
	}

	class CallableInfo : BaseInfo {
		[DllImport(Consts.GISO)]
		private static extern bool g_callable_info_can_throw_gerror (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern int g_callable_info_get_n_args (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern IntPtr g_callable_info_get_arg (IntPtr info, int n);
		[DllImport(Consts.GISO)]
		private static extern Transfer g_callable_info_get_caller_owns (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern IntPtr g_callable_info_get_return_type (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern bool g_callable_info_is_method (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern bool g_callable_info_may_return_null (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern bool g_callable_info_skip_return (IntPtr info);
		[DllImport(Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern IntPtr g_callable_info_get_return_attribute (IntPtr info, string name);
		/*[DllImport(Consts.GISO)]
		private static extern void g_callable_info_load_return_type (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern bool g_callable_info_iterate_return_attributes (IntPtr info);
		[DllImport(Consts.GISO)]
		boolean g_callable_info_invoke ();
		[DllImport(Consts.GISO)]
		private static extern void g_callable_info_load_arg (IntPtr info);*/

		public bool Throws () {
			return g_callable_info_can_throw_gerror (_handle);
		}

		public ArgInfo[] GetArgs () {
			List<ArgInfo> args = new List<ArgInfo> ();
			var n_args = g_callable_info_get_n_args (_handle);

			for (int i = 0; i < n_args; i++) {
				args.Add(new ArgInfo (g_callable_info_get_arg (_handle, n_args)));
			}

			return args.ToArray ();
		}

		public Transfer GetCallerOwns () {
			return g_callable_info_get_caller_owns (_handle);
		}

		public TypeInfo GetReturnType () {
			return new TypeInfo (g_callable_info_get_return_type (_handle));
		}

		public bool IsMethod () {
			return g_callable_info_is_method (_handle);
		}

		public bool MayReturnNull () {
			return g_callable_info_may_return_null (_handle);
		}
			
		public bool SkipReturn () {
			return g_callable_info_skip_return (_handle);
		}
	}

	class FunctionInfo : CallableInfo {
		[DllImport(Consts.GISO)]
		private static extern FunctionInfoFlags g_function_info_get_flags (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern IntPtr g_function_info_get_property (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern IntPtr g_function_info_get_symbol (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern IntPtr g_function_info_get_vfunc (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern bool g_function_info_invoke (IntPtr info);

		internal FunctionInfo (IntPtr handle) {
			_handle = handle;
		}

		public PropertyInfo GetProperty () {
			return new BaseInfo (g_function_info_get_property (_handle)) as PropertyInfo;
		}

		public string GetSymbol () {
			return Marshal.PtrToStringAnsi (g_function_info_get_symbol (_handle));
		}

		public VFuncInfo GetVFunc () {
			return new VFuncInfo (g_function_info_get_vfunc(_handle));
		}
	}

	class VFuncInfo : CallableInfo {
		internal VFuncInfo (IntPtr handle) {
			_handle = handle;
		}
	}

	class ConstantInfo : BaseInfo {
		internal ConstantInfo (IntPtr handle) {
			_handle = handle;
		}
	}

	class FieldInfo : BaseInfo {
		internal FieldInfo (IntPtr handle) {
			_handle = handle;
		}
	}

	class PropertyInfo : BaseInfo {
		internal PropertyInfo (IntPtr handle) {
			_handle = handle;
		}
	}

	class RegisteredTypeInfo : BaseInfo {
		internal RegisteredTypeInfo (IntPtr handle) {
			_handle = handle;
		}
	}

	class TypeInfo : BaseInfo {
		internal TypeInfo (IntPtr handle) {
			_handle = handle;
		}
	}

	class GIRepository {
		internal IntPtr _girepository;

		[DllImport(Consts.GISO)]
		private static extern IntPtr g_irepository_get_default ();
		[DllImport(Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern int g_irepository_get_n_infos (IntPtr repository, string namespace_);
		[DllImport(Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern IntPtr g_irepository_require (IntPtr repository, string namespace_, string version, RepositoryLoadFlags flags, IntPtr error);
		[DllImport(Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern IntPtr g_irepository_require (IntPtr repository, string namespace_, IntPtr version, RepositoryLoadFlags flags, IntPtr error);
		[DllImport(Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern IntPtr g_irepository_get_shared_library (IntPtr repository, string namespace_);
		[DllImport(Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern IntPtr g_irepository_get_dependencies (IntPtr repository, string namespace_);
		[DllImport(Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern IntPtr g_irepository_get_immediate_dependencies (IntPtr repository, string namespace_);
		[DllImport(Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern IntPtr g_irepository_get_loaded_namespaces (IntPtr repository);
		[DllImport(Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern IntPtr g_irepository_get_info (IntPtr repository, string namespace_, int index);
		[DllImport(Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern IntPtr g_irepository_get_option_group ();
		[DllImport(Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern IntPtr g_irepository_enumerate_versions (IntPtr repository, string namespace_);
		[DllImport(Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern void g_irepository_prepend_library_path (string directory);
		[DllImport(Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern void g_irepository_prepend_search_path (string directory);
		[DllImport (Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern IntPtr g_irepository_get_search_path ();
		[DllImport(Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern IntPtr g_irepository_load_typelib (IntPtr repository, IntPtr typelib, RepositoryLoadFlags flags);
		[DllImport(Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern IntPtr g_irepository_get_typelib_path (IntPtr repository, string namespace_);
		[DllImport(Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern bool g_irepository_is_registered (IntPtr repository, string namespace_, string version);
		[DllImport(Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern bool g_irepository_is_registered (IntPtr repository, string namespace_, IntPtr version);
		[DllImport(Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern IntPtr g_irepository_get_c_prefix (IntPtr repository, string namespace_);
		[DllImport(Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern IntPtr g_irepository_get_version (IntPtr repository, string namespace_);
		[DllImport(Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern IntPtr g_irepository_find_by_gtype (IntPtr repository, GLib.GType type);
		[DllImport(Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern IntPtr g_irepository_find_by_error_domain (IntPtr repository, UInt32 quark);
		[DllImport(Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern IntPtr g_irepository_find_by_name (IntPtr repository, string namespace_, string name);
		[DllImport(Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern bool g_irepository_dump (string arg, IntPtr error);
		/*[DllImport(Consts.GISO, CharSet = CharSet.Ansi)]
		private static extern void gi_cclosure_marshal_generic ();*/

		public GIRepository () {
			_girepository = g_irepository_get_default ();
		}

		public void Require (string @namespace, string version = "") {
			if (version == "")
				g_irepository_require (_girepository, @namespace, IntPtr.Zero, RepositoryLoadFlags.LAZY, IntPtr.Zero);
			g_irepository_require (_girepository, @namespace, version, RepositoryLoadFlags.LAZY, IntPtr.Zero);
		}

		public int GetNInfos (string @namespace) {
			return g_irepository_get_n_infos (_girepository, @namespace);
		}

		public string GetSharedLibrary (string @namespace) {
			var ptr = g_irepository_get_shared_library (_girepository, @namespace);

			if (ptr == IntPtr.Zero)
				return null;

			var s = Marshal.PtrToStringAnsi (ptr);
			return s;
		}

		public BaseInfo GetInfo (string @namespace, int index) {
			var infoptr = g_irepository_get_info (_girepository, @namespace, index);
			return new BaseInfo (infoptr);
		}

		public BaseInfo[] GetAllInfos (string @namespace) {
			var infos = new List<BaseInfo> ();
			for (int i = 0; i < GetNInfos (@namespace); i++) {
				infos.Add (GetInfo (@namespace, i));
			}

			return infos.ToArray ();
		}
	}
	
	class Program {
		public static string MangleName (string symbol) {
			string mangled = "";

			foreach (var part in symbol.Split("_".ToCharArray ())) {
				var ca = part.ToCharArray ();
				ca [0] = Char.ToUpper (part [0]);
				mangled += new string (ca);
			}

			return mangled;
		}

		public static void Main (string[] args) {
			var gir = new GIRepository ();
			gir.Require ("Soup");

			foreach (var i in gir.GetAllInfos ("Soup")) {
				if (i.Type == InfoType.FUNCTION) {
					var f = new FunctionInfo(i._handle);
					Console.WriteLine ("{0} {1} {2}", f.Name, f.GetSymbol (), MangleName (f.Name));
				}
			}
		}
	}
}