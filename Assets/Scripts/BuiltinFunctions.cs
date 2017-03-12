using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SLua;
using UnityEngine;

namespace Assets.Scripts
{
	public static class BuiltinFunctions
	{
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

		public static int New(IntPtr l)
		{
			throw new NotImplementedException();
		}

		public static void Register(IntPtr l)
		{
			LuaObject.reg(l, Dist);
		}
	}
}
