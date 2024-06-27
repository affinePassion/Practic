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
using System.Threading.Tasks;

namespace Test;



/// <summary>
/// Класс - сервер для обработки http-запросов и передачи данных пользователю
/// </summary>
class HttpServer
{
    /// <summary>
    /// Слушатель - пользователь
    /// </summary>
    private readonly HttpListener _listener;

    /// <summary>
    /// Строка соединения с базой данных на postgres
    /// </summary>
    private readonly string _connectionString;


    public HttpServer(string connectionString)
    {
        _listener = new HttpListener();
        _connectionString = connectionString;
    }

    /// <summary>
    /// Запуск сервера, разрешение и предобработка запросов
    /// </summary>
    public void Start()
    {
        _listener.Prefixes.Add("http://localhost:8080/");
        _listener.Start();

        Console.WriteLine("Сервер запущен. Ожидание входящих запросов...");

        while (true)
        {
            HttpListenerContext context = _listener.GetContext();
            context.Response.Headers.Add("Access-Control-Allow-Origin", "*");
            context.Response.Headers.Add("Access-Control-Allow-Methods", "*");
            context.Response.Headers.Add("Access-Control-Allow-Headers", "*");
            HandleRequest(context);
        }
    }

    /// <summary>
    /// Остановка работы сервера
    /// </summary>
    public void Stop()
    {
        _listener.Stop();
    }

    /// <summary>
    /// Обработка дальнейших запросов, вычисление метода, url и body входящего запроса
    /// </summary>
    /// <param name="context"></param>
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

    /// <summary>
    /// Предобработка запросов, для включения в список разрешаемых запросов 
    /// все последующие POST, GET, PUT и DELETE запросы
    /// </summary>
    /// <param name="requestUri"></param>
    /// <param name="context"></param>
    private void HandleOptionsRequest(string requestUri, HttpListenerContext context)
    {
        context.Response.StatusCode = 200;
        context.Response.StatusDescription = "OK";

        context.Response.OutputStream.Close();
    }

