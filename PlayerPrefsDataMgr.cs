using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Xml.Serialization;
using Unity.VisualScripting.FullSerializer;
using UnityEngine;
using static UnityEditor.LightingExplorerTableColumn;

public class PlayerPrefsDataMgr
{
    //����ģʽ
    private static PlayerPrefsDataMgr instance=new PlayerPrefsDataMgr();
    //˽�л����캯������ֹ��new
    private PlayerPrefsDataMgr() 
    { 

    }
    public static PlayerPrefsDataMgr Instance
    {
        get
        {
            return instance;
        }
    }

    /// <summary>
    /// �洢����
    /// </summary>
    /// <param name="data">���ݶ���</param>
    /// <param name="keyName">���ݶ����Ψһkey �Լ�����</param>
    public void SaveData(object data,string keyName)
    {
        Type dataType = data.GetType();
        FieldInfo[] fields = dataType.GetFields();

        //���ݶ����Ψһkey
        string saveKeyName = "";
        //��¼�洢��ÿһ����Ա����
        FieldInfo fieldInfo;
        for (int i = 0; i < fields.Length; i++)
        {
            fieldInfo = fields[i];
            saveKeyName = keyName + "_"+dataType.Name + "_" + fieldInfo.FieldType.Name + "_" + fieldInfo.Name;

            //��װ��һ��˽�з�������ר�Ŵ洢ֵ
            SaveValue(fieldInfo.GetValue(data), saveKeyName);
        }
        PlayerPrefs.Save();
    }
    /// <summary>
    /// �洢�������ݵķ���
    /// </summary>
    /// <param name="value"></param>
    /// <param name="saveKeyName"></param>
    private void SaveValue(object value,string saveKeyName )
    {
        Type fieldType = value.GetType();

        #region ��ͨ����������洢
        if (fieldType == typeof(string))
        {
            PlayerPrefs.SetString(saveKeyName, (string)value);
        }
        else if (fieldType == typeof(int))
        {
            PlayerPrefs.SetInt(saveKeyName, (int)value);
        }
        else if (fieldType == typeof(float))
        {
            PlayerPrefs.SetFloat(saveKeyName, (float)value);
        }
        else if (fieldType == typeof(bool))
        {
            //�Լ�����Ĵ洢bool�Ĺ���
            int num = (bool)value ? 1 : 0;
            PlayerPrefs.SetInt(saveKeyName, num);
        }
        #endregion

        #region List�������ʹ洢
        //���൱���жϸ��ֶ��Ƿ���IList���ࡪ�����Ƿ���List<T>����
        else if (typeof(IList).IsAssignableFrom(fieldType))
        {
            //Debug.Log("�洢List" + saveKeyName);
            //����װ����
            IList list=value as IList;

            //�ȴ洢������֮���ȡ��֪����ȡ����
            PlayerPrefs.SetInt(saveKeyName , list.Count);
            //��֤����keyΨһ��
            int index = 0;
            foreach (object obj in list)
            {
                //�洢�����ֵ,���õݹ��ֻص��������жϷ���
                SaveValue(obj,saveKeyName+index);
                index++;
            }
        }
        #endregion

        #region Dictionary�������ʹ洢
        //���൱���жϸ��ֶ��Ƿ���IDictionary���ࡪ�����Ƿ���Dictionary����
        else if (typeof(IDictionary).IsAssignableFrom(fieldType))
        {
            //Debug.Log("�洢Dictionary" + saveKeyName);

            //����װ����
            IDictionary dic = value as IDictionary;
            //�ȴ洢�ֵ�������֮���ȡ��֪����ȡ����
            PlayerPrefs.SetInt (saveKeyName , dic.Count);
            int index = 0;
            foreach(object key in dic.Keys)
            {
                SaveValue(key,saveKeyName+"_key_"+index);
                SaveValue(dic[key], saveKeyName + "_value_" + index);
                index++;
            }
        }
        #endregion

        #region �Զ�����洢
        //�������Ͷ����ǣ���ô�Ϳ������Զ������ͣ��Ǿ����µ���SaveData
        else
        {
            SaveData(value,saveKeyName);
        }
        #endregion
    }


