namespace Fone;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Timers;
using Microsoft.Data.Sqlite;

public class Sqlitefo {
    Sqlitefo() {
        this.resultset = new Dictionary<string, List<DbSerializable>>();
        this.tableNames = new List<string>();
    }
    SqliteType Convert(Type t) => Type.GetTypeCode(t) switch {
        TypeCode.Boolean => SqliteType.Integer,
        TypeCode.Byte => SqliteType.Integer,
        TypeCode.Char => SqliteType.Text,
        TypeCode.DateTime => SqliteType.Text,
        TypeCode.DBNull => SqliteType.Text,
        TypeCode.Decimal => SqliteType.Real,
        TypeCode.Double => SqliteType.Real,
        TypeCode.Int16 => SqliteType.Integer,
        TypeCode.Int32 => SqliteType.Integer,
        TypeCode.Int64 => SqliteType.Integer,
        TypeCode.SByte => SqliteType.Integer,
        TypeCode.Single => SqliteType.Real,
        TypeCode.String => SqliteType.Text,
        TypeCode.UInt16 => SqliteType.Integer,
        TypeCode.UInt32 => SqliteType.Integer,
        TypeCode.UInt64 => SqliteType.Integer,
    };
    public Sqlitefo Insert<T>(params T[] pocoEntitys) where T : DbSerializable {
        var cmd = dbcon.CreateCommand();
        var sb = new StringBuilder();
        foreach (var i in pocoEntitys) {
            sb.AppendLine($"{i.ToInsert()}");
        }
        cmd.CommandText = sb.ToString();
        cmdInserts.Add(cmd);
        return this;
    }
    public Sqlitefo Update<T>(params T[] pocoEntitys) where T : DbSerializable {
        var cmd = dbcon.CreateCommand();
        var sb = new StringBuilder();
        foreach (var i in pocoEntitys) {
            sb.AppendLine(i.ToUpdate());
        }
        cmd.CommandText = sb.ToString();
        cmdUpdates.Add(cmd);
        return this;
    }
    public Sqlitefo Remove(string table, params int[] ids) {
        var cmd = dbcon.CreateCommand();
        var sb = new StringBuilder();
        sb.AppendLine($"delete from {table} where Id in({string.Join(",", ids)})");
        cmd.CommandText = sb.ToString();
        cmdRemoves.Add(cmd);
        return this;
    }
    public void Query<T>(string query = null) where T : DbSerializable {
        var t = typeof(T);
        var cmd = dbcon.CreateCommand();
        if (query == null) {
            cmd.CommandText = $"select * from {t.Name}";
        } else {
            cmd.CommandText = $"select * from {t.Name} where {query}";
        }
        var list = new List<DbSerializable>();
        var pis = t.GetProperties();
        try {
            dbcon.Open();
            var reader = cmd.ExecuteReader();
            var dic = new Dictionary<string, object>();
            while (reader.Read()) {
                var ins = Activator.CreateInstance<T>();
                dic.Clear();
                foreach (var i in pis) {
                    dic.Add(i.Name, reader[i.Name]);
                }
                ins.SetQuery(dic);
                list.Add(ins);
            }
            this.resultset.Add(t.Name, list);
        } finally {
            dbcon.Close();
        }
    }
    string dbconstr;
    SqliteConnection dbcon;
    List<SqliteCommand> cmdInserts;
    List<SqliteCommand> cmdUpdates;
    List<SqliteCommand> cmdRemoves;
    List<SqliteCommand> cmdTable;
    public List<string> tableNames;
    public Dictionary<string, List<DbSerializable>> resultset;
    public Sqlitefo CreateTable<T>() where T : DbSerializable {
        var cmd = dbcon.CreateCommand();
        var ins = Activator.CreateInstance<T>();
        cmd.CommandText = ins.ToTableSchema();
        cmdTable.Add(cmd);
        return this;
    }
    public Sqlitefo RemoveTable<T>() {
        var cmd = dbcon.CreateCommand();
        var t = typeof(T);
        var pis = t.GetProperties();
        var sb = new StringBuilder();
        sb.AppendLine($"drop table {t.Name}");
        cmd.CommandText = sb.ToString();
        cmdTable.Add(cmd);
        return this;
    }
    public void QueryTable() {
        var cmd = dbcon.CreateCommand();
        cmd.CommandText = $"SELECT name FROM sqlite_master WHERE type='table' ORDER BY name";
        var list = new List<DbSerializable>();
        try {
            dbcon.Open();
            var reader = cmd.ExecuteReader();
            while (reader.Read()) {
                this.tableNames.Add(reader["name"].ToString());
            }
        } finally {
            dbcon.Close();
        }
    }
    public void Detach() {
        dbcon.Open();
        try {
            foreach (var i in cmdInserts) {
                // System.Console.WriteLine(i.CommandText);
                i.ExecuteNonQuery();
            }
            foreach (var i in cmdUpdates) {
                // System.Console.WriteLine(i.CommandText);
                i.ExecuteNonQuery();
            }
            foreach (var i in cmdRemoves) {
                // System.Console.WriteLine(i.CommandText);
                i.ExecuteNonQuery();
            }
            foreach (var i in cmdTable) {
                // System.Console.WriteLine(i.CommandText);
                i.ExecuteReader();
            }
        } catch (System.Exception e) {
            System.Console.WriteLine(e.Message);
        } finally {
            dbcon.Close();
        }
    }
    static public Sqlitefo Attach(string dbpath) => Core.Attach($"Data Source = {dbpath};");
    class Core {
        static public Sqlitefo Attach(string dbconstr) {
            Sqlitefo sfo = new();
            sfo.dbcon = new SqliteConnection(dbconstr);
            sfo.cmdInserts = new List<SqliteCommand>();
            sfo.cmdUpdates = new List<SqliteCommand>();
            sfo.cmdRemoves = new List<SqliteCommand>();
            sfo.cmdTable = new List<SqliteCommand>();
            return sfo;
        }
    }
}
