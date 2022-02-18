namespace Netlibs.Test {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    [TestClass]
    public class Lico {
        [TestMethod]
        public void m14() {
            // System.Console.WriteLine(System.Text.RegularExpressions.Regex.Match("aa","*a").Groups[1].Value);
            // string.IsNullOrWhiteSpace
            System.Console.WriteLine(new string("ab".Reverse().ToArray()));
            // var x=1byte;
            System.Console.WriteLine(Math.Log(100,10));
            System.Console.WriteLine(int.Parse(" 0000000000012345678"));
        }
        [TestMethod]
        public void m13() {
            var x="1";
            System.Console.WriteLine(x);
            System.Console.WriteLine(x+='2');
        }
        [TestMethod]
        public void m12() {
            var s="2ldkjdls";
            System.Console.WriteLine(s[0..s.Length]);
            System.Console.WriteLine(s[0..(s.Length-1)]);
            System.Console.WriteLine(s[0..^0]);
            System.Console.WriteLine(s[0..^1]);
            System.Console.WriteLine(s[1..^1]);
            System.Console.WriteLine("ccc"[0..2]);
            System.Console.WriteLine("ccc"[0..3]);
            System.Console.WriteLine(string.IsNullOrWhiteSpace(s[0..0])?"true":"false");
            System.Console.WriteLine("ccc".LastIndexOf('c',0,1));
            System.Console.WriteLine("ccc".IndexOf('c',0,2));
            System.Console.WriteLine("ccc".IndexOf('c',0,3));
            System.Console.WriteLine("cccbcd".IndexOf('c',0,3));
            System.Console.WriteLine("cccbcd".LastIndexOf('c',3));
            System.Console.WriteLine("cccbcd".LastIndexOf('c',0,1));
            System.Console.WriteLine("aka"==string.Concat("aka".Reverse()));
            System.Console.WriteLine("abcde"[^1]);
            System.Console.WriteLine("abcde"[1..^1]);
        }       
        [TestMethod]
        public void m11() {
            var current=default(ListNode);
            var l1 = new ListNode(3, null);
            l1 = new ListNode(4, l1);
            l1 = new ListNode(2, l1);
            var l2 = new ListNode(4, null);
            l2 = new ListNode(6, l2);
            l2 = new ListNode(5, l2);
            current=l1;
            System.Console.Write("[");
            while (current != null) {
                System.Console.Write(current.val);
                System.Console.Write(",");
                current = current.next;
            }
            System.Console.Write("]");
            current=l2;
            System.Console.Write("[");
            while (current != null) {
                System.Console.Write(current.val);
                System.Console.Write(",");
                current = current.next;
            }
            System.Console.Write("]");
            System.Console.WriteLine("----------");
            var s = new Solution();
            var x = s.AddTwoNumbers(l1, l2);
            current = x;
            System.Console.Write("[");
            while (current != null) {
                System.Console.Write(current.val);
                System.Console.Write(",");
                current = current.next;
            }
            System.Console.Write("]");
        }
    }
    // Definition for singly-linked list.
    public class ListNode {
        public int val;
        public ListNode next;
        public ListNode(int val = 0, ListNode next = null) {
            this.val = val;
            this.next = next;
        }
    }

    public class Solution {
        public ListNode AddTwoNumbers(ListNode l1, ListNode l2) {
            var result = new ListNode();
            int carry = 0;//表示进位
            var p1 = l1;
            var p2 = l1;
            var hs=new HashSet<char>();
            var s=new string(hs.ToArray());
            // hs.Remove()
            Stack<char> xx=new();
            Queue<char> xx2=new ();
            // xx2.Dequeue()
            // xx2.Enqueue()
            // var i=xx.IndexOf('k');
            // xx.Contains()
            xx.Pop();
            var current = result;
            while (p1 != null || p2 != null) {
                switch ((p1, p2, p1.next, p2.next)) {
                    case (ListNode, ListNode, ListNode, ListNode):
                    case (ListNode, ListNode, null, ListNode):
                    case (ListNode, ListNode, ListNode, null):
                    if (p1.val + p2.val + carry >= 10) {
                        current.val = (p1.val + p2.val + carry) % 10;
                        carry = 1;
                    } else {
                        current.val = p1.val + p2.val + carry;
                        carry = 0;
                    }
                    p1 = p1.next;
                    p2 = p2.next;
                    break;
                    case (ListNode, null, ListNode, null):
                    if (p1.val + carry >= 10) {
                        current.val = (p1.val + carry) % 10;
                        carry = 1;
                    } else {
                        current.val = p1.val + carry;
                        carry = 0;
                    }
                    p1 = p1.next;
                    // p2=p2.next;
                    break;
                    case (null, ListNode, null, ListNode):
                    if (p2.val + carry >= 10) {
                        current.val = (p2.val + carry) % 10;
                        carry = 1;
                    } else {
                        current.val = p2.val + carry;
                        carry = 0;
                    }
                    // p1=p1.next;
                    p2 = p2.next;
                    break;
                    case (ListNode, ListNode, null, null):
                    if (p1.val + p2.val + carry >= 10) {
                        current.val = (p1.val + p2.val + carry) % 10;
                        carry = 1;
                        current.next = new ListNode(1, null);
                    } else {
                        current.val = p1.val + p2.val + carry;
                        carry = 0;
                    }

                    p1 = p1.next;
                    p2 = p2.next;
                    continue;
                    case (ListNode, null, null, null):
                    if (p1.val + carry >= 10) {
                        current.val = (p1.val + carry) % 10;
                        carry = 1;
                        current.next = new ListNode(1, null);
                    } else {
                        current.val = p1.val + carry;
                        carry = 0;
                    }
                    p1 = p1.next;
                    // p2=p2.next;
                    continue;
                    case (null, ListNode, null, null):
                    if (p2.val + carry >= 10) {
                        current.val = (p2.val + carry) % 10;
                        carry = 1;
                        current.next = new ListNode(1, null);
                    } else {
                        current.val = p2.val + carry;
                        carry = 0;
                    }
                    // p1=p1.next;
                    p2 = p2.next;
                    continue;
                }
                current.next = new ListNode();
                current = current.next;
            }
            return result;
        }
    }
}