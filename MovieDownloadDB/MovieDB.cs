using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using System.Data;
using System.Data.Common;
using System.Data.SQLite;


/*
 * // 创建数据库文件
File.Delete("test1.db3");
SQLiteConnection.CreateFile("test1.db3");

DbProviderFactory factory = SQLiteFactory.Instance;
using (DbConnection conn = factory.CreateConnection())
{
// 连接数据库
conn.ConnectionString = "Data Source=test1.db3";
conn.Open();

// 创建数据表
string sql = "create table [test1] ([id] INTEGER PRIMARY KEY, [s] TEXT COLLATE NOCASE)";
DbCommand cmd = conn.CreateCommand();
cmd.Connection = conn;
cmd.CommandText = sql;
cmd.ExecuteNonQuery();

// 添加参数
cmd.Parameters.Add(cmd.CreateParameter());

// 开始计时
Stopwatch watch = new Stopwatch();
watch.Start();

DbTransaction trans = conn.BeginTransaction(); // <-------------------
try 
{
    // 连续插入1000条记录
    for (int i = 0; i < 1000; i++)
    {
      cmd.CommandText = "insert into [test1] ([s]) values (?)";
      cmd.Parameters[0].Value = i.ToString();

      cmd.ExecuteNonQuery();
    }

    trans.Commit(); // <-------------------
}
catch
{
    trans.Rollback(); // <-------------------
    throw; // <-------------------
}

// 停止计时
watch.Stop();
Console.WriteLine(watch.Elapsed);
}
*/


namespace MovieDownloadDB
{
    class MovieDB
    {
        private DbProviderFactory factory = SQLiteFactory.Instance;

        private MovieDB()
        {
            PrepareDB();
        }

        private void PrepareDB()
        {
            if (!File.Exists(GetDBFileName()))
            {
                SQLiteConnection.CreateFile(GetDBFileName());
                CreateTable();
            }
        }

        private String GetConnectionString()
        {
            return "Data Source=" + GetDBFileName();
        }

        private String GetCreateTableString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("CREATE TABLE [movies] (");
            stringBuilder.AppendLine("[id] INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT,");
            stringBuilder.AppendLine("[name] VARCHAR(255)  NOT NULL,");
            stringBuilder.AppendLine("[size] NUMERIC  NOT NULL,");
            stringBuilder.AppendLine("[time] TIMESTAMP  NOT NULL,");
            stringBuilder.AppendLine("[path] VARCHAR(10240)  NOT NULL");
            stringBuilder.AppendLine(" )");
            return stringBuilder.ToString();
        }

