using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Fone {
    /// <summary>
    /// 自定义日志提供程序
    /// </summary>
    public class FileLogProvider : ILoggerProvider {
        string root;
        public FileLogProvider() {
            root = "./logs";
            this.logq = new ConcurrentQueue<(DateTime date, string conent)>();
            this.timer = new System.Timers.Timer(10 * 1000);
            this.timer.Elapsed += WriteToFile;
            this.timer.Start();
        }
        public FileLogProvider(string rootDir) : this() {
            if (!string.IsNullOrWhiteSpace(rootDir))
                root = rootDir;
        }
        /// <summary>
        /// 自定义日志实现
        /// </summary>
        /// <param name="categoryName"></param>
        /// <returns></returns>
        public ILogger CreateLogger(string categoryName) {
            return new Log(this, categoryName);
        }
        internal ConcurrentQueue<(DateTime date, string conent)> logq;
        System.Timers.Timer timer;
        string GetPath() {
            var r = string.Empty;
            var date = DateTime.Now;
            var falder = $"_{date.ToString("yyyyMM")}";
            var file = $"_{date.ToString("dd")}.txt";
            var falderDir = System.IO.Path.Combine(this.root, falder);
            if (!Directory.Exists(falderDir)) {
                Directory.CreateDirectory(falderDir);
            }
            r = System.IO.Path.Combine(this.root, falder, file);
            return r;
        }
        private void WriteToFile(object sender, System.Timers.ElapsedEventArgs e) {
            if (this.logq.Count < 1) {
                return;
            }
            using (var sw = new StreamWriter(File.Open(this.GetPath(), FileMode.Append, FileAccess.Write))) {
                var sb = new StringBuilder();
                while (!this.logq.IsEmpty) {
                    this.logq.TryDequeue(out var result);
                    sb.AppendLine($"{result.date}>>{result.conent }");
                }
                sw.Write(sb.ToString());
            }
        }
        public void WriteToLog(string format) {
            this.logq.Enqueue((DateTime.Now, format));
        }
        public void Dispose() {
            this.timer.Dispose();
            this.logq.Clear();
            this.logq = null;
        }

    }
    public class Log : ILogger {
        private readonly FileLogProvider flp;
        private readonly string categoryName;

        public Log(FileLogProvider p, string categoryName) {
            this.flp = p;
            this.categoryName = categoryName;
        }
        public IDisposable BeginScope<TState>(TState state) {
            return null;
        }

        public bool IsEnabled(LogLevel logLevel) {
            var r = false;
            switch (logLevel) {
                case LogLevel.Trace:
                r = true;
                break;
                case LogLevel.Debug:
                break;
                case LogLevel.Information:
                case LogLevel.Warning:
                case LogLevel.Error:
                r = true;
                break;
                case LogLevel.Critical:
                break;
                case LogLevel.None:
                r = true;
                break;
            }
            return r;
        }

        void ILogger.Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter) {
            if (!IsEnabled(logLevel)) {
                return;
            }
            this.flp.WriteToLog(formatter.Invoke(state, exception));
            //Console.WriteLine($"{formatter(state, exception)}");
        }
    }
    public class DbSerializable {
        public virtual string ToInsert() => null;
        public virtual string ToUpdate() => null;
        public virtual string ToTableSchema() => null;
        public virtual void SetQuery(IDictionary<string, object> kvs){}
    }
    //log model
    public class SimpleLog : DbSerializable {
        public int Id { get; set; }
        public string Message { get; set; }
        public string Note { get; set; }
        public DateTime OnTime { get; set; }
        public override string ToInsert() {
            return $"insert into {nameof(SimpleLog)}({nameof(Id)},{nameof(Message)},{nameof(Note)},{nameof(OnTime)}) values(NULL,'{Message}','{Note}','{OnTime}')";
        }
        public override string ToUpdate() {
            return $"update {nameof(SimpleLog)} set {nameof(Message)}='{Message}',{nameof(Note)}='{Note}',{nameof(OnTime)}='{OnTime}' where {nameof(Id)}={Id}";
        }
        public override string ToTableSchema() {
            var sb = new StringBuilder();
            sb.AppendLine($"create table {nameof(SimpleLog)}");
            sb.AppendLine($"(");
            sb.AppendLine($"{nameof(Id)} {SqliteType.Integer} primary key,");
            sb.AppendLine($"{nameof(Message)} {SqliteType.Text},");
            sb.AppendLine($"{nameof(Note)} {SqliteType.Text},");
            sb.AppendLine($"{nameof(OnTime)} {SqliteType.Text}");
            sb.AppendLine($")");
            return sb.ToString();
        }
        public override string ToString() {
            return $"{nameof(Id)}:{Id},{nameof(Message)}:{Message},{nameof(Note)}:{Note},{nameof(OnTime)}:{OnTime}";
        }
        public override void SetQuery(IDictionary<string, object> kvs) {
            foreach(var i in kvs){
                switch(i.Key){
                    case nameof(Id):Id=int.Parse(i.Value.ToString()); break;
                    case nameof(Message):Message=(string)i.Value;break;
                    case nameof(Note):Note=(string)i.Value;break;
                    case nameof(OnTime):OnTime=DateTime.Parse((string)i.Value);break;
                }
            }
        }
    }
}
