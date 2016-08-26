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

	public enum ArrayType : uint {
		C,
		ARRAY,
		PTR_ARRAY,
		BYTE_ARRAY
	}

	public enum TypeTag : uint {
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

	public struct TypeMaps {
		
		public static Dictionary <TypeTag, Type> tagmap = new Dictionary <TypeTag, Type> {
			{TypeTag.VOID,    typeof(void)},
			{TypeTag.BOOLEAN, typeof(bool)},
			{TypeTag.INT8,    typeof(sbyte)},
			{TypeTag.UINT8,   typeof(byte)},
			{TypeTag.INT16,   typeof(Int16)},
			{TypeTag.UINT16,  typeof(UInt16)},
			{TypeTag.INT32, typeof (Int32)},
			{TypeTag.UINT32,typeof (UInt32)},
			{TypeTag.INT64, typeof (Int64)},
			{TypeTag.UINT64,typeof (UInt64)},
			{TypeTag.FLOAT, typeof (float)},
			{TypeTag.DOUBLE,typeof (double)},
			{TypeTag.GTYPE, typeof (GType)},
			{TypeTag.UTF8,  typeof (string)},
			{TypeTag.FILENAME, typeof (string)},
			{TypeTag.ARRAY,    typeof (Array)},
			//{TypeTag.INTERFACE,typeof ()},
			//{TypeTag.GLIST,    typeof ()},
			//{TypeTag.GSLIST,   typeof ()},
			//{TypeTag.GHASH,    typeof ()},
			//{TypeTag.ERROR,    typeof ()},
			//{TypeTag.UNICHAR,  typeof ()},
		};

		//Marshaller map
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

	public enum VFuncInfoFlags : uint {
		MUST_CHAIN_UP,
		MUST_OVERRIDE,
		MUST_NOT_OVERRIDE,
		THROWS
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct AttributeIter {
		IntPtr a;
		IntPtr b;
		IntPtr c;
		IntPtr d;
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

			Console.WriteLine ("DISPOSE");
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
				args.Add(new ArgInfo (g_callable_info_get_arg (_handle, i)));
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

	class SignalInfo : CallableInfo {
		internal SignalInfo (IntPtr handle) {
			_handle = handle;
		}
	}

	class VFuncInfo : CallableInfo {
		internal VFuncInfo (IntPtr handle) {
			_handle = handle;
		}
		[DllImport(Consts.GISO)]
		private static extern VFuncInfoFlags g_vfunc_info_get_flags (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern int g_vfunc_info_get_offset (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern IntPtr g_vfunc_info_get_signal (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern IntPtr  g_vfunc_info_get_invoker (IntPtr info);
		[DllImport(Consts.GISO)]
		private static extern IntPtr g_vfunc_info_get_address (IntPtr info, uint gtype, IntPtr error);

		public VFuncInfoFlags Flags { get { return g_vfunc_info_get_flags (_handle); } }
		public int Offset { get { return g_vfunc_info_get_offset (_handle); } }
		public SignalInfo Signal { get { return new SignalInfo (g_vfunc_info_get_signal (_handle)); } }
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

		[DllImport(Consts.GISO)]
		public static extern IntPtr g_type_tag_to_string (TypeTag tag);
		[DllImport(Consts.GISO)]
		public static extern IntPtr g_info_type_to_string (InfoType info);
		[DllImport(Consts.GISO)]
		public static extern bool g_type_info_is_pointer (IntPtr info);
		[DllImport(Consts.GISO)]
		public static extern TypeTag g_type_info_get_tag (IntPtr info);
		[DllImport(Consts.GISO)]
		public static extern IntPtr g_type_info_get_param_type (IntPtr info, int n);
		[DllImport(Consts.GISO)]
		public static extern IntPtr g_type_info_get_interface (IntPtr info);
		[DllImport(Consts.GISO)]
		public static extern int	g_type_info_get_array_length (IntPtr info);
		[DllImport(Consts.GISO)]
		public static extern int	g_type_info_get_array_fixed_size (IntPtr info);
		[DllImport(Consts.GISO)]
		public static extern bool g_type_info_is_zero_terminated (IntPtr info);
		[DllImport(Consts.GISO)]
		public static extern ArrayType g_type_info_get_array_type (IntPtr info);

		string TagToString (TypeTag tag) {
			return Marshal.PtrToStringAnsi (g_type_tag_to_string (tag));
		}

		string InfoTypeToString (InfoType type) {
			return Marshal.PtrToStringAnsi (g_info_type_to_string (type));
		}

		public bool IsPointer () {
			return g_type_info_is_pointer (_handle);
		}

		public TypeTag GetTag () {
			return g_type_info_get_tag (_handle);
		}

		public TypeInfo GetParamType (int n) {
			return new TypeInfo (g_type_info_get_param_type (_handle, n));
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

		public static void create_methods (GIRepository gir, string ns) {
			var ab = AppDomain.CurrentDomain.DefineDynamicAssembly (new AssemblyName (ns),
				AssemblyBuilderAccess.RunAndSave);
			var mb = ab.DefineDynamicModule (ns, true);

			foreach (var i in gir.GetAllInfos (ns)) {
				if (i.Type == InfoType.FUNCTION) {
					bool cont = true;
					var f = new FunctionInfo(i._handle);
					Console.WriteLine ("{0} {1} {2}", f.Name, f.GetSymbol (), MangleName (f.Name));

					var rettype = f.GetReturnType ();
					var rettag = rettype.GetTag ();
					var fargs = f.GetArgs ();

					if (!TypeMaps.tagmap.ContainsKey (rettag))
						continue;

					Type managedRettype = rettype.IsPointer () ? typeof(IntPtr) : TypeMaps.tagmap [rettag];

					List<Type> argtypes = new List<Type> ();

					foreach (var a in fargs) {
						var argtinfo = a.GetTypeInfo ();
						var tag = argtinfo.GetTag ();
						if (!TypeMaps.tagmap.ContainsKey (tag)) {
							cont = false;
							break;
						}
						Type at;
						if (argtinfo.IsPointer () ||
							a.GetDirection() == Direction.OUT ||
							a.GetDirection() == Direction.INOUT)
							at = typeof(IntPtr);
						else
							at = TypeMaps.tagmap [tag];
						
						argtypes.Add (at);
					}

					if (!cont)
						continue;

					var internalmethod = mb.DefinePInvokeMethod (f.GetSymbol (), gir.GetSharedLibrary (ns),
						MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.PinvokeImpl,
						CallingConventions.Any,
						managedRettype	,
						argtypes.ToArray (),
						CallingConvention.Cdecl,
						CharSet.Ansi);
					internalmethod.SetImplementationFlags (MethodImplAttributes.PreserveSig | internalmethod.GetMethodImplementationFlags ());
				}
			}

			mb.CreateGlobalFunctions ();
			ab.Save ("assembly.dll");
		}

		private static bool CheckIsRef (ArgInfo ai) {
			var ti = ai.GetTypeInfo ();

			if (ti.IsPointer ())
				return true;

			switch (ai.GetDirection ()) {
				case Direction.OUT:
				case Direction.INOUT:
					return true;
				default:
					break;
			}

			switch (ai.GetTypeInfo ().GetTag ()) {
				case TypeTag.UTF8: 
				case TypeTag.FILENAME:
					return true;
				default:
					break;
			}
			
			return false;
		}

		public static void Main (string[] args) {
			var gir = new GIRepository ();
			gir.Require ("Soup");
			create_methods (gir, "Soup");
		}
	}
}