        private String GetCreateTempTableString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("CREATE TABLE [movies_temp] (");
            stringBuilder.AppendLine("[id] INTEGER  NOT NULL PRIMARY KEY AUTOINCREMENT,");
            stringBuilder.AppendLine("[name] VARCHAR(255)  NOT NULL,");
            stringBuilder.AppendLine("[time] TIMESTAMP  NOT NULL,");
            stringBuilder.AppendLine("[size] NUMERIC  NOT NULL,");
            stringBuilder.AppendLine("[path] VARCHAR(10240)  NOT NULL");
            stringBuilder.AppendLine(" )");
            return stringBuilder.ToString();
        }

        private void CreateTable()
        {
            using (DbConnection conn = factory.CreateConnection())
            {
                // 连接数据库
                conn.ConnectionString = GetConnectionString();
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                try
                {
                    // 创建数据表
                    string sql = GetCreateTableString();
                    DbCommand cmd = conn.CreateCommand();
                    cmd.CommandText = sql;
                    cmd.ExecuteNonQuery();
                    cmd.CommandText = GetCreateTempTableString();
                    cmd.ExecuteNonQuery();
                    trans.Commit();

                    cmd.CommandText = "CREATE INDEX [movies_name_index] ON [movies]([name]  DESC)";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "CREATE INDEX [movies_time_index] ON [movies]([time]  DESC)";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "CREATE INDEX [movies_temp_name_index] ON [movies_temp]([name]  DESC)";
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "CREATE INDEX [movies_temp_time_index] ON [movies_temp]([time]  DESC)";
                    cmd.ExecuteNonQuery();

                }
                catch
                {
                    trans.Rollback();
                }


            }
        }

        private String GetDBFileName()
        {
            string str = System.AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            return str + "movies.db";
        }

        private static MovieDB _instance = null;

        public static MovieDB Instance()
        {
            if (_instance == null)
            {
                _instance = new MovieDB();
            }

            return _instance;
        }

        public List<MovieFile> Find(String name)
        {
            List<MovieFile> movies = new List<MovieFile>();
            using (DbConnection conn = factory.CreateConnection())
            {
                // 连接数据库
                conn.ConnectionString = GetConnectionString();
                conn.Open();
                string sql = "select name,time,size,path from movies where path like ?";
                DbCommand cmd = conn.CreateCommand();
                cmd.Connection = conn;
                cmd.CommandText = sql;
                // 添加参数
                cmd.Parameters.Add(cmd.CreateParameter());
                cmd.Parameters[0].Value = "%" + name + "%";
                try
                {
                    DbDataReader reader = cmd.ExecuteReader();
                    while (reader.HasRows && reader.Read())
                    {
                        MovieFile movie = new MovieFile(reader.GetString(0), reader.GetDateTime(1).ToString(),reader.GetInt64(2), reader.GetString(3));
                        movies.Add(movie);
                    }
                }
                catch
                {

                    throw; // <-------------------
                }
                return movies;
            }
        }

        public List<MovieFile> FindTheSameMovies()
        {
            List<MovieFile> movies = new List<MovieFile>();
            using (DbConnection conn = factory.CreateConnection())
            {
                // 连接数据库
                conn.ConnectionString = GetConnectionString();
                conn.Open();
                string sql = "select distinct m1.name,m1.time,m1.size,m1.path from movies m1 join movies m2 on m1.name = m2.name  and m1.path<>m2.path order by m1.name";
                DbCommand cmd = conn.CreateCommand();
                cmd.Connection = conn;
                cmd.CommandText = sql;
                try
                {
                    DbDataReader reader = cmd.ExecuteReader();
                    while (reader.HasRows && reader.Read())
                    {
                        MovieFile movie = new MovieFile(reader.GetString(0), reader.GetDateTime(1).ToString(), reader.GetInt64(2), reader.GetString(3));
                        movies.Add(movie);
                    }
                }
                catch
                {

                    throw; // <-------------------
                }
                return movies;
            }
        }

        public void Add(List<MovieFile> files)
        {
            using (DbConnection conn = factory.CreateConnection())
            {
                // 连接数据库
                conn.ConnectionString = GetConnectionString();
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                try
                {
                    string sqlDelete = "delete from movies_temp";
                    DbCommand deleteCmd = conn.CreateCommand();
                    deleteCmd.CommandText = sqlDelete;
                    deleteCmd.ExecuteNonQuery();


                    string sqlInsertToMovieTemp = "insert into  movies_temp (name,time,size,path) values(?,?,?,?)";
                    DbCommand cmdInsertToMovieTemp = conn.CreateCommand();
                    //cmd.Connection = conn;
                    cmdInsertToMovieTemp.CommandText = sqlInsertToMovieTemp;
                    // 添加参数
                    cmdInsertToMovieTemp.Parameters.Add(cmdInsertToMovieTemp.CreateParameter());
                    cmdInsertToMovieTemp.Parameters.Add(cmdInsertToMovieTemp.CreateParameter());
                    cmdInsertToMovieTemp.Parameters.Add(cmdInsertToMovieTemp.CreateParameter());
                    cmdInsertToMovieTemp.Parameters.Add(cmdInsertToMovieTemp.CreateParameter());
                    cmdInsertToMovieTemp.Prepare();

                    foreach (MovieFile movie in files)
                    {
                        cmdInsertToMovieTemp.Parameters[0].Value = movie.Name;
                        cmdInsertToMovieTemp.Parameters[1].Value = movie.Time;
                        cmdInsertToMovieTemp.Parameters[2].Value = movie.Size;
                        cmdInsertToMovieTemp.Parameters[3].Value = movie.Path;
                        cmdInsertToMovieTemp.ExecuteNonQuery();
                    }

                    DbCommand cmdInsertToMovie = conn.CreateCommand();
                    cmdInsertToMovie.CommandText = "insert into movies (name,time,size,path) select movies_temp.name,movies_temp.time,movies_temp.size,movies_temp.path from movies_temp where not exists(select 1 from movies where movies.name=movies_temp.name and movies.time=movies_temp.time)";
                    //cmdInsertToMovie.Connection = conn;
                    cmdInsertToMovie.ExecuteNonQuery();
                    
                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw; // <-------------------
                }
            }
        }

        public void Clear()
        {
            using (DbConnection conn = factory.CreateConnection())
            {
                // 连接数据库
                /*
                delete from '表名';
                2
                select * from sqlite_sequence;
                3
                update sqlite_sequence set seq=0 where name='表名';
                */

                conn.ConnectionString = GetConnectionString();
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                try
                {
                    DbCommand cmd = conn.CreateCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = "delete from movies"; ;
                    cmd.ExecuteNonQuery();

                    cmd.CommandText = "update sqlite_sequence set seq=0 where name='movies'";
                    cmd.ExecuteNonQuery();

                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw; // <-------------------
                }
            }
        }

        internal void Delete(MovieFile movie)
        {
            using (DbConnection conn = factory.CreateConnection())
            {
                conn.ConnectionString = GetConnectionString();
                conn.Open();
                DbTransaction trans = conn.BeginTransaction();
                try
                {
                    DbCommand cmd = conn.CreateCommand();
                    cmd.Connection = conn;
                    cmd.CommandText = "delete from movies where name=? and size=? and path=?";
                    cmd.Parameters.Add(cmd.CreateParameter());
                    cmd.Parameters.Add(cmd.CreateParameter());
                    cmd.Parameters.Add(cmd.CreateParameter());

                    cmd.Parameters[0].Value = movie.Name;
                    cmd.Parameters[1].Value = movie.Size;
                    cmd.Parameters[2].Value = movie.Path;

                    cmd.ExecuteNonQuery();

                    trans.Commit();
                }
                catch
                {
                    trans.Rollback();
                    throw; // <-------------------
                }
            }
        }
    }
}