    /// <summary>
    /// Обработка всех GET-запросов для получения информации Менеджеру
    /// </summary>
    /// <param name="requestUri"></param>
    /// <param name="context"></param>
    private void HandleGetRequest(string requestUri, HttpListenerContext context)
    {
        string tablename = requestUri.Split('/')[1];
        
        switch(tablename)
        {
            case "workers":
                WorkersGetRequest(tablename, context);
                break;
            case "materials":
                MaterialsGetRequest(tablename, context);
                break;
            case "materialdeliveries":
                DeliveriesGetRequest(tablename, context);
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Запрос на проверку рабочих на объектах
    /// </summary>
    /// <param name="tablename">таблица "workers"</param>
    /// <param name="context"></param>
    private void WorkersGetRequest(string tablename, HttpListenerContext context)
    {
        try
        {

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            var command = new NpgsqlCommand("SELECT * FROM " + tablename + " WHERE role = 'worker'", conn);
            var reader = command.ExecuteReader();
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            context.Response.ContentType = "application/json";
            var data = new List<Dictionary<string, object>>();
            while (reader.Read())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row.Add(reader.GetName(i), reader.GetValue(i));
                }
                data.Add(row);
            }
            string response = JsonConvert.SerializeObject(data);
            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(response);
            }
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.StatusDescription = "Internal Server Error";
            context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(ex.Message), 0, ex.Message.Length);
        }
    }

    /// <summary>
    /// Запрос на проверку материалов на объекте
    /// </summary>
    /// <param name="tablename">таблица "materials"</param>
    /// <param name="context"></param>
    private void MaterialsGetRequest(string tablename, HttpListenerContext context)
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            var command = new NpgsqlCommand("SELECT name, quantity, unit, deliverydate FROM " + tablename, conn);
            var reader = command.ExecuteReader();
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            context.Response.ContentType = "application/json";
            var data = new List<Dictionary<string, object>>();
            while (reader.Read())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row.Add(reader.GetName(i), reader.GetValue(i));
                }
                data.Add(row);
            }
            string response = JsonConvert.SerializeObject(data);
            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(response);
            }
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.StatusDescription = "Internal Server Error";
            context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(ex.Message), 0, ex.Message.Length);
        }
    }

    /// <summary>
    /// Запрос на проверку поставок на объект
    /// </summary>
    /// <param name="tablename">таблица "materialdeliveries"</param>
    /// <param name="context"></param>
    private void DeliveriesGetRequest(string tablename, HttpListenerContext context)
    {
        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            var command = new NpgsqlCommand("SELECT materialid, quantity, unit, deliverydate FROM " + tablename, conn);
            var reader = command.ExecuteReader();
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            context.Response.ContentType = "application/json";
            var data = new List<Dictionary<string, object>>();
            while (reader.Read())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                    row.Add(reader.GetName(i), reader.GetValue(i));

                data.Add(row);
            }

            string response = JsonConvert.SerializeObject(data);

            using var writer = new StreamWriter(context.Response.OutputStream);
            writer.Write(response);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.StatusDescription = "Internal Server Error";
            context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(ex.Message), 0, ex.Message.Length);
        }
    }

    /// <summary>
    /// Обработка POST-запросов в зависимости от uri запроса
    /// </summary>
    /// <param name="requestUri">url запроса</param>
    /// <param name="requestBody">body запроса</param>
    /// <param name="context"></param>
    private void HandlePostRequest(string requestUri, string requestBody, HttpListenerContext context)
    {
        string uri = JsonProcessing(requestUri, requestBody).Item1;
        switch (uri)
        {
            case "login":
                PostLogin(requestUri, requestBody, context);
                break;
            case "objects":
                PostObjects(requestUri, requestBody, context);
                break;
            case "register":
                PostRegister(requestUri, requestBody, context);
                break;
            case "tasks":
                PostTasks(requestUri, requestBody, context);
                break;
        } 
    }

    /// <summary>
    /// Обработка POST-запроса для логина, входа в приложение
    /// </summary>
    /// <param name="requestUri">url запроса = login</param>
    /// <param name="requestBody">body запроса = id пользователя в БД</param>
    /// <param name="context"></param>
    private void PostLogin(string requestUri, string requestBody, HttpListenerContext context)
    {
        string[] raw = JsonProcessing(requestUri, requestBody).Item2;

        try
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                var command = new NpgsqlCommand($"SELECT id, position, fio, role FROM workers WHERE id = {raw[0]}", conn);
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
                    using (var writer = new StreamWriter(context.Response.OutputStream))
                    {
                        writer.Write(response);
                    }
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

    /// <summary>
    /// Обработка POST-запроса для регистрации нового пользователя в приложение
    /// </summary>
    /// <param name="requestUri">url запроса = register</param>
    /// <param name="requestBody">body запроса = данные нового пользователя, заполненные на сайте</param>
    /// <param name="context"></param>
    private void PostRegister(string requestUri, string requestBody, HttpListenerContext context)
    {
        string[] raw = JsonProcessing(requestUri, requestBody).Item2;

        try
        {
            using (var conn = new NpgsqlConnection(_connectionString))
            {
                conn.Open();
                var command = new NpgsqlCommand($"INSERT INTO workers (id, password, fio, position, dateofbirth, role) " +
                    $"VALUES ({raw[0]},'{raw[1]}','{raw[2]}', '{raw[3]}', '{raw[4]}', '{raw[5]}')", conn);
                command.ExecuteNonQuery();
                context.Response.StatusCode = 201;
                context.Response.StatusDescription = "Created";
                string response = JsonConvert.SerializeObject(raw);
                context.Response.ContentLength64 = response.Length;
                context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(response), 0, response.Length);
            }
        }
        catch(Exception ex)
        {
            context.Response.StatusCode = 400;
            context.Response.StatusDescription = "Bad Request";
            string response = "Ошибка: Пользователь с таким ID уже существует.";
            context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(response), 0, response.Length);
        }
    }

    /// <summary>
    /// Обработка POST-запроса для получения информации об объектах, на которые назначен рабочий
    /// </summary>
    /// <param name="requestUri">url запроса = objects</param>
    /// <param name="requestBody">id рабочего, который назначен на какие-то объекты</param>
    /// <param name="context"></param>
    private void PostObjects(string requestUri, string requestBody, HttpListenerContext context)
    {
        string[] raw = JsonProcessing(requestUri, requestBody).Item2;

        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            var command = new NpgsqlCommand($"SELECT co.id, co.name, co.status, co.address, co.enddate FROM public.workers w " +
                $"JOIN public.taskexecutions te ON w.id = te.workerid " +
                $"JOIN public.tasks t ON te.taskid = t.id " +
                $"JOIN public.constructionobjects co ON t.constructionobjectid = co.id " +
                $"WHERE w.id = {raw[0]};", conn);
            var reader = command.ExecuteReader();
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            context.Response.ContentType = "application/json";
            var data = new List<Dictionary<string, object>>();
            while (reader.Read())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row.Add(reader.GetName(i), reader.GetValue(i));
                }
                data.Add(row);
            }
            string response = JsonConvert.SerializeObject(data);
            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(response);
            }
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 400;
            context.Response.StatusDescription = "Bad Request";
            context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(ex.Message), 0, ex.Message.Length);
        }
    }

    /// <summary>
    /// Обработка POST-запроса для получения текущих задач на объекте
    /// </summary>
    /// <param name="requestUri">url запроса = tasks</param>
    /// <param name="requestBody">body запроса = id объекта</param>
    /// <param name="context"></param>
    private void PostTasks(string requestUri, string requestBody, HttpListenerContext context)
    {
        string[] raw = JsonProcessing(requestUri, requestBody).Item2;

        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            var command = new NpgsqlCommand($"SELECT id, name, description, enddate, status " +
                $"FROM tasks WHERE constructionobjectid = {raw[0]};", conn);
            var reader = command.ExecuteReader();
            context.Response.StatusCode = 200;
            context.Response.StatusDescription = "OK";
            context.Response.ContentType = "application/json";
            var data = new List<Dictionary<string, object>>();
            while (reader.Read())
            {
                var row = new Dictionary<string, object>();
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    row.Add(reader.GetName(i), reader.GetValue(i));
                }
                data.Add(row);
            }
            string response = JsonConvert.SerializeObject(data);
            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                writer.Write(response);
            }
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 400;
            context.Response.StatusDescription = "Bad Request";
            context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(ex.Message), 0, ex.Message.Length);
        }
    }

    /// <summary>
    /// Обработка PUT запроса для обновления данных в БД
    /// </summary>
    /// <param name="requestUri">url запроса = tasks-update</param>
    /// <param name="requestBody">body запроса = id задачи</param>
    /// <param name="context"></param>
    private void HandlePutRequest(string requestUri, string requestBody, HttpListenerContext context)
    {
        string[] raw = JsonProcessing(requestUri, requestBody).Item2;

        try
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
                var command = new NpgsqlCommand($"UPDATE public.tasks SET status = '{raw[1]}' " +
                    $"WHERE id = {raw[0]};", conn);
                command.ExecuteNonQuery();
                context.Response.StatusCode = 200;
                context.Response.StatusDescription = "OK";
                string response = JsonConvert.SerializeObject(raw);
                context.Response.ContentLength64 = response.Length;
                context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(response), 0, response.Length);
        }
        catch (Exception ex)
        {
            context.Response.StatusCode = 500;
            context.Response.StatusDescription = "Internal Server Error";
            context.Response.OutputStream.Write(Encoding.UTF8.GetBytes(ex.Message), 0, ex.Message.Length);
        }
    }

    /// <summary>
    /// Обработка DELETE-запроса для удаления ненужных или неверных данных в БД
    /// </summary>
    /// <param name="requestUri"></param>
    /// <param name="context"></param>
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

    /// <summary>
    /// Вспомогательный метод, отвечающий за обработку JSON объекта для получения массива данных raw
    /// </summary>
    /// <param name="requestUri">url запроса</param>
    /// <param name="requestBody">body запроса</param>
    /// <returns>распаршенное url и body входящего http-запроса</returns>
    private (string, string[]) JsonProcessing(string requestUri, string requestBody)
    {
        string url = requestUri.Split('/')[1];

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

    /// <summary>
    /// Вспомогательный метод, используемый для десериализации данных в массив
    /// </summary>
    /// <param name="json"></param>
    /// <returns></returns>
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