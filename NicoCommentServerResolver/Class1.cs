using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ryu_s
{
    /// <summary>
    /// 名前はめちゃくちゃ適当。
    /// </summary>
    public class ProviderAddrPortResolver
    {
        public Elem<AddrPort> AddrPortCircle { get; set; }
        public ProviderAddrPortResolver()
        {
            AddrPortCircle = CreateEmptyCircle();
        }
        public static Elem<AddrPort> CreateEmptyCircle()
        {
            var element = new Elem<AddrPort>();
            element.Before = element;
            element.After = element;
            return element;
        }
        /// <summary>
        /// 先頭の要素を取得
        /// </summary>
        /// <returns></returns>
        public Elem<AddrPort> GetHead(Elem<AddrPort> a)
        {
            var start = a;
            var current = a;
            var head = current;
            do
            {
                if (IsLower(current, head))
                    head = current;
                current = current.After;
            } while (current != start);
            return head;
        }
        /// <summary>
        /// aはlowerよりもlowerか
        /// </summary>
        /// <param name="a"></param>
        /// <param name="lower"></param>
        /// <returns></returns>
        public bool IsLower(Elem<AddrPort> a, Elem<AddrPort> lower)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// elemの次に要素を追加
        /// </summary>
        /// <param name="elem"></param>
        /// <param name="newElem"></param>
        public static Elem<AddrPort> InsertNext(Elem<AddrPort> elem, Elem<AddrPort> newElem)
        {
            newElem.After = elem.After;
            newElem.Before = elem;
            elem.After.Before = newElem;
            elem.After = newElem;
            return newElem;
        }
        public static Elem<AddrPort> InsertNext(Elem<AddrPort> elem, AddrPort newAddrPort)
        {
            return InsertNext(elem, new Elem<AddrPort>(newAddrPort));
        }
        public static Elem<AddrPort> InsertNext(Elem<AddrPort> elem, IEnumerable<AddrPort> newAddrPorts)
        {
            var tmp = elem;
            foreach (var newAddrPort in newAddrPorts)
            {
                tmp = InsertNext(tmp, newAddrPort);
            }
            return tmp;
        }

        /// <summary>
        /// 空要素が無くなったか
        /// </summary>
        /// <returns></returns>
        public static bool IsCompleted(Elem<AddrPort> a)
        {
            var start = a;
            var current = a;
            do
            {
                if (current.IsEmpty)
                    return false;
                current = current.After;
            } while (current != start);
            return true;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="circle"></param>
        /// <param name="addrPort"></param>
        /// <returns></returns>
        public static Elem<AddrPort> GetElement(Elem<AddrPort> circle, AddrPort addrPort)
        {
            var start = circle;
            var current = circle;
            do
            {
                if (!current.IsEmpty && current.Value.Equals(addrPort))
                    return current;
                current = current.After;
            } while (current != start);
            return null;
        }
        /// <summary>
        /// headの後ろ、tailの前を新しい要素に付け替える。
        /// </summary>
        /// <param name="head"></param>
        /// <param name="tail"></param>
        /// <param name="newAddrPorts"></param>
        /// <returns></returns>
        public static Elem<AddrPort> Replace(Elem<AddrPort> head, Elem<AddrPort> tail, IEnumerable<AddrPort> newAddrPorts)
        {
            var tmpCircle = CreateEmptyCircle();
            InsertNext(tmpCircle, newAddrPorts);
            var begin = tmpCircle.After;
            var end = tmpCircle.Before;
            head.After = begin;
            begin.Before = head;
            tail.Before = end;
            end.After = tail;
            return head;
        }
        public class CircleAndNotResolved<T>
        {
            public T NotResolved { get; private set; }
            public Elem<AddrPort> Circle { get; private set; }
            public CircleAndNotResolved(Elem<AddrPort> circle, T notResolved)
            {
                Circle = circle;
                NotResolved = notResolved;
            }
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="circle"></param>
        /// <param name="parts"></param>
        /// <returns></returns>
        public static CircleAndNotResolved<IEnumerable<IEnumerable<AddrPort>>> Concat(Elem<AddrPort> circle, IEnumerable<IEnumerable<AddrPort>> parts)
        {
            var ret0 = circle;
            var list = parts;
            int before;
            int after;
            var tmp = new List<IEnumerable<AddrPort>>();
            //listの要素数が変わらなくなるまでひたすら繰り返す。
            do
            {
                before = list.Count();
                foreach (var arr in list)
                {
                    var ret = ProviderAddrPortResolver.Concat(ret0, arr);
                    if (ret.NotResolved.Count() != 0)
                        tmp.Add(ret.NotResolved);
                    ret0 = ret.Circle;
                }
                list = tmp;
                after = list.Count();
            } while (before != after);
            return new CircleAndNotResolved<IEnumerable<IEnumerable<AddrPort>>>(ret0, list);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="part1"></param>
        /// <param name="part2"></param>
        /// <returns>できたやつと結合できなかったやーつ</returns>
        public static CircleAndNotResolved<IEnumerable<AddrPort>> Concat(Elem<AddrPort> circle, IEnumerable<AddrPort> part2)
        {
            //空
            if (circle.Count == 1 && circle.IsEmpty)
            {
                foreach (var addrPort in part2)
                {
                    circle = InsertNext(circle, addrPort);
                }
                return new CircleAndNotResolved<IEnumerable<AddrPort>>(circle, new List<AddrPort>());
            }

            var part = part2.ToList();
            for (int i = 0; i < part.Count; i++)
            {
                var start = circle;
                var current = circle;
                do
                {
                    if (!current.IsEmpty && current.Value.Equals(part[i]))
                    {
                        if (i == 0)
                        {
                            //もし、次の空で無い要素が2つ目以降と一致したら、一致したところまでをつなぎ替える
                            var t = current;//次の空でない要素
                            do
                            {
                                t = t.After;
                            } while (t.IsEmpty);
                            for (int m = 1; m < part.Count; m++)
                            {
                                if (t.Value.Equals(part[m]))
                                {
                                    Replace(current, t.After, part.GetRange(i + 1, m));
                                    //                                    System.Diagnostics.Debug.WriteLine(circle.ToStr());
                                    //一致したところ以降は最初から
                                    return Concat(circle, part.GetRange(m, part.Count - m));
                                }
                            }

                            //次の要素も一致したら最初の要素を削除してもう一回最初から。
                            if (i + 1 < part.Count && !current.After.IsEmpty && current.After.Value.Equals(part[i + 1]))
                            {
                                part.RemoveAt(i);
                                return Concat(circle, part);
                            }

                            //3要素目以降でもし一致したら一致したところまでを新しい要素に付け替える。これで途中にEmptyがあっても消える
                            Elem<AddrPort> matchElem = null;
                            int j;//一致したインデックス
                            for (j = 2; j < part.Count; j++)
                            {
                                matchElem = GetElement(circle, part[j]);
                            }
                            if (matchElem == null)
                            {
                                //既に登録済みの中に今回の分の2つ目以降の要素は含まれていなかった。今回の分が1つしか無かった場合もここに来る。余分な計算をする羽目になるが、一般的に考える。
                                if (i + 1 < part.Count)
                                {
                                    //2つ目以降の要素を挿入する。
                                    InsertNext(current, part.GetRange(i + 1, part.Count - (i + 1)));
                                }
                                return new CircleAndNotResolved<IEnumerable<AddrPort>>(circle, new List<AddrPort>());
                            }
                            Replace(current, matchElem.After, part.GetRange(i + 1, part.Count - (i + j)));                            
                            if (j + 1 < part.Count)
                            {
                                //一致した以降にまだ要素があったら、一致した要素以降をもう一回最初から
                                part.RemoveRange(0, j - 1);
                                return Concat(circle, part);
                            }
                        }
                        else
                        {
                            //2つ目以降が一致した。
                            //一致したところまでの要素を単純につなぎ、再度やり直す。
                            InsertNext(current.Before, part.GetRange(0, i));
                            part.RemoveRange(0, i);
                            return Concat(circle, part);
                        }
                    }
                    current = current.After;
                } while (current != start);
            }
            //一切一致しなかった。
            return new CircleAndNotResolved<IEnumerable<AddrPort>>(circle, part2);
        }
        /// <summary>
        /// リストどうしを外部結合する
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <returns>結合して出来たリスト、結合できなかった場合は空リスト</returns>
        public static IEnumerable<AddrPort> OuterJoin(List<AddrPort> left, List<AddrPort> right)
        {            
            for(int i = 0;i< left.Count; i++)
            {
                for(int j = 0;j< right.Count; j++)
                {
                    if (left[i] != null && left[i].Equals(right[j]))
                    {
                        //i, jはそれぞれleft, righの合致した位置
                        //統合後の要素数
                        var count = ((i >= j) ? i : j) + (i >= j ? (right.Count - j) : (left.Count - i));
                        
                        var list1 = Enumerable.Repeat<AddrPort>(null, count).ToList();
                        var large = (i >= j) ? left : right;
                        var small = (i >= j) ? right : left;
                        //合致したところのインデックスが大きい方の要素は前から、小さい方は合致した位置から要素を挿入する。
                        for (int k = 0; k < large.Count ; k++)
                        {
                            list1[k] = large[k];
                        }
                        //kはsmallのインデックス、mは結合結果のリストにおけるsmallの開始位置。
                        for(int k = 0, m = ((i >= j) ? i : j) - ((i >= j) ? j : i); k < small.Count; k++)
                        {
                            if(small[k] != null)
                                list1[k + m] = small[k];
                        }
                        return list1;
                    }
                }
            }
            return new List<AddrPort>();
        }
        /// <summary>
        /// 重複を削除
        /// </summary>
        /// <param name="target"></param>
        public static void Distinct(List<List<AddrPort>> target)
        {
            if (target.Count <= 1)
                return;
            start:
            for (int i = 0; i < target.Count; i++)
            {
                for (int j = i + 1; j < target.Count; j++)
                {
                    var a = target[i];
                    var b = target[j];
                    if (Equals(a, b))
                    {
                        target.Remove(b);
                        goto start;
                    }
                }
            }
        }
        /// <summary>
        /// 総当りで外部結合を試みる
        /// </summary>
        /// <param name="target">総当りしてみるリスト。shortageでもsequentiallyでも可</param>
        /// <param name="shortage">欠けがあるリストを保持するリスト</param>
        /// <param name="sequentially">欠けがない（以下同文</param>
        public static void ComplementShortage(List<List<AddrPort>> target, List<List<AddrPort>> shortage, List<List<AddrPort>> sequentially)
        {
            if (target.Count <= 1)
                return;
            start:
            for (int i = 0; i < target.Count; i++)
            {
                for (int j = i + 1; j < target.Count; j++)
                {
                    var a = target[i];
                    var b = target[j];
                    var ret = ProviderAddrPortResolver.OuterJoin(a, b);
                    if (ret.Count() > 0)
                    {
                        if (ret.Contains(null))
                        {
                            shortage.Add(ret.ToList());
                        }
                        else
                        {
                            sequentially.Add(ret.ToList());
                        }
                        target.Remove(a);
                        target.Remove(b);
                        //リストに対して追加したり削除したりと荒らすから処理を継続すると問題が起こりかねない。
                        //ちょっと無駄が多いけどしょうがないから最初からやり直す。
                        goto start;
                    }
                }
            }
        }
    }

    public class AddrPort
    {
        public string Addr { get; private set; }
        public int Port { get; private set; }
        public AddrPort(string addr, int port)
        {
            Addr = addr;
            Port = port;
        }
        public override string ToString()
        {
            return $"{Addr}, {Port}";
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;
            AddrPort p = obj as AddrPort;
            if ((object)p == null)
                return false;
            return this.Equals(p);
        }
        public bool Equals(AddrPort a)
        {
            return this.Addr == a.Addr && this.Port == a.Port;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
    public class Elem<T>
    {
        /// <summary>
        /// 前の要素
        /// </summary>
        public Elem<T> Before;
        public T Value { get; set; }
        public bool IsEmpty
        {
            get
            {
                return Value == null;
            }
        }
        /// <summary>
        /// 次の要素
        /// </summary>
        public Elem<T> After;
        public Elem()
        {

        }
        public Elem(T v)
        {
            Value = v;
        }
        public override string ToString()
        {
            if (IsEmpty)
                return $"(Empty)";
            else
                return Value.ToString();
        }
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }
        public bool Equals(AddrPort a)
        {
            if (a == null || this.Value == null)
                return false;
            return this.Value.Equals(a);
        }
        public IEnumerable<Elem<T>> Elems()
        {
            var start = this;
            var current = this;
            do
            {
                yield return current;
                current = current.After;
            } while (current != start);
        }
        public int Count
        {
            get
            {
                return Elems().ToList().Count;
            }
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public string ToStr()
        {
            var start = this;
            var current = this;
            var str = "";
            do
            {
                str += current.ToString() + Environment.NewLine;
                current = current.After;
            } while (current != start);
            return str;
        }
    }
}
