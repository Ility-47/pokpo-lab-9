using System;
using System.Linq;
using System.Windows.Forms;
using MediaShareApp;

namespace MediaShareApp.WinForms
{
    /// <summary>
    /// Главное окно — открывается после успешного входа.
    /// </summary>
    public class MainForm : Form
    {
        private readonly string currentUser;

        private Label lblWelcome = null!;
        private Button btnLogout = null!;
        private TextBox txtAlbumTitle = null!;
        private Button btnCreateAlbum = null!;
        private ComboBox cmbAlbums = null!;
        private TextBox txtFileName = null!;
        private ComboBox cmbFileType = null!;
        private Button btnAddFile = null!;
        private Button btnRefreshFiles = null!;
        private ListBox lstFiles = null!;
        private TextBox txtShareUser = null!;
        private ComboBox cmbPermission = null!;
        private Button btnShare = null!;
        private Label lblStatus = null!;

        public MainForm(string username)
        {
            this.currentUser = username;
            this.BuildUI(username);
            this.RefreshAlbums();
        }

        private void BuildUI(string username)
        {
            this.Text = "Главная — MediaShare";
            this.Name = "MainForm";
            this.Size = new System.Drawing.Size(900, 600);
            this.StartPosition = FormStartPosition.CenterScreen;

            this.lblWelcome = new Label
            {
                Name = "lblWelcome",
                Text = $"Добро пожаловать, {username}!",
                Font = new System.Drawing.Font("Segoe UI", 12),
                Location = new System.Drawing.Point(20, 15),
                Size = new System.Drawing.Size(520, 30),
            };

            this.btnLogout = new Button
            {
                Name = "btnLogout",
                Text = "Выйти",
                Location = new System.Drawing.Point(760, 12),
                Size = new System.Drawing.Size(110, 32),
                AccessibleName = "LogoutButton",
            };
            this.btnLogout.Click += (_, _) => this.Close();

            var lblAlbumTitle = new Label
            {
                Text = "Название альбома:",
                Location = new System.Drawing.Point(20, 70),
                Size = new System.Drawing.Size(150, 20),
            };
            this.txtAlbumTitle = new TextBox
            {
                Location = new System.Drawing.Point(170, 67),
                Size = new System.Drawing.Size(230, 23),
            };
            this.btnCreateAlbum = new Button
            {
                Text = "Создать альбом",
                Location = new System.Drawing.Point(410, 66),
                Size = new System.Drawing.Size(130, 26),
            };
            this.btnCreateAlbum.Click += this.BtnCreateAlbum_Click;

            var lblSelectAlbum = new Label
            {
                Text = "Текущий альбом:",
                Location = new System.Drawing.Point(20, 110),
                Size = new System.Drawing.Size(150, 20),
            };
            this.cmbAlbums = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new System.Drawing.Point(170, 107),
                Size = new System.Drawing.Size(370, 23),
            };
            this.cmbAlbums.SelectedIndexChanged += (_, _) => this.RefreshFiles();

            var lblFileName = new Label
            {
                Text = "Имя файла:",
                Location = new System.Drawing.Point(20, 150),
                Size = new System.Drawing.Size(150, 20),
            };
            this.txtFileName = new TextBox
            {
                Location = new System.Drawing.Point(170, 147),
                Size = new System.Drawing.Size(230, 23),
            };

            this.cmbFileType = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new System.Drawing.Point(410, 147),
                Size = new System.Drawing.Size(130, 23),
            };
            this.cmbFileType.Items.AddRange(new object[] { "photo", "video" });
            this.cmbFileType.SelectedIndex = 0;

            this.btnAddFile = new Button
            {
                Text = "Добавить файл",
                Location = new System.Drawing.Point(550, 145),
                Size = new System.Drawing.Size(130, 27),
            };
            this.btnAddFile.Click += this.BtnAddFile_Click;

            this.btnRefreshFiles = new Button
            {
                Text = "Обновить список",
                Location = new System.Drawing.Point(690, 145),
                Size = new System.Drawing.Size(130, 27),
            };
            this.btnRefreshFiles.Click += (_, _) => this.RefreshFiles();

