using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSCore;
using CSCore.Codecs;
using CSCore.Streams.SampleConverter;
using SLua;
using UnityEngine;

public enum BlendMode
{
	AddAdd = 1,
	AddAlpha,
	AddSub,
	AddRev,
	MulAdd,
	MulAlpha,
	MulSub,
	MulRev
};

/// <summary>
///     资源抽象基类
/// </summary>
/// <remarks>
///     资源必须有一个构造函数接受一个字符串参数作为name
/// </remarks>
public abstract class Resource : IDisposable
{
	private readonly string _name;

	protected Resource(string name)
	{
		_name = name;
	}

	public string GetName()
	{
		return _name;
	}

	public abstract bool InitFromStream(Stream stream);

	public virtual void Dispose()
	{
		// 默认啥都不做。。。
	}
}

/// <summary>
/// 需要路径信息的资源
/// </summary>
/// <remarks>
/// 说白了就是用来糊ResAudio的。。
/// </remarks>
public abstract class ResourceNeedPath : Resource
{
	protected ResourceNeedPath(string name) 
		: base(name)
	{
	}

	public abstract void OnAcquirePath(string path);
}

public interface IResourceDataProvider : IDisposable
{
	Stream GetResourceStream(string path, object additionalData = null);
}

public class ResourceException
	: Exception
{
	private readonly Type _resourceType;

	public ResourceException()
	{
	}

	public ResourceException(string message)
		: base(message)
	{
	}

	public ResourceException(Type resourceType, string message = null)
		: this(message ?? string.Format("Exception from resource of type {0}.", resourceType))
	{
		_resourceType = resourceType;
	}

	public Type GetResourceType()
	{
		return _resourceType;
	}
}

public class ResourceAlreadyInitializedException
	: ResourceException
{
	public ResourceAlreadyInitializedException()
	{
	}

	public ResourceAlreadyInitializedException(string message)
		: base(message)
	{
	}

	public ResourceAlreadyInitializedException(Type resourceType, string message = null)
		: base(resourceType, message ?? string.Format("Resource of type {0} has already initialized.", resourceType))
	{
	}
}

public class ResourceNotInitializedException
	: ResourceException
{
	public ResourceNotInitializedException()
	{
	}

	public ResourceNotInitializedException(string message)
		: base(message)
	{
	}

	public ResourceNotInitializedException(Type resourceType, string message = null)
		: base(resourceType, message ?? string.Format("Resource of type {0} has not initialized.", resourceType))
	{
	}
}

public sealed class LocalFileProvider
	: IResourceDataProvider
{
	private readonly string _basePath;

	public LocalFileProvider(string basePath)
	{
		if (!Directory.Exists(basePath))
		{
			throw new DirectoryNotFoundException("basePath does not exist.");
		}

		_basePath = basePath;
	}

	public void Dispose()
	{
	}

	public Stream GetResourceStream(string path, object additionalData = null)
	{
		return new FileStream(Path.Combine(_basePath, path), FileMode.Open);
	}
}

public class ResTexture
	: Resource
{
	private readonly bool _mipmap;
	private Texture2D _texture;
	private Material _cachedMaterial;

	public ResTexture(string name)
		: base(name)
	{
	}

	public ResTexture(string name, bool mipmap)
		: base(name)
	{
		_mipmap = mipmap;
	}

	public override bool InitFromStream(Stream stream)
	{
		if (_texture != null)
		{
			return false;
		}

		_texture = new Texture2D(0, 0, TextureFormat.RGBA32, _mipmap);
		var buffer = new byte[stream.Length];
		stream.Read(buffer, 0, (int) stream.Length);
		return _texture.LoadImage(buffer);
	}

	public Texture2D GetTexture()
	{
		if (_texture == null)
		{
			throw new ResourceNotInitializedException(typeof(ResTexture));
		}
		return _texture;
	}

	public Material GetMaterial()
	{
		var texture = GetTexture();
		return _cachedMaterial ?? (_cachedMaterial = new Material(Shader.Find("Standard"))
		{
			mainTexture = texture
		});
	}
}

