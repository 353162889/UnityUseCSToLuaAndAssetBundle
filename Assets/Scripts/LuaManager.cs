using Framework;
using LuaInterface;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LuaManager : MonoBehaviour{
    private static LuaManager m_instance;
    public static LuaManager Instance { get {
            return m_instance;
        } }

    protected void Awake()
    {
        m_instance = this;
    }

    private LuaState m_cLuaState;

    public void Init()
    {
        var resLoader = new CustomLuaResLoader();
        string rootDir = ResourceSys.Instance.ResRootDir;
        if (!rootDir.EndsWith("/")) rootDir += "/";
        resLoader.beZip = !ResourceSys.Instance.DirectLoadMode;
        resLoader.AddAssetBundleSearchPath(rootDir + "LuaBytes/Custom");
        resLoader.AddAssetBundleSearchPath(rootDir + "LuaBytes/ToLua");
        m_cLuaState = new LuaState();
        OpenLibs();
        m_cLuaState.LuaSetTop(0);
        Bind();
    }

    public void StartLua()
    {
        m_cLuaState.Start();
        m_cLuaState.DoFile("FirstRequire.lua");
        if (m_cLuaState != null)
        {
            m_cLuaState.Call("Main", false);
        }
    }

    protected virtual void OpenLibs()
    {
        m_cLuaState.OpenLibs(LuaDLL.luaopen_pb);
        m_cLuaState.OpenLibs(LuaDLL.luaopen_struct);
        m_cLuaState.OpenLibs(LuaDLL.luaopen_lpeg);
#if UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
        luaState.OpenLibs(LuaDLL.luaopen_bit);
#endif
    }


    protected virtual void Bind()
    {
        LuaBinder.Bind(m_cLuaState);
        DelegateFactory.Init();
        LuaCoroutine.Register(m_cLuaState, this);
    }

    public virtual void Destroy()
    {
        if (m_cLuaState != null)
        {
            m_cLuaState.Call("OnApplicationQuit", false);
            LuaState state = m_cLuaState;
            m_cLuaState = null;
            state.Dispose();
            m_instance = null;
        }
    }

    protected void OnDestroy()
    {
        Destroy();
    }

    protected void OnApplicationQuit()
    {
        Destroy();
    }
}
