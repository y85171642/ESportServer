using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using MySql.Data.MySqlClient;
using System.Data;

namespace Esports.mysql
{

    public class MysqlConnection
    {
        MySqlConnection mysqlConnection;
        DataSet dataSet;
        string IP = null;
        string UserName = "root";
        string Password = "root";
        string Database = null;

        public MysqlConnection()
        {
            try
            {
                mysqlConnection = new MySqlConnection("datasource=20.0.0.20;username=root;password=root;database=sysinfo;charset=gb2312");
            }
            catch (MySqlException ex)
            {

            }
        }

        public MysqlConnection(string IP, string UserName, string Password, string Database)
        {
            try
            {
                string connectionString = "datasource=" + IP + ";username=" + UserName + ";password=" + Password + ";database=" + Database + ";charset=gb2312";
                mysqlConnection = new MySqlConnection(connectionString);
            }
            catch (MySqlException ex)
            {

            }
        }

        public string MysqlInfo()
        {
            string mysqlInfo = null;
            try
            {
                mysqlConnection.Open();
                mysqlInfo += "Connection Opened." + Environment.NewLine;
                mysqlInfo += "Connection String:" + mysqlConnection.ConnectionString.ToString() + Environment.NewLine;
                mysqlInfo += "Database:" + mysqlConnection.Database.ToString() + Environment.NewLine;
                mysqlInfo += "Connection ServerVersion:" + mysqlConnection.ServerVersion.ToString() + Environment.NewLine;
                mysqlInfo += "Connection State:" + mysqlConnection.State.ToString() + Environment.NewLine;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("MySqlException Error:" + ex.ToString());
            }
            finally
            {
                mysqlConnection.Close();
            }
            return mysqlInfo;
        }

        public int MysqlCommand(string MysqlCommand)
        {
            try
            {
                mysqlConnection.Open();
                Console.WriteLine("MysqlConnection Opened.");
                MySqlCommand mysqlCommand = new MySqlCommand(MysqlCommand, mysqlConnection);
                return mysqlCommand.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine("MySqlException Error:" + ex.ToString());
            }
            finally
            {
                mysqlConnection.Close();
            }
            return -1;
        }

        //
        public DataView MysqlDataAdapter(string table)
        {
            DataView dataView = new DataView();
            try
            {
                mysqlConnection.Open();
                MySqlDataAdapter mysqlDataAdapter = new MySqlDataAdapter("Select * from " + table, mysqlConnection);
                dataSet = new DataSet();
                mysqlDataAdapter.Fill(dataSet, table);
                dataView = dataSet.Tables[table].DefaultView;
            }
            catch (MySqlException ex)
            {

            }
            finally
            {
                mysqlConnection.Close();
            }
            return dataView;
        }
    }//end class

}