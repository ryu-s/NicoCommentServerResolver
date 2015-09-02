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


            //var liveIdCollectorTask = collector.StartLiveIdCollection(ct);
            //var roomInfoCollectorTask1 = collector.StartRoomInfoCollection(ryu_s.MyCommon.Browser.BrowserType.Chrome, ct);
            //var roomInfoCollectorTask2 = collector.StartRoomInfoCollection(ryu_s.MyCommon.Browser.BrowserType.Firefox, ct);
            //var roomInfoCollectorTask3 = collector.StartRoomInfoCollection(ryu_s.MyCommon.Browser.BrowserType.IE, ct);

            //var createCircleTask = collector.CreateCircle(ct);
            //await Task.WhenAll(liveIdCollectorTask, roomInfoCollectorTask1, roomInfoCollectorTask2, roomInfoCollectorTask3, createCircleTask);
//            Console.WriteLine(createCircleTask.Result);
        }
        public bool Equals(List<AddrPort> left, List<AddrPort> right)
        {
            if (left.Count != right.Count)
                return false;
            for(int i = 0;i < left.Count;i++)
            {
                if ((left[i] == null && right[i] != null) || (left[i] != null && right[i] == null))
                    return false;
                if (left[i] != right[i])
                    return false;
            }
            return true;
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
            });
            return;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if(cts != null)
            {
                cts.Cancel();
            }
        }
    }
}
