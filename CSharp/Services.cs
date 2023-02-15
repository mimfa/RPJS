using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace MiMFa.RP.CSharp { 
    public class Service
    {
        public static Dictionary<long, Timer> ProcessTimers = new Dictionary<long, Timer>();
        public static void Run<TOutput>(int milisecondDelay, Func<TOutput> action, bool background = true)
        {
            var l = DateTime.UtcNow.Ticks;
            ProcessTimers.Add(l, new Timer((s) =>
            {
                try
                {
                    var ll = (long)s;
                    Run(action, background);
                    ProcessTimers[ll].Dispose();
                    ProcessTimers.Remove(ll);
                }
                catch { }
            }, l, milisecondDelay, milisecondDelay));
        }
        public static void Run(int milisecondDelay, Action action, bool background = true)
        {
            var l = DateTime.UtcNow.Ticks;
            ProcessTimers.Add(l, new Timer((s) =>
            {
                try
                {
                    var ll = (long)s;
                    Run(action, background);
                    ProcessTimers[ll].Dispose();
                    ProcessTimers.Remove(ll);
                }
                catch { }
            }, l, milisecondDelay, milisecondDelay));
        }
        public static Thread Run<TOutput>(Func<TOutput> action, bool background = true)
        {
            var th = new Thread(() => action());
            th.IsBackground = background;
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
            return th;
        }
        public static Thread Run(Action action, bool background = true)
        {
            return RunThread(new ThreadStart(action), background);
        }
        public static Thread RunThread(ThreadStart action, bool background = true)
        {
            var th = new Thread(action);
            th.IsBackground = background;
            th.SetApartmentState(ApartmentState.STA);
            th.Start();
            return th;
        }
        public static Task RunTask(Action action, TaskCreationOptions taskCreationOptions = TaskCreationOptions.LongRunning)
        {
            var t = new Task(action, taskCreationOptions);
            t.RunSynchronously();
            return t;
        }
        public static Task RunTask<InputT>(Action<InputT> task, InputT arg = default(InputT), TaskCreationOptions taskCreationOptions = TaskCreationOptions.LongRunning)
        {
            var t = new Task(() => task(arg), taskCreationOptions);
            t.RunSynchronously();
            return t;
        }
        public static Task<OutputT> RunTask<InputT, OutputT>(Func<InputT, OutputT> task, InputT arg = default(InputT))
        {
            return Task.Run(() => task(arg));
        }
        public static OutputT TaskHandler<OutputT>(Task<OutputT> task, int secondsLimit = 2)
        {
            try
            {
                task.Wait(new TimeSpan(0, 0, secondsLimit));
                return task.Result;
            }
            catch { return default(OutputT); }
        }
        public static OutputT TaskHandler<OutputT>(Task<OutputT> task, int secondsLimit, OutputT defaultVal)
        {
            return TaskHandler(task, secondsLimit);
        }
        public static OutputT TaskHandler<InputT, OutputT>(Task<InputT> task, int secondsLimit, InputT defaultVal, Func<InputT, OutputT> convertor)
        {
            return convertor(TaskHandler(task, secondsLimit, defaultVal));
        }
 
        public static string ToHotKeys(string text)
        {
            var sb = new System.Text.StringBuilder();
            foreach (var c in text)
            {
                switch (c)
                {
                    case '+':
                    case '^':
                    case '%':
                    case '~':
                    case '(':
                    case ')':
                    case '[':
                    case ']':
                    case '{':
                    case '}':
                        sb.Append('{');
                        sb.Append(c);
                        sb.Append('}');
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            return sb.ToString();
        }
        public static string ToConcatedName(string name, bool justASCIIAlfabet = true)
        {
            if (name == null) return "";
            if (justASCIIAlfabet)
            {
                string nn = "";
                foreach (var item in name)
                    if (char.IsLetterOrDigit(item))
                        nn += item.ToString();
                    else nn += " ";
                name = nn;
            }
            string[] stra = name.Split(new string[] { " " }, StringSplitOptions.RemoveEmptyEntries);
            return ToConcatedName(stra);
        }
        public static string ToConcatedName(params string[] parts) => parts[0] + string.Join("", from v in parts.Skip(1) select CapitalFirstLetter(v));
        public static object CapitalFirstLetter(string word) => word[0].ToString().ToUpper() + string.Join("", word.Skip(1)).ToLower();


        public static IEnumerable<TOut> Loop<TIn, TOut>(IEnumerable<TIn> collection, Func<TIn, TOut> action)
        {
            foreach (var item in collection)
                yield return action(item);
        }
        public static void Loop<T>(IEnumerable<T> collection, Action<T> action)
        {
            foreach (var item in collection)
                action(item);
        }
        public static IEnumerable<TOut> Loop<TOut>(long length, Func<TOut> action)
        {
            long index = 0;
            while (index++ < length)
                yield return action();
        }
        public static void Loop(long length, Action action)
        {
            long index = 0;
            while (index++ < length)
                action();
        }
        public static void LimitedLoop(Func<bool> condition, long length, Action action)
        {
            long index = 0;
            while (index++ < length && condition())
                action();
        }
        public static IEnumerable<T> Loop<T>(int index, int length, Func<int, T> action)
        {
            while (index < length) yield return action(index++);
        }
        public static IEnumerable<T> Loop<T>(long index, long length, Func<long, T> action)
        {
            while (index < length) yield return action(index++);
        }
        public static IEnumerable<T> LimitedLoop<T>(Func<int, bool> condition, int index, int length, Func<int, T> action)
        {
            while (index < length && condition(index)) yield return action(index++);
        }
        public static IEnumerable<T> LimitedLoop<T>(Func<long, bool> condition, long index, long length, Func<long, T> action)
        {
            while (index < length && condition(index)) yield return action(index++);
        }
    }
}