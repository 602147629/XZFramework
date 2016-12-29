/*
 * FileName:    LuaManager
 * Author:      熊哲
 * CreateTime:  11/22/2016 6:13:35 PM
 * Description:
 * 
*/
using UnityEngine;
using System.Collections;
using LuaInterface;
using System.IO;
using System;

namespace XZFramework
{
    public class LuaManager : ManagerTemplate<LuaManager>
    {
        private LuaState luaState;

        void InitLuaLoader()
        {

        }
        void InitLuaPath()
        {
            if (Facade.Instance.settings.DebugMode)
            {
                luaState.AddSearchPath(LuaConst.luaDir);
                luaState.AddSearchPath(LuaConst.toluaDir);
            }
            else
            {
                luaState.AddSearchPath(Utility.persistentDirPath + "Lua");
            }
        }
        void InitLuaLibs()
        {
            luaState.LuaGetField(LuaIndexes.LUA_REGISTRYINDEX, "_LOADED");
            luaState.OpenLibs(LuaDLL.luaopen_cjson);
            luaState.LuaSetField(-2, "cjson");

            luaState.OpenLibs(LuaDLL.luaopen_cjson_safe);
            luaState.LuaSetField(-2, "cjson.safe");
            luaState.LuaSetTop(0);
        }
        void InitLuaLooper()
        {
            gameObject.AddComponent<LuaLooper>().luaState = luaState;
        }
        void InitLuaBind()
        {
            LuaBinder.Bind(luaState);
            LuaCoroutine.Register(luaState, this);
        }

        /// <summary>
        /// 运行lua的主逻辑
        /// </summary>
        public override void Initialize()
        {
            luaState = new LuaState();
            InitLuaLoader();
            InitLuaPath();
            InitLuaLibs();
            InitLuaLooper();
            InitLuaBind();
            luaState.Start();
            luaState.DoFile("Main.lua");
            LuaFunction main = luaState.GetFunction("Main");
            main.Call();
            main.Dispose();
            main = null;
        }

        /// <summary>
        /// 执行某个lua文件
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public object[] DoFile(string filePath)
        {
            return luaState.DoFile(filePath);
        }
        /// <summary>
        /// 执行某个lua方法
        /// </summary>
        /// <param name="funcName"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public object[] CallFunction(string funcName, params object[] args)
        {
            LuaFunction func = luaState.GetFunction(funcName);
            if (func != null)
            {
                return func.Call(args);
            }
            return null;
        }

        public void Exit()
        {
            if (luaState != null)
            {
                CallFunction("Main.Exit", null);
            }
        }

        /// <summary>
        /// 退出时先销毁lua栈
        /// </summary>
         void OnApplicationQuit()
        {
            if (luaState != null)
            {
                luaState.Dispose();
                luaState = null;
            }
        }
    }
}