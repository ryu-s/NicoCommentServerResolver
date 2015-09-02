using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Timers;
using ryu_s.NicoLibrary;
using System.Text.RegularExpressions;
namespace ryu_s
{
    public class RoomInfoCollector
    {
        public RoomInfoCollector()
        {            
        }
        public Task StartLiveIdCollection(CancellationToken ct)
        {
            return Task.Factory.StartNew(async() =>
            {
                foreach(var c in Settings.Instance.SourceList)
                {
                    c.Timer.Start();
                    await LiveIdSource.LiveIdSourceTimer_Elapsed(c);
                }
            }, ct);
        }
            
        public Task<Elem<AddrPort>> CreateCircle(CancellationToken ct)
        {
            return Task.Factory.StartNew(() =>
            {
                var circle = ProviderAddrPortResolver.CreateEmptyCircle();
                do
                {
                    var liveContextList = new List<LiveContext>();
                    var dicList = Settings.Instance.LiveInfoDic.ToList();
                    for (int i = 0;i< dicList.Count; i++)
                    {
                        liveContextList.Add(dicList[i].Value);
                    }
                    foreach (var liveContext in liveContextList)
                    {
                        var listlist = liveContext.ToList();
                        foreach (var m in listlist)
                        {
                            if (m.Count == 0)
                                continue;
                            if (m.Count == 1)
                            {
                                Settings.Instance.One.Add(m);
                            }
                            else if (m.Where(s => s == null).Count() > 0)
                            {
                                Settings.Instance.Shortage.Add(m);
                            }
                            else
                            {
                                Settings.Instance.Sequentially.Add(m);
                            }
                        }
                    }
                    ProviderAddrPortResolver.Distinct(Settings.Instance.Shortage);
                    ProviderAddrPortResolver.ComplementShortage(Settings.Instance.Shortage, Settings.Instance.Shortage, Settings.Instance.Sequentially);
                    ProviderAddrPortResolver.ComplementShortage(Settings.Instance.Sequentially, Settings.Instance.Shortage, Settings.Instance.Sequentially);
                    //ひと通り済んだら
                    var c = ProviderAddrPortResolver.Concat(circle, Settings.Instance.Shortage);
                    var k = ProviderAddrPortResolver.Concat(circle, Settings.Instance.Sequentially);
                    //面倒だから全部Shortageに入れちゃう。重複もなんのその。
                    Settings.Instance.Shortage.AddRange(c.NotResolved.Select(b => b.ToList()));
                    Settings.Instance.Shortage.AddRange(k.NotResolved.Select(b => b.ToList()));
                    Console.WriteLine($"circle.count={circle.Count}");
                } while (!ProviderAddrPortResolver.IsCompleted(circle));
                return circle;
            }, ct);
        }



        public void AddLiveIdSource(string url)
        {
            if (Settings.Instance.SourceList.Where(source => source.Url == url).Count() == 0)
            {
                var source1 = new LiveIdSource(url);
                Settings.Instance.SourceList.Add(source1);
            }
        }

    }
    [Serializable]
    public class LiveContext
    {
        /// <summary>
        /// 配信中か。配信が切れたらfalseにし、二度とアクセスしない
        /// </summary>
        public bool IsBroadcasting { get; set; }
        public string live_id { get; private set; }
        public DateTime LastAccess { get; set; }
        public Dictionary<ryu_s.MyCommon.Browser.BrowserType, DateTime> LastAccessDic { get; set; } = new Dictionary<MyCommon.Browser.BrowserType, DateTime>();
        public List<Room4> RoomList { get; private set; } = new List<Room4>();
        public LiveContext(string live_id,bool isBroadcasting)
        {
            this.live_id = live_id;
            this.IsBroadcasting = isBroadcasting;
        }
        public void AddRoom(Room4 room)
        {
            if (!RoomList.Contains(room))
            {
                RoomList.Add(room);
            }
        }
        public List<List<AddrPort>> ToList()
        {
            var listList = new List<List<AddrPort>>();

            if (RoomList.Count == 0)
                return listList;
            
            RoomList.Sort((x, y) => x.ms.thread.CompareTo(y.ms.thread));
            var beforeThread = int.Parse(RoomList[0].ms.thread);
            var list = new List<AddrPort>();
            foreach(var room in RoomList)
            {
                var currentThread = int.Parse(room.ms.thread);
                if (beforeThread + 1 == currentThread)
                    list.Add(new AddrPort(room.ms.addr, room.ms.port));
                else if(beforeThread + 20 > currentThread)
                {
                    //threadの値が20以内であれば連続的に配置できると判断する。
                    var emptyCount = currentThread - (beforeThread + 1);
                    for (int i = 0; i < emptyCount; i++)
                        list.Add(null);//不明な部分はnullで埋める
                    list.Add(new AddrPort(room.ms.addr, room.ms.port));
                }
                else
                {
                    listList.Add(list);
                    list = new List<AddrPort>();
                    list.Add(new AddrPort(room.ms.addr, room.ms.port));
                }
                beforeThread = currentThread;
            }
            listList.Add(list);
            return listList;
        }
    }
    [Serializable]
    public class LiveIdSource
    {
        public string Url { get; private set; }
        [NonSerialized]
        private System.Timers.Timer timer;
        public System.Timers.Timer Timer { get { return timer; } }
        public LiveIdSource(string url)
        {
            Url = url;
            Initialize();
        }

        public void Initialize()
        {
            this.timer = new System.Timers.Timer();
            this.Timer.AutoReset = true;
            this.Timer.Interval = 5 * 60 * 1000;
            this.Timer.Elapsed += async (sender, e) => await LiveIdSourceTimer_Elapsed(this);
        }
        public static async Task LiveIdSourceTimer_Elapsed(LiveIdSource source)
        {

        }
    }
}
