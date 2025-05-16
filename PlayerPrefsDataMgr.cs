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
    //单例模式
    private static PlayerPrefsDataMgr instance=new PlayerPrefsDataMgr();
    //私有化构造函数，防止被new
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
    /// 存储数据
    /// </summary>
    /// <param name="data">数据对象</param>
    /// <param name="keyName">数据对象的唯一key 自己定义</param>
    public void SaveData(object data,string keyName)
    {
        Type dataType = data.GetType();
        FieldInfo[] fields = dataType.GetFields();

        //数据对象的唯一key
        string saveKeyName = "";
        //记录存储下每一个成员变量
        FieldInfo fieldInfo;
        for (int i = 0; i < fields.Length; i++)
        {
            fieldInfo = fields[i];
            saveKeyName = keyName + "_"+dataType.Name + "_" + fieldInfo.FieldType.Name + "_" + fieldInfo.Name;

            //封装的一个私有方法用于专门存储值
            SaveValue(fieldInfo.GetValue(data), saveKeyName);
        }
        PlayerPrefs.Save();
    }
    /// <summary>
    /// 存储单个数据的方法
    /// </summary>
    /// <param name="value"></param>
    /// <param name="saveKeyName"></param>
    private void SaveValue(object value,string saveKeyName )
    {
        Type fieldType = value.GetType();

        #region 普通常用数据类存储
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
            //自己定义的存储bool的规则
            int num = (bool)value ? 1 : 0;
            PlayerPrefs.SetInt(saveKeyName, num);
        }
        #endregion

        #region List数据类型存储
        //这相当于判断该字段是否是IList子类———是否是List<T>类型
        else if (typeof(IList).IsAssignableFrom(fieldType))
        {
            //Debug.Log("存储List" + saveKeyName);
            //父类装子类
            IList list=value as IList;

            //先存储数量，之后读取才知道读取几个
            PlayerPrefs.SetInt(saveKeyName , list.Count);
            //保证数据key唯一性
            int index = 0;
            foreach (object obj in list)
            {
                //存储具体的值,利用递归又回到了上面判断方法
                SaveValue(obj,saveKeyName+index);
                index++;
            }
        }
        #endregion

        #region Dictionary数据类型存储
        //这相当于判断该字段是否是IDictionary子类———是否是Dictionary类型
        else if (typeof(IDictionary).IsAssignableFrom(fieldType))
        {
            //Debug.Log("存储Dictionary" + saveKeyName);

            //父类装子类
            IDictionary dic = value as IDictionary;
            //先存储字典数量，之后读取才知道读取几个
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

        #region 自定义类存储
        //基础类型都不是，那么就可能是自定义类型，那就重新调用SaveData
        else
        {
            SaveData(value,saveKeyName);
        }
        #endregion
    }


    /// <summary>
    /// 读取数据
    /// </summary>
    /// <param name="type">想要读取数据 的数据类型Type</param>
    /// <param name="keyName">数据对象的唯一key 自己定义</param>
    /// <returns></returns>
    public object LoadData(Type type,string keyName)
    {
        //如果用object而不用Type则必须在外部new一个对象传进来，才能再使用这个方法
        //有了Type，可以直接传入typeof(类型) ，然后可以直接在内部动态创建一个对象

        //1.根据传入的Type创建一个对象，用于存储数据（有无参构造，所有可以直接创建）
        object data=Activator.CreateInstance(type);

        //2.要往new出来的这个对象中填充已存储好的数据后返回出去
          //得到所有字段信息
        FieldInfo[] fields = type.GetFields();

        //根据这个唯一key来读取值,用于拼接key的字符串
        string loadKeyName = "";
        //获取到每一个公共成员变量
        FieldInfo fieldInfo;
        for (int i = 0; i < fields.Length; i++)
        {
            fieldInfo = fields[i];
            //key的拼接规则一定是和存的一模一样，这样才能找到对应数据
            loadKeyName=keyName+ "_" + type.Name+"_"+fieldInfo.FieldType.Name + "_" + fieldInfo.Name;

            //封装的一个私有方法用于专门读取值
            //通过反射的为成员变量设置值的SetValue方法，为data的每一个成员变量赋值后返回出去
            fieldInfo.SetValue(data,LoadValue(fieldInfo.FieldType, loadKeyName));
        }
        return data;
    }
    /// <summary>
    /// 读取单个数据的方法
    /// </summary>
    /// <param name="fieldInfo">字段类型，用于判断使用哪个api来读取</param>
    /// <param name="saveKeyName"></param>
    /// <returns></returns>
    private object LoadValue(Type fieldType,string loadKeyName)
    {
        //根据字段类型,判断使用哪个api来读取
        #region 普通常用数据类读取
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

        #region List数据类型读取
        else if (typeof(IList).IsAssignableFrom(fieldType))
        {
            //得到List长度
            int count = PlayerPrefs.GetInt(loadKeyName, 0);

            //实例化一个List对象来进行赋值并且返回
            IList list = Activator.CreateInstance(fieldType) as IList;

            for (int i = 0;i < count; i++)
            {
                //得到List中的泛型类型后添加到新建对象中（注意：这是新建的List，所以是依次添加数据）
                list.Add(LoadValue(fieldType.GetGenericArguments()[0], loadKeyName + i));
            }
            return list;
        }
        #endregion

        #region Dictionary数据类型读取
        //这相当于判断该字段是否是IDictionary子类———是否是Dictionary类型
        else if (typeof(IDictionary).IsAssignableFrom(fieldType))
        {
            //得到Dictionary长度
            int count = PlayerPrefs.GetInt(loadKeyName,0);

            //实例化一个Dictionary对象来进行赋值并且返回
            IDictionary dic = Activator.CreateInstance(fieldType) as IDictionary;

            for (int i = 0; i < count; i++)
            {
                dic.Add(LoadValue(fieldType.GetGenericArguments()[0], loadKeyName + "_key_" + i), 
                    LoadValue(fieldType.GetGenericArguments()[1], loadKeyName + "_value_" + i));
            }
            return dic;
        }
        #endregion

        #region 自定义类读取
        else
        {
            return LoadData(fieldType, loadKeyName);
        }
        #endregion
    }
}
