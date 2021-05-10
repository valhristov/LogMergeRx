using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using LogMergeRx.Model;

namespace LogMergeRx.Demo
{
    public class LogGenerator
    {
        private static string[] texts = new[]
        {
            "Lorem ipsum dolor sit amet, consectetur adipiscing elit.",
            "In ac dapibus lorem.",
            "Etiam ullamcorper scelerisque congue.",
            "Phasellus orci enim, aliquam sit amet ipsum malesuada, faucibus mollis augue.",
            "Quisque dapibus eros sed pretium viverra.",
            "Nullam eget auctor orci.",
            "Praesent cursus purus arcu, non ultrices nibh vehicula sed.",
            "Aliquam a nibh est.",
            "Maecenas quam libero, tempus id nunc ac, aliquet tincidunt felis.",
            "Nam mollis dui ac aliquet laoreet.",
            "Quisque viverra pretium mi, ac rutrum quam sollicitudin non.",
            "Donec tempor sem vitae facilisis aliquet.",
            "Donec euismod eros vel lacus luctus mollis.",
            "Maecenas feugiat erat id felis sodales, id finibus magna viverra.",
            "Sed quis arcu turpis.",
            "Curabitur quis condimentum eros.",
            "Pellentesque commodo ultricies tincidunt.",
            "Nullam fermentum, libero eu tristique molestie, sapien tellus efficitur magna, scelerisque tempor felis justo id turpis.",
            "Integer aliquet rutrum tortor a sodales.",
            "Aliquam ullamcorper velit at lorem rhoncus pharetra.",
            "Ut sed malesuada quam, eu tincidunt libero.",
            "Duis interdum mi nec lorem suscipit faucibus.Nunc nec risus laoreet, ornare purus quis, blandit risus.",
            "Sed leo arcu, molestie quis laoreet nec, tristique ut diam.",
            "Mauris ac ipsum iaculis, elementum enim at, lacinia sapien.",
            "Aenean hendrerit, eros in pharetra consequat, felis elit accumsan massa, id porttitor eros sem nec mi.",
        };

        private static string[] levels = new[] { "ERROR", "WARN", "NOTICE", "INFO" };

        private static string[] sources = new[] { "Source 1", "Source 2", "Source 3", "Source 4" };

        private static RelativePath[] logFiles = new[]
        {
            RelativePath.FromPath("file 1.csv"),
            RelativePath.FromPath("file 2.csv"),
            RelativePath.FromPath("file 3.csv"),
        };

        private static TimeSpan[] delays = new[]
        {
            TimeSpan.FromMilliseconds(100),
            TimeSpan.FromMilliseconds(200),
            TimeSpan.FromMilliseconds(500),
            TimeSpan.FromSeconds(1),
        };

        private static Random random = new Random();

        private static T RandomItem<T>(T[] items) =>
            items[random.Next(0, items.Length)];

        private static string GetLog() =>
            $"\"{DateTime.Now:yyyy-MM-dd HH:mm:ss,fff}\";\"\";\"{RandomItem(levels)}\";\"{RandomItem(sources)}\";\"{RandomItem(texts)}\"";

        public static Task Start(AbsolutePath path, CancellationToken token) =>
            Task.Run(async () =>
            {
                while (!token.IsCancellationRequested)
                {
                    File.AppendAllLines(RandomItem(logFiles).ToAbsolute(path), new[] { GetLog() });
                    await Task.Delay(RandomItem(delays));
                }
            });
    }
}
