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
        public Task StartRoomInfoCollection(ryu_s.MyCommon.Browser.BrowserType browser, CancellationToken ct)
        {
            return Task.Factory.StartNew(async () =>
            {
                while (true)
                {

                    var liveContextList = new List<LiveContext>();

                    //var dicList = Settings.Instance.LiveInfoDic.ToList();
                    //for (int i = 0;i< dicList.Count; i++)
                    //{
                    //    liveContextList.Add(dicList[i].Value);
                    //}
                    foreach(var kv in Settings.Instance.LiveInfoDic)
                    {
                        liveContextList.Add(kv.Value);
                    }


                    for (int i = 0;i<liveContextList.Count;i++)
                    {
                        var liveContext = liveContextList[i];
                        getplayerstatus_new playerstatus = null;
                        try {
                            if (!liveContext.IsBroadcasting)
                                continue;
                            var pair = await GetPlayerStatusTest(liveContext.live_id, new[] { browser }, new Progress<StringReport>());
                            playerstatus = pair.Key;
                        }catch(Exception ex)
                        {
                            MyCommon.Logging.LogException(MyCommon.LogLevel.error, ex);
                            liveContext.IsBroadcasting = false;
                            continue;
                        }
                        if (liveContext.RoomList.Where(room => room.room_label == playerstatus.user.room_label).Count() == 0)
                        {
                            liveContext.RoomList.Add(new Room4(playerstatus.user.room_label, playerstatus.ms));
                        }
                        
                        await Task.Delay(1000);
                    }
                }
            }, ct);
        }
        public class StringReport
        {
            public string Message;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="live_id"></param>
        /// <param name="cc"></param>
        /// <param name="progress"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        private async Task<Response<getplayerstatus_new>> GetSeatLoopTest(string live_id, System.Net.CookieContainer cc, IProgress<StringReport> progress, System.Threading.CancellationToken token, bool logging)
        {
            var report = new StringReport();
            int loopCounter = 0;
            Response<getplayerstatus_new> response;
            do
            {
                //座席が取れた場合と座席を取るのが不可能な場合に結果を戻す。それ以外の場合には経過をReportして継続する。
                token.ThrowIfCancellationRequested();
                loopCounter++;
                response = await Api.GetPlayerStatus_new(live_id, cc, logging);
                if (response.status == status.ok)
                {
                    return response.GetResponse();
                }
                else
                {
                    var code = response.GetError().code;
                    switch (code)
                    {
                        case errorcode.closed:
                        case errorcode.notfound:
                        case errorcode.notlogin:
                        case errorcode.require_community_member:
                        case errorcode.deletedbyuser:
                            return response;
                        default:
                            break;
                    }
                    report.Message = string.Format("{0} {1, 4}回目", code, loopCounter);
                    progress.Report(report);
                }
            } while (true);
        }

        private async Task<KeyValuePair<getplayerstatus_new, System.Net.CookieContainer>> GetPlayerStatusTest(string live_id, IEnumerable<ryu_s.MyCommon.Browser.BrowserType> browsersAvailable, IProgress<StringReport> progress)
        {
            getplayerstatus_new playerStatus = null;
            string errMessage = "";
            System.Net.CookieContainer cc = null;
            foreach (var browser in browsersAvailable)
            {
                cc = ryu_s.NicoLibrary.Tool.GetNicoCookieContainer(browser);
                Response<getplayerstatus_new> response = null;
                do
                {
                    try
                    {
                        response = await GetSeatLoopTest(live_id, cc, progress, new System.Threading.CancellationToken(), true).ConfigureAwait(false);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"playerstatusが取得出来なかった。リダイレクトされたかも。 live_id={live_id}", ex);
                    }
                    if (response is error_new<getplayerstatus_new>)
                    {
                        errMessage = $"playerstatusが取得出来なかった。code={response.GetError().code}, live_id={live_id}";
                        break;
                    }
                    playerStatus = response.GetResponse();
                    goto end;
                } while (true);
            }
        end:
            if (playerStatus == null)
            {
                throw new Exception(errMessage);
            }
            return new KeyValuePair<getplayerstatus_new, System.Net.CookieContainer>(playerStatus, cc);
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
            try {
                var headers = new KeyValuePair<string, string>[]{
                new KeyValuePair<string,string>("Accept-Language", "ja-JP"),
            };
                var html = await ryu_s.Net.Http.GetAsync(source.Url, headers, null, Encoding.UTF8);
                var pattern = "lv[\\d]+";
                var matches = Regex.Matches(html, pattern, RegexOptions.Compiled);
                foreach (Match m in matches)
                {
                    if (m.Success)
                    {
                        if (!Settings.Instance.LiveInfoDic.ContainsKey(m.Value))
                        {
                            Settings.Instance.LiveInfoDic.AddOrUpdate(m.Value, new LiveContext(m.Value, true), (k,oldValue) => new LiveContext(m.Value, true));
                        }
                    }
                }
            }catch(Exception ex)
            {
                ryu_s.MyCommon.Logging.LogException(MyCommon.LogLevel.error, ex);
            }
        }
    }
}
