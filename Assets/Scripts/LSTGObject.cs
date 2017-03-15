using UnityEngine;
using System.Collections;
using Assets.Scripts;
using SLua;

public class LSTGObject : MonoBehaviour
{
	public enum Status
	{
		Free,
		Default,
		Kill,
		Del
	}

	public Status ObjectStatus { get; private set; }

	public int Id
	{
		get { return GetInstanceID(); }
	}
	
	public Vector2 CurrentPosition
	{
		get { return transform.position; }
		set { transform.position = value; }
	}

	public Vector2 LastPosition { get; set; }
	public Vector2 Delta { get; private set; }

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

	public float Omiga { get; set; }
	public Vector2 Velocity { get; set; }
	public Vector2 Acceleration { get; set; }
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

	public Collider2D Collider
	{
		get { return GetComponent<Collider2D>(); }
	}

	public bool Colli
	{
		get { return Collider.enabled; }
		set { Collider.enabled = value; }
	}

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

	public bool Bound { get; set; }

	public bool Hide
	{
		get { return GetComponent<SpriteRenderer>().enabled; }
		set { GetComponent<SpriteRenderer>().enabled = value; }
	}

	public bool Navi { get; set; }

	public int Group { get; set; }
	public int Timer { get; private set; }
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
