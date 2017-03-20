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
	public const int MaxObjectCount = 32767;

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

	public string DataPath { get; private set; }
	public string LogFilePath { get; private set; }
	public LuaSvr LuaVM { get; private set; }
	public ResourceManager ResourceManager { get; private set; }
	public readonly Dictionary<int, LSTGObject> ObjectDictionary = new Dictionary<int, LSTGObject>();
	public Collider2D Bound { get; private set; }

	public float CurrentFPS { get; private set; }

	public LuaTable GlobalTable { get; private set; }

	private LuaFunction _gameExitFunc;
	private LuaFunction _focusLoseFunc;
	private LuaFunction _focusGainFunc;
	private LuaFunction _frameFunc;

	public static Game GameInstance { get; private set; }

	private class GameLogHandler
		: ILogHandler, IDisposable
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

		public void Dispose()
		{
			_writer.Dispose();
		}
	}

	public Logger GameLogger { get; private set; }

	public void SetViewPort(float left, float right, float bottom, float top)
	{
		var mainCamera = Camera.main;
		mainCamera.rect = new Rect(left, bottom, right - left, top - bottom);
	}

	public int NewObject(IntPtr l)
	{
		LuaTable classTable;
		LuaObject.checkType(l, 1, out classTable);
		if (classTable == null)
		{
			return LuaDLL.luaL_error(l, "invalid argument #1, luastg object class required for 'New'.");
		}
		if (!(classTable["is_class"] is bool))
		{
			return LuaDLL.luaL_error(l, "invalid argument #1, luastg object class required for 'New'.");
		}

		var resultObj = new GameObject();
		var lstgObj = resultObj.AddComponent<LSTGObject>();
		ObjectDictionary.Add(lstgObj.Id, lstgObj);

		LuaDLL.lua_pushlightuserdata(l, l);
		LuaDLL.lua_gettable(l, LuaIndexes.LUA_REGISTRYINDEX);
		LuaDLL.lua_createtable(l, 2, 0);
		LuaDLL.lua_pushvalue(l, 1);  // t(class) ... ot t(object) class
		LuaDLL.lua_rawseti(l, -2, 1);  // t(class) ... ot t(object)  设置class
		LuaDLL.lua_pushinteger(l, lstgObj.Id);  // t(class) ... ot t(object) id
		LuaDLL.lua_rawseti(l, -2, 2);  // t(class) ... ot t(object)  设置id
		LuaDLL.lua_getfield(l, -2, "mt");  // t(class) ... ot t(object) mt
		LuaDLL.lua_setmetatable(l, -2);  // t(class) ... ot t(object)  设置元表
		LuaDLL.lua_pushvalue(l, -1);  // t(class) ... ot t(object) t(object)
		LuaDLL.lua_rawseti(l, -3, lstgObj.Id + 1);  // t(class) ... ot t(object)  设置到全局表
		LuaDLL.lua_insert(l, 1);  // t(object) t(class) ... ot
		LuaDLL.lua_pop(l, 1);  // t(object) t(class) ...
		//LuaDLL.lua_rawgeti(l, 2, 1);  // t(object) t(class) ... f(init)
		LuaDLL.lua_getfield(l, 2, "init");
		LuaDLL.lua_insert(l, 3);  // t(object) t(class) f(init) ...
		LuaDLL.lua_pushvalue(l, 1);  // t(object) t(class) f(init) ... t(object)
		LuaDLL.lua_insert(l, 4);  // t(object) t(class) f(init) t(object) ...
		LuaDLL.lua_call(l, LuaDLL.lua_gettop(l) - 3, 0);  // t(object) t(class)  执行构造函数
		LuaDLL.lua_pop(l, 1);  // t(object)

		LuaTable objTable;
		LuaObject.checkType(l, -1, out objTable);
		lstgObj.OnAcquireLuaTable(objTable);

		return 1;
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

		Bound = gameObject.AddComponent<BoxCollider2D>();
		Bound.isTrigger = true;

		LuaVM = new LuaSvr();
		LuaVM.init(null, () =>
		{
			var l = LuaVM.luaState.L;
			LuaDLL.lua_gc(l, LuaGCOptions.LUA_GCSTOP, 0);

			LuaDLL.luaL_openlibs(l);

			BuiltinFunctions.Register(l);
			BuiltinFunctions.InitMetaTable(l);

			LuaDLL.lua_gc(l, LuaGCOptions.LUA_GCRESTART, -1);

			ResourceManager.FindResourceAs<ResLuaScript>("launch").Execute();

			LuaDLL.lua_pushglobaltable(l);
			LuaTable globalTable;
			LuaObject.checkType(l, -1, out globalTable);
			if (globalTable == null)
			{
				throw new Exception("Cannot get global table");
			}
			GlobalTable = globalTable;
			LuaDLL.lua_pop(l, 1);

			ResourceManager.FindResourceAs<ResLuaScript>("core.lua").Execute();
			
			var gameInit = globalTable["GameInit"] as LuaFunction;
			if (gameInit == null)
			{
				throw new Exception("GameInit does not exist or is not a function");
			}

			_gameExitFunc = globalTable["GameExit"] as LuaFunction;
			if (_gameExitFunc == null)
			{
				throw new Exception("GameExit does not exist or is not a function");
			}

			_focusLoseFunc = globalTable["FocusLoseFunc"] as LuaFunction;
			if (_focusLoseFunc == null)
			{
				throw new Exception("FocusLoseFunc does not exist or is not a function");
			}
			
			_focusGainFunc = globalTable["FocusGainFunc"] as LuaFunction;
			if (_focusGainFunc == null)
			{
				throw new Exception("FocusGainFunc does not exist or is not a function");
			}
			
			_frameFunc = globalTable["FrameFunc"] as LuaFunction;
			if (_frameFunc == null)
			{
				throw new Exception("FrameFunc does not exist or is not a function");
			}

			gameInit.call();

			CurrentStatus = Status.Initialized;
		});
	}
	
	// Update is called once per frame
	void Update()
	{
		switch (CurrentStatus)
		{
			case Status.NotInitialized:
			case Status.Initializing:
				throw new Exception("Illegal status.");
			case Status.Initialized:
				CurrentStatus = Status.Running;
				break;
			case Status.Running:
				break;
			case Status.Aborted:
				// TODO: 完成回收操作
				CurrentStatus = Status.Destroyed;
				break;
			case Status.Destroyed:
				Application.Quit();
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		CurrentFPS = 1.0f / Time.deltaTime;

		// TODO: 完成更新操作，包括对象的更新等
		
		if ((bool) _frameFunc.call())
		{
			CurrentStatus = Status.Aborted;
		}
	}

	private void OnApplicationQuit()
	{

	}

	private void OnApplicationFocus(bool focus)
	{
		// 可能在初始化完成之前被调用，因此我们必须检查状态
		switch (CurrentStatus)
		{
			case Status.NotInitialized:
			case Status.Initializing:
				return;
			default:
				break;
		}

		if (focus)
		{
			_focusGainFunc.call();
		}
		else
		{
			_focusLoseFunc.call();
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (collision.gameObject.tag == "")
		{
			
		}
	}
	// TODO: 完成资源加载及渲染
	// 对象是否还需要排序？是否需要重用？
	// 还有多少未实现的API？
}
