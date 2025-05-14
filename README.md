# GodVNC Project

Проект состоит из двух компонентов:

- **GodVNC_agent** — лёгкий агент для сбора информации о пользователе и передачи её на сервер.
- **GodVNC (WinForms)** — графическое приложение для администратора, позволяет подключаться к компьютерам пользователей через VNC и управлять подключениями.

---

## 📁 Структура проекта

GodVNC/
├── GodVNC/ # WinForms GUI приложение
├── GodVNC_agent/ # Фоновый агент
├── config.ini # Конфигурационный файл (подключение к БД)
├── README.md # Документация
└── .gitignore

---

## ⚙️ GodVNC_agent

### Назначение

Агент запускается при входе пользователя в систему и:

- определяет текущего пользователя и его IP-адрес;
- сохраняет информацию в базу данных (MySQL);
- поддерживает автозапуск через планировщик задач;
- устанавливается вместе с UltraVNC.

### Настройка

1. **Создайте конфигурационный файл** `config.ini` рядом с `GodVNC_agent.exe` и `GodVNC.exe`:
```ini
[database]
connectionString = Server=IPadress;port=3306;Database=godvnc;User Id=writer;Password=yourpassword!;
```

🗄️ Структура базы данных
Для работы GodVNC необходима база данных MySQL или MariaDB с одной таблицей clients, которая хранит информацию о подключённых клиентах.

📋 Таблица clients
CREATE TABLE `clients` (
  `id` INT AUTO_INCREMENT PRIMARY KEY,
  `username` VARCHAR(100) NOT NULL,        -- Имя пользователя (например, krivoy.yura)
  `ip_address` VARCHAR(45) NOT NULL,       -- IP-адрес клиента
  `hostname` VARCHAR(100) DEFAULT NULL,    -- Имя компьютера (опционально)
  `last_seen` DATETIME NOT NULL,           -- Время последнего запуска агента
  `online` BOOLEAN NOT NULL DEFAULT 1      -- Статус: online/offline
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4;
🔄 Поля username, ip_address, last_seen обновляются агентом каждый раз при запуске или перезапуске пользователя.

🔐 Пользователи базы
Рекомендуется создать отдельного пользователя базы данных с ограниченными правами, например:
CREATE USER 'writer'@'%' IDENTIFIED BY 'yourpassword';
GRANT INSERT, UPDATE, SELECT ON godvnc.clients TO 'writer'@'%';
FLUSH PRIVILEGES;
