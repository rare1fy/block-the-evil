using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using YooAsset;

namespace Framework {
    public class ResourceManagerNew : Singleton<ResourceManagerNew>
    {
        private ResourcePackage package = null;

        private const string PACKAGENAME = "DefaultPackage";

        /// <summary>
        /// 临时资源缓存
        /// </summary>
        private readonly List<AssetHandle> _handles = new(1024);

        /// <summary>
        /// 公共资源缓存
        /// </summary>
        private readonly List<AssetHandle> _commonHandles = new(1024);

        public override void Init()
        {
            package = YooAssets.TryGetPackage(PACKAGENAME);
            if (package == null)
            {
                package = YooAssets.CreatePackage(PACKAGENAME);
                YooAssets.SetDefaultPackage(package);
            }
        }

        /// <summary>
        /// 加载Sprite资源
        /// </summary>
        /// <param name="assetName">资源名</param>
        /// <param name="image">需要赋值的image</param>
        public void LoadSpriteAsset(string assetName, Image image)
        {
            if (!ReferenceEquals(image, null))
            {
                LoadAssetAsync(assetName, delegate (Sprite sprite)
                {
                     image.sprite = sprite;
                });
            }
        }
        
        /// <summary>
        /// 加载Texture资源
        /// </summary>
        /// <param name="assetName">资源名</param>
        /// <param name="image">需要赋值的image</param>
        public void LoadTextureAsset(string assetName, RawImage image)
        {
            if (!ReferenceEquals(image, null))
            {
                LoadAssetAsync(assetName, delegate (Texture texture)
                {
                    image.texture = texture;
                });
            }
        }

        /// <summary>
        /// 异步加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param> 资源名称
        /// <param name="succeedCallback"></param> 成功回调
        /// <param name="failedCallback"></param>   失败回调
        /// <param name="isCommonAsset"></param> 是否公用资源
        public async void LoadAssetAsync<T>(string assetName, Action<T> succeedCallback = null,
            Action failedCallback = null, bool isCommonAsset = false)
            where T : UnityEngine.Object
        {
            try
            {
                if (package != null)
                {
                    int index = assetName.LastIndexOf(".");
                    var name = index >= 0 ? assetName.Substring(0, index) : assetName;
                    AssetHandle assetHandle = package.LoadAssetAsync<T>(name);
                    await assetHandle.Task;
                    //  非公共资源
                    if (!isCommonAsset)
                    {
                        _handles.Add(assetHandle);
                    }
                    else
                    {
                        _commonHandles.Add(assetHandle);
                    }
                    if (assetHandle.Status == EOperationStatus.Succeed)
                    {
                        var asset = assetHandle.AssetObject as T;
                        succeedCallback?.Invoke(asset);
                    }
                    else
                    {
                        SDDebug.LogError("资源加载失败 name = " + assetName);
                        failedCallback?.Invoke();
                    }
                }
                else
                {
                    SDDebug.LogError("资源包为空！！！");
                    failedCallback?.Invoke();
                }
            }
            catch (Exception e)
            {
                SDDebug.LogError(e.ToString());
            }
        }
        
        // /// <summary>
        // /// 同步加载资源
        // /// </summary>
        // /// <typeparam name="T"></typeparam>
        // /// <param name="assetName"></param> 资源名称
        // /// <param name="succeedCallback"></param> 成功回调
        // /// <param name="failedCallback"></param>   失败回调
        // /// <param name="isCommonAsset"></param> 是否公用资源
        // public T LoadAssetSync<T>(string assetName, bool isCommonAsset = false)
        //     where T : UnityEngine.Object
        // {
        //     try
        //     {
        //         if (package != null)
        //         {
        //             int index = assetName.LastIndexOf(".");
        //             var name = index >= 0 ? assetName.Substring(0, index) : assetName;
        //             AssetHandle assetHandle = package.LoadAssetSync<T>(name);
        //             //  非公共资源
        //             if (!isCommonAsset)
        //             {
        //                 _handles.Add(assetHandle);
        //             }
        //             else
        //             {
        //                 _commonHandles.Add(assetHandle);
        //             }
        //             return assetHandle.AssetObject as T;
        //         }
        //         else
        //         {
        //             SDDebug.LogError("资源包为空！！！");
        //             return null;
        //         }
        //     }
        //     catch (Exception e)
        //     {
        //         SDDebug.LogError(e.ToString());
        //         return null;
        //     }
        // }

        /// <summary>
        /// 异步加载配置文件
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param>
        /// <param name="succeedCallback"></param>
        /// <param name="failedCallback"></param>
        public async void LoadConfigAsync(string assetName, Action<byte[]> succeedCallback, Action failedCallback = null)
        {
            try
            {
                if (package != null)
                {
                    int index = assetName.LastIndexOf(".");
                    var name = index >= 0 ? assetName.Substring(0, index) : assetName;
                    AssetHandle assetHandle = package.LoadAssetAsync<TextAsset>(name);
                    await assetHandle.Task;
                    //配置表也试做公共资源
                    _commonHandles.Add(assetHandle);
                    if (assetHandle.Status == EOperationStatus.Succeed) 
                    {
                        TextAsset textAsset = assetHandle.GetAssetObject<TextAsset>();
                        succeedCallback?.Invoke(textAsset.bytes);
                    }
                    else
                    {
                        SDDebug.LogError("资源加载失败 name = " + assetName);
                        failedCallback?.Invoke();
                    }
                }
                else
                {
                    SDDebug.LogError("资源包为空！！！");
                    failedCallback?.Invoke();
                }
            }
            catch (Exception e)
            {
                SDDebug.LogError(e.ToString());
            }
        }
        
