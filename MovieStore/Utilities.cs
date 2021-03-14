using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using NLog;

namespace MovieStore
{
    public class Utilities
    {
        public const string ListSeparator = " & ";

        public const string InvisibleSeparator = "\u001E";

        public static readonly string[] MovieFeaturedJobs = { "Director", "Novel", "Screenplay", "Screenstory", "Story", "Writer" };

        public static readonly string[] TvShowFeaturedJobs = { "Creator", "Novel", "Characters" };
    }

    public static class Extensions
    {
        public static string ComputeHash(this string text)
        {
            using (MD5 md5 = MD5.Create())
            {
                StringBuilder builder = new StringBuilder();
                byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(text));
                for (int i = 0; i < bytes.Length; i++)
                    builder.Append(bytes[i].ToString("x2"));
                
                return builder.ToString();
            }
        }

        public static string Join(this IEnumerable<string> collection)
        {
            if (collection == null)
                return String.Empty;

            return String.Join(Utilities.ListSeparator, collection.Select(x => String.Format("{1}{0}{1}", x, Utilities.InvisibleSeparator)));
        }

        public static IEnumerable<string> SplitByListSeparator(this string joinedString)
        {
            if (joinedString == null)
                return new string[0];

            return joinedString.Split(new string[] { Utilities.ListSeparator }, StringSplitOptions.RemoveEmptyEntries);
        }

        public static string FirstOfJoinedByListSeparator(this string collection)
        {
            int listSeparatorIndex = collection.IndexOf(Utilities.ListSeparator);
            if (listSeparatorIndex < 0)
                return collection;
            else
                return collection.Remove(listSeparatorIndex);
        }

        public static void LogIfFaulted(this Task task)
        {
            task.ContinueWith(t => { Journal.WriteError(t.Exception); }, TaskContinuationOptions.OnlyOnFaulted);
        }

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if (collection != null && items != null)
            {
                foreach (T item in items)
                    collection.Add(item);
            }
        }

        public static void RemoveRange<T>(this ICollection<T> collection, IEnumerable<T> items)
        {
            if (collection != null && items != null)
            {
                List<T> itemList = items.ToList();

                foreach (T item in itemList)
                    collection.Remove(item);
            }
        }

        public static void RemoveRange<T>(this ICollection<T> collection, Func<T, bool> match)
        {
            if (collection != null)
            {
                List<T> itemList = collection.Where(match).ToList();

                foreach (T item in itemList)
                    collection.Remove(item);
            }
        }
    }

    public class Journal
    {
        public static void WriteInformation(string message)
        {
            logger.Info(message);
            LogManager.Flush();
        }

        public static void WriteWarning(string message)
        {
            logger.Warn(message);
            LogManager.Flush();
        }

        public static void WriteError(string message, bool fatal = false)
        {
            if (fatal)
                logger.Fatal(message);
            else
                logger.Error(message);

            LogManager.Flush();
        }

        public static void WriteError(Exception ex, bool fatal = false)
        {
            if (fatal)
                logger.Fatal(ex);
            else
                logger.Error(ex);

            LogManager.Flush();
        }

        public static void Shutdown()
        {
            LogManager.Shutdown();
        }

        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
    }
}
