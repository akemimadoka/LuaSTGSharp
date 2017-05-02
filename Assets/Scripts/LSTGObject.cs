using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using SLua;

/// <summary>
/// LuaSTGSharp 游戏对象
/// </summary>
/// <remarks>
/// 属性可能存在别名，别名不得与已经存在的其他属性及其别名冲突
/// </remarks>
public class LSTGObject : MonoBehaviour
{
	public enum ObjFuncIndex
	{
		Init = 1,
		Del,
		Frame,
		Render,
		Colli,
		Kill
	}

	public const string ObjMetadataTableName = "mt";

	private static readonly Dictionary<string, PropertyInfo> PropertiesMap = new Dictionary<string, PropertyInfo>();

	public static PropertyInfo FindProperty(string key)
	{
		PropertyInfo info;
		PropertiesMap.TryGetValue(key, out info);
		return info;
	}

	static LSTGObject()
	{
		foreach (var prop in typeof(LSTGObject).GetProperties())
		{
			foreach (var attr in prop.GetCustomAttributes(false))
			{
				var aliasAttr = attr as LObjectPropertyAliasAsAttribute;
				if (aliasAttr != null)
				{
					PropertiesMap.Add(aliasAttr.Alias, prop);
					break;
				}
			}
		}
	}

	public enum Status
	{
		Free,
		Default,
		Kill,
		Del
	}

	[LObjectPropertyAliasAs("status")]
	public Status ObjectStatus { get; private set; }

	public int Id
	{
		get { return GetInstanceID(); }
	}

	[LObjectPropertyAliasAs("")]
	public Vector2 CurrentPosition
	{
		get { return transform.position; }
		set { transform.position = value; }
	}

	[LObjectPropertyAliasAs("last")]
	public Vector2 LastPosition { get; set; }
	[LObjectPropertyAliasAs("d")]
	public Vector2 Delta { get; private set; }

	[LObjectPropertyAliasAs("rot")]
	public float Rotation
	{
		get
		{
			float angle;
			Vector3 axis;
			transform.rotation.ToAngleAxis(out angle, out axis);
			//Debug.Assert(axis == Vector3.back || axis == Vector3.zero);
			return angle;
		}
		set { transform.rotation = Quaternion.AngleAxis(value % 360, Vector3.back); }
	}

	[LObjectPropertyAliasAs("omiga")]
	public float Omiga { get; set; }
	[LObjectPropertyAliasAs("v")]
	public Vector2 Velocity { get; set; }
	[LObjectPropertyAliasAs("a")]
	public Vector2 Acceleration { get; set; }

	[LObjectPropertyAliasAs("layer")]
	public int Layer
	{
		get
		{
			var spriteRenderer = GetComponent<SpriteRenderer>();
			if (spriteRenderer == null || !spriteRenderer)
			{
				return 0;
			}

			return spriteRenderer.sortingLayerID;
		}
		set
		{
			var spriteRenderer = GetComponent<SpriteRenderer>();
			if (spriteRenderer == null || !spriteRenderer)
			{
				return;
			}

			spriteRenderer.sortingLayerID = value;
		}
	}

	[LObjectPropertyAliasAs("order")]
	public int Order
	{
		get
		{
			var spriteRenderer = GetComponent<SpriteRenderer>();
			if (spriteRenderer == null || !spriteRenderer)
			{
				return 0;
			}

			return spriteRenderer.sortingOrder;
		}
		set
		{
			var spriteRenderer = GetComponent<SpriteRenderer>();
			if (spriteRenderer == null || !spriteRenderer)
			{
				return;
			}

			spriteRenderer.sortingOrder = value;
		}
	}

	private Vector2 _ab;
	public Vector2 Ab
	{
		get { return _ab; }
		set
		{
			_ab = value;
			var currentCollider = Collider;
			var boxCollider2D = currentCollider as BoxCollider;
			if (boxCollider2D != null)
			{
				boxCollider2D.size = _ab;
			}
			else
			{
				var circleCollider2D = (SphereCollider) currentCollider;
				circleCollider2D.radius = (_ab.x + _ab.y) / 2;
			}
		}
	}

