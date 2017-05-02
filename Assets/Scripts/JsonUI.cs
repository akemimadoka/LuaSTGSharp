using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using SLua;

public static partial class ExtendClass
{
	public static T GetOrExecute<T>(this KeyValuePair<string, JToken> tokenPair, string key, T defaultValue = default(T))
	{
		return tokenPair.Value[key].CastOrExecute<T>(string.Format("{0}.{1}", tokenPair.Key, key), defaultValue);
	}

	public static T CastOrExecute<T>(this JToken token, string chunkName, T defaultValue = default(T))
	{
		if (token == null)
		{
			return defaultValue;
		}

		if (token.Type != JTokenType.String)
		{
			return token.ToObject<T>();
		}

		var tokenValue = token.ToObject<string>();
		object resultValue = tokenValue;

		string evaluateString = null;
		if (tokenValue.StartsWith("@"))
		{
			evaluateString = string.Format("return {0}", tokenValue.TrimStart('@'));
		}
		else if (tokenValue.StartsWith("#"))
		{
			evaluateString = tokenValue.TrimStart('#');
		}

		if (evaluateString != null)
		{
			resultValue = Game.GameInstance.LuaVM.luaState.doString(evaluateString, chunkName);
		}

		var converter = TypeDescriptor.GetConverter(typeof(T));
		return (T) converter.ConvertFrom(resultValue);
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
					var text = item.GetOrExecute<string>("caption");
					var style = GUI.skin.label;
					if (rect.width <= 0 || rect.height <= 0)
					{
						var content = new GUIContent { text = text };
						var size = style.CalcSize(content);
						if (rect.width <= 0)
						{
							rect.width = size.x;
						}
						if (rect.height <= 0)
						{
							rect.height = size.y;
						}
					}

					var oldColor = style.normal.textColor;
					var colorInt = (uint?) item.GetOrExecute<double?>("color");
					if (colorInt != null)
					{
						var color = new Color32(
							(byte) (colorInt >> 24 & 0xff),
							(byte) (colorInt >> 16 & 0xff),
							(byte) (colorInt >> 8 & 0xff),
							(byte) (colorInt & 0xff));
						style.normal.textColor = color;
					}
					GUI.Label(rect, text);
					style.normal.textColor = oldColor;
					break;
				case "image":
					var texture = Game.GameInstance.ResourceManager.FindResourceAs<ResTexture>(item.GetOrExecute<string>("image"),
						autoLoad: false);
					if (texture == null)
					{
						goto default;
					}
					var tex = texture.GetTexture();
					if (rect.width <= 0)
					{
						rect.width = -rect.width * tex.width;
					}
					if (rect.height <= 0)
					{
						rect.height = -rect.height * tex.height;
					}
					GUI.Label(rect, tex);
					break;
				case "edit":
					var l = luaState.L;
					var binding = item.GetOrExecute<string>("binding");
					if (string.IsNullOrEmpty(binding))
					{
						GUI.TextField(rect, "");
						break;
					}
					string bindingNamespace = null;
					var dotPos = binding.LastIndexOf('.');
					if (dotPos != -1)
					{
						bindingNamespace = binding.Substring(0, dotPos);
						binding = binding.Substring(dotPos + 1);
					}
					
					if (string.IsNullOrEmpty(binding))
					{
						goto default;
					}
					LuaObject.newTypeTable(l, bindingNamespace);
					LuaDLL.lua_pushstring(l, binding);
					LuaDLL.lua_gettable(l, -2);
					string bindingValue;
					LuaObject.checkType(l, -1, out bindingValue);
					LuaDLL.lua_pop(l, 1);
					if (bindingValue == null)
					{
						bindingValue = "";
					}
					bindingValue = GUI.TextField(rect, bindingValue);
					LuaDLL.lua_pushstring(l, binding);
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
