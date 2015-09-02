using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Concurrent;
namespace ryu_s
{
    [Serializable]
    public class Settings
    {
        /// <summary>
        /// 
        /// </summary>
        public ConcurrentDictionary<string, LiveContext> LiveInfoDic = new ConcurrentDictionary<string, LiveContext>();
        /// <summary>
        /// 
        /// </summary>
        public List<LiveIdSource> SourceList { get; set; } = new List<LiveIdSource>();
        /// <summary>
        /// 一つしか分かっていない
        /// </summary>
        public List<List<AddrPort>> One = new List<List<AddrPort>>();
        /// <summary>
        /// 一部に欠けがある
        /// </summary>
        public List<List<AddrPort>> Shortage = new List<List<AddrPort>>();
        /// <summary>
        /// 連続している
        /// </summary>
        public List<List<AddrPort>> Sequentially = new List<List<AddrPort>>();
        private static string filename = "settings.config";
        private Settings()
        {
        }
        private static Settings instance;
        public static Settings Instance
        {
            get
            {
                return instance;
            }
        }
        public static Settings Load()
        {
            var bf = new BinaryFormatter();
            if (System.IO.File.Exists(filename))
            {
                using (var fs = new System.IO.FileStream(filename, System.IO.FileMode.Open))
                {
                    instance = (Settings)bf.Deserialize(fs);
                }
            }
            else
            {
                instance = new Settings();
            }
            return instance;
        }
        public static void Save()
        {
            try {
                var bf = new BinaryFormatter();
                using (var fs = new System.IO.FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write))
                {
                    bf.Serialize(fs, Instance);
                }
            }catch(Exception ex)
            {
                ryu_s.MyCommon.Logging.LogException(MyCommon.LogLevel.error, ex);
            }
        }
    }
}
