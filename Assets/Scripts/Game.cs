using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;
using SLua;

public class Game : MonoBehaviour
{
	public enum Status
	{
		NotInitialized,
		Initializing,
		Initialized,
		Running,
		Aborted,
		Destroyed
	}

	public Status CurrentStatus { get; private set; }
	public LuaSvr LuaVM { get; private set; }
	public ResourceManager ResourceManager { get; private set; }
	public readonly Dictionary<int, LSTGObject> ObjectDictionary = new Dictionary<int, LSTGObject>();
	public Collider2D Bound { get; private set; }

	public static Game GameInstance { get; private set; }

	public void SetViewPort(float left, float right, float bottom, float top)
	{
		var mainCamera = Camera.main;
		mainCamera.rect = new Rect(left, bottom, right - left, top - bottom);
	}

	public LSTGObject GetObject(int id)
	{
		LSTGObject result;
		ObjectDictionary.TryGetValue(id, out result);
		return result;
	}

	// Use this for initialization
	void Start()
	{
		Debug.Assert(CurrentStatus == Status.NotInitialized);
		Debug.Assert(GameInstance == null);
		GameInstance = this;
		CurrentStatus = Status.Initializing;
		ResourceManager = new ResourceManager();
		ResourceManager.AddResourceDataProvider(new ResourcePack(""));
		ResourceManager.FindResourceAs<ResTexture>("233");
		LuaVM = new LuaSvr();
		LuaVM.init(null, () =>
		{
			var L = LuaVM.luaState.L;
			LuaDLL.lua_gc(L, LuaGCOptions.LUA_GCSTOP, 0);

			LuaDLL.luaL_openlibs(L);

			BuiltinFunctions.Register(L);
		});

		Bound = gameObject.AddComponent<BoxCollider2D>();
		Bound.isTrigger = true;

		
	}
	
	// Update is called once per frame
	void Update()
	{
		
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "")
		{
			
		}
	}
}
