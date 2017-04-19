using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SLua;

public static partial class ExtendClass
{
	public static T GetOrExecute<T>(this KeyValuePair<string, JToken> tokenPair, string key)
	{
		return tokenPair.Value[key].CastOrExecute<T>(string.Format("{0}.{1}", tokenPair.Key, key));
	}

	public static T CastOrExecute<T>(this JToken token, string chunkName)
	{
		if (token.Type != JTokenType.String)
		{
			return token.ToObject<T>();
		}

		var tokenValue = token.ToObject<string>();
		object resultValue = tokenValue;

		if (tokenValue.StartsWith("@"))
		{
			resultValue = Game.GameInstance.LuaVM.luaState.doString(string.Format("return {0}", tokenValue.TrimStart('@')), chunkName);
		}

		return (T) Convert.ChangeType(resultValue, typeof(T));
	}
}

public class JsonUI : MonoBehaviour
{
	private JObject _jobject;
	
	public void OnAcquireJson(string json)
	{
		if (_jobject != null)
		{
			throw new Exception("Json already acquired.");
		}

		_jobject = JObject.Parse(json);
	}

	public JObject GetJObject()
	{
		return _jobject;
	}

	// Use this for initialization
	private void Start()
	{
	}

	// Update is called once per frame
	private void Update()
	{
	}

	private void OnGUI()
	{
		if (_jobject == null)
		{
			return;
		}

		var luaState = Game.GameInstance.LuaVM.luaState;

		foreach (var item in _jobject)
		{
			var value = item.Value;
			var rect = new Rect(item.GetOrExecute<float>("x"), item.GetOrExecute<float>("y"), item.GetOrExecute<float>("width"),
				item.GetOrExecute<float>("height"));
			switch (item.GetOrExecute<string>("type"))
			{
				case "button":
					if (GUI.Button(rect, item.GetOrExecute<string>("caption")))
					{
						var action = value["action"];
						if (action != null)
						{
							luaState.doString(action.ToObject<string>(), string.Format("{0}.action", item.Key));
						}
					}
					break;
				case "text":
					GUI.Label(rect, item.GetOrExecute<string>("caption"));
					break;
				case "image":
					var texture = Game.GameInstance.ResourceManager.FindResourceAs<ResTexture>(item.GetOrExecute<string>("image"),
						autoLoad: false);
					if (texture == null)
					{
						goto default;
					}
					GUI.Label(rect, texture.GetTexture());
					break;
				case "edit":
					var l = luaState.L;
					var binding = item.GetOrExecute<string>("binding");
					var dotPos = binding.LastIndexOf('.');
					var bindingNamespace = binding.Substring(0, dotPos);
					var bindingVar = binding.Substring(dotPos + 1);
					if (string.IsNullOrEmpty(bindingVar))
					{
						goto default;
					}
					LuaObject.newTypeTable(l, bindingNamespace);
					LuaDLL.lua_pushstring(l, bindingVar);
					LuaDLL.lua_gettable(l, -2);
					string bindingValue;
					LuaObject.checkType(l, -1, out bindingValue);
					LuaDLL.lua_pop(l, 1);
					if (bindingValue == null)
					{
						bindingValue = "";
					}
					bindingValue = GUI.TextField(rect, bindingValue);
					LuaDLL.lua_pushstring(l, bindingVar);
					LuaDLL.lua_pushstring(l, bindingValue);
					LuaDLL.lua_settable(l, -3);
					LuaDLL.lua_pop(l, 1);
					break;
				default:
					GUI.Label(rect, "Error item");
					break;
			}
		}
	}
}
