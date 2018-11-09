using Framework;
using LuaInterface;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Starter : MonoBehaviour {

	// Use this for initialization
	void Start () {

        InitSingleton();
        StartCoroutine(UnCompressPackage());
	}

    protected void InitSingleton()
    {
        gameObject.AddComponentOnce<ResourceSys>();
        bool directLoadMode = true;
#if !UNITY_EDITOR || BUNDLE_MODE
            directLoadMode = false;
#endif
        ResourceSys.Instance.Init(directLoadMode, "Assets/ResourceEx");

        gameObject.AddComponent<LuaManager>();
        LuaManager.Instance.Init();
    }

    private IEnumerator UnCompressPackage()
    {
       
        if (ResourceSys.Instance.DirectLoadMode)
        {
            OnStartGame();
        }
        else
        {
            ResourceSys.Instance.assetBundleFile.Init("assetpath_mapping", OnLoadAssetBundleFile);
        }
        yield return null;
    }

    private int luaFileCount;
    private void OnLoadAssetBundleFile(bool succ)
    {
        if (succ)
        {
            //下载所有的lua的bundle，并设置LuaFileUtil的参数
            var dic = ResourceSys.Instance.assetBundleFile.dicAssetPathToAssetBundleNames;
            List<string> luaFiles = new List<string>();
            foreach (var item in dic)
            {
                if(item.Key.StartsWith("LuaBytes/"))
                {
                    luaFiles.Add(item.Key);
                }
            }
            luaFileCount = luaFiles.Count;
            if (luaFileCount > 0)
            {
                for (int i = 0; i < luaFiles.Count; i++)
                {
                    ResourceSys.Instance.GetResource(luaFiles[i], OnLoadLuaAssetBundle);
                }
            }
            else
            {
                OnStartGame();
            }
          
        }
    }

    private static int LuaDirPrexLen = "LuaBytes/".Length;
    private void OnLoadLuaAssetBundle(Resource res, string path)
    {
        //将当前lua路径与bundle添加到luaFileUtil中
        //LuaBytes下ToLua目录与Custom目录都是以当前目录为根节点处理的，所以会有点问题，明天再想
        string rootDir = ResourceSys.Instance.ResRootDir;
        if (!rootDir.EndsWith("/")) rootDir += "/";
        string fileName = rootDir + path;
        if(res.assetBundle != null)
        {
            ((CustomLuaResLoader)LuaFileUtils.Instance).AddSearchBundleResource(fileName,res);
        }
        luaFileCount--;
        if(luaFileCount <= 0)
        {
            OnStartGame();
        }
    }

    private void OnStartGame()
    {
       
        LuaManager.Instance.StartLua();
    }
}
