using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Assets.Scripts;
using UnityEngine;
using SLua;
using Object = UnityEngine.Object;

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

	public string DataPath { get; private set; }
	public string LogFilePath { get; private set; }
	public Status CurrentStatus { get; private set; }
	public LuaSvr LuaVM { get; private set; }
	public ResourceManager ResourceManager { get; private set; }
	public readonly Dictionary<int, LSTGObject> ObjectDictionary = new Dictionary<int, LSTGObject>();
	public Collider2D Bound { get; private set; }

	public static Game GameInstance { get; private set; }

	private class GameLogHandler
		: ILogHandler
	{
		private readonly TextWriter _writer;

		public GameLogHandler(string logFilePath)
		{
			_writer = new StreamWriter(new FileStream(logFilePath, FileMode.Create));
		}

		public void LogFormat(LogType logType, Object context, string format, params object[] args)
		{
			var logContent = string.Format("[{0:yy/MM/dd H:mm:ss}][{1}] ({2}) {3}", DateTime.Now, logType, context == null || !context ? "No context" : context.ToString(), string.Format(format, args));
			_writer.WriteLine(logContent);
			Debug.Log(logContent);
		}

		public void LogException(Exception exception, Object context)
		{
			LogFormat(LogType.Exception, context, "Exception logged: {0}", exception);
		}
	}

	public Logger GameLogger { get; private set; }

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
	
	private void Awake()
	{
		switch (Application.platform)
		{
			case RuntimePlatform.WindowsEditor:
			case RuntimePlatform.OSXEditor:
			case RuntimePlatform.LinuxEditor:
			case RuntimePlatform.WindowsPlayer:
				DataPath = "./Data/";
				LogFilePath = "./Log/log.log";
				break;
			default:
				DataPath = Path.Combine(Application.persistentDataPath, "Data/");
				LogFilePath = Path.Combine(Application.persistentDataPath, "Log/log.log");
				break;
		}
	}

	// Use this for initialization
	void Start()
	{
		Debug.Assert(CurrentStatus == Status.NotInitialized);
		Debug.Assert(GameInstance == null);
		GameInstance = this;
		CurrentStatus = Status.Initializing;
		GameLogger = new Logger(new GameLogHandler(LogFilePath));
		ResourceManager = new ResourceManager();
		ResourceManager.AddResourceDataProvider(new LocalFileProvider(DataPath));
		ResourceManager.AddResourceDataProvider(new ResourcePack(ResourceManager.GetResourceStream("data.zip")));
		LuaVM = new LuaSvr();
		LuaVM.init(null, () =>
		{
			var L = LuaVM.luaState.L;
			LuaDLL.lua_gc(L, LuaGCOptions.LUA_GCSTOP, 0);

			LuaDLL.luaL_openlibs(L);

			BuiltinFunctions.Register(L);

			LuaDLL.lua_gc(L, LuaGCOptions.LUA_GCRESTART, -1);
		});

		Bound = gameObject.AddComponent<BoxCollider2D>();
		Bound.isTrigger = true;

		Debug.Log(ResourceManager.FindResourceAs<ResLuaScript>("test.lua").Execute());

		CurrentStatus = Status.Initialized;
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
