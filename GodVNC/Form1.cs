using System;
using System.Data;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using GodVNC.Properties;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using IniReader; 

namespace GodVNC
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        // Загружаем данные при старте формы
        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                LoadData(); // Загрузка данных при старте
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка: {ex.Message}");
            }
        }
        DataTable originalData;

        // Метод для загрузки данных из базы и отображения в DataGridView
        private void LoadData(string searchQuery = "")
        {
            try
            {
                string filePath = @"config.ini";

                IniFile iniFile = new IniFile(filePath);

                string connectionString = iniFile.GetValue("Database", "ConnectionString");

                if (connectionString == null)
                {
                    throw new Exception("Отсутсвует строка подключения к базе данных в конфиге");
                }

                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();

                    // Формируем запрос с возможностью фильтрации
                    string query = "SELECT Username, Hostname, IPAddress, LastSeen FROM UserSessions";
                    if (!string.IsNullOrEmpty(searchQuery))
                    {
                        // Фильтрация по имени пользователя, имени компьютера или IP
                        query += $" WHERE Username LIKE '%{searchQuery}%' OR Hostname LIKE '%{searchQuery}%' OR IPAddress LIKE '%{searchQuery}%'";
                    }

                    MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn);
                    DataTable dt = new DataTable();
                    adapter.Fill(dt);
                    dataGridView1.DataSource = dt; // DataGridView на форме
                    originalData = dt; // Сохраняем оригинал
                    dataGridView1.DataSource = dt;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных: {ex.Message}");
            }
        }

        // Подключение к выбранному пользователю
        private void btnConnect_Click(object sender, EventArgs e)
        {
            if (dataGridView1.SelectedRows.Count > 0)
            {
                string ip = dataGridView1.SelectedRows[0].Cells["IPAddress"].Value.ToString();
                string username = dataGridView1.SelectedRows[0].Cells["Username"].Value.ToString();

                // Проверка наличия пути к VNC-клиенту
                string vncPath = Settings.Default.VncClientPath;
                if (string.IsNullOrEmpty(vncPath))
                {
                    // Если путь не сохранен, откроем диалог для выбора
                    vncPath = SelectVncClientPath();
                    if (!string.IsNullOrEmpty(vncPath))
                    {
                        Settings.Default.VncClientPath = vncPath;
                        Settings.Default.Save();  // Сохраняем путь
                    }
                }

                if (!string.IsNullOrEmpty(vncPath))
                {
                    // Подключаемся через VNC
                    ConnectViaVnc(vncPath, ip);
                }
                else
                {
                    MessageBox.Show("Не выбран путь к VNC-клиенту.");
                }
            }
            else
            {
                MessageBox.Show("Выберите пользователя для подключения.");
            }
        }

        // Метод для выбора пути к VNC-клиенту через диалоговое окно
        private string SelectVncClientPath()
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "VNC Viewer|vncviewer.exe|Все файлы|*.*";
                openFileDialog.Title = "Выберите VNC-клиент";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    return openFileDialog.FileName;
                }
            }
            return string.Empty; // Путь не выбран
        }

        // Метод для подключения через VNC
        private void ConnectViaVnc(string vncClientPath, string ipAddress)
        {
            try
            {
                // Запуск VNC-клиента с выбранным путем и IP-адресом
                System.Diagnostics.Process.Start(vncClientPath, ipAddress);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при подключении через VNC: {ex.Message}");
            }
        }

        // Метод, который вызывается при нажатии кнопки поиска
        private void btnSearch_Click(object sender, EventArgs e)
        {
            string search = txtSearch.Text.ToLower();
            string translit = Transliterate(search);

            var filteredRows = originalData.AsEnumerable().Where(row =>
            {
                string username = row["Username"].ToString().ToLower();
                return
                    row["Username"].ToString().ToLower().Contains(search) ||
                      row["Username"].ToString().ToLower().Contains(translit) ||
                          row["Hostname"].ToString().ToLower().Contains(search) ||
                              row["IPAddress"].ToString().ToLower().Contains(search);
            });

            if (filteredRows.Any())
                dataGridView1.DataSource = filteredRows.CopyToDataTable();
            else
                dataGridView1.DataSource = originalData.Clone();
        }

        private string Transliterate(string input)
        {
            Dictionary<char, string> map = new Dictionary<char, string>
            {
                ['а'] = "a",
                ['б'] = "b",
                ['в'] = "v",
                ['г'] = "g",
                ['д'] = "d",
                ['е'] = "e",
                ['ё'] = "e",
                ['ж'] = "zh",
                ['з'] = "z",
                ['и'] = "i",
                ['й'] = "y",
                ['к'] = "k",
                ['л'] = "l",
                ['м'] = "m",
                ['н'] = "n",
                ['о'] = "o",
                ['п'] = "p",
                ['р'] = "r",
                ['с'] = "s",
                ['т'] = "t",
                ['у'] = "u",
                ['ф'] = "f",
                ['х'] = "kh",
                ['ц'] = "ts",
                ['ч'] = "ch",
                ['ш'] = "sh",
                ['щ'] = "shch",
                ['ъ'] = "",
                ['ы'] = "y",
                ['ь'] = "",
                ['э'] = "e",
                ['ю'] = "yu",
                ['я'] = "ya",
                [' '] = ".",
                ['.'] = "."
            };

            var sb = new StringBuilder();
            foreach (char ch in input.ToLower())
            {
                if (map.TryGetValue(ch, out string val))
                    sb.Append(val);
                else
                    sb.Append(ch);
            }

            return sb.ToString();
        }
    }
}