        public async UniTask<byte[]> LoadConfigAsync(string assetName)
        {
            try
            {
                if (package == null)
                {
                    SDDebug.LogError("资源包为空！！！");
                    return null;
                }

                int index = assetName.LastIndexOf(".");
                var name = index >= 0 ? assetName.Substring(0, index) : assetName;
        
                AssetHandle assetHandle = package.LoadAssetAsync<TextAsset>(name);
                await assetHandle.Task;
        
                // 配置表也视作公共资源
                _commonHandles.Add(assetHandle);
        
                if (assetHandle.Status == EOperationStatus.Succeed)
                {
                    TextAsset textAsset = assetHandle.GetAssetObject<TextAsset>();
                    return textAsset.bytes;
                }
                else
                {
                    SDDebug.LogError("资源加载失败 name = " + assetName);
                    return null;
                }
            }
            catch (Exception e)
            {
                SDDebug.LogError(e.ToString());
                return null;
            }
        }

        /// <summary>
        /// 通过标签加载资源
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="assetName"></param> 资源名称
        /// <param name="succeedCallback"></param> 成功回调
        /// <param name="failedCallback"></param>   失败回调
        public async void LoadByTagAllAsset<T>(string tag, bool isCommonAsset, Action<T> succeedCallback, Action allSucceedCallback)
            where T : ScriptableObject
        {
            try
            {
                if (package != null)
                {
                    var assetInfos = package.GetAssetInfos(tag);
                    var length = assetInfos.Length;
                    for (int i = 0; i < length; i++)
                    {
                        var assetInfo = assetInfos[i];
                        var assetName = assetInfo.AssetPath;
                        var index = assetName.LastIndexOf(".");
                        var name = index >= 0 ? assetName.Substring(0, index) : assetName;
                        AssetHandle assetHandle = package.LoadAssetAsync<T>(name);
                        await assetHandle.Task;
                        //  非公共资源
                        if (!isCommonAsset)
                        {
                            _handles.Add(assetHandle);
                        }
                        else
                        {
                            _commonHandles.Add(assetHandle);
                        }
                        if (assetHandle.Status == EOperationStatus.Succeed)
                        {
                            var asset = assetHandle.AssetObject as T;
                            succeedCallback?.Invoke(asset);
                        }
                        else
                        {
                            SDDebug.LogError("资源加载失败 name = " + assetName);
                        }
                    }
                    allSucceedCallback?.Invoke();
                }
                else
                {
                    SDDebug.LogError("资源包为空！！！");
                }
            }
            catch (Exception e)
            {
                SDDebug.LogError(e.ToString());
            }
        }

        /// <summary>
        /// 资源清理
        /// </summary>
        public async void ClearPackageUnusedCacheFiles(Action succeedCallback = null)
        {
            try
            {
                if (package != null)
                {
                    if (_handles.Count > 0)
                    {
                        foreach (var handle in _handles)
                        {
                            handle.Release();
                        }
                        _handles.Clear();
                    }
                    var operation = package.UnloadUnusedAssetsAsync();
                    await operation.Task;
                    if (operation.Status == EOperationStatus.Succeed)
                    {
                        //清理成功
                        GC.Collect(); // 手动触发垃圾收集
                        succeedCallback?.Invoke();
                    }
                    else
                    {
                        //清理失败
                        SDDebug.LogError(operation.Error);
                    }
                }
                else
                {
                    SDDebug.LogError("资源包为空！！！");
                }
            }
            catch (Exception e)
            {
                SDDebug.LogError(e.ToString());
            }
        }

        /// <summary>
        /// 尝试卸载指定的资源对象,注意：如果该资源还在被使用，该方法会无效
        /// </summary>
        /// <param name="assetName"></param>
        public void TryUnloadUnusedAsset(string assetName)
        {
            try
            {
                if (package != null)
                {
                    package.TryUnloadUnusedAsset(assetName);
                }
                else
                {
                    SDDebug.LogError("资源包为空！！！");
                }
            }
            catch (Exception e)
            {
                SDDebug.LogError(e.ToString());
            }
        }

        public override void Dispose()
        {
            if (YooAssets.Initialized)
            {
                if (_handles.Count > 0)
                {
                    foreach (var handle in _handles)
                    {
                        handle.Release();
                    }
                    _handles.Clear();
                }
                if (_commonHandles.Count > 0)
                {
                    foreach (var handle in _commonHandles)
                    {
                        handle.Release();
                    }
                    _commonHandles.Clear();
                }
                var package = YooAssets.TryGetPackage(PACKAGENAME);
                package?.UnloadAllAssetsAsync();
                YooAssets.Destroy();
            }
        }
    }
}

