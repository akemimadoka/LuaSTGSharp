using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CSCore;
using UnityEngine;
using CSCore.Codecs;
using CSCore.Streams.SampleConverter;

namespace Assets.Scripts
{
	/// <summary>
	///     资源接口
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
		public abstract void Dispose();
	}

	public interface IResourceDataProvider : IDisposable
	{
		Stream GetResource(string path, object additionalData = null);
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

		public Stream GetResource(string path, object additionalData = null)
		{
			return new FileStream(Path.Combine(_basePath, path), FileMode.Open);
		}
	}

	public class ResTexture
		: Resource
	{
		private Texture2D _texture;
		private Material _cachedMaterial;

		public ResTexture(string name)
			: base(name)
		{
		}
		
		public override bool InitFromStream(Stream stream)
		{
			if (_texture != null)
			{
				return false;
			}

			_texture = new Texture2D(0, 0);
			var buffer = new byte[stream.Length];
			stream.Read(buffer, 0, (int) stream.Length);
			return _texture.LoadImage(buffer);
		}

		public override void Dispose()
		{
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
						var stream = _resourceManager.GetResource(name);
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

		public T GetResourceAs<T>(string name) where T : Resource
		{
			return GetResourceAs(name, typeof(T)) as T;
		}
	}

	public sealed class ResourceManager
	{
		private readonly Dictionary<ResourcePoolType, ResourcePool> _resourcePools =
			new Dictionary<ResourcePoolType, ResourcePool>();
		private readonly List<IResourceDataProvider> _resourceDataProviders = new List<IResourceDataProvider>();
		private ResourcePoolType _activedPoolType = ResourcePoolType.Global;

		public ResourceManager()
		{
			AddResourceDataProvider(new LocalFileProvider(Application.persistentDataPath));
			foreach (var resourcePoolType in Enum.GetValues(typeof(ResourcePoolType)).OfType<ResourcePoolType>())
			{
				_resourcePools.Add(resourcePoolType, new ResourcePool(this, resourcePoolType));
			}
		}

		public ResourcePoolType GetActivedPoolType()
		{
			return _activedPoolType;
		}

		public void SetActivedPoolType(ResourcePoolType resourcePoolType)
		{
			_activedPoolType = resourcePoolType;
		}

		public void AddResourceDataProvider(IResourceDataProvider resourceDataProvider)
		{
			_resourceDataProviders.Add(resourceDataProvider);
		}

		public void DeleteAllResourceDataProvider()
		{
			_resourceDataProviders.Clear();
		}

		public ResourcePool GetActivedPool()
		{
			return GetResourcePool(_activedPoolType);
		}

		public ResourcePool GetResourcePool(ResourcePoolType resourcePoolType)
		{
			ResourcePool resourcePool;
			_resourcePools.TryGetValue(resourcePoolType, out resourcePool);
			return resourcePool;
		}

		public Stream GetResource(string path, string password = null)
		{
			return _resourceDataProviders.Reverse<IResourceDataProvider>().Select(pack => pack.GetResource(path, password)).FirstOrDefault(data => data != null);
		}

		public Resource FindResource(string name)
		{
			return Enum.GetValues(typeof(ResourcePoolType))
					.OfType<ResourcePoolType>()
					.Reverse()
					.Select(resourcePoolType => _resourcePools[resourcePoolType].GetResource(name))
					.FirstOrDefault(resource => resource != null);
		}

		public Resource FindResourceAs(string name, Type resourceType)
		{
			return Enum.GetValues(typeof(ResourcePoolType))
					.OfType<ResourcePoolType>()
					.Reverse()
					.Select(resourcePoolType => _resourcePools[resourcePoolType].GetResourceAs(name, resourceType))
					.FirstOrDefault(resource => resource != null);
		}

		public T FindResourceAs<T>(string name) where T : Resource
		{
			return Enum.GetValues(typeof(ResourcePoolType))
					.OfType<ResourcePoolType>()
					.Reverse()
					.Select(resourcePoolType => _resourcePools[resourcePoolType].GetResourceAs<T>(name))
					.FirstOrDefault(resource => resource != null);
		}
	}
}
