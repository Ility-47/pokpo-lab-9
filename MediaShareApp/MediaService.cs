using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace MediaShareApp
{
    public enum MediaType
    {
        Photo,
        Video,
    }

    public enum Permission
    {
        View,
        Add,
        Delete,
    }

    public class User
    {
        public int Id { get; }

        public string Username { get; }

        private string passwordHash;

        public User(int id, string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username))
            {
                throw new ArgumentException("Имя пользователя не может быть пустым.");
            }

            if (string.IsNullOrWhiteSpace(password) || password.Length < 6)
            {
                throw new ArgumentException("Пароль должен быть не менее 6 символов.");
            }

            this.Id = id;
            this.Username = username;
            this.passwordHash = Hash(password);
        }

        public static string Hash(string s)
        {
            using var sha = SHA256.Create();
            return Convert.ToBase64String(sha.ComputeHash(Encoding.UTF8.GetBytes(s)));
        }

        public bool CheckPassword(string password) => this.passwordHash == Hash(password);
    }

    public class MediaFile
    {
        public int Id { get; }

        public string Name { get; }

        public MediaType Type { get; }

        public string Owner { get; }

        public MediaFile(int id, string name, MediaType type, string owner)
        {
            this.Id = id;
            this.Name = name;
            this.Type = type;
            this.Owner = owner;
        }
    }

    public class Album
    {
        private readonly List<MediaFile> files = new();
        private readonly Dictionary<string, HashSet<Permission>> perms = new();
        private readonly object sync = new();
        private int nextId = 1;

        public int Id { get; }

        public string Title { get; }

        public string Owner { get; }

        public IReadOnlyList<MediaFile> Files
        {
            get
            {
                lock (this.sync)
                {
                    return this.files.ToList().AsReadOnly();
                }
            }
        }

        public Album(int id, string title, string owner)
        {
            this.Id = id;
            this.Title = title;
            this.Owner = owner;
        }

        public void Grant(string username, Permission perm)
        {
            lock (this.sync)
            {
                if (!this.perms.ContainsKey(username))
                {
                    this.perms[username] = new HashSet<Permission>();
                }

                this.perms[username].Add(perm);
            }
        }

        public void Revoke(string username, Permission perm)
        {
            lock (this.sync)
            {
                if (this.perms.ContainsKey(username))
                {
                    this.perms[username].Remove(perm);
                }
            }
        }

        public bool Can(string username, Permission perm)
        {
            lock (this.sync)
            {
                return username == this.Owner || (this.perms.TryGetValue(username, out var p) && p.Contains(perm));
            }
        }

        public MediaFile AddFile(string requester, string name, MediaType type)
        {
            lock (this.sync)
            {
                if (!this.Can(requester, Permission.Add))
                {
                    throw new UnauthorizedAccessException("Нет прав на добавление.");
                }

                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException("Имя файла не может быть пустым.");
                }

                var f = new MediaFile(this.nextId++, name, type, requester);
                this.files.Add(f);
                return f;
            }
        }

        public void RemoveFile(string requester, int fileId)
        {
            lock (this.sync)
            {
                if (!this.Can(requester, Permission.Delete))
                {
                    throw new UnauthorizedAccessException("Нет прав на удаление.");
                }

                var f = this.files.FirstOrDefault(x => x.Id == fileId)
                    ?? throw new KeyNotFoundException($"Файл {fileId} не найден.");
                this.files.Remove(f);
            }
        }

        public IReadOnlyList<MediaFile> Browse(string requester)
        {
            lock (this.sync)
            {
                if (!this.Can(requester, Permission.View))
                {
                    throw new UnauthorizedAccessException("Нет прав на просмотр.");
                }

                return this.files.ToList().AsReadOnly();
            }
        }
    }

    public class MediaService
    {
        private readonly Dictionary<string, User> users = new();
        private readonly Dictionary<int, Album> albums = new();
        private readonly object sync = new();
        private int nextUserId = 1;
        private int nextAlbumId = 1;

        public User Register(string username, string password)
        {
            lock (this.sync)
            {
                if (this.users.ContainsKey(username))
                {
                    throw new InvalidOperationException($"Пользователь '{username}' уже существует.");
                }

                var u = new User(this.nextUserId++, username, password);
                this.users[username] = u;
                return u;
            }
        }

        public User Login(string username, string password)
        {
            lock (this.sync)
            {
                if (!this.users.TryGetValue(username, out var u))
                {
                    throw new KeyNotFoundException("Пользователь не найден.");
                }

                if (!u.CheckPassword(password))
                {
                    throw new UnauthorizedAccessException("Неверный пароль.");
                }

                return u;
            }
        }

        public Album CreateAlbum(string owner, string title)
        {
            lock (this.sync)
            {
                if (!this.users.ContainsKey(owner))
                {
                    throw new KeyNotFoundException("Пользователь не найден.");
                }

                if (string.IsNullOrWhiteSpace(title))
                {
                    throw new ArgumentException("Название альбома не может быть пустым.");
                }

                var a = new Album(this.nextAlbumId++, title, owner);
                this.albums[a.Id] = a;
                return a;
            }
        }

        public Album GetAlbum(int id)
        {
            lock (this.sync)
            {
                return this.albums.TryGetValue(id, out var a) ? a : throw new KeyNotFoundException($"Альбом {id} не найден.");
            }
        }

        public void Share(string owner, int albumId, string target, Permission perm)
        {
            var album = this.GetAlbum(albumId);
            if (album.Owner != owner)
            {
                throw new UnauthorizedAccessException("Только владелец может выдавать права.");
            }

            lock (this.sync)
            {
                if (!this.users.ContainsKey(target))
                {
                    throw new KeyNotFoundException("Целевой пользователь не найден.");
                }
            }

            album.Grant(target, perm);
        }

        public bool UserExists(string username)
        {
            lock (this.sync)
            {
                return this.users.ContainsKey(username);
            }
        }

        // Метод добавлен для Задания 2 (TDD)
        public bool IsPasswordStrong(string password) => new PasswordPolicy().IsPasswordStrong(password);
    }

    public sealed class LoadTestResult
    {
        public string ScenarioName { get; init; } = string.Empty;

        public int Users { get; init; }

        public int TotalOperations { get; init; }

        public int ErrorsCount { get; init; }

        public double ElapsedMilliseconds { get; init; }

        public IReadOnlyCollection<string> Errors { get; init; } = Array.Empty<string>();
    }

    public static class LoadScenarioRunner
    {
        public static LoadTestResult RegisterAndCreateAlbum(int userCount)
        {
            var svc = new MediaService();
            var errors = new ConcurrentBag<string>();
            var sw = Stopwatch.StartNew();

            Parallel.For(0, userCount, i =>
            {
                try
                {
                    var username = $"user_{i}";
                    svc.Register(username, "password1");
                    svc.CreateAlbum(username, $"Album_{i}");
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                }
            });

            sw.Stop();
            return BuildResult("RegisterAndCreateAlbum", userCount, 0, errors, sw);
        }

        public static LoadTestResult MixedOperations(int userCount)
        {
            var svc = new MediaService();
            var errors = new ConcurrentBag<string>();
            int totalOps = 0;
            var sw = Stopwatch.StartNew();

            for (int i = 0; i < userCount; i++)
            {
                svc.Register($"u{i}", "pass123");
            }

            Parallel.For(0, userCount, i =>
            {
                try
                {
                    var album = svc.CreateAlbum($"u{i}", $"Album_{i}");
                    for (int j = 0; j < 5; j++)
                    {
                        album.AddFile($"u{i}", $"photo_{j}.jpg", MediaType.Photo);
                    }

                    _ = album.Browse($"u{i}");
                    Interlocked.Add(ref totalOps, 7);
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                }
            });

            sw.Stop();
            return BuildResult("MixedOperations", userCount, totalOps, errors, sw);
        }

        public static LoadTestResult SharedAlbumConcurrentUploads(int uploaders)
        {
            var svc = new MediaService();
            svc.Register("owner", "pass123");
            var album = svc.CreateAlbum("owner", "Общий");

            for (int i = 0; i < uploaders; i++)
            {
                svc.Register($"uploader_{i}", "pass123");
                svc.Share("owner", album.Id, $"uploader_{i}", Permission.Add);
            }

            var errors = new ConcurrentBag<string>();
            var sw = Stopwatch.StartNew();

            Parallel.For(0, uploaders, i =>
            {
                try
                {
                    album.AddFile($"uploader_{i}", $"file_{i}.jpg", MediaType.Photo);
                }
                catch (Exception ex)
                {
                    errors.Add(ex.Message);
                }
            });

            sw.Stop();
            return BuildResult("SharedAlbumConcurrentUploads", uploaders, album.Files.Count, errors, sw);
        }

        private static LoadTestResult BuildResult(
            string scenarioName,
            int users,
            int totalOperations,
            ConcurrentBag<string> errors,
            Stopwatch sw)
        {
            return new LoadTestResult
            {
                ScenarioName = scenarioName,
                Users = users,
                TotalOperations = totalOperations,
                ErrorsCount = errors.Count,
                ElapsedMilliseconds = sw.Elapsed.TotalMilliseconds,
                Errors = errors.ToArray(),
            };
        }
    }
}
