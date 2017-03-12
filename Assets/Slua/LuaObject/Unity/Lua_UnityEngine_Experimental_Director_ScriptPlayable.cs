using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Experimental_Director_ScriptPlayable : LuaObject {
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int constructor(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.ScriptPlayable o;
			o=new UnityEngine.Experimental.Director.ScriptPlayable();
			pushValue(l,true);
			pushValue(l,o);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.Experimental.Director.ScriptPlayable");
		createTypeMetatable(l,constructor, typeof(UnityEngine.Experimental.Director.ScriptPlayable));
	}
}