            var lblShareUser = new Label
            {
                Text = "Выдать права пользователю:",
                Location = new System.Drawing.Point(20, 190),
                Size = new System.Drawing.Size(180, 20),
            };
            this.txtShareUser = new TextBox
            {
                Location = new System.Drawing.Point(205, 187),
                Size = new System.Drawing.Size(195, 23),
            };
            this.cmbPermission = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Location = new System.Drawing.Point(410, 187),
                Size = new System.Drawing.Size(130, 23),
            };
            this.cmbPermission.Items.AddRange(new object[] { "view", "add", "delete" });
            this.cmbPermission.SelectedIndex = 0;
            this.btnShare = new Button
            {
                Text = "Выдать",
                Location = new System.Drawing.Point(550, 185),
                Size = new System.Drawing.Size(110, 27),
            };
            this.btnShare.Click += this.BtnShare_Click;

            this.lstFiles = new ListBox
            {
                Location = new System.Drawing.Point(20, 230),
                Size = new System.Drawing.Size(850, 280),
            };

            this.lblStatus = new Label
            {
                Location = new System.Drawing.Point(20, 530),
                Size = new System.Drawing.Size(850, 25),
            };

            this.Controls.AddRange(
                new Control[]
                {
                    this.lblWelcome, this.btnLogout,
                    lblAlbumTitle, this.txtAlbumTitle, this.btnCreateAlbum,
                    lblSelectAlbum, this.cmbAlbums,
                    lblFileName, this.txtFileName, this.cmbFileType, this.btnAddFile, this.btnRefreshFiles,
                    lblShareUser, this.txtShareUser, this.cmbPermission, this.btnShare,
                    this.lstFiles, this.lblStatus,
                });
        }

        private void BtnCreateAlbum_Click(object? sender, EventArgs e)
        {
            try
            {
                var album = Program.Service.CreateAlbum(this.currentUser, this.txtAlbumTitle.Text.Trim());
                this.txtAlbumTitle.Clear();
                this.SetStatus($"Альбом создан: #{album.Id} {album.Title}", false);
                this.RefreshAlbums(album.Id);
            }
            catch (Exception ex)
            {
                this.SetStatus(ex.Message, true);
            }
        }

        private void BtnAddFile_Click(object? sender, EventArgs e)
        {
            var selected = this.cmbAlbums.SelectedItem as AlbumComboItem;
            if (selected is null)
            {
                this.SetStatus("Сначала выберите альбом.", true);
                return;
            }

            try
            {
                var album = Program.Service.GetAlbum(selected.Id);
                var type = string.Equals(this.cmbFileType.SelectedItem?.ToString(), "video", StringComparison.OrdinalIgnoreCase)
                    ? MediaType.Video
                    : MediaType.Photo;
                var file = album.AddFile(this.currentUser, this.txtFileName.Text.Trim(), type);
                this.txtFileName.Clear();
                this.SetStatus($"Файл добавлен: #{file.Id} {file.Name}", false);
                this.RefreshFiles();
            }
            catch (Exception ex)
            {
                this.SetStatus(ex.Message, true);
            }
        }

        private void BtnShare_Click(object? sender, EventArgs e)
        {
            var selected = this.cmbAlbums.SelectedItem as AlbumComboItem;
            if (selected is null)
            {
                this.SetStatus("Сначала выберите альбом.", true);
                return;
            }

            try
            {
                var permission = ParsePermission(this.cmbPermission.SelectedItem?.ToString());
                Program.Service.Share(this.currentUser, selected.Id, this.txtShareUser.Text.Trim(), permission);
                this.SetStatus("Права успешно выданы.", false);
            }
            catch (Exception ex)
            {
                this.SetStatus(ex.Message, true);
            }
        }

        private void RefreshAlbums(int? selectAlbumId = null)
        {
            var previousSelected = (this.cmbAlbums.SelectedItem as AlbumComboItem)?.Id;
            this.cmbAlbums.Items.Clear();

            for (var id = 1; id <= 500; id++)
            {
                try
                {
                    var album = Program.Service.GetAlbum(id);
                    if (album.Owner == this.currentUser || album.Can(this.currentUser, Permission.View) || album.Can(this.currentUser, Permission.Add) || album.Can(this.currentUser, Permission.Delete))
                    {
                        this.cmbAlbums.Items.Add(new AlbumComboItem(album.Id, album.Title, album.Owner));
                    }
                }
                catch
                {
                    // Пропускаем несуществующие id.
                }
            }

            var targetId = selectAlbumId ?? previousSelected;
            if (targetId.HasValue)
            {
                foreach (var item in this.cmbAlbums.Items.OfType<AlbumComboItem>())
                {
                    if (item.Id == targetId.Value)
                    {
                        this.cmbAlbums.SelectedItem = item;
                        break;
                    }
                }
            }

            if (this.cmbAlbums.SelectedIndex < 0 && this.cmbAlbums.Items.Count > 0)
            {
                this.cmbAlbums.SelectedIndex = 0;
            }

            this.RefreshFiles();
        }

        private void RefreshFiles()
        {
            this.lstFiles.Items.Clear();
            var selected = this.cmbAlbums.SelectedItem as AlbumComboItem;
            if (selected is null)
            {
                return;
            }

            try
            {
                var album = Program.Service.GetAlbum(selected.Id);
                var files = album.Browse(this.currentUser);
                foreach (var file in files)
                {
                    this.lstFiles.Items.Add($"[{file.Type}] #{file.Id} {file.Name} (owner: {file.Owner})");
                }

                if (files.Count == 0)
                {
                    this.lstFiles.Items.Add("Файлов пока нет.");
                }
            }
            catch (Exception ex)
            {
                this.lstFiles.Items.Add("Нет доступа к просмотру: " + ex.Message);
            }
        }

        private static Permission ParsePermission(string? value) =>
            value?.ToLowerInvariant() switch
            {
                "view" => Permission.View,
                "add" => Permission.Add,
                "delete" => Permission.Delete,
                _ => throw new ArgumentException("Права: view, add или delete."),
            };

        private void SetStatus(string message, bool isError)
        {
            this.lblStatus.ForeColor = isError ? System.Drawing.Color.Crimson : System.Drawing.Color.DarkGreen;
            this.lblStatus.Text = message;
        }

        private sealed class AlbumComboItem
        {
            public int Id { get; }

            public string Title { get; }

            public string Owner { get; }

            public AlbumComboItem(int id, string title, string owner)
            {
                this.Id = id;
                this.Title = title;
                this.Owner = owner;
            }

            public override string ToString() => $"#{this.Id} {this.Title} (owner: {this.Owner})";
        }
    }
}
