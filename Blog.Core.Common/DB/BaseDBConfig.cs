﻿using System.IO;

namespace Blog.Core.Common.DB
{
    public class BaseDBConfig
    {
        private static string sqliteConnection = Appsettings.app(new string[] { "AppSettings", "Sqlite", "SqliteConnection" });
        private static bool isSqliteEnabled = (Appsettings.app(new string[] { "AppSettings", "Sqlite", "Enabled" })).ObjToBool();

        private static string sqlServerConnection = Appsettings.app(new string[] { "AppSettings", "SqlServer", "SqlServerConnection" });
        private static bool isSqlServerEnabled = (Appsettings.app(new string[] { "AppSettings", "SqlServer", "Enabled" })).ObjToBool();

        private static string mySqlConnection = Appsettings.app(new string[] { "AppSettings", "MySql", "MySqlConnection" });
        private static bool isMySqlEnabled = (Appsettings.app(new string[] { "AppSettings", "MySql", "Enabled" })).ObjToBool();

        private static string oracleConnection = Appsettings.app(new string[] { "AppSettings", "Oracle", "OracleConnection" });
        private static bool IsOracleEnabled = (Appsettings.app(new string[] { "AppSettings", "Oracle", "Enabled" })).ObjToBool();


        public static string ConnectionString => InitConn();
        public static DataBaseType DbType = DataBaseType.SqlServer;


        private static string InitConn()
        {
            if (isSqliteEnabled)
            {
                DbType = DataBaseType.Sqlite;
                return sqliteConnection;
            }
            else if (isSqlServerEnabled)
            {
                DbType = DataBaseType.SqlServer;
                return File.Exists(@"D:\my-file\dbCountPsw1.txt") ? File.ReadAllText(@"D:\my-file\dbCountPsw1.txt").Trim() : sqlServerConnection;
            }
            else if (isMySqlEnabled)
            {
                DbType = DataBaseType.MySql;
                return File.Exists(@"D:\my-file\dbCountPsw1_MySqlConn.txt") ? File.ReadAllText(@"D:\my-file\dbCountPsw1_MySqlConn.txt").Trim() : mySqlConnection;
            }
            else if (IsOracleEnabled)
            {
                DbType = DataBaseType.Oracle;
                return File.Exists(@"D:\my-file\dbCountPsw1_OracleConn.txt") ? File.ReadAllText(@"D:\my-file\dbCountPsw1_OracleConn.txt").Trim() : oracleConnection;
            }
            else
            {
                DbType = DataBaseType.SqlServer;
                return "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=BlogDB;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
            }

        }

    }

    public enum DataBaseType
    {
        MySql = 0,
        SqlServer = 1,
        Sqlite = 2,
        Oracle = 3,
        PostgreSQL = 4
    }
}
