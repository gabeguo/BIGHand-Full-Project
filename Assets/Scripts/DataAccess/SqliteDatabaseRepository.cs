using Mono.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class SqliteDatabaseRepository : IDataRepository
{
    private User currentUser;

    private const int USER_ID_HASH_ITERATIONS = 1000, USER_PASSWORD_HASH_ITERATIONS = 1500;
    private const int IV_HASH_BYTES = 8, KEY_HASH_BYTES = 24, PWD_HASH_BYTES = 32;
    //private const string DB_CONNECTION_STRING = "Data Source=Assets/DB_Files/HGR_LocalDB.sqlite;Version=3";
    private string DB_CONNECTION_STRING = "URI=file:" + Application.persistentDataPath + "/HGR_LocalDB";

    private SqliteConnection GetConnection()
    {
        Debug.Log(DB_CONNECTION_STRING);
        return new SqliteConnection(DB_CONNECTION_STRING);
    }

    public SqliteDatabaseRepository()
    {
        using (SqliteConnection connection = GetConnection())
        {
            connection.Open();
            InitializeTables(connection);
        }
    }

    private void InitializeTables(SqliteConnection connection)
    {
        using (SqliteCommand command = connection.CreateCommand())
        {
            command.CommandType = System.Data.CommandType.Text;

            foreach (string tableText in tableCreationTexts)
            {
                command.CommandText = tableText;
                command.ExecuteNonQuery();
            }
        }
    }

    public void AddData(IGameSessionData data)
    {
        using (SqliteConnection connection = GetConnection())
        {
            connection.Open();
            AddData(data, connection);
        }
    }

    private void AddData(IGameSessionData data, SqliteConnection connection)
    {
        if (!IsValidGame(data.GetGameName()) || currentUser == null)
            throw new ArgumentException();

        string sql = data.GetInsertionString();
        using (SqliteCommand command = new SqliteCommand(sql, connection))
        {
            command.ExecuteNonQuery();
        }
    }

    private int GenerateUserId(SqliteConnection connection)
    {
        int id = UnityEngine.Random.Range(0, 1000000);

        string sql = "SELECT * FROM identifiers WHERE id = " + id + ";";
        using (SqliteCommand command = new SqliteCommand(sql, connection))
        {
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    return GenerateUserId(connection);
                }
                else
                {
                    return id;
                }
            }
        }
    }

    public bool CreateUser(string username, string password)
    {
        using (SqliteConnection connection = GetConnection())
        {
            connection.Open();
            return CreateUser(username, password, connection);
        }
    }

    private bool CreateUser(string username, string password, SqliteConnection connection)
    {
        if (UsernameInUse(username, connection))
            return false;

        try
        {
            string salt = GenerateSalt();
            int userId = GenerateUserId(connection);
            string encUserId = EncryptUserId(userId, password, salt);

            string sql = "INSERT INTO users (userId, username, password, salt) VALUES ('" + encUserId
                + "', '" + username + "', '" + HashPassword(password, salt) + "', '" + salt + "');";
            using (SqliteCommand command = new SqliteCommand(sql, connection))
            {
                command.ExecuteNonQuery();
                SaveIdToIdentifiers(userId, connection);
            }
        }
        catch (SqliteException e)
        {
            Debug.Log("Exception Caught When Creating User Profile: " + e.StackTrace);
            CreateUser(username, password, connection);
        }

        return true;
    }

    public bool UsernameInUse(string username)
    {
        using (SqliteConnection connection = GetConnection())
        {
            connection.Open();
            return UsernameInUse(username, connection);
        }
    }

    private void SaveIdToIdentifiers(int id, SqliteConnection connection)
    {
        string sql = "INSERT INTO identifiers (id) VALUES (" + id + ");";
        using (SqliteCommand command = new SqliteCommand(sql, connection))
        {
            command.ExecuteNonQuery();
        }
    }

    private bool UsernameInUse(string username, SqliteConnection connection)
    {
        string sql = "SELECT username FROM users WHERE username = '" + username + "';";
        using (SqliteCommand command = new SqliteCommand(sql, connection))
        {
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                return reader.Read();
            }
        }
    }

    public bool SignUserIn(string username, string password)
    {
        using (SqliteConnection connection = GetConnection())
        {
            connection.Open();
            return SignUserIn(username, password, connection);
        }
    }

    private bool SignUserIn(string username, string password, SqliteConnection connection)
    {
        if (!UsernameInUse(username, connection))
            return false;

        string sql = "SELECT * FROM users WHERE username = '" + username + "';";
        using (SqliteCommand command = new SqliteCommand(sql, connection))
        {
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                reader.Read();

                string hashedPassword = (string)reader["password"];
                string salt = (string)reader["salt"];

                if (hashedPassword != HashPassword(password, salt))
                    return false;

                int id = DecryptUserId((string)reader["userId"], password, salt);
                currentUser = new User(username, id);

                return true;
            }
        }
    }

    public User GetCurrentUser()
    {
        return currentUser;
    }

    public void SignOut()
    {
        currentUser = null;
    }

    public IGameSessionData[] GetLastSessionData(string game)
    {
        return GetSessionData(game, 1);
    }

    public IGameSessionData[] GetSessionData(string game, DateTime date)
    {
        return GetSessionData(game, date, date);
    }

    public IGameSessionData[] GetSessionData(string game, DateTime startDate, DateTime endDate)
    {
        using (SqliteConnection connection = GetConnection())
        {
            connection.Open();
            return GetSessionData(game, startDate, endDate, connection);
        }
    }

    private IGameSessionData[] GetSessionData(string game, DateTime startDate, DateTime endDate, SqliteConnection connection)
    {
        if (!IsValidGame(game) || currentUser == null)
            throw new ArgumentException();

        startDate = startDate.Date;
        endDate = endDate.Date.AddDays(1);

        string sql = "SELECT * FROM " + game + " WHERE date > " + startDate.ToUnixEpochTime() +
            " AND date < " + endDate.ToUnixEpochTime() + " AND userId = " + currentUser.GetUserId() + ";";
        using (SqliteCommand command = new SqliteCommand(sql, connection))
        {
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                return UnpackReader(game, reader, connection);
            }
        }
    }

    public IGameSessionData[] GetSessionData(string game, int numPreviousSessions)
    {
        using (SqliteConnection connection = GetConnection())
        {
            connection.Open();
            return GetSessionData(game, numPreviousSessions, connection);
        }
    }

    private IGameSessionData[] GetSessionData(string game, int numPreviousSessions, SqliteConnection connection)
    {
        if (!IsValidGame(game) || currentUser == null)
            throw new ArgumentException();

        List<IGameSessionData> sessionsList = new List<IGameSessionData>();
        double afterDateEpoch = DateTime.Now.ToUnixEpochTime();

        for (int i = 0; i < numPreviousSessions; i++)
        {
            afterDateEpoch = GetLastEntryBeforeDate(game, afterDateEpoch, connection);
            if (afterDateEpoch == -1)
                return sessionsList.ToArray();

            string sql = "SELECT * FROM " + game + " WHERE date = " + afterDateEpoch
                + " AND userId = " + currentUser.GetUserId() + ";";
            using (SqliteCommand command = new SqliteCommand(sql, connection))
            {
                using (SqliteDataReader reader = command.ExecuteReader())
                {
                    sessionsList.AddRange(UnpackReader(game, reader, connection));
                }
            }
        }

        return sessionsList.ToArray();
    }

    public void CloseConnection()
    {
        //throw new NotImplementedException();
        SqliteConnection.ClearAllPools();
    }

    private bool IsValidGame(string game)
    {
        return gameSessionObjectLookup.ContainsKey(game);
    }

    private DateTime GetLastEntryBeforeDate(string game, DateTime afterDate, SqliteConnection connection)
    {
        DateTime result = new DateTime();
        double unixEpochDate = GetLastEntryBeforeDate(game, afterDate.ToUnixEpochTime(), connection);

        return result.FromUnixEpochTime(unixEpochDate);
    }

    private double GetLastEntryBeforeDate(string game, double afterDate, SqliteConnection connection)
    {
        string sql = "SELECT date FROM " + game + " WHERE date < " + afterDate + " AND userId = "
            + currentUser.GetUserId() + " ORDER BY date DESC LIMIT 1;";
        using (SqliteCommand command = new SqliteCommand(sql, connection))
        {
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                if (!reader.Read())
                    return -1;

                return (double)reader["date"];
            }
        }
    }
    
    //added as of 02/21/20: not sure if this works
    //TODO: debug
    public bool WasGamePlayedOnDate(string game, DateTime currDate)
    {
        using (SqliteConnection connection = GetConnection())
        {
            connection.Open();
            return WasGamePlayedOnDate(game, currDate, connection);
        }
    }
    private bool WasGamePlayedOnDate(string game, DateTime currDate, SqliteConnection connection)
    {
        DateTime startOfDate = GetStartOfDate(currDate);
        DateTime endOfDate = GetEndOfDate(currDate);
        
        return IsThereEntryInTimeRange(game, startOfDate.ToUnixEpochTime(), endOfDate.ToUnixEpochTime(), connection);
    }
    public DateTime GetStartOfDate(DateTime date)
    {
        //thanks to https://stackoverflow.com/questions/902789/how-to-get-the-start-and-end-times-of-a-day
        return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
    }
    public DateTime GetEndOfDate(DateTime date)
    {
        return new DateTime(date.Year, date.Month, date.Day, 23, 59, 59, 999);
    }
    private bool IsThereEntryInTimeRange(string game, double minDate, double maxDate, SqliteConnection connection)
    {
        string sql = "SELECT date FROM " + game
        + " WHERE date > " + minDate + " AND date < " + maxDate
        + " AND userId = " + currentUser.GetUserId() + " ORDER BY date DESC LIMIT 1;";
        using (SqliteCommand command = new SqliteCommand(sql, connection))
        {
            using (SqliteDataReader reader = command.ExecuteReader())
            {
                //TODO: Check this assumption
                if (reader.Read()) return true; //if there is actually data to be read
                return false;   //no entries in this range
            }
        }
    }
    //end 02/21/20

    private IGameSessionData[] UnpackReader(string game, SqliteDataReader reader, SqliteConnection connection)
    {
        List<IGameSessionData> resultRaw = new List<IGameSessionData>();
        List<string> columns = Enumerable.Range(0, reader.FieldCount).Select(reader.GetName).ToList();

        while (reader.Read())
        {
            Dictionary<string, object> data = new Dictionary<string, object>();

            data.Add("game", game);

            foreach (string s in columns)
            {
                if (s != "id")
                    data.Add(s, reader[s]);
            }

            object sessionDataObj = Activator.CreateInstance(gameSessionObjectLookup[game], data);
            resultRaw.Add((IGameSessionData)sessionDataObj);
        }

        return resultRaw.ToArray();
    }

    //-----------------------------------
    //-----------------------------------

    public static string GenerateSalt()
    {
        byte[] salt = new byte[8];
        RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
        rng.GetBytes(salt);

        return new UTF8Encoding(false).GetString(salt);
    }

    public static string HashPassword(string password, string salt)
    {
        Rfc2898DeriveBytes hasher = new Rfc2898DeriveBytes(password,
            new UTF8Encoding(false).GetBytes(salt), USER_PASSWORD_HASH_ITERATIONS);

        return new UTF8Encoding(false).GetString(hasher.GetBytes(PWD_HASH_BYTES));
    }

    public static string EncryptUserId(int id, string password, string salt)
    {
        byte[] saltBytes = new UTF8Encoding(false).GetBytes(salt);
        Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, saltBytes, USER_ID_HASH_ITERATIONS);

        TripleDES encryptor = TripleDES.Create();
        encryptor.Padding = PaddingMode.PKCS7;
        encryptor.Mode = CipherMode.CBC;

        byte[] relevantBytes = key.GetBytes(KEY_HASH_BYTES);
        PrintBytes(relevantBytes);

        encryptor.Key = relevantBytes;
        encryptor.IV = key.GetBytes(IV_HASH_BYTES);

        string result;
        using (MemoryStream mem = new MemoryStream())
        {
            using (CryptoStream crypt = new CryptoStream(mem, encryptor.CreateEncryptor(), CryptoStreamMode.Write))
            {
                byte[] utfData = new UTF8Encoding(false).GetBytes(id.ToString());

                crypt.Write(utfData, 0, utfData.Length);
            }
            byte[] encUtf = mem.ToArray();
            result = Convert.ToBase64String(encUtf);
        }

        return result;
    }

    public int DecryptUserId(string id, string password, string salt)
    {
        byte[] saltBytes = new UTF8Encoding(false).GetBytes(salt);
        Rfc2898DeriveBytes key = new Rfc2898DeriveBytes(password, saltBytes, USER_ID_HASH_ITERATIONS);

        TripleDES decryptor = TripleDES.Create();
        decryptor.Padding = PaddingMode.PKCS7;
        decryptor.Mode = CipherMode.CBC;

        byte[] relevantBytes = key.GetBytes(KEY_HASH_BYTES);
        PrintBytes(relevantBytes);

        decryptor.Key = relevantBytes;
        decryptor.IV = key.GetBytes(IV_HASH_BYTES);

        int result;
        using (MemoryStream mem = new MemoryStream())
        {
            using (CryptoStream crypt = new CryptoStream(mem, decryptor.CreateDecryptor(), CryptoStreamMode.Write))
            {
                byte[] base64Data = Convert.FromBase64String(id);

                crypt.Write(base64Data, 0, base64Data.Length);
            }
            byte[] utf8Bytes = mem.ToArray();
            PrintBytes(utf8Bytes);

            result = int.Parse(new UTF8Encoding(false).GetString(utf8Bytes));
        }

        return result;
    }

    public static void PrintBytes(byte[] a)
    {
        string result = "";
        foreach (byte b in a)
            result += (" " + b);

        Debug.Log("Bytes:" + result);
    }

    //-----------------------------------
    //-----------------------------------

    private readonly Dictionary<string, Type> gameSessionObjectLookup = new Dictionary<string, Type>() {
        {"maze", typeof(MazeSessionData)},
        {"pigs", typeof(PigsSessionData)},
        {"balloon", typeof(BalloonSessionData)},
        {"happy", typeof(HappySessionData)}
    };
    private readonly string[] tableCreationTexts = {

        "CREATE TABLE IF NOT EXISTS users (" +
            "id INTEGER PRIMARY KEY, " +
            "userId TEXT NOT NULL, " +
            "username TEXT NOT NULL, " +
            "password TEXT NOT NULL, " +
            "salt TEXT NOT NULL" +
            ");",

        "CREATE TABLE IF NOT EXISTS identifiers (" +
            "id INTEGER NOT NULL" +
            ");",

        "CREATE TABLE IF NOT EXISTS maze (" +
            "id INTEGER PRIMARY KEY, " +
            "userId INTEGER NOT NULL, " +
            "level INTEGER NOT NULL, " +
            "stage INTEGER NOT NULL, " +
            "date DOUBLE NOT NULL, " +
            "time INTEGER NOT NULL, " +
            "collisions INTEGER NOT NULL" +
            ");",

        "CREATE TABLE IF NOT EXISTS balloon (" +
            "id INTEGER PRIMARY KEY, " +
            "userId INTEGER NOT NULL, " +
            "level INTEGER NOT NULL, " +
            "stage INTEGER NOT NULL, " +
            "date DOUBLE NOT NULL, " +
            "leftCollisions INTEGER NOT NULL, " +
            "leftMaxStrength INTEGER NOT NULL, " +
            "rightCollisions INTEGER NOT NULL, " +
            "rightMaxStrength INTEGER NOT NULL" +
        ");",

        "CREATE TABLE IF NOT EXISTS pigs (" +
            "id INTEGER PRIMARY KEY, " +
            "userId INTEGER NOT NULL, " +
            "level INTEGER NOT NULL, " +
            "stage INTEGER NOT NULL, " +
            "date DOUBLE NOT NULL, " +
            "coinPercentage FLOAT NOT NULL" +
        ");",

        "CREATE TABLE IF NOT EXISTS happy (" +
            "id INTEGER PRIMARY KEY, " +
            "userId INTEGER NOT NULL, " +
            "level INTEGER NOT NULL, " +
            "stage INTEGER NOT NULL, " +
            "date DOUBLE NOT NULL, " +
            "leftGrips INTEGER NOT NULL, " +
            "leftHits INTEGER NOT NULL, " +
            "rightGrips INTEGER NOT NULL, " +
            "rightHits INTEGER NOT NULL, " +
            "leftAvgGripTime FLOAT NOT NULL, " +
            "rightAvgGripTime FLOAT NOT NULL" +
        ");"
    };
}
