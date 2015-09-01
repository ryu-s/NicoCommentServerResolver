using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using ryu_s;
using System.Collections.Generic;
namespace UnitTestProject1
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public void TestMethod1()
        {
            var resolver = new ProviderAddrPortResolver();
            var t = new AddrPort("omsg101", 2805);
            var k = new AddrPort("omsg102", 2818);
            var g = new AddrPort("omsg103", 2831);
            var ret = ProviderAddrPortResolver.Concat(ProviderAddrPortResolver.CreateEmptyCircle(), new[] { t, k, g });
            var a = new Elem<AddrPort>(t);
            var b = new Elem<AddrPort>(k);
            var c = new Elem<AddrPort>(g);
            var circle = ProviderAddrPortResolver.CreateEmptyCircle();
            circle = ProviderAddrPortResolver.InsertNext(circle, a);
            circle = ProviderAddrPortResolver.InsertNext(circle, b);
            circle = ProviderAddrPortResolver.InsertNext(circle, c);
            Assert.AreEqual(ret.Circle.ToStr(), circle.ToStr());
        }
        [TestMethod]
        public void TestMethod2()
        {
            var a = new[]
            {
                new AddrPort("omsg101" ,2815),
                new AddrPort("omsg102" ,2828),
                new AddrPort("omsg103" ,2841),
                new AddrPort("omsg104" ,2854),
                new AddrPort("omsg101" ,2816),
                new AddrPort("omsg102" ,2829),
                new AddrPort("omsg103" ,2842),
                new AddrPort("omsg104" ,2855),
                new AddrPort("omsg101" ,2817),
                new AddrPort("omsg102" ,2830),
                new AddrPort("omsg103" ,2843),
                new AddrPort("omsg104" ,2856),
                new AddrPort("omsg105" ,2867),
                new AddrPort("omsg106" ,2880),
                new AddrPort("omsg105" ,2868),
                new AddrPort("omsg106" ,2881),
                new AddrPort("omsg105" ,2869),
                new AddrPort("omsg106" ,2882),
            };
            var b = new[]
            {
                new AddrPort("omsg106" ,2882),
                new AddrPort("omsg101" ,2815),
            };
            var ret1 = ProviderAddrPortResolver.Concat(ProviderAddrPortResolver.CreateEmptyCircle(), a);
            var ret2 = ProviderAddrPortResolver.Concat(ret1.Circle, b);
            Assert.IsTrue(ProviderAddrPortResolver.IsCompleted(ret2.Circle));
        }
        [TestMethod]
        public void TestMethod3()
        {
            var a = new[]
            {
                new AddrPort("omsg103" ,2843),
                new AddrPort("omsg104" ,2856),
                new AddrPort("omsg105" ,2867),
                new AddrPort("omsg106" ,2880),
                new AddrPort("omsg105" ,2868),
                new AddrPort("omsg106" ,2881),
                new AddrPort("omsg105" ,2869),
                new AddrPort("omsg106" ,2882),
            };
            var b = new[]
            {
                new AddrPort("omsg106" ,2882),
                new AddrPort("omsg101" ,2815),
            };
            var c = new[]
            {
                new AddrPort("omsg101" ,2815),
                new AddrPort("omsg102" ,2828),
                new AddrPort("omsg103" ,2841),
                new AddrPort("omsg104" ,2854),
            };

            var d = new[]
            {
                new AddrPort("omsg104" ,2854),
                new AddrPort("omsg101" ,2816),
                new AddrPort("omsg102" ,2829),
                new AddrPort("omsg103" ,2842),
                new AddrPort("omsg104" ,2855),
                new AddrPort("omsg101" ,2817),
                new AddrPort("omsg102" ,2830),
                new AddrPort("omsg103" ,2843),
            };
            var ret0 = ProviderAddrPortResolver.CreateEmptyCircle();
            var ret1 = ProviderAddrPortResolver.Concat(ret0, a);
            var ret2 = ProviderAddrPortResolver.Concat(ret1.Circle, b);
            var ret3 = ProviderAddrPortResolver.Concat(ret2.Circle, c);
            var ret4 = ProviderAddrPortResolver.Concat(ret3.Circle, d);
            Assert.IsTrue(ProviderAddrPortResolver.IsCompleted(ret4.Circle));
            Assert.IsTrue(ret1.NotResolved.Count() == 0);
            Assert.IsTrue(ret2.NotResolved.Count() == 0);
            Assert.IsTrue(ret3.NotResolved.Count() == 0);
            Assert.IsTrue(ret4.NotResolved.Count() == 0);
        }
        [TestMethod]
        public void TestMethod4()
        {
            var a = new[]
            {
                new AddrPort("omsg103" ,2843),
                new AddrPort("omsg104" ,2856),
                new AddrPort("omsg105" ,2867),
                new AddrPort("omsg106" ,2880),
                new AddrPort("omsg105" ,2868),
                new AddrPort("omsg106" ,2881),
                new AddrPort("omsg105" ,2869),
                new AddrPort("omsg106" ,2882),
            };
            var b = new[]
            {
                new AddrPort("omsg101" ,2815),
                new AddrPort("omsg102" ,2828),
                new AddrPort("omsg103" ,2841),
                new AddrPort("omsg104" ,2854),
            };
            var c = new[]
            {
                new AddrPort("omsg106" ,2882),
                new AddrPort("omsg101" ,2815),
            };            
            var d = new[]
            {
                new AddrPort("omsg104" ,2854),
                new AddrPort("omsg101" ,2816),
                new AddrPort("omsg102" ,2829),
                new AddrPort("omsg103" ,2842),
                new AddrPort("omsg104" ,2855),
                new AddrPort("omsg101" ,2817),
                new AddrPort("omsg102" ,2830),
                new AddrPort("omsg103" ,2843),
            };
            var ret0 = ProviderAddrPortResolver.CreateEmptyCircle();
            var list = new List<IEnumerable<AddrPort>>() { a, b, c, d };
            int before;
            int after;
            var tmp = new List<IEnumerable<AddrPort>>();
            do
            {
                before = list.Count;
                Console.WriteLine($"before={before}");
                foreach (var arr in list)
                {
                    var ret = ProviderAddrPortResolver.Concat(ret0, arr);
                    if (ret.NotResolved.Count() != 0)
                        tmp.Add(ret.NotResolved);
                    ret0 = ret.Circle;
                }
                list = tmp;
                after = list.Count;
                Console.WriteLine($"after={after}");
            } while (before != after);
            Console.WriteLine(ret0.ToStr());
            Assert.IsTrue(ProviderAddrPortResolver.IsCompleted(ret0));
        }
        [TestMethod]
        public void TestMethod5()
        {
            var list1 = new List<AddrPort>();
            list1.Add(new AddrPort("msg101", 2805));
            list1.Add(new AddrPort("msg101", 2806));
            list1.Add(null);
            list1.Add(new AddrPort("msg101", 2808));

            var list2 = new List<AddrPort>();
            list2.Add(new AddrPort("msg101", 2807));
            list2.Add(new AddrPort("msg101", 2808));

            var list = ProviderAddrPortResolver.OuterJoin(list1, list2).ToList();
            Assert.AreEqual(list[0], new AddrPort("msg101", 2805));
            Assert.AreEqual(list[1], new AddrPort("msg101", 2806));
            Assert.AreEqual(list[2], new AddrPort("msg101", 2807));
            Assert.AreEqual(list[3], new AddrPort("msg101", 2808));
        }
        [TestMethod]
        public void TestMethod6()
        {
            var list1 = new List<AddrPort>();
            list1.Add(new AddrPort("msg101", 2805));
            list1.Add(new AddrPort("msg101", 2806));
            list1.Add(new AddrPort("msg101", 2807));
            list1.Add(new AddrPort("msg101", 2808));

            var list2 = new List<AddrPort>();
            list2.Add(new AddrPort("msg101", 2808));
            list2.Add(new AddrPort("msg101", 2809));
            list2.Add(new AddrPort("msg101", 2810));
            list2.Add(new AddrPort("msg101", 2811));


            var list = ProviderAddrPortResolver.OuterJoin(list1, list2).ToList();
            Assert.AreEqual(list[0], new AddrPort("msg101", 2805));
            Assert.AreEqual(list[1], new AddrPort("msg101", 2806));
            Assert.AreEqual(list[2], new AddrPort("msg101", 2807));
            Assert.AreEqual(list[3], new AddrPort("msg101", 2808));
            Assert.AreEqual(list[4], new AddrPort("msg101", 2809));
            Assert.AreEqual(list[5], new AddrPort("msg101", 2810));
            Assert.AreEqual(list[6], new AddrPort("msg101", 2811));
        }
    }
}
