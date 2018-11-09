using UnityEngine;
using System.Collections;
using LuaInterface;
using System.Collections.Generic;
using System.IO;
using Framework;

public class CustomLuaResLoader : LuaFileUtils
{
    private Dictionary<string, Resource> m_dicZipAssetBundleRes = new Dictionary<string, Resource>();
    public CustomLuaResLoader()
    {
        Instance = this;
        beZip = false;
    }

    protected List<string> bundleSearchPaths = new List<string>();

    public override void Dispose()
    {
        if (instance != null)
        {
            instance = null;
            searchPaths.Clear();
            bundleSearchPaths.Clear();
            foreach (KeyValuePair<string, AssetBundle> iter in zipMap)
            {
                iter.Value.Unload(true);
            }

            zipMap.Clear();

            foreach (var item in m_dicZipAssetBundleRes)
            {
                item.Value.Release();
            }
            zipMap.Clear();
        }
    }

    public void AddSearchBundleResource(string name, Resource res)
    {
        if (!m_dicZipAssetBundleRes.ContainsKey(name))
        {
            res.Retain();
            m_dicZipAssetBundleRes[name] = res;
        }
    }

    public bool AddAssetBundleSearchPath(string searchRootPath)
    {
        if (!searchRootPath.EndsWith("/")) searchRootPath += "/";
        searchRootPath += "?.lua.bytes";
        int index = bundleSearchPaths.IndexOf(searchRootPath);

        if (index >= 0)
        {
            return false;
        }
        bundleSearchPaths.Add(searchRootPath);
        return true;
    }

    public override byte[] ReadFile(string fileName)
    {
        if (!beZip)
        {
            string path = FindFile(fileName);
            byte[] str = null;

            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
#if !UNITY_WEBPLAYER
                str = File.ReadAllBytes(path);
#else
                    throw new LuaException("can't run in web platform, please switch to other platform");
#endif
            }

            return str;
        }
        else
        {
            return ReadZipFile(fileName);
        }
    }

    public string FindAssetBundleFile(string fileName)
    {
        if (fileName == string.Empty)
        {
            return string.Empty;
        }

        if (fileName.EndsWith(".lua"))
        {
            fileName = fileName.Substring(0, fileName.Length - 4);
        }

        string fullPath = null;

        for (int i = 0; i < bundleSearchPaths.Count; i++)
        {
            fullPath = bundleSearchPaths[i].Replace("?", fileName);
            if (m_dicZipAssetBundleRes.ContainsKey(fullPath))
            {
                return fullPath;
            }
        }

        return null;
    }

    byte[] ReadZipFile(string fileName)
    {
        Resource zipFile = null;
        byte[] buffer = null;
        string zipName = null;

        zipName = FindAssetBundleFile(fileName);
        fileName = zipName.ToLower();
        m_dicZipAssetBundleRes.TryGetValue(zipName, out zipFile);
        if (zipFile != null && zipFile.assetBundle != null)
        {
#if UNITY_4_6 || UNITY_4_7
                TextAsset luaCode = zipFile.assetBundle.Load(fileName, typeof(TextAsset)) as TextAsset;
#else
            TextAsset luaCode = zipFile.assetBundle.LoadAsset<TextAsset>(fileName);
#endif
            if (luaCode != null)
            {
                buffer = luaCode.bytes;
                Resources.UnloadAsset(luaCode);
            }
        }

        return buffer;
    }
}
