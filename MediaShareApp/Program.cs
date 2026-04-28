using System;
using System.Linq;
using System.Windows.Forms;
using MediaShareApp;

namespace MediaShareApp.WinForms
{
    internal static class Program
    {
        // ?????? ????????? ??????? ????? ?? ????? ????????.
        internal static readonly MediaService Service = new MediaService();

        [STAThread]
        private static void Main(string[] args)
        {
            if (args.Any(a => string.Equals(a, "--cli", StringComparison.OrdinalIgnoreCase)))
            {
                RunCli();
                return;
            }

            ApplicationConfiguration.Initialize();
            Application.Run(new LoginForm());
        }

        private static void RunCli()
        {
            var service = new MediaService();
            string? currentUser = null;

            Console.WriteLine("MediaShareApp CLI");
            PrintHelp();

            while (true)
            {
                Console.Write(currentUser is null ? "> " : $"{currentUser}> ");
                var input = Console.ReadLine();
                if (string.IsNullOrWhiteSpace(input))
                {
                    continue;
                }

                var parts = input.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                var command = parts[0].ToLowerInvariant();

                try
                {
                    switch (command)
                    {
                        case "help":
                            PrintHelp();
                            break;
                        case "exit":
                            return;
                        case "register":
                            RequireArgCount(parts, 3, "register <username> <password>");
                            service.Register(parts[1], parts[2]);
                            Console.WriteLine("???????????? ???????????????.");
                            break;
                        case "login":
                            RequireArgCount(parts, 3, "login <username> <password>");
                            service.Login(parts[1], parts[2]);
                            currentUser = parts[1];
                            Console.WriteLine($"???? ????????: {currentUser}");
                            break;
                        case "create-album":
                            RequireLoggedIn(currentUser);
                            RequireArgCount(parts, 2, "create-album <title>");
                            var created = service.CreateAlbum(currentUser!, parts[1]);
                            Console.WriteLine($"?????? ??????. Id={created.Id}");
                            break;
                        case "add-file":
                            RequireLoggedIn(currentUser);
                            RequireArgCount(parts, 4, "add-file <albumId> <name> <photo|video>");
                            var album = service.GetAlbum(ParseInt(parts[1], "albumId"));
                            var type = ParseMediaType(parts[3]);
                            var file = album.AddFile(currentUser!, parts[2], type);
                            Console.WriteLine($"???? ????????. Id={file.Id}");
                            break;
                        case "share":
                            RequireLoggedIn(currentUser);
                            RequireArgCount(parts, 4, "share <albumId> <username> <view|add|delete>");
                            service.Share(currentUser!, ParseInt(parts[1], "albumId"), parts[2], ParsePermission(parts[3]));
                            Console.WriteLine("????? ??????.");
                            break;
                        case "browse":
                            RequireLoggedIn(currentUser);
                            RequireArgCount(parts, 2, "browse <albumId>");
                            var browsingAlbum = service.GetAlbum(ParseInt(parts[1], "albumId"));
                            var files = browsingAlbum.Browse(currentUser!);
                            Console.WriteLine($"??????: {files.Count}");
                            foreach (var f in files)
                            {
                                Console.WriteLine($"- [{f.Type}] #{f.Id} {f.Name} (owner: {f.Owner})");
                            }

                            break;
                        case "load-test":
                            RequireArgCount(parts, 3, "load-test <register|mixed|shared> <users>");
                            RunLoadTest(parts[1], ParseInt(parts[2], "users"));
                            break;
                        default:
                            Console.WriteLine("??????????? ???????. ??????? 'help' ??? ?????? ??????.");
                            break;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"??????: {ex.Message}");
                }
            }
        }

        private static void RunLoadTest(string scenario, int userCount)
        {
            if (userCount <= 0)
            {
                throw new ArgumentException("?????????? ????????????? ?????? ???? > 0.");
            }

            LoadTestResult result = scenario.ToLowerInvariant() switch
            {
                "register" => LoadScenarioRunner.RegisterAndCreateAlbum(userCount),
                "mixed" => LoadScenarioRunner.MixedOperations(userCount),
                "shared" => LoadScenarioRunner.SharedAlbumConcurrentUploads(userCount),
                _ => throw new ArgumentException("????????: register, mixed ??? shared."),
            };

            Console.WriteLine($"????????: {result.ScenarioName}");
            Console.WriteLine($"?????????????: {result.Users}");
            Console.WriteLine($"?????: {result.ElapsedMilliseconds:F3} ??");
            Console.WriteLine($"??????: {result.ErrorsCount}");
            if (result.TotalOperations > 0)
            {
                Console.WriteLine($"????????: {result.TotalOperations}");
                Console.WriteLine($"????????/???: {result.TotalOperations * 1000.0 / Math.Max(0.001, result.ElapsedMilliseconds):F0}");
            }

            if (result.ErrorsCount > 0)
            {
                Console.WriteLine("?????? ??????: " + result.Errors.First());
            }
        }

        private static void PrintHelp()
        {
            Console.WriteLine("???????:");
            Console.WriteLine("  register <username> <password>");
            Console.WriteLine("  login <username> <password>");
            Console.WriteLine("  create-album <title>");
            Console.WriteLine("  add-file <albumId> <name> <photo|video>");
            Console.WriteLine("  share <albumId> <username> <view|add|delete>");
            Console.WriteLine("  browse <albumId>");
            Console.WriteLine("  load-test <register|mixed|shared> <users>");
            Console.WriteLine("  help");
            Console.WriteLine("  exit");
        }

        private static int ParseInt(string value, string argName)
        {
            if (!int.TryParse(value, out var parsed))
            {
                throw new ArgumentException($"???????? '{argName}' ?????? ???? ??????.");
            }

            return parsed;
        }

        private static MediaType ParseMediaType(string value) =>
            value.ToLowerInvariant() switch
            {
                "photo" => MediaType.Photo,
                "video" => MediaType.Video,
                _ => throw new ArgumentException("??? ?????: photo ??? video."),
            };

        private static Permission ParsePermission(string value) =>
            value.ToLowerInvariant() switch
            {
                "view" => Permission.View,
                "add" => Permission.Add,
                "delete" => Permission.Delete,
                _ => throw new ArgumentException("?????: view, add ??? delete."),
            };

        private static void RequireArgCount(string[] parts, int expected, string usage)
        {
            if (parts.Length != expected)
            {
                throw new ArgumentException($"?????????????: {usage}");
            }
        }

        private static void RequireLoggedIn(string? currentUser)
        {
            if (string.IsNullOrWhiteSpace(currentUser))
            {
                throw new InvalidOperationException("??????? ????????? login.");
            }
        }
    }
}
