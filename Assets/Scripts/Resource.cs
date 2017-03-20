using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CSCore;
using UnityEngine;
using CSCore.Codecs;
using CSCore.Streams.SampleConverter;

namespace Assets.Scripts
{
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

	public class ResSound
		: Resource
	{
		private readonly string _ext;
		private IWaveSource _waveSource;
		private WaveToSampleBase _decoder;
		private AudioClip _audioClip;

		public ResSound(string name)
			: base(name)
		{
			_ext = Path.GetExtension(name);
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
			if (_waveSource != null)
			{
				return false;
			}

			_waveSource = CodecFactory.Instance.GetCodec(stream, _ext);
			return _waveSource != null;
		}
		
		public AudioClip GetAudioClip()
		{
			if (_waveSource == null)
			{
				throw new ResourceNotInitializedException(typeof(ResSound));
			}

			if (_audioClip == null)
			{
				switch (_waveSource.WaveFormat.BitsPerSample)
				{
					case 8:
						_decoder = new Pcm8BitToSample(_waveSource);
						break;
					case 16:
						_decoder = new Pcm16BitToSample(_waveSource);
						break;
					case 24:
						_decoder = new Pcm24BitToSample(_waveSource);
						break;
					case 32:
						_decoder = new Pcm32BitToSample(_waveSource);
						break;
					default:
						throw new NotSupportedException("No supported converter.");
				}

				_audioClip = AudioClip.Create(GetName(), (int) (_decoder.Length / _decoder.WaveFormat.Channels),
					_decoder.WaveFormat.Channels, _decoder.WaveFormat.SampleRate, true, OnReadAudio, OnSetPosition);
			}

			return _audioClip;
		}

		private void OnReadAudio(float[] data)
		{
			if (_decoder != null)
			{
				_decoder.Read(data, 0, data.Length);
			}
		}

		private void OnSetPosition(int position)
		{
			if (_decoder != null)
			{
				_decoder.Position = position * _decoder.WaveFormat.Channels;
			}
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

		public object Execute()
		{
			return Game.GameInstance.LuaVM.luaState.doString(GetContent(), GetName());
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
		private readonly bool _isRect;

		public ResSprite(string name)
			: base(name)
		{
			throw new NotSupportedException("This resource should be created manually.");
		}

		public ResSprite(string name, Sprite sprite, bool isRect)
			: base(name)
		{
			_sprite = sprite;
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
		
		public Resource GetResourceAs(string name, Type asResourceType)
		{
			Dictionary<string, Resource> resources;
			if (!_resourcePool.TryGetValue(asResourceType, out resources))
			{
				_resourcePool.Add(asResourceType, resources = new Dictionary<string, Resource>());
			}

			Resource resource;
			resources.TryGetValue(name, out resource);

			if (resource == null)
			{
				var constructor = asResourceType.GetConstructor(new[] { typeof(string) });
				if (constructor != null)
				{
					resource = constructor.Invoke(new object[] { name }) as Resource;
					if (resource != null)
					{
						using (var stream = _resourceManager.GetResourceStream(name))
						{
							if (stream == null || !resource.InitFromStream(stream))
							{
								resource.Dispose();
								return null;
							}
						}

						resources.Add(name, resource);
					}
				}
			}

			return resource;
		}

		public T GetResourceAs<T>(string name) where T : Resource
		{
			return GetResourceAs(name, typeof(T)) as T;
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

		public Resource FindResourceAs(string name, Type resourceType)
		{
			ThrowIfDisposed();
			return Enum.GetValues(typeof(ResourcePoolType))
					.OfType<ResourcePoolType>()
					.Reverse()
					.Select(resourcePoolType => _resourcePools[resourcePoolType].GetResourceAs(name, resourceType))
					.FirstOrDefault(resource => resource != null);
		}

		public T FindResourceAs<T>(string name) where T : Resource
		{
			ThrowIfDisposed();
			return Enum.GetValues(typeof(ResourcePoolType))
					.OfType<ResourcePoolType>()
					.Reverse()
					.Select(resourcePoolType => _resourcePools[resourcePoolType].GetResourceAs<T>(name))
					.FirstOrDefault(resource => resource != null);
		}
	}
}
