using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ryu_s;
namespace WindowsFormsApplication1
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        /// <summary>
        /// 一つしか分かっていない
        /// </summary>
        List<List<AddrPort>> One = new List<List<AddrPort>>();
        /// <summary>
        /// 一部に欠けがある
        /// </summary>
        List<List<AddrPort>> Shortage = new List<List<AddrPort>>();
        /// <summary>
        /// 連続している
        /// </summary>
        List<List<AddrPort>> Sequentially = new List<List<AddrPort>>();
        /// <summary>
        /// 欠けを無くそうと努力してみる
        /// </summary>
        public void ComplementShortage()
        {
        start:
            for (int i = 0; i < Shortage.Count; i++)
            {
                for (int j = i + 1; j < Shortage.Count; j++)
                {
                    var a = Shortage[i];
                    var b = Shortage[j];
                    var ret = ProviderAddrPortResolver.OuterJoin(a, b);
                    if (ret.Count() > 0)
                    {
                        if (ret.Contains(null))
                        {
                            Shortage.Add(ret.ToList());
                        }
                        else
                        {
                            Sequentially.Add(ret.ToList());
                        }
                        Shortage.Remove(a);
                        Shortage.Remove(b);
                        //リストに対して追加したり削除したりと荒らすから処理を継続すると問題が起こりかねない。
                        //ちょっと無駄が多いけどしょうがないから最初からやり直す。
                        goto start;
                    }
                }
            }
        }
        private void button1_Click(object sender, EventArgs e)
        {


            var list1 = new List<AddrPort>();
            list1.Add(new AddrPort("msg101", 2805));
            list1.Add(new AddrPort("msg101", 2806));
            list1.Add(new AddrPort("msg101", 2807));
            list1.Add(new AddrPort("msg101", 2808));
            Shortage.Add(list1);

            var list2 = new List<AddrPort>();
            list2.Add(new AddrPort("msg101", 2808));
            list2.Add(new AddrPort("msg101", 2809));
            list2.Add(new AddrPort("msg101", 2810));
            list2.Add(new AddrPort("msg101", 2811));
            Shortage.Add(list2);

            //            var list = ProviderAddrPortResolver.OuterJoin(list1, list2).ToList();
            ComplementShortage();
            foreach (var k in Shortage)
            {
                foreach (var t in k)
                {
                    Console.WriteLine(t);
                }
            }

            return;
        }
    }
}
