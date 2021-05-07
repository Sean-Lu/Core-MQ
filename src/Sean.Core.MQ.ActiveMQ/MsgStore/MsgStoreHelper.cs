using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;

namespace Sean.Core.MQ.ActiveMQ.MsgStore
{
    internal class MsgStoreHelper
    {
        private static readonly string _dataType;
        private static readonly string _fullPath;
        private static readonly string _resendFullPath;
        private static readonly object _lockObject = new object();

        static MsgStoreHelper()
        {
            _dataType = "dat";
            _fullPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ActiveMqData");
            _resendFullPath = Path.Combine(_fullPath, "Resend");
            if (!Directory.Exists(_fullPath))
                Directory.CreateDirectory(_fullPath);
            if (!Directory.Exists(_resendFullPath))
                Directory.CreateDirectory(_resendFullPath);
        }

        /// <summary>
        /// 持久化数据
        /// </summary>
        /// <param name="list"></param>
        public static void Persist(List<MsgStoreModel> list)
        {
            if (list == null || !list.Any())
                return;

            foreach (var item in list)
            {
                var timeStamp = DateTime.Now.ToString("yyyyMMddHHmm");
                var randomVal = Guid.NewGuid().ToString().Replace("-", "");
                var fileName = $"{item.Options.Name}@{timeStamp}+{randomVal}.{_dataType}";

                var filePath = Path.Combine(_fullPath, fileName);
                File.WriteAllText(filePath, JsonConvert.SerializeObject(item));
            }

            Producer.StartResendPesistMsgTimer();
        }

        /// <summary>
        /// 是否有持久化存储的数据
        /// </summary>
        /// <returns></returns>
        public static bool ExistPersistMsg(out string[] files, string prefix = null)
        {
            var pattern = $"*.{_dataType}";
            if (!string.IsNullOrEmpty(prefix))
                pattern = $"{prefix}*.{_dataType}";
            files = Directory.GetFiles(_fullPath, pattern);
            return files.Length > 0;
        }

        /// <summary>
        /// 提取数据并且销毁
        /// </summary>
        /// <param name="prefix">文件前缀</param>
        /// <returns></returns>
        public static List<MsgStoreModel> Extract(string prefix = null)
        {
            lock (_lockObject)
            {
                var list = new List<MsgStoreModel>();
                if (!ExistPersistMsg(out var files, prefix))
                {
                    return list;
                }
                foreach (var file in files)
                {
                    try
                    {
                        #region 读取文本
                        var json = File.ReadAllText(file, Encoding.UTF8);
                        if (string.IsNullOrEmpty(json.Trim()))
                        {
                            File.Delete(file);
                            continue;
                        }
                        #endregion

                        #region 装载到集合
                        var item = JsonConvert.DeserializeObject<MsgStoreModel>(json);

                        #region 转换MsgDataType
                        if (item.Msg is JObject jObject)
                            item.Msg = jObject.ToObject(item.MsgDataType);
                        else if (item.Msg is JArray jArray)
                            item.Msg = jArray.ToObject(item.MsgDataType);
                        else if (item.Msg is JToken jToken)
                            item.Msg = jToken.ToObject(item.MsgDataType);
                        #endregion

                        list.Add(item);
                        #endregion

                        if (item.Options.BackupResendPersistMsg)
                        {
                            // 移动到新目录
                            File.Move(file, Path.Combine(_resendFullPath, Path.GetFileName(file)));
                        }
                        else
                        {
                            File.Delete(file);
                        }
                    }
                    catch (Exception ex)
                    {
                        DebugHelper.Output("获取持久化存储消息异常", ex);
                        File.Delete(file);
                    }
                }

                return list;
            }
        }
    }
}
