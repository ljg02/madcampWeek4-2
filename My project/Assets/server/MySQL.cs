using System;
using MySql.Data.MySqlClient;
using UnityEngine;


public class MySQL : MonoBehaviour
{
    private MySqlConnection connection;

    void Start()
    {
        // 데이터베이스 연결 문자열 설정
        string server = "34.64.63.49";
        string database = "insideOut";
        string user = "root";
        string password = "week4";
        string connectionString = $"Server={server};Database={database};User ID={user};Password={password};";

        // 연결 시도
        try
        {
            connection = new MySqlConnection(connectionString);
            connection.Open();
            Debug.Log("MySQL 데이터베이스 연결 성공!");
        }
        catch (Exception e)
        {
            Debug.LogError("MySQL 연결 실패: " + e.Message);
        }
    }

    void OnApplicationQuit()
    {
        if (connection != null)
        {
            connection.Close();
            Debug.Log("MySQL 연결 종료");
        }
    }

    public void GetUserData()
    {
        if (connection == null) return;

        try
        {
            string query = "SELECT * FROM users";
            MySqlCommand cmd = new MySqlCommand(query, connection);

            using (MySqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    string name = reader.GetString("name");
                    string email = reader.GetString("email");
                    int score = reader.GetInt32("score");

                    Debug.Log($"Name: {name}, Email: {email}, Score: {score}");
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError("데이터 읽기 오류: " + e.Message);
        }
    }

    public void InsertUserData(string name, string email, int score)
    {
        if (connection == null) return;

        try
        {
            string query = $"INSERT INTO users (name, email, score) VALUES ('{name}', '{email}', {score})";
            MySqlCommand cmd = new MySqlCommand(query, connection);

            int rowsAffected = cmd.ExecuteNonQuery();
            Debug.Log($"{rowsAffected}개의 데이터 삽입 성공!");
        }
        catch (Exception e)
        {
            Debug.LogError("데이터 삽입 오류: " + e.Message);
        }
    }
}
