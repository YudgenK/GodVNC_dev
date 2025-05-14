using System.Net.NetworkInformation;
using MySql.Data.MySqlClient;
using IniReader;

class Program
{
    static void Main()
    {
        FreeConsole();
        string username = Environment.UserName;
        string hostname = Environment.MachineName;
        string ipAddress = GetLocalIPAddress();

        try
        {
            string filePath = @"config.ini";

            // Создаем экземпляр класса IniFile
            IniFile iniFile = new IniFile(filePath);

            string connectionString = iniFile.GetValue("Database", "ConnectionString");

            if (connectionString != null)
            {
                Console.WriteLine($"Строка подключения: {connectionString}");
            }
            else
            {
                Console.WriteLine("Не удалось найти строку подключения.");
            }
      
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();

                string query = @"
                        INSERT INTO UserSessions (Username, Hostname, IPAddress, LastSeen)
                            VALUES (@Username, @Hostname, @IPAddress, NOW())
                                ON DUPLICATE KEY UPDATE
                                     IPAddress = @IPAddress,
                                         LastSeen = NOW();";

                using (MySqlCommand cmd = new MySqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@Username", username);
                    cmd.Parameters.AddWithValue("@Hostname", hostname);
                    cmd.Parameters.AddWithValue("@IPAddress", ipAddress);

                    cmd.ExecuteNonQuery();
                }

                Console.WriteLine("Данные отправлены в MySQL.");
            }
        }
        catch (MySqlException ex)
        {
            // Ловим ошибки подключения и выводим их
            Console.WriteLine($"Ошибка подключения к базе данных: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Ловим любые другие ошибки
            Console.WriteLine($"Произошла ошибка: {ex.Message}");
        }
    }

    static string GetLocalIPAddress()
    {
        foreach (var ni in NetworkInterface.GetAllNetworkInterfaces())
        {
            if (ni.OperationalStatus == OperationalStatus.Up &&
                ni.NetworkInterfaceType != NetworkInterfaceType.Loopback)
            {
                foreach (var ip in ni.GetIPProperties().UnicastAddresses)
                {
                    if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.Address.ToString();
                    }
                }
            }
        }
        return "0.0.0.0";
    }

    [System.Runtime.InteropServices.DllImport("kernel32.dll")]
    static extern bool FreeConsole();
}