	public Vector2 Scale
	{
		get { return transform.localScale; }
		set { transform.localScale = value; }
	}

	public Collider Collider
	{
		get { return GetComponent<Collider>(); }
	}

	[LObjectPropertyAliasAs("colli")]
	public bool Colli
	{
		get { return Collider.enabled; }
		set { Collider.enabled = value; }
	}

	[LObjectPropertyAliasAs("rect")]
	public bool Rect
	{
		get { return Collider is BoxCollider; }
		set
		{
			var currentCollider = Collider;
			var boxCollider2D = currentCollider as BoxCollider;
			if (boxCollider2D == null && value)
			{
				Destroy(currentCollider);
				gameObject.AddComponent<BoxCollider>().isTrigger = true;
			}
			else if (boxCollider2D != null && !value)
			{
				Destroy(boxCollider2D);
				gameObject.AddComponent<SphereCollider>().isTrigger = true;
			}
			Ab = _ab;
		}
	}

	[LObjectPropertyAliasAs("bound")]
	public bool Bound { get; set; }

	[LObjectPropertyAliasAs("hide")]
	public bool Hide
	{
		get { return GetComponent<SpriteRenderer>().enabled; }
		set { GetComponent<SpriteRenderer>().enabled = value; }
	}

	[LObjectPropertyAliasAs("freezed")]
	public bool Freezed { get; set; }

	[LObjectPropertyAliasAs("navi")]
	public bool Navi { get; set; }

	private int _group;
	[LObjectPropertyAliasAs("group")]
	public int Group {
		get
		{
			return _group;
		}
		set
		{
			gameObject.layer = _group = value;
		}
	}
	[LObjectPropertyAliasAs("timer")]
	public int Timer { get; private set; }
	[LObjectPropertyAliasAs("ani_timer")]
	public int AniTimer { get; private set; }

	private Resource _renderResource;
	public Resource RenderResource
	{
		get { return _renderResource; }
		set
		{
			if (_renderResource == value)
			{
				return;
			}

			UnloadRenderResource();
			_renderResource = value;
			LoadRenderResource();
		}
	}

	public LuaTable ObjTable { get; private set; }
	public LuaTable ClassTable { get; private set; }

	private void UnloadRenderResource()
	{
		if (_renderResource == null)
		{
			return;
		}

		if (_renderResource is ResSprite || _renderResource is ResAnimation)
		{
			Destroy(gameObject.GetComponent<SpriteRenderer>());
		}
		else if (_renderResource is ResParticle)
		{
			Destroy(gameObject.GetComponent<ParticleSystem>());
		}

		_renderResource = null;
	}

	private void LoadRenderResource()
	{
		if (_renderResource == null)
		{
			return;
		}

		var sprite = _renderResource as ResSprite;
		if (sprite != null)
		{
			var spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
			spriteRenderer.sprite = sprite.GetSprite();
		}
		else if (_renderResource is ResAnimation)
		{
			var spriteRenderer = gameObject.AddComponent<SpriteRenderer>();
			spriteRenderer.sprite = ((ResAnimation) _renderResource).GetSprite(0);
		}
		else if (_renderResource is ResParticle)
		{
			var resParticle = (ResParticle) _renderResource;
			var particleSys = gameObject.AddComponent<ParticleSystem>();
			resParticle.SetParticleSystem(particleSys);
		}
	}

	public LSTGObject()
	{
		ObjectStatus = Status.Free;
		Bound = true;
		/*Delta = Vector2.zero;
		Rotation = 0;
		Omiga = 0;
		Velocity = Vector2.zero;
		Acceleration = Vector2.zero;
		Layer = 0;
		Navi = false;
		Group = 0;
		Timer = 0;
		AniTimer = 0;*/
	}