public class ResAudio
	: ResourceNeedPath
{
	private string _ext;
	private IWaveSource _waveSource;
	private ISampleSource _decoder;
	private AudioClip _audioClip;
	private long _loopBegin, _loopEnd;

	public ResAudio(string name)
		: base(name)
	{
	}

	public override void OnAcquirePath(string path)
	{
		var ext = Path.GetExtension(path);
		if (string.IsNullOrEmpty(ext))
		{
			throw new ArgumentException("path is invalid.", "path");
		}
		_ext = ext.TrimStart('.');
	}

	public override void Dispose()
	{
		if (_decoder != null)
		{
			_decoder.Dispose();
			_decoder = null;
		}

		if (_waveSource != null)
		{
			_waveSource.Dispose();
			_waveSource = null;
		}
	}

	public override bool InitFromStream(Stream stream)
	{
		if (_waveSource != null || string.IsNullOrEmpty(_ext))
		{
			return false;
		}

		//var memStream = new MemoryStream();
		//stream.CopyTo(memStream);
		_waveSource = CodecFactory.Instance.GetCodec(stream, _ext);
		return _waveSource != null;
	}

	public void SetLoopInfo(float begin, float end)
	{
		begin *= 1000;
		end = Mathf.Min(end * 1000, _waveSource.GetLength().Milliseconds);

		if (begin >= end)
		{
			throw new ArgumentOutOfRangeException("begin", "begin should be smaller than end and both should be in the range of audio.");
		}

		if (begin <= 0)
		{
			throw new ArgumentOutOfRangeException("begin", "begin and end should not be smaller than zero.");
		}

		var waveFormat = _waveSource.WaveFormat;
		_loopBegin = waveFormat.MillisecondsToBytes(begin) / waveFormat.BytesPerSample;
		_loopEnd = waveFormat.MillisecondsToBytes(end) / waveFormat.BytesPerSample;
	}

	public AudioClip GetAudioClip()
	{
		if (_waveSource == null)
		{
			throw new ResourceNotInitializedException(typeof(ResAudio));
		}

		if (_audioClip == null)
		{
			_decoder = WaveToSampleBase.CreateConverter(_waveSource);
			_loopBegin = 0;
			_loopEnd = _decoder.Length / _decoder.WaveFormat.Channels;
			_audioClip = AudioClip.Create(GetName(), (int) _loopEnd,
				_decoder.WaveFormat.Channels, _decoder.WaveFormat.SampleRate, true, OnReadAudio, OnSetPosition);
		}

		return _audioClip;
	}

	private void OnReadAudio(float[] data)
	{
		if (_decoder == null)
		{
			throw new ObjectDisposedException("this", "This resource has already disposed.");
		}

		var remainedLength = _loopEnd - _decoder.Position / _decoder.WaveFormat.Channels;
		if (remainedLength < data.Length)
		{
			var readLength = remainedLength == 0 ? 0 : _decoder.Read(data, 0, (int) remainedLength);
			_decoder.Position = _loopBegin;
			_decoder.Read(data, readLength, data.Length - readLength);
		}
		else
		{
			var buffer = new float[data.Length];
			_decoder.Read(buffer, 0, buffer.Length);
			buffer.CopyTo(data, 0);
		}
	}

	private void OnSetPosition(int position)
	{
		if (_decoder == null)
		{
			throw new ObjectDisposedException("this", "This resource has already disposed.");
		}

		_decoder.Position = position * _decoder.WaveFormat.Channels;
	}
}

public class ResText
	: Resource
{
	private string _content;

	public ResText(string name)
		: base(name)
	{
	}

	public override bool InitFromStream(Stream stream)
	{
		using (var reader = new StreamReader(stream))
		{
			_content = reader.ReadToEnd();
		}

		return true;
	}

	public string GetContent()
	{
		return _content;
	}
}

public class ResLuaScript
	: ResText
{
	public ResLuaScript(string name)
		: base(name)
	{
	}

	public object Execute(LuaState luaState)
	{
		return luaState.doString(GetContent(), GetName());
	}
}

/// <summary>
///     精灵资源
/// </summary>
/// <remarks>
///     只是为了能统一从资源池中加载而写的适配器
/// </remarks>
public class ResSprite
	: Resource
{
	private readonly Sprite _sprite;
	private readonly Vector2 _ab;
	private readonly bool _isRect;

	public ResSprite(string name)
		: base(name)
	{
		throw new NotSupportedException("This resource should be created manually.");
	}

	public ResSprite(string name, Sprite sprite, float a, float b, bool isRect)
		: base(name)
	{
		_sprite = sprite;
		_ab = new Vector2(a, b);
		_isRect = isRect;
	}

	public override bool InitFromStream(Stream stream)
	{
		throw new NotSupportedException("This resource cannot be initialized from stream.");
	}

	public Sprite GetSprite()
	{
		return _sprite;
	}

	public Vector2 GetAb()
	{
		return _ab;
	}

	public bool IsRect()
	{
		return _isRect;
	}
}

