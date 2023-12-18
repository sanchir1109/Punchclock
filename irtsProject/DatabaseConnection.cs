using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Data.Sqlite;
namespace irtsProject
{
    public class DatabaseConnection
    {
        public String connectionString;
        public List<DateInfo> infos = new List<DateInfo>();
        public User user;
        public DatabaseConnection()
        {
            this.connectionString = $"Data Source='C:\\Users\\user\\Downloads\\SQLiteDatabaseBrowserPortable\\TimeSchedule.db'";
            readDatas();
        }
        /// <summary>
        /// Датабаазаас бүх утгуудыг объект болгон авч Лист-эд хадгална.
        /// </summary>
        public void readDatas()
        {
            infos.Clear();
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"SELECT id, udur, ehleh_tsag, orson_tsag, garsan_tsag FROM tsag_burtgel";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader.GetInt32(0));
                        DateTime udur = reader.GetDateTime(1);
                        string ehleh_tsag = reader.GetString(2);
                        string orson_tsag = reader.GetString(3);
                        string garsan_tsag = reader.GetString(4);
                        DateInfo p = new DateInfo(id, udur, ehleh_tsag, orson_tsag, garsan_tsag);
                        infos.Add(p);
                    }
                }
                command.CommandText = @"SELECT id, ner, ovog, uureg, sys_shinjeec, suljee_ajilan, email, utas  FROM user_info";

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        int id = Convert.ToInt32(reader.GetInt32(0));
                        string ner = reader.GetString(1);
                        string ovog = reader.GetString(2);
                        string uureg = reader.GetString(3);
                        string sys_shinjeec = reader.GetString(4);
                        string suljee_ajiltan = reader.GetString(5);
                        string email = reader.GetString(6);
                        string utas = reader.GetString(7);
                        user = new User(id, ner, ovog, uureg, sys_shinjeec, suljee_ajiltan, email, utas);
                    }
                }
            }
        }
        /// <summary>
        /// цаг бүртгэх функц
        /// </summary>
        /// <param name="type"></param>
        /// <param name="tsag"></param>
        /// <param name="day"></param>
        public void tsagBurtgeh(int type, string tsag, string day)
        {
            using (var connection = new SqliteConnection(connectionString))
            {

                MessageBox.Show(day + " " + tsag);
                connection.Open();
                string updateQuery;
                if (type == 0)
                {
                    updateQuery = "UPDATE tsag_burtgel SET " +
                        "orson_tsag = @tsag " +
                        "WHERE udur LIKE @day";
                }
                else
                {
                    updateQuery = "UPDATE tsag_burtgel SET " +
                        "garsan_tsag = @tsag " +
                        "WHERE udur LIKE @day";
                }

                using (var command = new SqliteCommand(updateQuery, connection))
                {
                    command.Parameters.AddWithValue("@tsag", tsag);
                    command.Parameters.AddWithValue("@day", day);

                    command.ExecuteNonQuery();
                }
            }
        }
        /// <summary>
        /// Өгсөн өдрийн харьяалагдах 7 хоногийн мэдээллийг объект Лист хэлбэрээр эрэмблэн буцаана.
        /// </summary>
        /// <param name="selectedDate"></param>
        /// <returns></returns>
        public List<DateInfo> GetWeekInfo(DateTime selectedDate)
        {
            List<DateInfo> week = new List<DateInfo>();

            int dayOfWeekValue = (int)selectedDate.DayOfWeek;
            if (dayOfWeekValue == 0) { dayOfWeekValue = 7; }

            int id = 0;
            foreach (DateInfo i in infos)
            {
                if (i.date.Equals(selectedDate)) { id = i.id; break; };
            }


            if (dayOfWeekValue == 1)
            {
                int up = id;
                for (int k = dayOfWeekValue; k < 8; k++)
                {
                    week.Add(getById(up));
                    up++;

                }
            }
            if (dayOfWeekValue == 7)
            {
                int down = id;
                for (int k = dayOfWeekValue; k > 0; k--)
                {
                    week.Add(getById(down));
                    down--;

                }
            }
            if (dayOfWeekValue != 7 && dayOfWeekValue != 1)
            {
                int up = id;
                for (int k = dayOfWeekValue; k < 8; k++)
                {
                    week.Add(getById(up));
                    up++;

                }
                int down = id - 1;
                for (int k = dayOfWeekValue - 1; k > 0; k--)
                {
                    week.Add(getById(down));
                    down--;
                }
            }
            List<DateInfo> sortedList = week.OrderBy(obj => obj.date).ToList();
            return sortedList;
        }
        /// <summary>
        /// id-аар нь Листээс хайж олдсон Объектийг буцаана.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public DateInfo getById(int id)
        {
            foreach (DateInfo i in infos)
            {
                if (i.id.Equals(id)) { return i; break; };
            }
            return null;
        }
        /// <summary>
        /// Өгсөн өдрүүдийн хоорондох утгуудыг объект Лист хэлбэрээр эрэмблэн буцаана.
        /// </summary>
        /// <param name="startDate"></param>
        /// <param name="endDate"></param>
        /// <returns></returns>
        public List<DateInfo> getByInterval(DateTime startDate, DateTime endDate)
        {
            List<DateInfo> filteredList = infos
            .Where(obj => obj.date >= startDate && obj.date <= endDate)
            .ToList();
            return filteredList;
        }
        /// <summary>
        /// Датабаазаас хэрэглэгчийн мэдээллийг шалган true , false буцаана.
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public bool checkUser(string username, string password)
        {
            using (var connection = new SqliteConnection(connectionString))
            {
                connection.Open();
                using (var command = connection.CreateCommand())
                {
                    command.CommandText = "SELECT COUNT(*) FROM users WHERE username = @username AND password = @password";
                    command.Parameters.AddWithValue("@username", username);
                    command.Parameters.AddWithValue("@password", password);

                    int count = Convert.ToInt32(command.ExecuteScalar());

                    return count > 0;
                }
            }
        }
    }
    /// <summary>
    /// Өдөр өдрөөр нь объект болгон хадгалах зорилгоор үүсгэсэн класс
    /// </summary>
    public class DateInfo
    {
        public int id;
        public DateTime date;
        public string startTime;
        public string orsonTsag;
        public string endTime;

        public DateInfo(int Id, DateTime dt, string s, string o, string e)
        {
            id = Id;
            date = dt;
            startTime = s;
            orsonTsag = o;
            endTime = e;
        }
    }
    /// <summary>
    /// Хэрэглэгчийн мэдээллийг хадгалах зорилгоор тодорхойлсон класс
    /// </summary>
    public class User
    {
        public int id;
        public string ner;
        public string ovog;
        public string uureg;
        public string sys_shinjeec;
        public string suljee_ajiltan;
        public string email;
        public string utas;

        public User(int id, string ner, string ovog, string uureg, string sys_shinjec, string suljee_ajil, string email, string utas)
        {
            this.id = id;
            this.ner = ner;
            this.ovog = ovog;
            this.uureg = uureg;
            this.sys_shinjeec = sys_shinjec;
            this.suljee_ajiltan = suljee_ajil;
            this.email = email;
            this.utas = utas;
        }
    }

}
