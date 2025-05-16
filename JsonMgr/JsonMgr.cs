using LitJson;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

/// <summary>
/// Json���ݴ�ȡ��ʽ
/// </summary>
public enum Save_Load_Type
{
    JsonUtility,
    LitJson
}
/// <summary>
/// Json���ݹ���������Ҫ����Json���л��洢��Ӳ�̺ͷ����л���Ӳ���ж�ȡ
/// </summary>
public class JsonMgr
{
    private static JsonMgr instace=new JsonMgr();
    public static JsonMgr Instance => instace;

    private JsonMgr(){}

    /// <summary>
    ///���л�
    /// </summary>
    /// <param name="obj">������Ҫ����������</param>
    /// <param name="fileName">���뱣����ļ���·��</param>
    /// <param name="type">ѡ�񱣴淽ʽ��Ĭ��ѡ��LitJson</param>
    public void SaveData(object obj, string fileName, Save_Load_Type type=Save_Load_Type.LitJson)
    {
        //���ݴ�����ļ������ɴ洢·��
        string path=Application.persistentDataPath + "/" + fileName+".json";
        //����һ����jsonStr����Switch��ֵ�󱣴�
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
        //�����л���Json�ַ��� �洢��ָ��·����
        File.WriteAllText(path, jsonStr);
    }

    /// <summary>
    ///�����л�
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="fileName">������Ҫ��ȡ���ļ���·��</param>
    /// <param name="type">ѡ���ȡ��ʽ��Ĭ��ѡ��LitJson</param>
    /// <returns>���ͷ��ظ�T���Ͷ���</returns>
    public T LoadData<T>(string fileName, Save_Load_Type type = Save_Load_Type.LitJson)where T:new()
    {
        //�����ж� Ĭ�������ļ����Ƿ�����Ҫ�����ݣ����������л�ȡ
        string path = Application.streamingAssetsPath + "/" + fileName + ".json";
        //���������Ĭ���ļ��ʹӶ�д�ļ���ȥѰ��
        if (!File.Exists(path))
        {
            path= Application.persistentDataPath + "/" + fileName + ".json";
        }
        //�����д�ļ��ж���û�� �Ǿͷ���һ��Ĭ�϶���
        if (!File.Exists(path))
        {
            return new T();
        }

        //��jsonStr���ն�ȡ����Json�ַ���
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