	// Use this for initialization
	public void Start()
	{
		++Game.GameInstance.ObjectCount;
		gameObject.AddComponent<BoxCollider>().isTrigger = true;
		//gameObject.AddComponent<BoxCollider2D>().isTrigger = true;
		var rigidBody = gameObject.AddComponent<Rigidbody>();
		rigidBody.isKinematic = true;
		LastPosition = transform.position;
	}

	public void OnAcquireLuaTable(LuaTable objTable)
	{
		ObjTable = objTable;
		ClassTable = (LuaTable) objTable[1];
	}

	// Update is called once per frame
	public void Update()
	{
		if (ObjTable == null || Freezed)
		{
			return;
		}
		
		var frameFunc = ClassTable[(int) ObjFuncIndex.Frame] as LuaFunction;
		if (frameFunc != null)
		{
			frameFunc.call(ObjTable);
		}

		LastPosition = transform.position;
		Velocity += Acceleration;
		CurrentPosition += Velocity;
		Rotation += Omiga;

		Delta = CurrentPosition - LastPosition;

		// AfterFrame?
		++Timer;
		++AniTimer;

		var renderFunc = ClassTable[(int) ObjFuncIndex.Render] as LuaFunction;
		if (renderFunc != null)
		{
			renderFunc.call(ObjTable);
		}
		else
		{
			DefaultRenderFunc();
		}
		
		if (ObjectStatus != Status.Default && ObjectStatus != Status.Free)
		{
			var l = Game.GameInstance.LuaVM.luaState.L;

			LuaDLL.lua_pushlightuserdata(l, l);
			LuaDLL.lua_gettable(l, LuaIndexes.LUA_REGISTRYINDEX);
			LuaDLL.lua_pushnil(l);
			LuaDLL.lua_rawseti(l, -2, Id + 1);
			LuaDLL.lua_pop(l, 1);

			Destroy(gameObject);
		}
	}
	
	private void OnDisable()
	{
		--Game.GameInstance.ObjectCount;
	}

	private void OnDestroy()
	{
		Game.GameInstance.ObjectDictionary.Remove(Id);
	}

	public void DefaultRenderFunc()
	{
		var ani = _renderResource as ResAnimation;
		if (ani == null)
		{
			return;
		}

		var spriteRenderer = GetComponent<SpriteRenderer>();
		spriteRenderer.sprite = ani.GetSprite((int) (AniTimer / ani.GetInterval()) % ani.GetCount());
	}

	public void KillSelf()
	{
		ObjectStatus = Status.Kill;
		var killFunc = ClassTable[(int) ObjFuncIndex.Kill] as LuaFunction;
		if (killFunc != null)
		{
			killFunc.call(ObjTable);
		}
	}

	public void DelSelf()
	{
		ObjectStatus = Status.Del;
		var delFunc = ClassTable[(int) ObjFuncIndex.Del] as LuaFunction;
		if (delFunc != null)
		{
			delFunc.call(ObjTable);
		}
	}

	public void GetV(out float v, out float a)
	{
		v = Velocity.magnitude;
		a = Mathf.Atan2(Velocity.y, Velocity.x) * Mathf.Rad2Deg;
	}

	public void SetV(float v, float a, bool updateRot)
	{
		a *= Mathf.Deg2Rad;
		Velocity = new Vector2(v * Mathf.Cos(a), v * Mathf.Sin(a));
		if (updateRot)
		{
			Rotation = a;
		}
	}

	public void OnTriggerEnter(Collider collision)
	{
		var other = collision.gameObject.GetComponent<LSTGObject>();
		if (other == null || !other)
		{
			return;
		}

		var colliFunc = ClassTable[(int) ObjFuncIndex.Colli] as LuaFunction;
		if (colliFunc != null)
		{
			colliFunc.call(ObjTable, other.ObjTable);
		}
	}

	public void OnTriggerExit(Collider collision)
	{
		if (collision == Game.GameInstance.Bound && Bound)
		{
			DelSelf();
		}
	}
}
