using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using SLua;
using UnityEngine;

namespace Assets.Scripts
{
	public static class BuiltinFunctions
	{
		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		public static int Print(IntPtr l)
		{
			var n = LuaDLL.lua_gettop(l);
			LuaDLL.lua_getglobal(l, "tostring"); // ... f
			LuaDLL.lua_pushstring(l, ""); // ... f s
			for (var i = 1; i <= n; ++i)
			{
				if (i > 1)
				{
					LuaDLL.lua_pushstring(l, "\t"); // ... f s s
					LuaDLL.lua_concat(l, 2); // ... f s
				}
				LuaDLL.lua_pushvalue(l, -2); // ... f s f
				LuaDLL.lua_pushvalue(l, i); // ... f s f arg[i]
				LuaDLL.lua_call(l, 1, 1); // ... f s ret
				LuaDLL.luaL_checktype(l, -1, LuaTypes.LUA_TSTRING);
				LuaDLL.lua_concat(l, 2); // ... f s
			}
			
			LuaDLL.luaL_checktype(l, -1, LuaTypes.LUA_TSTRING);
			Game.GameInstance.GameLogger.Log(LuaDLL.lua_tostring(l, -1));
			LuaDLL.lua_pop(l, 2);
			
			return 0;
		}

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		public static int Dist(IntPtr l)
		{
			if (LuaDLL.lua_gettop(l) == 2)
			{
				if (!LuaDLL.lua_istable(l, 1) || !LuaDLL.lua_istable(l, 2))
				{
					return LuaDLL.luaL_error(l, "invalid lstg object for 'Dist'.");
				}
				LuaDLL.lua_rawgeti(l, 1, 2);
				LuaDLL.lua_rawgeti(l, 2, 2);
				var obj1 = Game.GameInstance.GetObject(LuaDLL.luaL_checkinteger(l, -2));
				var obj2 = Game.GameInstance.GetObject(LuaDLL.luaL_checkinteger(l, -1));
				if (obj1 == null || obj2 == null)
				{
					return LuaDLL.luaL_error(l, "invalid lstg object for 'Dist'.");
				}
				LuaDLL.lua_pushnumber(l, Vector2.Distance(obj1.CurrentPosition, obj2.CurrentPosition));
			}
			else
			{
				var p1 = new Vector2((float) LuaDLL.luaL_checknumber(l, 1), (float) LuaDLL.luaL_checknumber(l, 2));
				var p2 = new Vector2((float) LuaDLL.luaL_checknumber(l, 3), (float) LuaDLL.luaL_checknumber(l, 4));
				LuaDLL.lua_pushnumber(l, Vector2.Distance(p1, p2));
			}

			return 1;
		}

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		public static int SetWindowed(IntPtr l)
		{
			bool value;
			if (LuaObject.checkType(l, -1, out value))
			{
				return LuaObject.error(l, "invalid argument for 'SetWindowed'");
			}
			Screen.fullScreen = value;
			return 0;
		}

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		public static int GetFPS(IntPtr l)
		{
			LuaDLL.lua_pushnumber(l, Game.GameInstance.CurrentFPS);
			return 1;
		}

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		public static int SetFPS(IntPtr l)
		{
			int fps;
			if (!LuaObject.checkType(l, -1, out fps) || fps <= 0)
			{
				fps = 60;
			}
			Application.targetFrameRate = fps;
			return 0;
		}

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		public static int SetVsync(IntPtr l)
		{
			bool value;
			if (LuaObject.checkType(l, -1, out value))
			{
				QualitySettings.vSyncCount = value ? 1 : 0;
			}

			return 0;
		}

		public static int New(IntPtr l)
		{
			throw new NotImplementedException();
		}

		public static void Register(IntPtr l)
		{
			foreach (var method in from method in typeof(BuiltinFunctions).GetMethods()
								   where method.IsDefined(typeof(MonoPInvokeCallbackAttribute), false)
								   select method)
			{
				LuaObject.reg(l, (LuaCSFunction) Delegate.CreateDelegate(typeof(LuaCSFunction), method));
			}
			
		}
	}
}
