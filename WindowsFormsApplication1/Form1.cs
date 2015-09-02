using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Windows.Forms;
using ryu_s;
using ryu_s.NicoLibrary;
using System.Text.RegularExpressions;
namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
        }
        protected override void OnClosing(CancelEventArgs e)
        {
            Settings.Save();
            base.OnClosing(e);
        }
        public async Task Start(CancellationToken ct)
        {
            var url1 = "http://live.nicovideo.jp/";
            var url2 = "http://www.chikuwachan.com/live/";
            var liveIdList = new List<string>();
            var availBrowsers = new[] { ryu_s.MyCommon.Browser.BrowserType.Chrome, ryu_s.MyCommon.Browser.BrowserType.Firefox, ryu_s.MyCommon.Browser.BrowserType.IE };
            var circle = ProviderAddrPortResolver.CreateEmptyCircle();
            var liveContextDic = new Dictionary<string, LiveContext>();
            var provider_type = ryu_s.NicoLibrary.Provider_Type.Community;
            /// <summary>
            /// 一つしか分かっていない
            /// </summary>
            var One = new List<List<AddrPort>>();
            /// <summary>
            /// 一部に欠けがある
            /// </summary>
            var Shortage = new List<List<AddrPort>>();
            /// <summary>
            /// 連続している
            /// </summary>
            var Sequentially = new List<List<AddrPort>>();
            var t = Task.Factory.StartNew(() =>
            {
                do
                {
                    var idSources = new[] { url1, url2 };
                    foreach (var idSource in idSources)
                    {
                        var idList = GetLiveId(idSource).Result;
                        liveIdList.AddRange(idList);
                    }
                    liveIdList = liveIdList.Select(s => s).Distinct().ToList();

                    //各ブラウザごとにplayerstatusを取得する。
                    foreach (var browser in availBrowsers)
                    {
                        foreach (var live_id in liveIdList)
                        {
                            Console.WriteLine(live_id);
                            if (!liveContextDic.ContainsKey(live_id))
                            {
                                liveContextDic.Add(live_id, new LiveContext(live_id, true));
                            }
                            var context = liveContextDic[live_id];
                            if (!context.IsBroadcasting)
                                continue;
                            //5分以内に同じ放送にアクセスしない。座席が変わることを期待して。
                            if (context.LastAccessDic.ContainsKey(browser) && context.LastAccessDic[browser].AddMinutes(5) > DateTime.Now)
                            {
                                continue;
                            }
                            if (!context.LastAccessDic.ContainsKey(browser))
                            {
                                context.LastAccessDic.Add(browser, new DateTime(0));
                            }
                            try
                            {
                                var playerstatus = GetPlayerStatusTest(live_id, browser, null).Result;
                                System.Diagnostics.Debug.WriteLine($"{playerstatus.user.room_label}, {playerstatus.user.room_seetno}");
                                if (playerstatus.stream.Provider_Type == provider_type && !context.RoomList.Select(room => room.room_label).Contains(playerstatus.user.room_label))
                                {
                                    context.RoomList.Add(new Room4(playerstatus.user.room_label, playerstatus.ms));
                                }
                            }
                            catch (AggregateException ex)
                            {
                                ryu_s.MyCommon.Logging.LogException(ryu_s.MyCommon.LogLevel.error, ex);
                                context.IsBroadcasting = false;
                            }
                            catch (Exception ex)
                            {
                                ryu_s.MyCommon.Logging.LogException(ryu_s.MyCommon.LogLevel.error, ex);
                                context.IsBroadcasting = false;
                            }
                            context.LastAccessDic[browser] = DateTime.Now;
                            Task.WaitAll(Task.Delay(500));
                        }
                        Task.WaitAll(Task.Delay(1000));
                    }
                    Console.WriteLine("roominfo collection completed");
                    foreach (var liveContext in liveContextDic.Select(pair => pair.Value))
                    {
                        var listlist = liveContext.ToList();
                        foreach (var m in listlist)
                        {
                            if (m.Count == 0)
                                continue;
                            if (m.Count == 1)
                            {
                                One.Add(m);
                            }
                            else if (m.Where(s => s == null).Count() > 0)
                            {
                                Shortage.Add(m);
                            }
                            else
                            {
                                Sequentially.Add(m);
                            }
                        }
                    }
                    ProviderAddrPortResolver.Distinct(Shortage);
                    ProviderAddrPortResolver.Distinct(Sequentially);
                    ProviderAddrPortResolver.ComplementShortage(Shortage, Shortage, Sequentially);
                    ProviderAddrPortResolver.ComplementShortage(Sequentially, Shortage, Sequentially);
                    Console.WriteLine($"Shortage.Count={One.Count}");
                    Console.WriteLine($"One.Count={Shortage.Count}");
                    Console.WriteLine($"Sequentially.Count={Sequentially.Count}");


                    //ひと通り済んだら
                    var c = ProviderAddrPortResolver.Concat(circle, Shortage);
                    var k = ProviderAddrPortResolver.Concat(circle, Sequentially);
                    //面倒だから全部Shortageに入れちゃう。重複もなんのその。
                    Shortage.AddRange(c.NotResolved.Select(b => b.ToList()));
                    Shortage.AddRange(k.NotResolved.Select(b => b.ToList()));
                    Console.WriteLine($"circle.count={circle.Count}");


                } while (!ProviderAddrPortResolver.IsCompleted(circle));

            }, ct);

            await t;
            Console.WriteLine(circle.ToStr());
        }

        public async Task<List<string>> GetLiveId(string url)
        {
            var list = new List<string>();
            try
            {
                var headers = new KeyValuePair<string, string>[]{
                new KeyValuePair<string,string>("Accept-Language", "ja-JP"),
            };
                var html = await ryu_s.Net.Http.GetAsync(url, headers, null, Encoding.UTF8);
                var pattern = "lv[\\d]+";
                var matches = Regex.Matches(html, pattern, RegexOptions.Compiled);
                foreach (Match m in matches)
                {
                    if (m.Success)
                    {
                        list.Add(m.Value);
                    }
                }
            }
            catch (Exception ex)
            {
                ryu_s.MyCommon.Logging.LogException(ryu_s.MyCommon.LogLevel.error, ex);
            }
            return list;
        }

        CancellationTokenSource cts;
        private async void button1_Click(object sender, EventArgs e)
        {
            //var liveContext = new LiveContext("lv9", true);
            //liveContext.AddRoom(new Room4("アリーナ", new ms("msg101.live.nicovideo.jp", 2805, 100 + "")));
            //liveContext.AddRoom(new Room4("アリーナ", new ms("msg101.live.nicovideo.jp", 2806, 101 + "")));
            //liveContext.AddRoom(new Room4("アリーナ", new ms("msg101.live.nicovideo.jp", 2807, 102 + "")));
            //liveContext.AddRoom(new Room4("アリーナ", new ms("msg101.live.nicovideo.jp", 2809, 104 + "")));
            //liveContext.AddRoom(new Room4("アリーナ", new ms("msg101.live.nicovideo.jp", 2900, 195 + "")));
            //liveContext.AddRoom(new Room4("アリーナ", new ms("msg101.live.nicovideo.jp", 2901, 196 + "")));

            //var t = liveContext.ToList();
            //Settings.Instance.Shortage.Add(t[0]);
            //Settings.Instance.Shortage.Add(t[0]);
            //var s = "";
            //Distinct(Settings.Instance.Shortage);
            cts = new CancellationTokenSource();
            await Start(cts.Token).ContinueWith(t =>
            {
                cts.Dispose();
                cts = null;
                return t;
            });
            System.Diagnostics.Debug.WriteLine("Completed!");
            return;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (cts != null)
            {
                cts.Cancel();
            }
        }

        private async Task<getplayerstatus_new> GetPlayerStatusTest(string live_id, ryu_s.MyCommon.Browser.BrowserType browser, IProgress<StringReport> progress)
        {
            if (progress == null)
                progress = new Progress<StringReport>();
            getplayerstatus_new playerStatus = null;
            string errMessage = "";
            System.Net.CookieContainer cc = null;
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
        end:
            if (playerStatus == null)
            {
                throw new Exception(errMessage);
            }
            return playerStatus;
        }
        /// <summary>
        /// comingsoonでも止まるように変更した！
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
                        //注意！！
                        case errorcode.comingsoon:
                        case errorcode.noauth:
                            return response;
                        default:
                            break;
                    }
                    report.Message = string.Format("{0} {1, 4}回目", code, loopCounter);
                    progress.Report(report);
                }
            } while (true);
        }
        public class StringReport
        {
            public string Message;
        }
    }
}