public class ResAnimation
	: Resource
{
	private readonly List<Sprite> _imageSequence = new List<Sprite>();
	private readonly uint _interval;
	public BlendMode BlendMode { get; set; }
	private readonly Vector2 _halfSize;
	private readonly bool _isRect;

	public ResAnimation(string name)
		: base(name)
	{
		throw new NotSupportedException("This resource should be created manually.");
	}

	public ResAnimation(string name, ResTexture texture, float x, float y, float width, float height, uint n, uint m,
		uint interval, float a, float b, bool rect = false)
		: base(name)
	{
		var rawTexture = texture.GetTexture();

		for (uint j = 0; j < m; ++j)
		{
			for (uint i = 0; i < n; ++i)
			{
				var sprite = Sprite.Create(rawTexture,
					new Rect(x + width * i, y + height * j, width, height), new Vector2(0.5f, 0.5f));
				_imageSequence.Add(sprite);
			}
		}

		_interval = interval;
		BlendMode = BlendMode.MulAlpha;
		_halfSize = new Vector2(a, b);
		_isRect = rect;
	}

	public override bool InitFromStream(Stream stream)
	{
		throw new NotSupportedException("This resource cannot be initialized from stream.");
	}

	public int GetCount()
	{
		return _imageSequence.Count;
	}

	public Sprite GetSprite(int index)
	{
		return index >= _imageSequence.Count ? null : _imageSequence[index];
	}

	public uint GetInterval()
	{
		return _interval;
	}

	public Vector2 GetHalfSize()
	{
		return _halfSize;
	}

	public bool IsRect()
	{
		return _isRect;
	}
}

/// <summary>
///     资源池类型
/// </summary>
/// <remarks>
///     更大的值表示资源池在查找中的优先度越高
/// </remarks>
public enum ResourcePoolType
{
	Global,
	Stage
}

public sealed class ResourcePool
{
	private readonly ResourceManager _resourceManager;
	private readonly ResourcePoolType _resourcePoolType;
	private readonly Dictionary<Type, Dictionary<string, Resource>> _resourcePool = new Dictionary<Type, Dictionary<string, Resource>>();

	public ResourcePool(ResourceManager resourceManager, ResourcePoolType resourcePoolType)
	{
		Debug.Assert(resourceManager != null);
		_resourceManager = resourceManager;
		_resourcePoolType = resourcePoolType;
	}

	public ResourcePoolType GetResourcePoolType()
	{
		return _resourcePoolType;
	}

	public Resource GetResource(string name)
	{
		foreach (var resources in _resourcePool)
		{
			Resource resource;
			if (resources.Value.TryGetValue(name, out resource))
			{
				return resource;
			}
		}

		return null;
	}
		
	public Resource GetResourceAs(string name, Type asResourceType, string path = null, bool autoLoad = true)
	{
		Dictionary<string, Resource> resources;
		if (!_resourcePool.TryGetValue(asResourceType, out resources))
		{
			_resourcePool.Add(asResourceType, resources = new Dictionary<string, Resource>());
		}

		Resource resource;
		resources.TryGetValue(name, out resource);

		if (path == null)
		{
			path = name;
		}

		if (resource == null && autoLoad)
		{
			var constructor = asResourceType.GetConstructor(new[] { typeof(string) });
			if (constructor != null)
			{
				resource = constructor.Invoke(new object[] { name }) as Resource;
				if (resource != null)
				{
					var resourceNeedPath = resource as ResourceNeedPath;
					if (resourceNeedPath != null)
					{
						resourceNeedPath.OnAcquirePath(path);
					}

					var stream = _resourceManager.GetResourceStream(path);
					if (stream == null || !resource.InitFromStream(stream))
					{
						resource.Dispose();
						return null;
					}

					resources.Add(name, resource);
				}
			}
		}

		return resource;
	}

	public T GetResourceAs<T>(string name, string path = null, bool autoLoad = true) where T : Resource
	{
		return GetResourceAs(name, typeof(T), path, autoLoad) as T;
	}

	public bool AddResource(Resource resource)
	{
		var resourceType = resource.GetType();
		Dictionary<string, Resource> resources;
		if (!_resourcePool.TryGetValue(resourceType, out resources))
		{
			_resourcePool.Add(resourceType, resources = new Dictionary<string, Resource>());
		}

		if (resources.ContainsKey(resource.GetName()))
		{
			return false;
		}

		resources.Add(resource.GetName(), resource);
		return true;
	}

	public bool ResourceExists(string name, Type resourceType)
	{
		Dictionary<string, Resource> resources;
		return _resourcePool.TryGetValue(resourceType, out resources) && resources.ContainsKey(name);
	}

	public void Clear()
	{
		try
		{
			foreach (var resources in _resourcePool)
			{
				foreach (var resource in resources.Value)
				{
					resource.Value.Dispose();
				}
			}
		}
		finally
		{
			_resourcePool.Clear();
		}
	}
}

