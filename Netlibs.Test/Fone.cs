﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Data;

namespace Netlibs.Test {
    [Microsoft.VisualStudio.TestTools.UnitTesting.TestClass]
    public class FoneTest {
        /// <summary>
        /// 探索转
        /// </summary>
        [TestMethod]
        public void m12() {
        }
        /// <summary>
        /// 考查，Func<IEnumerable<Obj>>,DataTable> GetTable<Obj>();
        /// </summary>
        [TestMethod]
        public void m11() {
            //DataTable的相关的了解
            var dt = new DataTable();
            //嵌套的三元运算 牛叉到五体投地
            var obj = dt.Compute("iif(1000=5,1000,iif(100>100,4001,2000))", null);
            //Response.Write(obj);
            Console.WriteLine(obj);

            var table = new DataTable();
            //计算常量，可以没有初始化列
            var test = table.Compute("1+1", "");
            Console.WriteLine(test);

            var a = "123";
            System.Double b = 123;
            var c = 123m;
            Console.WriteLine(Convert.ToDecimal(a));
            //test=2;

            test = table.Compute("1+1", "false");
            Console.WriteLine(test);
            //test=2;常数计算和filter无关

            test = table.Compute("abs(1)", "");
            Console.WriteLine(test);
            //test=null，不知道为这个什么没有报错，而且返回null,其他的数学函数都会抱错

            test = table.Compute("2%2", "");
            Console.WriteLine(test);
            //test=0;
            //其他函数参考下面的计算列



            //初始化datatale
            table.Columns.Add("id", typeof(string));
            table.Columns.Add("value", typeof(int));
            for (var i = 1; i <= 10; i++) {
                var dRow = table.NewRow();
                dRow["id"] = "id" + i.ToString();
                dRow["value"] = i;
                table.Rows.Add(dRow);
            }



            //test = table.Compute("value+1", "true");
            /**/
            ////抛出异常，这里必须是聚合函数



            //*************************************支持的聚合函数**********************//

            //求数量
            test = table.Compute("count(id)", "false");
            Console.WriteLine(test);
            //test=0;

            test = table.Compute("count(id)", "true");
            Console.WriteLine(test);
            //test=10;



            //求和
            test = table.Compute("sum(value)", "");
            Console.WriteLine(test);
            //test=55;

            //test = table.Compute("sum(id)","");
            /**/
            ////抛出异常，这里不能是string


            //平均
            test = table.Compute("avg(value)", "");
            Console.WriteLine(test);
            //test=5;


            //最小
            test = table.Compute("min(value)", "");
            Console.WriteLine(test);
            //test=1;

            //最大
            test = table.Compute("max(value)", "");
            Console.WriteLine(test);
            //test=10;

            //统计标准偏差
            test = table.Compute("StDev(value)", "");
            Console.WriteLine(test);
            //test=3.02765035409749

            //统计方差
            test = table.Compute("Var(value)", "");
            Console.WriteLine(test);
            //test=9.16666666666667


            //复杂计算
            test = table.Compute("max(value)/sum(value)", "");
            Console.WriteLine(test);
            //test=0.181818181818182

            /**/
            /*******************************************计算列*************************/

            var column = new DataColumn("exp1", typeof(float));
            table.Columns.Add(column);


            //简单计算
            column.Expression = "value*2";
            test = table.Select("id='id1'")[0]["exp1"];
            Console.WriteLine(test);
            //test=2;

            //字符串函数
            column.Expression = "len(id)";
            test = table.Select("id='id1'")[0]["exp1"];
            Console.WriteLine(test);
            //test=3;

            //字符串函数
            column.Expression = "len(' '+id+' ')";
            test = table.Select("id='id1'")[0]["exp1"];
            Console.WriteLine(test);
            //test=5;

            //字符串函数
            column.Expression = "len(trim(' '+id+' '))";
            test = table.Select("id='id1'")[0]["exp1"];
            Console.WriteLine(test);
            //test=3;

            //字符串函数
            column.Expression = "substring(id,3,len(id)-2)";
            test = table.Select("id='id1'")[0]["exp1"];
            Console.WriteLine(test);
            //test=1; //substring的起始字符位置为1不是0

            //类型转换
            column.Expression = "convert(substring(id,3,len(id)-2),'System.Int32')*1.6";
            test = table.Select("id='id1'")[0]["exp1"];
            Console.WriteLine(test);
            //test=1.6;

            //相当于sqlserver的isnull
            column.Expression = "isnull(value,10)";
            test = table.Select("id='id1'")[0]["exp1"];
            Console.WriteLine(test);
            //test=1;

            //三元运算符,相当于sqlserver的case when
            column.Expression = "iif(value>5,1000,2000)";
            test = table.Select("id='id1'")[0]["exp1"];
            Console.WriteLine(test);
            //test=2000;

            //like运算符
            column.Expression = "iif(id like '%1',1000,2000)";
            test = table.Select("id='id1'")[0]["exp1"];
            Console.WriteLine(test);
            //test=1000;

            //in运算符
            column.Expression = "iif(id not in('id1'),1000,2000)";
            test = table.Select("id='id1'")[0]["exp1"];
            Console.WriteLine(test);
            //test=2000;

            //嵌套的三元运算
            column.Expression = "iif(value>5,1000,iif(id like '%1',4000,2000))";
            test = table.Select("id='id1'")[0]["exp1"];
            Console.WriteLine(test);
            //test=4000;


            //客户端计算所占总数的百分比
            column.Expression = "value/sum(value)";
            test = table.Select("id='id1'")[0]["exp1"];
            Console.WriteLine(test);
            //test=0.01818182


            //客户端计算差值,比如nba常规赛的胜场差
            column.Expression = "max(value)-value";
            test = table.Select("id='id1'")[0]["exp1"];
            Console.WriteLine(test);
            //test=9


            //***********************父子表计算*************************************/


            //初始化子表,父子表关系
            var tableChild = new DataTable();

            tableChild.Columns.Add("id", typeof(string));
            tableChild.Columns.Add("value", typeof(int));

            var ds = new DataSet();
            ds.Tables.Add(tableChild);
            ds.Tables.Add(table);
            var relation = new DataRelation("relation", table.Columns["id"], tableChild.Columns["id"]);
            ds.Relations.Add(relation);

            for (var i = 1; i <= 10; i++) {
                var dRow = tableChild.NewRow();
                dRow["id"] = "id1";
                dRow["value"] = i;
                tableChild.Rows.Add(dRow);
            }


            //计算子表记录数
            column.Expression = "count(child(relation).value)";
            test = table.Select("id='id1'")[0]["exp1"];
            Console.WriteLine(test);
            //test=10;



            //计算父子表的百分比
            column.Expression = "value/sum(child(relation).value)";
            test = table.Select("id='id1'")[0]["exp1"];
            Console.WriteLine(test);
            //test=0.01818182;


            //计算父子表的差值,比如父表为库存数量，子表为订购数量，计算得出需要补充的数量
            column.Expression = "iif(value-sum(child(relation).value)>0,0,value-sum(child(relation).value))";
            test = table.Select("id='id1'")[0]["exp1"];
            Console.WriteLine(test);
            //test=-54;

            //比较遗憾的是没有发现能够计算同比和环比的方法，而且计算列无法作为约束
            //结束，DataTable可以让你尽量发挥聪明才智来减少繁杂的sql语句并且减轻服务器计算符合
        }
    }
}
