using MySql.Data.MySqlClient;
using System;
using System.Data;

public class CommonTools
{
    public static DataTable GetDataTable(string SQL,string ConnStr)
    {
        DataTable DT = new DataTable();
        MySqlConnection conn = new MySqlConnection(ConnStr);
        MySqlCommand command = conn.CreateCommand();
        command.CommandText = SQL;
        MySqlDataAdapter MDA = new MySqlDataAdapter(command.CommandText, conn);
        MDA.Fill(DT);
        return DT;
    }
    public static void doExecuteNonQuery(string SQL, string ConnStr)
    {
        MySqlConnection conn = new MySqlConnection(ConnStr);
        MySqlCommand command = conn.CreateCommand();
        command.CommandText = SQL;
        conn.Open();
        command.ExecuteNonQuery();
        conn.Close();
    }
}