using HG;
using HG.Coroutines;
using UnityEngine;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using RoR2;

namespace MysticsRisky2Utils.ContentManagement
{
    public class ContentLoadHelper
    {
        public ReadableProgress<float> progress;
        public ParallelProgressCoroutine coroutine;

        public ContentLoadHelper()
        {
            progress = new ReadableProgress<float>();
            coroutine = new ParallelProgressCoroutine(progress);
        }

        public static bool CheckTypeIsLoadableAsset(System.Type type)
        {
            if (!typeof(BaseLoadableAsset).IsAssignableFrom(type))
            {
                MysticsRisky2UtilsPlugin.logger.LogError($"Attempted to load {type.Name} that does not inherit from {typeof(BaseLoadableAsset).Name}");
                return false;
            }
            return true;
        }

        public void DispatchLoad<OutType>(Assembly assembly, System.Type loadType, System.Action<OutType[]> onComplete = null)
        {
            if (!CheckTypeIsLoadableAsset(loadType)) return;
            AsyncLoadingEnumerator<OutType> enumerator = new AsyncLoadingEnumerator<OutType>(assembly, loadType);
            enumerator.onComplete = onComplete;
            coroutine.Add(enumerator, enumerator.progressReceiver);
        }

        public static void PluginAwakeLoad(Assembly assembly, System.Type loadType)
        {
            if (!CheckTypeIsLoadableAsset(loadType)) return;
            foreach (System.Type type in GetAssemblyTypes(assembly).Where(x => !x.IsAbstract && loadType.IsAssignableFrom(x)).ToList())
            {
                BaseLoadableAsset loadableAsset = BaseLoadableAsset.Get(type);
                loadableAsset.OnPluginAwake();
            }
        }

        public static Dictionary<string, System.Type[]> assemblyNameToTypes = new Dictionary<string, System.Type[]>();
        public static System.Type[] GetAssemblyTypes(Assembly assembly)
        {
            string assemblyName = assembly.FullName;
            if (assemblyNameToTypes.ContainsKey(assemblyName)) return assemblyNameToTypes[assemblyName];
            System.Type[] types = assembly.GetTypes();
            assemblyNameToTypes.Add(assemblyName, types);
            return types;
        }

        public static void PluginAwakeLoad<T>(Assembly assembly)
        {
            PluginAwakeLoad(assembly, typeof(T));
        }

        public class AsyncLoadingEnumerator<OutType> : IEnumerator<object>, IEnumerator, System.IDisposable
        {
            object IEnumerator<object>.Current
            {
                get
                {
                    return current;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return current;
                }
            }

            public void Dispose() { }

            public void Reset() { }

            public System.Type current;
            public List<System.Type> types;
            public int position = 0;
            public List<OutType> loadedAssets = new List<OutType>();
            public System.Action<OutType[]> onComplete;
            public ReadableProgress<float> progressReceiver = new ReadableProgress<float>();

            public AsyncLoadingEnumerator(Assembly assembly, System.Type type)
            {
                types = GetAssemblyTypes(assembly).Where(x => !x.IsAbstract && type.IsAssignableFrom(x)).ToList();
            }

            public bool done
            {
                get
                {
                    return position >= types.Count;
                }
            }

            public bool sorted = false;

            bool IEnumerator.MoveNext()
            {
                if (!done)
                {
                    current = types[position];

                    BaseLoadableAsset loadableAsset = BaseLoadableAsset.Get(current);
                    loadableAsset.Load();
                    loadedAssets.Add((OutType)loadableAsset.asset);

                    position++;

                    progressReceiver.Report(Util.Remap(position / types.Count, 0f, 1f, 0f, 0.95f));
                }
                if (done)
                {
                    if (!sorted)
                    {
                        loadedAssets.Sort((x, y) => {
                            Object xObject = x as Object;
                            Object yObject = y as Object;
                            return string.Compare(xObject != null ? xObject.name : (x != null ? x.GetType().Name : ""), yObject != null ? yObject.name : (y != null ? y.GetType().Name : ""), System.StringComparison.OrdinalIgnoreCase);
                        });
                        progressReceiver.Report(0.97f);
                        sorted = true;
                        return true;
                    }
                    if (onComplete != null)
                    {
                        onComplete(loadedAssets.ToArray());
                    }
                    progressReceiver.Report(1f);
                    return false;
                }
                return true;
            }
        }

        public static void AddPrefixToAssets<T>(RoR2.ContentManagement.NamedAssetCollection<T> namedAssetCollection, string prefix) where T : Object
        {
            foreach (T asset in namedAssetCollection)
            {
                asset.name = prefix + asset.name;
            }
        }
    }

    public abstract class BaseLoadableAsset
    {
        public object asset;
        public virtual void OnPluginAwake() { }
        public virtual void OnLoad() { }
        public virtual void Load()
        {
            asset = this;
            OnLoad();
        }
        public abstract string TokenPrefix { get; }

        public static Dictionary<System.Type, BaseLoadableAsset> staticAssetDictionary = new Dictionary<System.Type, BaseLoadableAsset>();

        public static BaseLoadableAsset Get(System.Type type)
        {
            if (staticAssetDictionary.ContainsKey(type)) return staticAssetDictionary[type];
            else
            {
                BaseLoadableAsset obj = (BaseLoadableAsset)System.Activator.CreateInstance(type);
                staticAssetDictionary.Add(type, obj);
                return obj;
            }
        }
    }
}
