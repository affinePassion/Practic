using System.Net.Sockets;
using System.Net;
using System.Text;
using System.Data.SqlClient;
using Newtonsoft.Json;
using Npgsql;
using static Npgsql.Replication.PgOutput.Messages.RelationMessage;
using System.Collections.Generic;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Newtonsoft.Json.Linq;
using System.Diagnostics.Metrics;
using Npgsql.Replication.PgOutput.Messages;

namespace Test;




class HttpServer
{
    private readonly HttpListener _listener;
    private readonly string _connectionString;

    public HttpServer(string connectionString)
    {
        _listener = new HttpListener();
        _connectionString = connectionString;
    }

    public void Start()
    {
        _listener.Prefixes.Add("http://localhost:8080/api/");
        _listener.Start();

        Console.WriteLine("Сервер запущен. Ожидание входящих запросов...");

        while (true)
        {
            HttpListenerContext context = _listener.GetContext();
            HandleRequest(context);
        }
    }

    public void Stop()
    {
        _listener.Stop();
    }

    private void HandleRequest(HttpListenerContext context)
    {
        string requestMethod = context.Request.HttpMethod;
        string requestUri = context.Request.Url.AbsolutePath;
        string requestBody = new StreamReader(context.Request.InputStream).ReadToEnd();

        switch (requestMethod)
        {
            case "GET":
                HandleGetRequest(requestUri, context);
                break;
            case "POST":
                HandlePostRequest(requestUri, requestBody, context);
                break;
            case "PUT":
                HandlePutRequest(requestUri, requestBody, context);
                break;
            case "DELETE":
                HandleDeleteRequest(requestUri, context);
                break;
            case "OPTIONS":
                HandleOptionsRequest(requestUri, context);
                break;
            default:
                context.Response.StatusCode = 405;
                context.Response.StatusDescription = "Ошибка запроса.";
                break;
        }
    }

