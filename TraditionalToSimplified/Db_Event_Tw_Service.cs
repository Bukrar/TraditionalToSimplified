using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Web;

namespace TraditionalToSimplified
{
    public class Db_Event_Tw_Service
    {
        Utility utility = new Utility();
        public void UpdateTableMy_Event_Category_Tw()
        {
            Console.WriteLine("處理Db_Event_Tw資料庫 資料表:my_event_category_tw中...");
            //呼叫json資源
            var configuration = Utility.GetJson();

            List<Model.My_Event_Category_Tw> modelList = new List<Model.My_Event_Category_Tw>();

            //連接繁體資料庫
            MySqlConnection Db_Tw_SqlConnection = new MySqlConnection(configuration.GetSection("db:1:connectString").Value);
            try
            {
                Db_Tw_SqlConnection.Open();
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 1045:
                        Console.WriteLine("使用者帳號或密碼錯誤");
                        break;
                    default:
                        Console.WriteLine("無法連線到資料庫.");
                        break;
                }
            }

            //DB資料DECODE 繁體=>簡體 在ENCODE編碼在更新資料庫
            try
            {
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT Code,Name FROM DB_EVENT_TW.my_event_category_tw", Db_Tw_SqlConnection))
                {
                    MySqlDataReader myData = mySqlCommand.ExecuteReader();
                    if (!myData.HasRows)
                    {
                        Console.WriteLine("No data.");
                    }
                    else
                    {
                        while (myData.Read())
                        {
                            if (!myData.IsDBNull(1))
                            {
                                string pk = myData.GetString(0);
                                string traditionalContent = HttpUtility.UrlDecode(myData.GetString(1), Encoding.GetEncoding("big5"));
                                string simplifiedContent = utility.ToSimplified(traditionalContent, "ToSimplified");
                                string simplifiedEncodeContent = HttpUtility.UrlEncode(simplifiedContent, Encoding.GetEncoding("gb18030"));

                                Model.My_Event_Category_Tw modelData = new Model.My_Event_Category_Tw();
                                modelData.Code = pk;
                                modelData.Name = simplifiedEncodeContent;
                                modelList.Add(modelData);
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_EVENT_TW TABLE: my_event_category_tw 需轉換的欄位為NULL值的PK: " + myData.GetString(0));
                            }
                        }
                    }
                }
                Db_Tw_SqlConnection.Close();

                //連接簡體資料庫
                MySqlConnection Db_Cn_SqlConnection = new MySqlConnection(configuration.GetSection("db:3:connectString").Value);
                try
                {
                    Db_Cn_SqlConnection.Open();
                }
                catch (MySqlException ex)
                {
                    switch (ex.Number)
                    {
                        case 1045:
                            Console.WriteLine("使用者帳號或密碼錯誤");
                            break;
                        default:
                            Console.WriteLine("無法連線到資料庫.");
                            break;
                    }
                }

                foreach (var s in modelList)
                {
                    using (MySqlCommand updatMySqlCommand =
                        new MySqlCommand("update " + configuration.GetSection("db:3:dbname").Value +
                                         "." + configuration.GetSection("db:3:tables:my_event_category_tw").Key +
                                         " set " + configuration.GetSection("db:3:tables:my_event_category_tw:1").Value +
                                         "= @Name WHERE " + configuration.GetSection("db:3:tables:my_event_category_tw:0").Value +
                                         "= @Code", Db_Cn_SqlConnection))
                    {
                        updatMySqlCommand.Parameters.AddWithValue("@Name", s.Name);
                        updatMySqlCommand.Parameters.AddWithValue("@Code", s.Code);
                        updatMySqlCommand.ExecuteNonQuery();
                    }
                }
                Db_Cn_SqlConnection.Close();
                Console.WriteLine("DB_EVENT_CN資料庫 資料表:my_event_category_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Event_Press_Tw()
        {
            Console.WriteLine("處理Db_Event_Tw資料庫 資料表:my_event_press_tw中...");
            //呼叫json資源
            var configuration = Utility.GetJson();

            List<Model.My_Event_Press_Tw> modelList = new List<Model.My_Event_Press_Tw>();

            //連接繁體資料庫
            MySqlConnection Db_Tw_SqlConnection = new MySqlConnection(configuration.GetSection("db:1:connectString").Value);
            try
            {
                Db_Tw_SqlConnection.Open();
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 1045:
                        Console.WriteLine("使用者帳號或密碼錯誤");
                        break;
                    default:
                        Console.WriteLine("無法連線到資料庫.");
                        break;
                }
            }

            //DB資料DECODE 繁體=>簡體 在ENCODE編碼在更新資料庫
            try
            {
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT Press_SEQ,Press_Title,Press_Content FROM DB_EVENT_TW.my_event_press_tw", Db_Tw_SqlConnection))
                {
                    MySqlDataReader myData = mySqlCommand.ExecuteReader();
                    if (!myData.HasRows)
                    {
                        Console.WriteLine("No data.");
                    }
                    else
                    {
                        while (myData.Read())
                        {
                            string pk = myData.GetString(0);
                            string traditionalTitleContent, simplifiedTitleContent, simplifiedEncodeTitleContent = null;
                            string traditionalContent, simplifiedContent, simplifiedEncodeContent = null;
                            if (!myData.IsDBNull(1))
                            {
                                traditionalTitleContent = HttpUtility.UrlDecode(myData.GetString(1), Encoding.GetEncoding("big5"));
                                simplifiedTitleContent = utility.ToSimplified(traditionalTitleContent, "ToSimplified");
                                simplifiedEncodeTitleContent = HttpUtility.UrlEncode(simplifiedTitleContent, Encoding.GetEncoding("gb18030"));
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_EVENT_TW TABLE: my_event_press_tw 需轉換的欄位為NULL值的PK: " + myData.GetString(0));
                            }
                            if (!myData.IsDBNull(2))
                            {
                                traditionalContent = HttpUtility.UrlDecode(myData.GetString(2), Encoding.GetEncoding("big5"));
                                simplifiedContent = utility.ToSimplified(traditionalContent, "ToSimplified");
                                simplifiedEncodeContent = HttpUtility.UrlEncode(simplifiedContent, Encoding.GetEncoding("gb18030"));
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_EVENT_TW TABLE: my_event_press_tw 需轉換的欄位為NULL值的PK: " + myData.GetString(0));
                            }
                            Model.My_Event_Press_Tw modelData = new Model.My_Event_Press_Tw();
                            modelData.Press_SEQ = pk;
                            modelData.Press_Title = simplifiedEncodeTitleContent;
                            modelData.Press_Content = simplifiedEncodeContent;
                            modelList.Add(modelData);
                        }
                    }
                }

                Db_Tw_SqlConnection.Close();

                //連接簡體資料庫
                MySqlConnection Db_Cn_SqlConnection = new MySqlConnection(configuration.GetSection("db:3:connectString").Value);
                try
                {
                    Db_Cn_SqlConnection.Open();
                }
                catch (MySqlException ex)
                {
                    switch (ex.Number)
                    {
                        case 1045:
                            Console.WriteLine("使用者帳號或密碼錯誤");
                            break;
                        default:
                            Console.WriteLine("無法連線到資料庫.");
                            break;
                    }
                }

                foreach (var s in modelList)
                {
                    using (MySqlCommand updatMySqlCommand =
                         new MySqlCommand("update " + configuration.GetSection("db:3:dbname").Value +
                                         "." + configuration.GetSection("db:3:tables:my_event_press_tw").Key +
                                         " set " + configuration.GetSection("db:3:tables:my_event_press_tw:1").Value +
                                         "= @Press_Title," + configuration.GetSection("db:3:tables:my_event_press_tw:2").Value +
                                         "= @Press_Content WHERE " + configuration.GetSection("db:3:tables:my_event_press_tw:0").Value +
                                         "= @Press_SEQ", Db_Cn_SqlConnection))
                    {
                        updatMySqlCommand.Parameters.AddWithValue("@Press_Title", s.Press_Title);
                        updatMySqlCommand.Parameters.AddWithValue("@Press_Content", s.Press_Content);
                        updatMySqlCommand.Parameters.AddWithValue("@Press_SEQ", s.Press_SEQ);
                        updatMySqlCommand.ExecuteNonQuery();                 
                    }
                }
                Db_Cn_SqlConnection.Close();
                Console.WriteLine("DB_EVENT_CN資料庫 資料表:my_event_press_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Event_Region_Tw()
        {
            Console.WriteLine("處理Db_Event_Tw資料庫 資料表:my_event_region_tw中...");
            //呼叫json資源
            var configuration = Utility.GetJson();

            List<Model.My_Event_Region_Tw> modelList = new List<Model.My_Event_Region_Tw>();

            //連接繁體資料庫
            MySqlConnection Db_Tw_SqlConnection = new MySqlConnection(configuration.GetSection("db:1:connectString").Value);
            try
            {
                Db_Tw_SqlConnection.Open();
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 1045:
                        Console.WriteLine("使用者帳號或密碼錯誤");
                        break;
                    default:
                        Console.WriteLine("無法連線到資料庫.");
                        break;
                }
            }

            //DB資料DECODE 繁體=>簡體 在ENCODE編碼在更新資料庫
            try
            {
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT Code,Name FROM DB_EVENT_TW.my_event_region_tw", Db_Tw_SqlConnection))
                {
                    MySqlDataReader myData = mySqlCommand.ExecuteReader();
                    if (!myData.HasRows)
                    {
                        Console.WriteLine("No data.");
                    }
                    else
                    {
                        while (myData.Read())
                        {
                            if (!myData.IsDBNull(1))
                            {
                                string pk = myData.GetString(0);
                                string traditionalContent = HttpUtility.UrlDecode(myData.GetString(1), Encoding.GetEncoding("big5"));
                                string simplifiedContent = utility.ToSimplified(traditionalContent, "ToSimplified");
                                string simplifiedEncodeContent = HttpUtility.UrlEncode(simplifiedContent, Encoding.GetEncoding("gb18030"));

                                Model.My_Event_Region_Tw modelData = new Model.My_Event_Region_Tw();
                                modelData.Code = pk;
                                modelData.Name = simplifiedEncodeContent;
                                modelList.Add(modelData);
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_EVENT_TW TABLE: my_event_region_tw 需轉換的欄位為NULL值的PK: " + myData.GetString(0));
                            }
                        }
                    }
                }

                Db_Tw_SqlConnection.Close();

                //連接簡體資料庫
                MySqlConnection Db_Cn_SqlConnection = new MySqlConnection(configuration.GetSection("db:3:connectString").Value);
                try
                {
                    Db_Cn_SqlConnection.Open();
                }
                catch (MySqlException ex)
                {
                    switch (ex.Number)
                    {
                        case 1045:
                            Console.WriteLine("使用者帳號或密碼錯誤");
                            break;
                        default:
                            Console.WriteLine("無法連線到資料庫.");
                            break;
                    }
                }

                foreach (var s in modelList)
                {
                    using (MySqlCommand updatMySqlCommand =
                         new MySqlCommand("update " + configuration.GetSection("db:3:dbname").Value +
                                         "." + configuration.GetSection("db:3:tables:my_event_region_tw").Key +
                                         " set " + configuration.GetSection("db:3:tables:my_event_region_tw:1").Value +
                                         "= @Name WHERE " + configuration.GetSection("db:3:tables:my_event_region_tw:0").Value +
                                         "= @Code", Db_Cn_SqlConnection))
                    {
                        updatMySqlCommand.Parameters.AddWithValue("@Name", s.Name);
                        updatMySqlCommand.Parameters.AddWithValue("@Code", s.Code);
                        updatMySqlCommand.ExecuteNonQuery();
                    }
                }
                Db_Cn_SqlConnection.Close();
                Console.WriteLine("DB_EVENT_CN資料庫 資料表:my_event_region_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }

        public void UpdateTableMy_Partnar_Category_Tw()
        {
            Console.WriteLine("處理Db_Event_Tw資料庫 資料表:my_partnar_category_tw中...");
            //呼叫json資源
            var configuration = Utility.GetJson();

            List<Model.My_Partnar_Category_Tw> modelList = new List<Model.My_Partnar_Category_Tw>();

            //連接繁體資料庫
            MySqlConnection Db_Tw_SqlConnection = new MySqlConnection(configuration.GetSection("db:1:connectString").Value);
            try
            {
                Db_Tw_SqlConnection.Open();
            }
            catch (MySqlException ex)
            {
                switch (ex.Number)
                {
                    case 1045:
                        Console.WriteLine("使用者帳號或密碼錯誤");
                        break;
                    default:
                        Console.WriteLine("無法連線到資料庫.");
                        break;
                }
            }

            //DB資料DECODE 繁體=>簡體 在ENCODE編碼在更新資料庫
            try
            {
                using (MySqlCommand mySqlCommand =
                    new MySqlCommand("SELECT Code,Name,Title FROM DB_EVENT_TW.my_partnar_category_tw", Db_Tw_SqlConnection))
                {
                    MySqlDataReader myData = mySqlCommand.ExecuteReader();
                    if (!myData.HasRows)
                    {
                        Console.WriteLine("No data.");
                    }
                    else
                    {
                        while (myData.Read())
                        {
                            string pk = myData.GetString(0);
                            string traditionalNameContent, simplifiedNameContent, simplifiedEncodeNameContent = null;
                            string traditionalTitleContent, simplifiedTitleContent, simplifiedEncodeTitleContent = null;
                            if (!myData.IsDBNull(1))
                            {
                                traditionalNameContent = HttpUtility.UrlDecode(myData.GetString(1), Encoding.GetEncoding("big5"));
                                simplifiedNameContent = utility.ToSimplified(traditionalNameContent, "ToSimplified");
                                simplifiedEncodeNameContent = HttpUtility.UrlEncode(simplifiedNameContent, Encoding.GetEncoding("gb18030"));
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_EVENT_TW TABLE: my_partnar_category_tw 需轉換的欄位為NULL值的PK: " + myData.GetString(0));
                            }
                            if (!myData.IsDBNull(2))
                            {
                                traditionalTitleContent = HttpUtility.UrlDecode(myData.GetString(2), Encoding.GetEncoding("big5"));
                                simplifiedTitleContent = utility.ToSimplified(traditionalTitleContent, "ToSimplified");
                                simplifiedEncodeTitleContent = HttpUtility.UrlEncode(simplifiedTitleContent, Encoding.GetEncoding("gb18030"));
                            }
                            else
                            {
                                Console.WriteLine("DB:DB_EVENT_TW TABLE: my_partnar_category_tw 需轉換的欄位為NULL值的PK: " + myData.GetString(0));
                            }
                            Model.My_Partnar_Category_Tw modelData = new Model.My_Partnar_Category_Tw();
                            modelData.Code = pk;
                            modelData.Name = simplifiedEncodeNameContent;
                            modelData.Title = simplifiedEncodeTitleContent;
                            modelList.Add(modelData);
                        }
                    }
                }

                Db_Tw_SqlConnection.Close();

                //連接簡體資料庫
                MySqlConnection Db_Cn_SqlConnection = new MySqlConnection(configuration.GetSection("db:3:connectString").Value);
                try
                {
                    Db_Cn_SqlConnection.Open();
                }
                catch (MySqlException ex)
                {
                    switch (ex.Number)
                    {
                        case 1045:
                            Console.WriteLine("使用者帳號或密碼錯誤");
                            break;
                        default:
                            Console.WriteLine("無法連線到資料庫.");
                            break;
                    }
                }

                foreach (var s in modelList)
                {
                    using (MySqlCommand updatMySqlCommand =
                        new MySqlCommand("update " + configuration.GetSection("db:3:dbname").Value +
                                         "." + configuration.GetSection("db:3:tables:my_partnar_category_tw").Key +
                                         " set " + configuration.GetSection("db:3:tables:my_partnar_category_tw:1").Value +
                                         "= @Name," + configuration.GetSection("db:3:tables:my_partnar_category_tw:2").Value +
                                         "= @Title WHERE " + configuration.GetSection("db:3:tables:my_partnar_category_tw:0").Value +
                                         "= @Code", Db_Cn_SqlConnection))
                    {
                        updatMySqlCommand.Parameters.AddWithValue("@Name", s.Name);
                        updatMySqlCommand.Parameters.AddWithValue("@Title", s.Title);
                        updatMySqlCommand.Parameters.AddWithValue("@Code", s.Code);
                        updatMySqlCommand.ExecuteNonQuery();
                    }
                }
                Db_Cn_SqlConnection.Close();
                Console.WriteLine("DB_EVENT_CN資料庫 資料表:my_partnar_category_tw 資料處理完成");
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("Error " + ex.Number + " : " + ex.Message);
            }
        }
    }
}
