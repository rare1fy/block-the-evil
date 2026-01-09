using System.IO;
using System;
using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class CustomImporter : AssetPostprocessor
{
    public const string AndroidSettingName = "Android";
    public const string IPhoneSettingName = "iPhone";
    public const string WebGLSettingName = "WebGL";

    void OnPostprocessTexture(Texture2D texture)
    {
        if (!IsAtlas(assetPath) && !IsEditorTexture(assetPath))
        {
            CheckTexSize(texture, assetPath, true);
        }
    }

    /// <summary>
    /// 检查2d纹理的尺寸是否为4的倍数
    /// </summary>
    /// <param name="texture">纹理对象</param>
    /// <param name="path">资源路径</param>
    /// <param name="showHint">是否显示错误日志</param>
    /// <returns></returns>
    public static bool CheckTexSize(Texture2D texture, string path, bool showHint = false)
    {
        const int SIZE_BASE = 4;
        var width = texture.width;
        var height = texture.height;
        if (width % SIZE_BASE != 0 || height % SIZE_BASE != 0)
        {
            if (showHint)
            {
                if (width % SIZE_BASE != 0)
                    Debug.LogError("图片的长不是4的倍数>>> " + width % SIZE_BASE + ">>>" + path);
                if (height % SIZE_BASE != 0)
                    Debug.LogError("图片的宽不是4的倍数>>>" + height % SIZE_BASE + ">>>" + path);
            }
            return false;
        }
        else
        {
            return true; 
        }
    }

    /// <summary>
    /// 是否图集图片
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsAtlas(string path)
    {
        const string MARK = "Atlas";
        return !string.IsNullOrEmpty(path) && path.Contains(MARK);
    }

    /// <summary>
    /// 是否编辑器相关图片
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    public static bool IsEditorTexture(string path)
    {
        const string MARK = "Editor";
        return !string.IsNullOrEmpty(path) && path.Contains(MARK);
    }

    /// <summary>
    /// 纹理导入之前调用，针对入到的纹理进行设置  
    /// </summary>
    public void OnPreprocessTexture()
    {
        TextureImporter impor = this.assetImporter as TextureImporter;
        ProcessTexture(impor);
    }

    public static void ProcessTexture(TextureImporter impor)
    {
        var assetPath = impor.assetPath;
        if (!assetPath.Contains("Bundles") || !assetPath.EndsWith(".png"))
        {
            return;
        }
        var isSprite = IsAtlas(assetPath);
        if (isSprite)
        {
            impor.textureType = TextureImporterType.Sprite;
            impor.spriteImportMode = SpriteImportMode.Single;
            impor.spritePixelsPerUnit = 100;
            impor.fadeout = false;
        }
        else
        {
            impor.textureType = TextureImporterType.Default;
        }
        impor.textureCompression = TextureImporterCompression.Compressed;
        impor.textureShape = TextureImporterShape.Texture2D;
        impor.sRGBTexture = false;
        impor.alphaSource = TextureImporterAlphaSource.FromInput;
        impor.alphaIsTransparency = impor.DoesSourceTextureHaveAlpha();
        impor.isReadable = assetPath.EndsWith("_Read.png");
        impor.filterMode = FilterMode.Bilinear;
        impor.mipmapEnabled = false;

        TextureImporterPlatformSettings webFormat = new TextureImporterPlatformSettings();
        TextureImporterPlatformSettings androidFormat = new TextureImporterPlatformSettings();
        TextureImporterPlatformSettings iosFormat = new TextureImporterPlatformSettings();

        webFormat.overridden = true;
        androidFormat.overridden = true;
        iosFormat.overridden = true;

        webFormat.name = WebGLSettingName;
        androidFormat.name = AndroidSettingName;
        iosFormat.name = IPhoneSettingName;

        if (assetPath.EndsWith("_2048bg.png"))
        {
            webFormat.maxTextureSize = 2048;
            androidFormat.maxTextureSize = 2048;
            iosFormat.maxTextureSize = 2048;
        }
        else
        {
            webFormat.maxTextureSize = 1024;
            androidFormat.maxTextureSize = 1024;
            iosFormat.maxTextureSize = 1024;
        }

        androidFormat.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;
        iosFormat.resizeAlgorithm = TextureResizeAlgorithm.Mitchell;

        androidFormat.compressionQuality = 1;
        iosFormat.compressionQuality = 1;

        if (assetPath.Contains("[RGBA16]"))
        {
            if (impor.DoesSourceTextureHaveAlpha())
            {
                androidFormat.format = TextureImporterFormat.RGBA16;
                iosFormat.format = TextureImporterFormat.RGBA16;
            }
            else
            {
                androidFormat.format = TextureImporterFormat.RGB16;
                iosFormat.format = TextureImporterFormat.RGB16;
            }
        }
        else if (assetPath.Contains("[RGBA32]"))
        {
            if (impor.DoesSourceTextureHaveAlpha())
            {
                androidFormat.format = TextureImporterFormat.RGBA32;
                iosFormat.format = TextureImporterFormat.RGBA32;
            }
            else
            {
                androidFormat.format = TextureImporterFormat.RGB24;
                iosFormat.format = TextureImporterFormat.RGB24;
            }
        }
        else
        {
            if (impor.DoesSourceTextureHaveAlpha())
            {
                androidFormat.format = TextureImporterFormat.ETC2_RGBA8;
                iosFormat.format = TextureImporterFormat.ASTC_4x4;
            }
            else
            {
                androidFormat.format = TextureImporterFormat.ETC2_RGB4;
                iosFormat.format = TextureImporterFormat.ASTC_4x4;
            }

            androidFormat.androidETC2FallbackOverride = AndroidETC2FallbackOverride.Quality32Bit;
        }

        impor.mipmapEnabled = false;

        impor.SetPlatformTextureSettings(androidFormat);
        impor.SetPlatformTextureSettings(iosFormat);
        impor.SetPlatformTextureSettings(webFormat);
    }
}

public class CustomImporterEditor
{
    [MenuItem("Tools/TextureReImport")]
    public static void ProcessAllTexture()
    {
        string[] allFiles = Directory.GetFiles(Application.dataPath, "*.png", SearchOption.AllDirectories);
        int i = 0;
        foreach (string matFile in allFiles)
        {
            i++;
            try
            {
                string assetPath = "Assets" + matFile.Replace(Application.dataPath, "").Replace('\\', '/');
                EditorUtility.DisplayProgressBar("处理图片", assetPath, (float)i / (float)allFiles.Length);
                if (assetPath.EndsWith(".png") || assetPath.EndsWith(".PNG"))
                {
                    AssetImporter assetImporter = TextureImporter.GetAtPath(assetPath);
                    TextureImporter textureImporter = assetImporter as TextureImporter;
                    if (null != textureImporter)
                        CustomImporter.ProcessTexture(textureImporter);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.StackTrace);
            }
        }
        Debug.Log("处理完毕==================================");
        EditorUtility.ClearProgressBar();
    }
}