    /// <summary>
    /// ��ȡ����
    /// </summary>
    /// <param name="type">��Ҫ��ȡ���� ����������Type</param>
    /// <param name="keyName">���ݶ����Ψһkey �Լ�����</param>
    /// <returns></returns>
    public object LoadData(Type type,string keyName)
    {
        //�����object������Type��������ⲿnewһ�����󴫽�����������ʹ���������
        //����Type������ֱ�Ӵ���typeof(����) ��Ȼ�����ֱ�����ڲ���̬����һ������

        //1.���ݴ����Type����һ���������ڴ洢���ݣ����޲ι��죬���п���ֱ�Ӵ�����
        object data=Activator.CreateInstance(type);

        //2.Ҫ��new�������������������Ѵ洢�õ����ݺ󷵻س�ȥ
          //�õ������ֶ���Ϣ
        FieldInfo[] fields = type.GetFields();

        //�������Ψһkey����ȡֵ,����ƴ��key���ַ���
        string loadKeyName = "";
        //��ȡ��ÿһ��������Ա����
        FieldInfo fieldInfo;
        for (int i = 0; i < fields.Length; i++)
        {
            fieldInfo = fields[i];
            //key��ƴ�ӹ���һ���Ǻʹ��һģһ�������������ҵ���Ӧ����
            loadKeyName=keyName+ "_" + type.Name+"_"+fieldInfo.FieldType.Name + "_" + fieldInfo.Name;

            //��װ��һ��˽�з�������ר�Ŷ�ȡֵ
            //ͨ�������Ϊ��Ա��������ֵ��SetValue������Ϊdata��ÿһ����Ա������ֵ�󷵻س�ȥ
            fieldInfo.SetValue(data,LoadValue(fieldInfo.FieldType, loadKeyName));
        }
        return data;
    }
    /// <summary>
    /// ��ȡ�������ݵķ���
    /// </summary>
    /// <param name="fieldInfo">�ֶ����ͣ������ж�ʹ���ĸ�api����ȡ</param>
    /// <param name="saveKeyName"></param>
    /// <returns></returns>
    private object LoadValue(Type fieldType,string loadKeyName)
    {
        //�����ֶ�����,�ж�ʹ���ĸ�api����ȡ
        #region ��ͨ�����������ȡ
        if (fieldType == typeof(int))
        {
            return PlayerPrefs.GetInt(loadKeyName, 0);
        }
        else if (fieldType == typeof(float))
        {
            return PlayerPrefs.GetFloat(loadKeyName, 0);
        }
        else if (fieldType == typeof(string))
        {
            return PlayerPrefs.GetString(loadKeyName);
        }
        else if(fieldType == typeof(bool))
        {
            return PlayerPrefs.GetInt(loadKeyName) ==1;
        }
        #endregion

        #region List�������Ͷ�ȡ
        else if (typeof(IList).IsAssignableFrom(fieldType))
        {
            //�õ�List����
            int count = PlayerPrefs.GetInt(loadKeyName, 0);

            //ʵ����һ��List���������и�ֵ���ҷ���
            IList list = Activator.CreateInstance(fieldType) as IList;

            for (int i = 0;i < count; i++)
            {
                //�õ�List�еķ������ͺ���ӵ��½������У�ע�⣺�����½���List������������������ݣ�
                list.Add(LoadValue(fieldType.GetGenericArguments()[0], loadKeyName + i));
            }
            return list;
        }
        #endregion

        #region Dictionary�������Ͷ�ȡ
        //���൱���жϸ��ֶ��Ƿ���IDictionary���ࡪ�����Ƿ���Dictionary����
        else if (typeof(IDictionary).IsAssignableFrom(fieldType))
        {
            //�õ�Dictionary����
            int count = PlayerPrefs.GetInt(loadKeyName,0);

            //ʵ����һ��Dictionary���������и�ֵ���ҷ���
            IDictionary dic = Activator.CreateInstance(fieldType) as IDictionary;

            for (int i = 0; i < count; i++)
            {
                dic.Add(LoadValue(fieldType.GetGenericArguments()[0], loadKeyName + "_key_" + i), 
                    LoadValue(fieldType.GetGenericArguments()[1], loadKeyName + "_value_" + i));
            }
            return dic;
        }
        #endregion

        #region �Զ������ȡ
        else
        {
            return LoadData(fieldType, loadKeyName);
        }
        #endregion
    }
}
