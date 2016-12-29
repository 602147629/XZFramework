/*
 * FileName:    DatabaseManager
 * Author:      熊哲
 * CreateTime:  11/21/2016 5:43:02 PM
 * Description:
 * Description: 缓存数据库
 * 
 * 该轻量级数据库使用一次缓存，保证数据不丢失。
 * 其结构包含三部分，分别为：索引文件、缓存文件、数据文件。
 * 索引文件为该数据库实体，记录该数据库中有多少缓存文件，缓存文件中共有多少条命令未执行，两个备用参数，以及当前的数据库中所有的数据文件。
 * 缓存文件记录内存修改后，需要对外存进行的对应修改操作。
 * 数据文件记录程序保存的实际数据。
 * 
 * 其原理为：
 * 在内存修改后，暂时不修改外存，而是将所要做的修改操作以特定格式的命令化字符串存入缓存，并在索引文件头记录已缓存的命令条数。
 * 当缓存中命令达到一定数量时，对缓存的操作进行混合，消除重复操作，再进行外存文件的修改。
 * 当开始运行程序时，会通过索引文件判断是否有缓存命令尚未执行，待所有缓存命令执行完后，再进行外存文件的读取。
 * 该方法可以保证程序异常退出时缓存与内存同步，防止数据的丢失。并且能很大程度上减少外存的读写。
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;

namespace XZFramework
{
    public class DatabaseManager : ManagerTemplate<DatabaseManager>
    {
        /// <summary>
        /// 设置每个缓存文件的大小（缓存的最大命令数）
        /// </summary>
        public int cacheSize = 500;

        // 在此定义字符串读写时的分隔符
        protected const char DATA_DELIMITER = '|';
        protected const char OPERATION_DELIMITER = '\n';

        // 在此定义文件所用的扩展名
        protected const string DB_FILE_EXTENSION = ".index";
        protected const string CACHE_FILE_EXTENSION = ".cache";
        protected const string DATA_FILE_EXTENSION = ".json";

        // 数据库的运行路径
        protected string MainDirPath;
        protected string DBFilePath;
        protected string CacheDirPath;

        // 数据库的实体（即索引）
        protected List<string> DB = new List<string>();
        protected int cacheFileCount = 0;
        protected int operationCount = 0;
        protected int parameter2 = 0;
        protected int parameter3 = 0;
        /// <summary>
        /// 数据库的索引
        /// </summary>
        protected enum DBIndex
        {
            CacheFileCount = 0,         //当前缓存文件数
            OperationCount = 1,         //当前缓存命令数
            Parameter2 = 2,             //预留参数
            Parameter3 = 3,             //预留参数
            FirstFile = 4,              //数据文件开始地址
        }
        /// <summary>
        /// 命令的索引，不建议修改顺序；
        /// </summary>
        protected enum OperationIndex
        {
            FileName = 0,
            OperationType = 1,
            Key = 2,
            Value = 3,
            ValueType = 4,
        }
        /// <summary>
        /// 命令的操作类型
        /// </summary>
        protected enum OperationType
        {
            Add = 0,
            Set = 1,
            Del = 2,
        }
        /// <summary>
        /// 数据的值类型
        /// </summary>
        protected enum ValueType
        {
            Undefind = -1,
            Null = 0,
            String = 1,
            Double = 2,
            Boolean = 3,
        }

        // 数据文件的集合
        protected Dictionary<string, Hashtable> DBDict = new Dictionary<string, Hashtable>();

        /// <summary>
        /// 初始化路径，读取索引文件，检查执行残余命令，加载所有数据文件到内存
        /// </summary>
        public override void Initialize()
        {
            MainDirPath = Facade.Instance.settings.DebugMode
                        ? (Utility.dataDirPath + "Database/")
                        : (Utility.persistentDirPath + "Database/");
            DBFilePath = MainDirPath + "_DB" + DB_FILE_EXTENSION;
            CacheDirPath = MainDirPath + "Cache/";
            if (!Directory.Exists(CacheDirPath)) { Directory.CreateDirectory(CacheDirPath); }

            // 读取索引
            ReadIndexFile();

            // 执行剩余操作（先生成空的cache0文件，然后再读取其他cache）
            if (operationCount > 0)
            {
                Debug.Log("Refreshing Cache");
                cacheFileCount++;
                string cacheFile = CacheDirPath + 0 + CACHE_FILE_EXTENSION;
                File.Move(cacheFile, CacheDirPath + cacheFileCount + CACHE_FILE_EXTENSION);
                operationCount = 0;
                CacheToOperation();
            }

            // 加载所有表到内存
            Debug.Log("Loading Data...");
            for (int i = (int)DBIndex.FirstFile; i < DB.Count; i++)
            {
                try
                {
                    string path = MainDirPath + DB[i] + DATA_FILE_EXTENSION;
                    Debug.Log("Loading:\t" + path);
                    Hashtable table = (Hashtable)JSON.JsonDecode(Encoding.UTF8.GetString(File.ReadAllBytes(path)));
                    DBDict.Add(DB[i], table);
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                }
            }
            Debug.Log("Load Data Complete...");
        }

        /// <summary>
        /// 添加字符串字段
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="key">字段</param>
        /// <param name="value">值</param>
        /// <returns>成功返回true，已有该值返回false</returns>
        public bool Add(string tableName, string key, string value)
        {
            Hashtable table;
            if (DBDict.ContainsKey(tableName))
            {
                table = DBDict[tableName];
                if (table.ContainsKey(key)) return false;
            }
            else
            {
                table = new Hashtable();
                DBDict.Add(tableName, table);
                DB.Add(tableName);
                SaveIndexFile();
            }
            table.Add(key, value);

            string[] args = new string[Enum.GetValues(typeof(OperationIndex)).Length];
            args[(int)OperationIndex.FileName] = tableName;
            args[(int)OperationIndex.OperationType] = OperationType.Add.ToString();
            args[(int)OperationIndex.Key] = key;
            args[(int)OperationIndex.Value] = value.ToString();
            args[(int)OperationIndex.ValueType] = ValueType.String.ToString();
            string opr = string.Join(DATA_DELIMITER.ToString(), args);
            OperationToCache(opr);
            return true;
        }
        /// <summary>
        /// 添加数字字段
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="key">字段</param>
        /// <param name="value">值</param>
        /// <returns>成功返回true，已有该值返回false</returns>
        public bool Add(string tableName, string key, double value)
        {
            Hashtable table;
            if (DBDict.ContainsKey(tableName))
            {
                table = DBDict[tableName];
                if (table.ContainsKey(key)) return false;
            }
            else
            {
                table = new Hashtable();
                DBDict.Add(tableName, table);
                DB.Add(tableName);
                SaveIndexFile();
            }
            table.Add(key, value);

            string[] args = new string[Enum.GetValues(typeof(OperationIndex)).Length];
            args[(int)OperationIndex.FileName] = tableName;
            args[(int)OperationIndex.OperationType] = OperationType.Add.ToString();
            args[(int)OperationIndex.Key] = key;
            args[(int)OperationIndex.Value] = value.ToString();
            args[(int)OperationIndex.ValueType] = ValueType.Double.ToString();
            string opr = string.Join(DATA_DELIMITER.ToString(), args);
            OperationToCache(opr);
            return true;
        }
        /// <summary>
        /// 添加布尔字段
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="key">字段</param>
        /// <param name="value">值</param>
        /// <returns>成功返回true，已有该值返回false</returns>
        public bool Add(string tableName, string key, bool value)
        {
            Hashtable table;
            if (DBDict.ContainsKey(tableName))
            {
                table = DBDict[tableName];
                if (table.ContainsKey(key)) return false;
            }
            else
            {
                table = new Hashtable();
                DBDict.Add(tableName, table);
                DB.Add(tableName);
                SaveIndexFile();
            }
            table.Add(key, value);

            string[] args = new string[Enum.GetValues(typeof(OperationIndex)).Length];
            args[(int)OperationIndex.FileName] = tableName;
            args[(int)OperationIndex.OperationType] = OperationType.Add.ToString();
            args[(int)OperationIndex.Key] = key;
            args[(int)OperationIndex.Value] = value.ToString();
            args[(int)OperationIndex.ValueType] = ValueType.Boolean.ToString();
            string opr = string.Join(DATA_DELIMITER.ToString(), args);
            OperationToCache(opr);
            return true;
        }

        /// <summary>
        /// 修改字段
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="key">字段</param>
        /// <param name="value">值</param>
        /// <returns>成功返回true</returns>
        public bool Set(string tableName, string key, string value)
        {
            Hashtable table;
            if (DBDict.ContainsKey(tableName))
            {
                table = DBDict[tableName];
            }
            else
            {
                table = new Hashtable();
                DBDict.Add(tableName, table);
                DB.Add(tableName);
                SaveIndexFile();
            }
            table[key] = value;

            string[] args = new string[Enum.GetValues(typeof(OperationIndex)).Length];
            args[(int)OperationIndex.FileName] = tableName;
            args[(int)OperationIndex.OperationType] = OperationType.Set.ToString();
            args[(int)OperationIndex.Key] = key;
            args[(int)OperationIndex.Value] = value.ToString();
            args[(int)OperationIndex.ValueType] = ValueType.String.ToString();
            string opr = string.Join(DATA_DELIMITER.ToString(), args);
            OperationToCache(opr);
            return true;
        }
        /// <summary>
        /// 修改字段
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="key">字段</param>
        /// <param name="value">值</param>
        /// <returns>成功返回true</returns>
        public bool Set(string tableName, string key, double value)
        {
            Hashtable table;
            if (DBDict.ContainsKey(tableName))
            {
                table = DBDict[tableName];
            }
            else
            {
                table = new Hashtable();
                DBDict.Add(tableName, table);
                DB.Add(tableName);
                SaveIndexFile();
            }
            table[key] = value;

            string[] args = new string[Enum.GetValues(typeof(OperationIndex)).Length];
            args[(int)OperationIndex.FileName] = tableName;
            args[(int)OperationIndex.OperationType] = OperationType.Set.ToString();
            args[(int)OperationIndex.Key] = key;
            args[(int)OperationIndex.Value] = value.ToString();
            args[(int)OperationIndex.ValueType] = ValueType.Double.ToString();
            string opr = string.Join(DATA_DELIMITER.ToString(), args);
            OperationToCache(opr);
            return true;
        }
        /// <summary>
        /// 修改字段
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="key">字段</param>
        /// <param name="value">值</param>
        /// <returns>成功返回true</returns>
        public bool Set(string tableName, string key, bool value)
        {
            Hashtable table;
            if (DBDict.ContainsKey(tableName))
            {
                table = DBDict[tableName];
            }
            else
            {
                table = new Hashtable();
                DBDict.Add(tableName, table);
                DB.Add(tableName);
                SaveIndexFile();
            }
            table[key] = value;

            string[] args = new string[Enum.GetValues(typeof(OperationIndex)).Length];
            args[(int)OperationIndex.FileName] = tableName;
            args[(int)OperationIndex.OperationType] = OperationType.Set.ToString();
            args[(int)OperationIndex.Key] = key;
            args[(int)OperationIndex.Value] = value.ToString();
            args[(int)OperationIndex.ValueType] = ValueType.Boolean.ToString();
            string opr = string.Join(DATA_DELIMITER.ToString(), args);
            OperationToCache(opr);
            return true;
        }

        /// <summary>
        /// 获取字符串
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="key">字段</param>
        /// <returns></returns>
        public string GetString(string tableName, string key)
        {
            if (!DBDict.ContainsKey(tableName) || !DBDict[tableName].ContainsKey(key)) return null;
            return (string)DBDict[tableName][key];
        }
        /// <summary>
        /// 获取数字
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="key">字段</param>
        /// <returns></returns>
        public double GetNumber(string tableName, string key)
        {
            if (!DBDict.ContainsKey(tableName) || !DBDict[tableName].ContainsKey(key)) return 0;
            return (double)DBDict[tableName][key];
        }
        /// <summary>
        /// 获取布尔值
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="key">字段</param>
        /// <returns></returns>
        public bool GetBool(string tableName, string key)
        {
            if (!DBDict.ContainsKey(tableName) || !DBDict[tableName].ContainsKey(key)) return false;
            return (bool)DBDict[tableName][key];
        }

        /// <summary>
        /// 删除字段
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="key">要删除的字段</param>
        /// <returns></returns>
        public bool Del(string tableName, string key)
        {
            if (DBDict.ContainsKey(tableName))
            {
                DBDict[tableName].Remove(key);
            }
            else
            {
                return false;
            }

            string[] args = new string[Enum.GetValues(typeof(OperationIndex)).Length];
            args[(int)OperationIndex.FileName] = tableName;
            args[(int)OperationIndex.OperationType] = OperationType.Add.ToString();
            args[(int)OperationIndex.Key] = key;
            args[(int)OperationIndex.Value] = "";
            args[(int)OperationIndex.ValueType] = ValueType.Undefind.ToString();
            string opr = string.Join(DATA_DELIMITER.ToString(), args);
            OperationToCache(opr);
            return true;
        }

        /// <summary>
        /// 检查是否已存在某个存档
        /// </summary>
        /// <param name="tableName"></param>
        /// <returns></returns>
        public bool CheckExist(string tableName)
        {
            if (DBDict.ContainsKey(tableName)) return true;
            return false;
        }

        /// <summary>
        /// 把命令存入缓存
        /// </summary>
        /// <param name="opr"></param>
        protected void OperationToCache(string opr)
        {
            // 文件读写的流
            FileStream fileStream;
            // 0为当前读写的缓存，如果存在，以添加方式打开并在命令前加上分隔符，不存在则以新建方式打开
            string cacheFile = CacheDirPath + 0 + CACHE_FILE_EXTENSION;
            if (File.Exists(cacheFile))
            {
                fileStream = File.Open(cacheFile, FileMode.Append);
                opr = OPERATION_DELIMITER + opr;
            }
            else
            {
                fileStream = File.Open(cacheFile, FileMode.OpenOrCreate);
            }
            // 将命令写入缓存，命令数+1
            byte[] bytes = Encoding.UTF8.GetBytes(opr);
            fileStream.Write(bytes, 0, bytes.Length);
            fileStream.Close();
            operationCount++;

            // 命令缓存已满，增加缓存文件，原缓存文件更名，执行命令
            if (operationCount >= cacheSize)
            {
                cacheFileCount++;
                operationCount = 0;
                File.Move(cacheFile, CacheDirPath + cacheFileCount + CACHE_FILE_EXTENSION);
                CacheToFile();
            }

            //更新索引
            SaveIndexFile();
        }
        /// <summary>
        /// 缓存文件已满时，读取缓存，得到发生修改的文件列表，以当前内存数据更新这些文件
        /// </summary>
        protected void CacheToFile()
        {
            int fileCount = cacheFileCount;
            HashSet<string> fileList = new HashSet<string>();
            // 遍历所有缓存文件
            for (int i = 1; i <= fileCount; i++)
            {
                string path = CacheDirPath + i + CACHE_FILE_EXTENSION;
                try
                {
                    string str = Encoding.UTF8.GetString(File.ReadAllBytes(path));
                    string[] oprs = str.Split(OPERATION_DELIMITER);
                    for (int j = 0; j < oprs.Length; j++)
                    {
                        // 取操作的文件名加入fileList（保证多次修改的文件只需要进行一次储存）
                        string[] param = oprs[j].Split(DATA_DELIMITER);
                        fileList.Add(param[(int)OperationIndex.FileName]);
                    }
                    // 遍历fileList，把内存写到外存
                    foreach (var fileName in fileList)
                    {
                        string filePath = MainDirPath + fileName + DATA_FILE_EXTENSION;
                        File.WriteAllBytes(filePath, Encoding.UTF8.GetBytes(JSON.JsonEncode(DBDict[fileName])));
                    }
                    //删除缓存文件，文件-1，更新索引
                    File.Delete(path);
                    cacheFileCount--;
                    SaveIndexFile();
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                }
            }
        }

        /// <summary>
        /// 加载程序时，把缓存中的命令进行解析，修改外存后再读入
        /// </summary>
        protected void CacheToOperation()
        {
            int fileCount = cacheFileCount;
            // 遍历所有缓存文件
            for (int i = 1; i <= fileCount; i++)
            {
                Dictionary<string, Dictionary<string, string>> fileList = new Dictionary<string, Dictionary<string, string>>();
                //获取缓存文件路径
                string path = CacheDirPath + i + CACHE_FILE_EXTENSION;
                try
                {
                    string str = Encoding.UTF8.GetString(File.ReadAllBytes(path));
                    string[] oprs = str.Split(OPERATION_DELIMITER);
                    for (int j = 0; j < oprs.Length; j++)
                    {
                        // 命令按文件进行分组：将所有需要操作的文件与需要进行的操作存放为词典；
                        // 命令按类型进行混合：对同一个文件中同一个字段进行相同操作时，取消时间靠前的操作（Add命令除外）
                        string[] param = oprs[j].Split(DATA_DELIMITER);
                        string oprFile = param[(int)OperationIndex.FileName];
                        string oprKey = param[(int)OperationIndex.OperationType] + DATA_DELIMITER + param[(int)OperationIndex.Key];
                        string oprValue = param[(int)OperationIndex.Value] + DATA_DELIMITER + param[(int)OperationIndex.ValueType];
                        if (!fileList.ContainsKey(oprFile))
                        {
                            fileList.Add(oprFile, new Dictionary<string, string>());
                            fileList[oprFile].Add(oprKey, oprValue);
                        }
                        else
                        {
                            if (fileList[oprFile].ContainsKey(oprKey) && param[(int)OperationIndex.OperationType] != OperationType.Add.ToString())
                            {
                                fileList[oprFile].Remove(oprKey);
                            }
                            fileList[oprFile].Add(oprKey, oprValue);
                        }
                        // 遍历发生修改的文件列表，执行修改操作
                        foreach (var fileOpr in fileList)
                        {
                            Operate(fileOpr);
                        }
                    }
                    //删除文件，文件-1，更新索引
                    File.Delete(path);
                    cacheFileCount--;
                    SaveIndexFile();
                }
                catch (Exception ex)
                {
                    Debug.Log(ex.Message);
                }
            }
        }
        /// <summary>
        /// 执行某个文件的修改命令，参数以（文件名, 命令列表）的键值对传入
        /// </summary>
        /// <param name="args"></param>
        protected void Operate(KeyValuePair<string, Dictionary<string, string>> fileOprs)
        {
            // 获取要操作的文件，如果文件不存在则新建
            string filePath = MainDirPath + fileOprs.Key + DATA_FILE_EXTENSION;
            Hashtable table = null;
            try
            {
                table = (Hashtable)JSON.JsonDecode(Encoding.UTF8.GetString(File.ReadAllBytes(filePath)));
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
                table = new Hashtable();
            }
            // 遍历命令列表，对读取的文件数据进行修改
            foreach (var fileOpr in fileOprs.Value)
            {
                string[] oprKey = fileOpr.Key.Split(DATA_DELIMITER);
                string oprType = oprKey[0]; string key = oprKey[1];
                string[] oprValue = fileOpr.Value.Split(DATA_DELIMITER);
                string value = oprValue[0]; string valueType = oprValue[1];
                // 执行命令
                if (oprType == OperationType.Add.ToString() || oprType == OperationType.Set.ToString())
                {
                    if (valueType == ValueType.Null.ToString())
                    {
                        table[key] = null;
                    }
                    else if (valueType == ValueType.String.ToString())
                    {
                        table[key] = value;
                    }
                    else if (valueType == ValueType.Double.ToString())
                    {
                        table[key] = Convert.ToDouble(value);
                    }
                    else if (valueType == ValueType.Boolean.ToString())
                    {
                        table[key] = Convert.ToBoolean(value);
                    }
                    else if (valueType == ValueType.Undefind.ToString())
                    {
                        table[key] = value;
                    }
                }
                else if (oprType == OperationType.Del.ToString())
                {
                    table.Remove(key);
                }
            }
            // 文件写回
            try
            {
                File.WriteAllBytes(filePath, Encoding.UTF8.GetBytes(JSON.JsonEncode(table)));
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }

        /// <summary>
        /// 索引文件的读取
        /// </summary>
        protected void ReadIndexFile()
        {
            string[] args = null;
            try
            {
                string str = Encoding.UTF8.GetString(File.ReadAllBytes(DBFilePath));
                args = str.Split(DATA_DELIMITER);
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }

            if (args != null)
            {
                DB = args.ToList();
                cacheFileCount = Convert.ToInt32(DB[(int)DBIndex.CacheFileCount]);
                operationCount = Convert.ToInt32(DB[(int)DBIndex.OperationCount]);
                parameter2 = Convert.ToInt32(DB[(int)DBIndex.Parameter2]);
                parameter3 = Convert.ToInt32(DB[(int)DBIndex.Parameter3]);
            }
            else
            {
                DB.Add(cacheFileCount.ToString());
                DB.Add(operationCount.ToString());
                DB.Add(parameter2.ToString());
                DB.Add(parameter3.ToString());
            }
        }
        /// <summary>
        /// 索引文件的储存
        /// </summary>
        protected void SaveIndexFile()
        {
            DB[(int)DBIndex.CacheFileCount] = cacheFileCount.ToString();
            DB[(int)DBIndex.OperationCount] = operationCount.ToString();
            DB[(int)DBIndex.Parameter2] = parameter2.ToString();
            DB[(int)DBIndex.Parameter3] = parameter3.ToString();
            string[] args = DB.ToArray();
            string str = string.Join(DATA_DELIMITER.ToString(), args);
            try
            {
                File.WriteAllBytes(DBFilePath, Encoding.UTF8.GetBytes(str));
            }
            catch (Exception ex)
            {
                Debug.Log(ex.Message);
            }
        }
    }
}