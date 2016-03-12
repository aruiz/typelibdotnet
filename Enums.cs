namespace GLib.Typelib {
	public enum BlobType : ushort {
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

	public enum TypeTag : ushort {
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

	public enum ScopeType : ushort {
		INVALID,
		CALL,
		ASYNC,
		NOTIFIED
	}

	public enum SectionType : uint {
		END,
		INDEX
	}
}