    private void HandleOptionsRequest(string requestUri, HttpListenerContext context)
    {
        context.Response.StatusCode = 200;
        context.Response.StatusDescription = "OK";

        context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
        context.Response.Headers.Add("Access-Control-Allow-Methods", "*");

        context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(""));
    }
    private void HandleGetRequest(string requestUri, HttpListenerContext context)
    {
        string tablename = requestUri.Split('/')[2];
        try
        {

            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                var command = new NpgsqlCommand("SELECT * FROM " + tablename + " WHERE id = @id", conn);
                command.Parameters.AddWithValue("@id", int.Parse(requestUri.Split('/').Last()));
                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    context.Response.StatusCode = 200;
                    context.Response.StatusDescription = "OK";
                    context.Response.ContentType = "application/json";
                    var data = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        data.Add(reader.GetName(i), reader[i]);
                    }
                    string response = JsonConvert.SerializeObject(data);
                    context.Response.ContentLength64 = response.Length;
                    context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(response), 0, response.Length);
                }
                else
                {
                    HandlePostRequest(requestUri, "", context);
                }
            }
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.StatusDescription = "Internal Server Error";
            context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(ex.Message), 0, ex.Message.Length);
        }
    }

    private void HandlePostRequest(string requestUri, string requestBody, HttpListenerContext context)
    {
        string enter = JsonProcessing(requestUri, requestBody).Item1;
        switch (enter)
        {
            case "login":
                PostLogin(requestUri, requestBody, context);
                break;
            default:
                PostRegister(requestUri, requestBody, context);
                break;
        }


        
    }

    private void PostLogin(string requestUri, string requestBody, HttpListenerContext context)
    {
        string[] raw = JsonProcessing(requestUri, requestBody).Item2;

        try
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                var command = new NpgsqlCommand($"SELECT fio, role FROM workers WHERE id = {raw[0]}", conn);
                var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    context.Response.StatusCode = 200;
                    context.Response.StatusDescription = "OK";
                    context.Response.ContentType = "application/json";
                    var data = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        data.Add(reader.GetName(i), reader[i]);
                    }
                    string response = JsonConvert.SerializeObject(data);
                    context.Response.ContentLength64 = response.Length;
                    context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(response), 0, response.Length);
                }
            }
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 400;
            context.Response.StatusDescription = "Bad Request";
            context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(ex.Message), 0, ex.Message.Length);
        }
    }

    private void PostRegister(string requestUri, string requestBody, HttpListenerContext context)
    {
        string[] raw = JsonProcessing(requestUri, requestBody).Item2;

        try
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                var command = new NpgsqlCommand($"INSERT INTO workers (id, password, role) " +
                    $"VALUES ({raw[0]},'{raw[1]}','{raw[2]}')", conn);
                command.ExecuteNonQuery();
                context.Response.StatusCode = 201;
                context.Response.StatusDescription = "Created";
                string response = JsonConvert.SerializeObject("");
                context.Response.ContentLength64 = response.Length;
                context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(response), 0, response.Length);
            }
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 400;
            context.Response.StatusDescription = "Bad Request";
            context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(ex.Message), 0, ex.Message.Length);
        }
    }

    private void HandlePutRequest(string requestUri, string requestBody, HttpListenerContext context)
    {
        string tablename = JsonProcessing(requestUri, requestBody).Item1;
        string[] raw = JsonProcessing(requestUri, requestBody).Item2;

        try
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                var commandGetColumns = new NpgsqlCommand($"SELECT COLUMN_NAME FROM INFORMATION_SCHEMA.COLUMNS " +
                    $"WHERE TABLE_NAME = '{tablename}'", conn);

                var reader = commandGetColumns.ExecuteReader();
                List<string> columns = new List<string>();
                while (reader.Read())
                {
                    columns.Add(reader["COLUMN_NAME"].ToString());
                }

                ConnectionRebut(conn);

                var command = new NpgsqlCommand($"UPDATE {tablename} SET status = '{raw[SearchColumnStatus(columns)]}' " +
                    $"WHERE id = {raw[0]}", conn);
                command.ExecuteNonQuery();
                context.Response.StatusCode = 200;
                context.Response.StatusDescription = "OK";
                string response = JsonConvert.SerializeObject("");
                context.Response.ContentLength64 = response.Length;
                context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(response), 0, response.Length);
            }
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.StatusDescription = "Internal Server Error";
            context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(ex.Message), 0, ex.Message.Length);
        }
    }

    private void HandleDeleteRequest(string requestUri, HttpListenerContext context)
    {
        string tablename = requestUri.Split('/')[2];
        string deletedId = requestUri.Split("/")[3];
        try
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                var command = new NpgsqlCommand($"DELETE FROM {tablename} WHERE id = {deletedId}", conn);
                command.ExecuteNonQuery();
                context.Response.StatusCode = 200;
                context.Response.StatusDescription = "OK";
                string response = JsonConvert.SerializeObject("");
                context.Response.ContentLength64 = response.Length;
                context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(response), 0, response.Length);
            }
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.StatusDescription = "Internal Server Error";
            context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(ex.Message), 0, ex.Message.Length);
        }
    }


    private int SearchColumnStatus(List<string> columns)
    {
        for (int i = 0; i < columns.Count; i++)
            if (columns[i] == "status") return i;
        return -1;
    }

    private (string, string[]) JsonProcessing(string requestUri, string requestBody)
    {
        string url = requestUri.Split('/')[2];
        //string id = requestUri.Split('/')[3];
        string[] raw = JsonToValues(requestBody);
        int numRaws = raw.Length;
        int newId = int.Parse(raw[0]);
        for (int i = 0; i < numRaws; i++)
        {
            raw[i] = raw[i].Replace('.', '/');
            raw[i] = raw[i].Replace(" 0:00:00", "");
        }


        return (url, raw);
    }

    private string ForPostRequest(string[] columns, string[] values, string tablename, int newId)
    {
        string result = "INSERT INTO " + tablename + " (";

        for (int i = 0; i < columns.Length; i++)
        {
            result += columns[i];
            if (i < columns.Length - 1)
            {
                result += ", ";
            }
        }

        result += $") VALUES ({newId},";


        for (int i = 1; i < values.Length; i++)
        {
            result += "'" + values[i] + "'";
            if (i < values.Length - 1)
            {
                result += ", ";
            }
        }

        result += ");";

        return result;
    }

    private void ConnectionRebut(NpgsqlConnection conn)
    {
        conn.Close();
        conn.Open();
    }

    private static string[] JsonToValues(string json)
    {
        dynamic dyn = JsonConvert.DeserializeObject(json);
        List<string> values = new List<string>();

        foreach (var prop in dyn)
        {
            values.Add(prop.Value.ToString());
        }



        return values.ToArray();
    }
}

public class Testings
{
    const string connString = "Server=82.179.140.18; Port=5432; Database=parctic; User Id=mpi; Password=135a1;SSLMode=Prefer";
    static void Main()
    {
        

        HttpServer server = new HttpServer(connString);
        server.Start();



        server.Stop();
    }
}