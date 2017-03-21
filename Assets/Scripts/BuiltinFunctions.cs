using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using SLua;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Assets.Scripts
{
	public static class BuiltinFunctions
	{
		public static readonly Regex AttrRegex = new Regex(@"^(\w*?)([xy]?)$", RegexOptions.Compiled);

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

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		public static int Snapshot(IntPtr l)
		{
			string path;
			LuaObject.checkType(l, -1, out path);
			if (path == null)
			{
				return LuaDLL.luaL_error(l, "Path is not a valid argument for 'Snapshot'");
			}

			Application.CaptureScreenshot(path);
			return 0;
		}

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		public static int DoFile(IntPtr l)
		{
			string path;
			LuaObject.checkType(l, -1, out path);
			if (string.IsNullOrEmpty(path))
			{
				return LuaDLL.luaL_error(l, "invalid argument for 'DoFile'");
			}

			Game.GameInstance.ResourceManager.FindResourceAs<ResLuaScript>(path).Execute(LuaState.get(l));
			return 0;
		}

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		public static int LoadTexture(IntPtr l)
		{
			string name, path;
			LuaObject.checkType(l, 1, out name);
			LuaObject.checkType(l, 2, out path);

			var activedPool = Game.GameInstance.ResourceManager.GetActivedPool();
			if (activedPool == null)
			{
				return LuaDLL.luaL_error(l, "cannot load resource at this time.");
			}

			if (activedPool.GetResourceAs<ResTexture>(name, path) == null)
			{
				return LuaDLL.luaL_error(l, "cannot load texture from path '{1}' as name '{0}'", name, path);
			}

			return 0;
		}

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		public static int LoadImage(IntPtr l)
		{
			var top = LuaDLL.lua_gettop(l);

			string name, textureName;
			LuaObject.checkType(l, 1, out name);
			LuaObject.checkType(l, 2, out textureName);

			var activedPool = Game.GameInstance.ResourceManager.GetActivedPool();
			if (activedPool == null)
			{
				return LuaDLL.luaL_error(l, "cannot load resource at this time.");
			}

			if (activedPool.ResourceExists(name, typeof(ResSprite)))
			{
				return LuaDLL.luaL_error(l, "sprite '{0}' has already loaded", name);
			}

			if (!activedPool.ResourceExists(textureName, typeof(ResTexture)))
			{
				return LuaDLL.luaL_error(l, "texture '{0}' has not loaded", textureName);
			}
			var sprite = Sprite.Create(activedPool.GetResourceAs<ResTexture>(textureName).GetTexture(), new Rect(
				(float) LuaDLL.luaL_checknumber(l, 3),
				(float) LuaDLL.luaL_checknumber(l, 4),
				(float) LuaDLL.luaL_checknumber(l, 5),
				(float) LuaDLL.luaL_checknumber(l, 6)), new Vector2(0.5f, 0.5f));
			
			float a = 0, b = 0;
			var rect = false;

			if (top >= 7)
			{
				LuaObject.checkType(l, 7, out a);
			}

			if (top >= 8)
			{
				LuaObject.checkType(l, 8, out b);
			}

			if (top >= 9)
			{
				LuaObject.checkType(l, 9, out rect);
			}

			if (!activedPool.AddResource(new ResSprite(name, sprite, a, b, rect)))
			{
				return LuaDLL.luaL_error(l, "some error occured while adding sprite '{0}' to resource pool", name);
			}

			return 0;
		}

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		public static int GetAttr(IntPtr l)
		{
			LuaDLL.lua_rawgeti(l, 1, 2);
			var id = LuaDLL.lua_tointeger(l, -1);
			LuaDLL.lua_pop(l, 1);

			var obj = Game.GameInstance.GetObject(id);

			string key;
			LuaObject.checkType(l, -1, out key);
			if (key == null)
			{
				return LuaDLL.luaL_error(l, "invalid key for 'GetAttr'");
			}
			
			var match = AttrRegex.Match(key);
			if (!match.Success)
			{
				return LuaDLL.luaL_error(l, "key '{0}' is invalid", key);
			}
			var propName = match.Groups[1].Value;
			var optDimension = match.Groups[2].Value;
			
			var noAliasProperty = false;
			var prop = LSTGObject.FindProperty(propName);
			if (prop != null)
			{
				var value = prop.GetValue(obj, null);
				switch (optDimension)
				{
					case "x":
						LuaObject.pushValue(l, ((Vector2) value).x);
						break;
					case "y":
						LuaObject.pushValue(l, ((Vector2) value).y);
						break;
					case "":
						LuaObject.pushValue(l, value);
						break;
					default:
						noAliasProperty = true;
						break;
				}
			}

			if (prop == null || noAliasProperty)
			{
				switch (key)
				{
					case "a":
						LuaObject.pushValue(l, obj.Ab.x);
						break;
					case "b":
						LuaObject.pushValue(l, obj.Ab.y);
						break;
					case "hscale":
						LuaObject.pushValue(l, obj.Scale.x);
						break;
					case "vscale":
						LuaObject.pushValue(l, obj.Scale.y);
						break;
					case "img":
						LuaObject.pushValue(l, obj.RenderResource == null ? null : obj.RenderResource.GetName());
						break;
					default:
						return LuaDLL.luaL_error(l, "key '{0}' does not exist", key);
				}
			}

			return 1;
		}

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		public static int SetAttr(IntPtr l)
		{
			LuaDLL.lua_rawgeti(l, 1, 2);
			var id = LuaDLL.lua_tointeger(l, -1);
			LuaDLL.lua_pop(l, 1);

			var obj = Game.GameInstance.GetObject(id);

			string key;
			LuaObject.checkType(l, -2, out key);
			if (key == null)
			{
				return LuaDLL.luaL_error(l, "invalid key for 'GetAttr'");
			}
			
			var match = AttrRegex.Match(key);
			if (!match.Success)
			{
				return LuaDLL.luaL_error(l, "key '{0}' is invalid", key);
			}
			var propName = match.Groups[1].Value;

			var prop = LSTGObject.FindProperty(propName);
			if (prop != null)
			{
				var value = prop.GetValue(obj, null);
				switch (match.Groups[2].Value)
				{
					case "x":
					{
						var vec = (Vector2) value;
						vec.x = (float) LuaDLL.luaL_checknumber(l, -1);
						prop.SetValue(obj, vec, null);
					}
						break;
					case "y":
					{
						var vec = (Vector2) value;
						vec.y = (float) LuaDLL.luaL_checknumber(l, -1);
						prop.SetValue(obj, vec, null);
					}
						break;
					default:
						prop.SetValue(obj, Convert.ChangeType(LuaObject.checkVar(l, -1), prop.PropertyType), null);
						break;
				}
			}
			else
			{
				switch (key)
				{
					case "a":
					{
						var ab = obj.Ab;
						ab.x = (float) LuaDLL.luaL_checknumber(l, -1);
						obj.Ab = ab;
					}
						break;
					case "b":
					{
						var ab = obj.Ab;
						ab.y = (float) LuaDLL.luaL_checknumber(l, -1);
						obj.Ab = ab;
					}
						break;
					case "hscale":
					{
						var scale = obj.Scale;
						scale.x = (float) LuaDLL.luaL_checknumber(l, -1);
						obj.Scale = scale;
					}
						break;
					case "vscale":
					{
						var scale = obj.Scale;
						scale.y = (float) LuaDLL.luaL_checknumber(l, -1);
						obj.Scale = scale;
					}
						break;
					case "img":
					{
						string resName;
						LuaObject.checkType(l, -1, out resName);
						if (string.IsNullOrEmpty(resName))
						{
							return LuaDLL.luaL_error(l, "invalid img");
						}
						obj.RenderResource = Game.GameInstance.ResourceManager.FindResource(resName);
					}
						break;
					default:
						Debug.LogWarning(string.Format("key '{0}' does not exist", key));
						LuaDLL.lua_pushnil(l);
						break;
				}
			}

			return 0;
		}

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		public static int SetBound(IntPtr l)
		{
			float left, right, bottom, top;

			if (!LuaObject.checkType(l, 1, out left) ||
				!LuaObject.checkType(l, 2, out right) ||
				!LuaObject.checkType(l, 3, out bottom) ||
				!LuaObject.checkType(l, 4, out top))
			{
				return LuaDLL.luaL_error(l, "invalid argument for 'SetBound'");
			}

			var bound = Game.GameInstance.Bound;
			bound.offset = new Vector2((left + right) / 2.0f, (bottom + top) / 2.0f);
			bound.size = new Vector2(right - left, top - bottom);

			return 0;
		}

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		public static int DefaultRenderFunc(IntPtr l)
		{
			LuaTable table;
			LuaObject.checkType(l, 1, out table);
			var obj = Game.GameInstance.GetObject((int) table[1]);
			if (obj != null && obj)
			{
				obj.DefaultRenderFunc();
			}

			return 0;
		}

		[MonoPInvokeCallback(typeof(LuaCSFunction))]
		public static int New(IntPtr l)
		{
			return Game.GameInstance.NewObject(l);
		}

		public static void Register(IntPtr l)
		{
			foreach (var method in from method in typeof(BuiltinFunctions).GetMethods()
								   where method.IsDefined(typeof(MonoPInvokeCallbackAttribute), false)
								   select method)
			{
				LuaObject.reg(l, (LuaCSFunction) Delegate.CreateDelegate(typeof(LuaCSFunction), method), "lstg");
			}
		}

		public static void InitMetaTable(IntPtr l)
		{
			LuaDLL.lua_pushlightuserdata(l, l);
			LuaDLL.lua_createtable(l, Game.MaxObjectCount, 0);

			LuaDLL.lua_newtable(l);
			LuaDLL.lua_getglobal(l, "lstg");
			LuaDLL.lua_pushstring(l, "GetAttr");
			LuaDLL.lua_gettable(l, -2);
			LuaDLL.lua_pushstring(l, "SetAttr");
			LuaDLL.lua_gettable(l, -3);
			//Debug.Assert(LuaDLL.lua_iscfunction(l, -1) && LuaDLL.lua_iscfunction(l, -2));
			LuaDLL.lua_setfield(l, -4, "__newindex");
			LuaDLL.lua_setfield(l, -3, "__index");
			LuaDLL.lua_pop(l, 1);

			LuaDLL.lua_setfield(l, -2, "mt");
			LuaDLL.lua_settable(l, LuaIndexes.LUA_REGISTRYINDEX);
		}
	}

	[CustomLuaClass]
	public sealed class Rand
	{
		private Random.State _state;

		public void Seed(int seed)
		{
			Random.InitState(seed);
			_state = Random.state;
		}

		public int Int(int a, int b)
		{
			Random.state = _state;
			return Random.Range(a, b);
		}

		public float Float(float a, float b)
		{
			Random.state = _state;
			return Random.Range(a, b);
		}

		public int Sign()
		{
			Random.state = _state;
			return Random.Range(0, 1) * 2 - 1;
		}

		public override string ToString()
		{
			return "lstg.Rand object";
		}
	}
}
