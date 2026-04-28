using System;
using System.Collections.Generic;
using System.Linq;
using MediaShareApp;
using NUnit.Framework;
using Reqnroll;

namespace MediaShareApp.Tests.BDD;

[Binding]
public sealed class MediaShareSteps
{
    private readonly MediaService service = new();

    private User? currentUser;
    private Album? currentAlbum;
    private IReadOnlyList<MediaFile> lastBrowsedFiles = Array.Empty<MediaFile>();
    private Exception? lastException;

    [Given(@"приложение MediaShare доступно для нового пользователя")]
    public void GivenApplicationIsAvailable()
    {
        this.lastException = null;
    }

    [Given(@"пользователь уже зарегистрирован с логином ""(.*)"" и паролем ""(.*)""")]
    public void GivenUserRegisteredWithPassword(string username, string password)
    {
        this.service.Register(username, password);
        this.lastException = null;
    }

    [Given(@"пользователь ""(.*)"" зарегистрирован в системе")]
    public void GivenUserRegistered(string username)
    {
        this.service.Register(username, "pass123");
        this.lastException = null;
    }

    [Given(@"пользователь ""(.*)"" зарегистрирован и вошел в систему с паролем ""(.*)""")]
    public void GivenUserRegisteredAndLoggedIn(string username, string password)
    {
        this.service.Register(username, password);
        this.currentUser = this.service.Login(username, password);
        this.lastException = null;
    }

    [Given(@"существует альбом ""(.*)"", созданный пользователем ""(.*)""")]
    public void GivenAlbumExists(string title, string owner)
    {
        this.EnsureUser(owner, "pass123");
        this.currentAlbum = this.service.CreateAlbum(owner, title);
        this.currentUser = this.service.Login(owner, "pass123");
        this.lastException = null;
    }

    [Given(@"в альбоме есть файл ""(.*)"" типа ""(.*)""")]
    public void GivenAlbumContainsFile(string fileName, string mediaType)
    {
        Assert.That(this.currentAlbum, Is.Not.Null, "Текущий альбом должен быть создан до добавления файла.");
        var owner = this.currentAlbum!.Owner;
        this.currentAlbum.AddFile(owner, fileName, ParseMediaType(mediaType));
        this.lastException = null;
    }

    [Given(@"владелец выдал пользователю ""(.*)"" право ""(.*)"" на текущий альбом")]
    public void GivenOwnerSharedAlbum(string username, string permission)
    {
        Assert.That(this.currentAlbum, Is.Not.Null, "Текущий альбом должен существовать.");
        this.service.Share(this.currentAlbum!.Owner, this.currentAlbum.Id, username, ParsePermission(permission));
        this.lastException = null;
    }

    [When(@"пользователь регистрируется с логином ""(.*)"" и паролем ""(.*)""")]
    public void WhenUserRegisters(string username, string password)
    {
        this.Execute(() =>
        {
            this.currentUser = this.service.Register(username, password);
        });
    }

    [When(@"пользователь входит с логином ""(.*)"" и паролем ""(.*)""")]
    public void WhenUserLogsIn(string username, string password)
    {
        this.Execute(() =>
        {
            this.currentUser = this.service.Login(username, password);
        });
    }

    [When(@"пользователь создает альбом ""(.*)""")]
    public void WhenUserCreatesAlbum(string title)
    {
        this.Execute(() =>
        {
            Assert.That(this.currentUser, Is.Not.Null, "Пользователь должен быть авторизован.");
            this.currentAlbum = this.service.CreateAlbum(this.currentUser!.Username, title);
        });
    }

    [When(@"пользователь добавляет в текущий альбом файл ""(.*)"" типа ""(.*)""")]
    public void WhenUserAddsFile(string fileName, string mediaType)
    {
        this.Execute(() =>
        {
            Assert.That(this.currentAlbum, Is.Not.Null, "Текущий альбом должен существовать.");
            Assert.That(this.currentUser, Is.Not.Null, "Пользователь должен быть авторизован.");
            this.currentAlbum!.AddFile(this.currentUser!.Username, fileName, ParseMediaType(mediaType));
        });
    }

    [When(@"пользователь ""(.*)"" просматривает текущий альбом")]
    public void WhenUserBrowsesAlbum(string username)
    {
        this.Execute(() =>
        {
            Assert.That(this.currentAlbum, Is.Not.Null, "Текущий альбом должен существовать.");
            this.lastBrowsedFiles = this.currentAlbum!.Browse(username);
        });
    }

    [Then(@"вход должен быть выполнен для пользователя ""(.*)""")]
    public void ThenLoginCompleted(string username)
    {
        Assert.That(this.lastException, Is.Null, $"Ожидался успешный вход, но возникла ошибка: {this.lastException?.Message}");
        Assert.That(this.currentUser, Is.Not.Null);
        Assert.That(this.currentUser!.Username, Is.EqualTo(username));
    }

    [Then(@"должно появиться сообщение об ошибке ""(.*)""")]
    public void ThenErrorShouldAppear(string expectedMessage)
    {
        Assert.That(this.lastException, Is.Not.Null, "Ожидалась ошибка, но операция завершилась успешно.");
        Assert.That(this.lastException!.Message, Is.EqualTo(expectedMessage));
    }

    [Then(@"в текущем альбоме должен быть файл ""(.*)""")]
    [Then(@"список файлов должен содержать ""(.*)""")]
    public void ThenAlbumShouldContainFile(string fileName)
    {
        var files = this.lastBrowsedFiles;
        if (this.currentAlbum is not null && files.Count == 0)
        {
            files = this.currentAlbum.Browse(this.currentAlbum.Owner);
        }

        Assert.That(files.Any(f => f.Name == fileName), Is.True, $"Файл '{fileName}' не найден в альбоме.");
    }

    private void Execute(Action action)
    {
        this.lastException = null;
        try
        {
            action();
        }
        catch (Exception ex)
        {
            this.lastException = ex;
        }
    }

    private void EnsureUser(string username, string password)
    {
        if (!this.service.UserExists(username))
        {
            this.service.Register(username, password);
        }
    }

    private static MediaType ParseMediaType(string value) =>
        value.ToLowerInvariant() switch
        {
            "photo" => MediaType.Photo,
            "video" => MediaType.Video,
            _ => throw new ArgumentException("Тип файла должен быть photo или video."),
        };

    private static Permission ParsePermission(string value) =>
        value.ToLowerInvariant() switch
        {
            "view" => Permission.View,
            "add" => Permission.Add,
            "delete" => Permission.Delete,
            _ => throw new ArgumentException("Право должно быть view, add или delete."),
        };
}
