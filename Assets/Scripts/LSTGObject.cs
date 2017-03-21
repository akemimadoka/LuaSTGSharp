using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;
using Assets.Scripts;
using SLua;

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
	public float Layer { get; set; }

	private Vector2 _ab;
	public Vector2 Ab
	{
		get { return _ab; }
		set
		{
			_ab = value;
			var currentCollider = Collider;
			var boxCollider2D = currentCollider as BoxCollider2D;
			if (boxCollider2D != null)
			{
				boxCollider2D.size = _ab;
			}
			else
			{
				var circleCollider2D = (CircleCollider2D) currentCollider;
				circleCollider2D.radius = (_ab.x + _ab.y) / 2;
			}
		}
	}

	public Vector2 Scale
	{
		get { return transform.localScale; }
		set { transform.localScale = value; }
	}

	public Collider2D Collider
	{
		get { return GetComponent<Collider2D>(); }
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
		get { return Collider is BoxCollider2D; }
		set
		{
			var currentCollider = Collider;
			var boxCollider2D = currentCollider as BoxCollider2D;
			if (boxCollider2D == null && value)
			{
				Destroy(currentCollider);
				gameObject.AddComponent<BoxCollider2D>().isTrigger = true;
			}
			else if (boxCollider2D != null && !value)
			{
				Destroy(boxCollider2D);
				gameObject.AddComponent<CircleCollider2D>().isTrigger = true;
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

	[LObjectPropertyAliasAs("navi")]
	public bool Navi { get; set; }

	[LObjectPropertyAliasAs("group")]
	public int Group { get; set; }
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

	private LuaTable _objTable;
	private LuaTable _classTable;

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
		gameObject.AddComponent<BoxCollider2D>().isTrigger = true;
		LastPosition = transform.position;
	}

	public void OnAcquireLuaTable(LuaTable objTable)
	{
		_objTable = objTable;
		_classTable = (LuaTable) objTable[1];
	}

	// Update is called once per frame
	public void Update()
	{
		if (_objTable == null)
		{
			return;
		}
		
		var frameFunc = _classTable[(int) ObjFuncIndex.Frame] as LuaFunction;
		if (frameFunc != null)
		{
			frameFunc.call(_objTable);
		}

		LastPosition = transform.position;
		Velocity += Acceleration;
		CurrentPosition += Velocity;
		Rotation += Omiga;

		// AfterFrame?
		++Timer;
		++AniTimer;

		var renderFunc = _classTable[(int)ObjFuncIndex.Render] as LuaFunction;
		if (renderFunc != null)
		{
			renderFunc.call(_objTable);
		}

		if (ObjectStatus != Status.Default && ObjectStatus != Status.Free)
		{
			Destroy(gameObject);
		}
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
	}

	public void DelSelf()
	{
		ObjectStatus = Status.Del;
	}

	public void OnTriggerEnter2D(Collider2D collision)
	{
		var other = collision.gameObject.GetComponent<LSTGObject>();
		if (other == null || !other || !Game.GameInstance.ShouldCollideWith(Group, other.Group))
		{
			return;
		}

		var colliFunc = _classTable[(int) ObjFuncIndex.Colli] as LuaFunction;
		if (colliFunc != null)
		{
			colliFunc.call(_objTable, other._objTable);
		}
	}

	public void OnTriggerExit2D(Collider2D collision)
	{
		if (collision == Game.GameInstance.Bound && Bound)
		{
			DelSelf();
		}
	}
}
