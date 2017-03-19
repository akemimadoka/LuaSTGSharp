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
			Debug.Assert(axis == Vector3.forward);
			return angle;
		}
		set { transform.rotation = Quaternion.AngleAxis(value, Vector3.forward); }
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

	public Resource RenderResource { get; set; }
	public ResParticle Particle { get; set; }

	private LuaTable _luaTable;

	// Use this for initialization
	private void Start()
	{
		ObjectStatus = Status.Free;
		gameObject.AddComponent<BoxCollider2D>().isTrigger = true;
		LastPosition = transform.position;
		Delta = Vector2.zero;
		Omiga = 0;
		Velocity = Vector2.zero;
		Acceleration = Vector2.zero;
		Layer = 0;
		Bound = true;
		Navi = false;
		Group = 0;
		Timer = 0;
		AniTimer = 0;
	}

	private void OnAcquireLuaTable(LuaTable luaTable)
	{
		_luaTable = luaTable;
	}

	// Update is called once per frame
	private void Update()
	{
		LastPosition = transform.position;

		var frameFunc = _luaTable[3] as LuaFunction;
		if (frameFunc != null)
		{
			frameFunc.call(_luaTable);
		}

		// AfterFrame？
		++Timer;
		++AniTimer;

		if (ObjectStatus != Status.Default)
		{
			Destroy(gameObject);
		}
	}

	private void OnTriggerEnter2D(Collider2D collision)
	{

	}
}
