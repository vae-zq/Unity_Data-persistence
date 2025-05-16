using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Json数据存取方式
/// </summary>
public enum Save_Load_Type
{
    JsonUtility,
    LitJson
}
/// <summary>
/// Json数据管理器：主要用于Json序列化存储到硬盘和反序列化从硬盘中读取
/// </summary>
public class JsonMgr
{
    private static JsonMgr instace=new JsonMgr();
    public static JsonMgr Instance => instace;

    private JsonMgr(){}

    /// <summary>
    ///序列化
    /// </summary>
    /// <param name="obj">传入需要保存的类对象</param>
    /// <param name="fileName">传入保存的文件夹路径</param>
    /// <param name="type">选择保存方式，默认选择LitJson</param>
    public void SaveData(object obj, string fileName, Save_Load_Type type=Save_Load_Type.LitJson)
    {
        //根据传入的文件名生成存储路径
        string path=Application.persistentDataPath + "/" + fileName+".json";
        //定义一个空jsonStr用于Switch赋值后保存
        string jsonStr = null;
        switch (type)
        {
            case Save_Load_Type.JsonUtility:
                jsonStr = JsonUtility.ToJson(obj);
                break;
            case Save_Load_Type.LitJson:
                jsonStr = JsonMapper.ToJson(obj);
                break;
        }
        //把序列化的Json字符串 存储到指定路径中
        File.WriteAllText(path, jsonStr);
    }

    /// <summary>
    ///反序列化
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileName">传入需要读取的文件夹路径</param>
    /// <param name="type">选择读取方式，默认选择LitJson</param>
    /// <returns>泛型返回该T类型对象</returns>
    public T LoadData<T>(string fileName, Save_Load_Type type = Save_Load_Type.LitJson)where T:new()
    {
        //首先判断 默认数据文件夹是否有想要的数据，如果有则从中获取
        string path = Application.streamingAssetsPath + "/" + fileName + ".json";
        //如果不存在默认文件就从读写文件夹去寻找
        if (!File.Exists(path))
        {
            path= Application.persistentDataPath + "/" + fileName + ".json";
        }
        //如果读写文件夹都还没有 那就返回一个默认对象
        if (!File.Exists(path))
        {
            return new T();
        }

        //用jsonStr接收读取到的Json字符串
        string jsonStr = File.ReadAllText(path);
        switch (type)
        {
            case Save_Load_Type.JsonUtility:
                return JsonUtility.FromJson<T>(jsonStr);
            case Save_Load_Type.LitJson:
                return JsonMapper.ToObject<T>(jsonStr);
            default:
                return default(T);
        }
    }
}