public sealed class ResourceManager
	: IResourceDataProvider
{
	private readonly Dictionary<ResourcePoolType, ResourcePool> _resourcePools =
		new Dictionary<ResourcePoolType, ResourcePool>();
	private readonly List<IResourceDataProvider> _resourceDataProviders = new List<IResourceDataProvider>();
	private ResourcePoolType _activedPoolType = ResourcePoolType.Global;
	private bool _disposed;

	public ResourceManager()
	{
		foreach (var resourcePoolType in Enum.GetValues(typeof(ResourcePoolType)).OfType<ResourcePoolType>())
		{
			_resourcePools.Add(resourcePoolType, new ResourcePool(this, resourcePoolType));
		}
	}

	public void Dispose()
	{
		if (_disposed)
		{
			return;
		}

		try
		{
			DoReset();
		}
		finally
		{
			_disposed = true;
		}
	}

	public void ThrowIfDisposed()
	{
		if (_disposed)
		{
			throw new ObjectDisposedException("this", "ResourceManager has already disposed.");
		}
	}

	// 无视是否已释放
	private void DoReset()
	{
		foreach (var resourceDataProvider in _resourceDataProviders)
		{
			resourceDataProvider.Dispose();
		}
		_resourceDataProviders.Clear();
		foreach (var resourcePool in _resourcePools)
		{
			resourcePool.Value.Clear();
		}
		_resourcePools.Clear();
		_activedPoolType = ResourcePoolType.Global;
	}

	public void Reset()
	{
		ThrowIfDisposed();
		DoReset();
	}

	public ResourcePoolType GetActivedPoolType()
	{
		ThrowIfDisposed();
		return _activedPoolType;
	}

	public void SetActivedPoolType(ResourcePoolType resourcePoolType)
	{
		ThrowIfDisposed();
		_activedPoolType = resourcePoolType;
	}

	public void AddResourceDataProvider(IResourceDataProvider resourceDataProvider)
	{
		ThrowIfDisposed();
		_resourceDataProviders.Add(resourceDataProvider);
	}

	public void DeleteAllResourceDataProvider()
	{
		ThrowIfDisposed();
		_resourceDataProviders.Clear();
	}

	public ResourcePool GetActivedPool()
	{
		ThrowIfDisposed();
		return GetResourcePool(_activedPoolType);
	}

	public ResourcePool GetResourcePool(ResourcePoolType resourcePoolType)
	{
		ThrowIfDisposed();
		ResourcePool resourcePool;
		_resourcePools.TryGetValue(resourcePoolType, out resourcePool);
		return resourcePool;
	}

	public Stream GetResourceStream(string path, object additionalData = null)
	{
		ThrowIfDisposed();
		return _resourceDataProviders.Reverse<IResourceDataProvider>().Select(pack => pack.GetResourceStream(path, additionalData)).FirstOrDefault(data => data != null);
	}
		
	public Resource FindResource(string name)
	{
		ThrowIfDisposed();
		return Enum.GetValues(typeof(ResourcePoolType))
			.OfType<ResourcePoolType>()
			.Reverse()
			.Select(resourcePoolType => _resourcePools[resourcePoolType].GetResource(name))
			.FirstOrDefault(resource => resource != null);
	}

	public Resource FindResourceAs(string name, Type resourceType, string path = null, bool autoLoad = true)
	{
		ThrowIfDisposed();
		return Enum.GetValues(typeof(ResourcePoolType))
			.OfType<ResourcePoolType>()
			.Reverse()
			.Select(resourcePoolType => _resourcePools[resourcePoolType].GetResourceAs(name, resourceType, path, autoLoad))
			.FirstOrDefault(resource => resource != null);
	}

	public T FindResourceAs<T>(string name, string path = null, bool autoLoad = true) where T : Resource
	{
		ThrowIfDisposed();
		return Enum.GetValues(typeof(ResourcePoolType))
			.OfType<ResourcePoolType>()
			.Reverse()
			.Select(resourcePoolType => _resourcePools[resourcePoolType].GetResourceAs<T>(name, path, autoLoad))
			.FirstOrDefault(resource => resource != null);
	}

	public bool ResourceExists(string name, Type resourceType)
	{
		ThrowIfDisposed();
		return Enum.GetValues(typeof(ResourcePoolType))
			.OfType<ResourcePoolType>()
			.Reverse()
			.Select(resourcePoolType => _resourcePools[resourcePoolType].ResourceExists(name, resourceType))
			.FirstOrDefault(v => v);
	}
}
