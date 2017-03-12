﻿using System;
using SLua;
using System.Collections.Generic;
[UnityEngine.Scripting.Preserve]
public class Lua_UnityEngine_Experimental_Director_DirectorPlayer : LuaObject {
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int Play(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.DirectorPlayer self=(UnityEngine.Experimental.Director.DirectorPlayer)checkSelf(l);
			UnityEngine.Experimental.Director.Playable a1;
			checkValueType(l,2,out a1);
			self.Play(a1);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int Stop(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.DirectorPlayer self=(UnityEngine.Experimental.Director.DirectorPlayer)checkSelf(l);
			self.Stop();
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int SetTime(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.DirectorPlayer self=(UnityEngine.Experimental.Director.DirectorPlayer)checkSelf(l);
			System.Double a1;
			checkType(l,2,out a1);
			self.SetTime(a1);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int GetTime(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.DirectorPlayer self=(UnityEngine.Experimental.Director.DirectorPlayer)checkSelf(l);
			var ret=self.GetTime();
			pushValue(l,true);
			pushValue(l,ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int SetTimeUpdateMode(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.DirectorPlayer self=(UnityEngine.Experimental.Director.DirectorPlayer)checkSelf(l);
			UnityEngine.Experimental.Director.DirectorUpdateMode a1;
			checkEnum(l,2,out a1);
			self.SetTimeUpdateMode(a1);
			pushValue(l,true);
			return 1;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[MonoPInvokeCallbackAttribute(typeof(LuaCSFunction))]
	[UnityEngine.Scripting.Preserve]
	static public int GetTimeUpdateMode(IntPtr l) {
		try {
			UnityEngine.Experimental.Director.DirectorPlayer self=(UnityEngine.Experimental.Director.DirectorPlayer)checkSelf(l);
			var ret=self.GetTimeUpdateMode();
			pushValue(l,true);
			pushEnum(l,(int)ret);
			return 2;
		}
		catch(Exception e) {
			return error(l,e);
		}
	}
	[UnityEngine.Scripting.Preserve]
	static public void reg(IntPtr l) {
		getTypeTable(l,"UnityEngine.Experimental.Director.DirectorPlayer");
		addMember(l,Play);
		addMember(l,Stop);
		addMember(l,SetTime);
		addMember(l,GetTime);
		addMember(l,SetTimeUpdateMode);
		addMember(l,GetTimeUpdateMode);
		createTypeMetatable(l,null, typeof(UnityEngine.Experimental.Director.DirectorPlayer),typeof(UnityEngine.Behaviour));
	}
}
