using Client.Models;
using Microsoft.AspNetCore.SignalR.Client;
using System.Net.Http.Json;

namespace Client
{
    internal class Program
    {
        public static string host = "https://localhost:5001";
        static void Main(string[] args)
        {
            /*Console.WriteLine("Введите логин: ");
            string login = Console.ReadLine();
            Console.WriteLine("Введите пароль: ");
            string password = Console.ReadLine();*/


            //GetUserById(1);
            User user = new();

            user = Login("ivanov_ii", "123").Result;
            
            if (user.Token is not null) 
            {
                HubConnect(user.Name);
            };

            while (true) 
            {
                Console.WriteLine("Для завершения программы нажмите ESC");

                if (Console.ReadKey().Key == ConsoleKey.Escape) 
                {
                    break;
                }
            }
            


            
           
        }

        private static void HubConnect(string userName, string token)
        {
            HubConnection hubConnection;

            hubConnection = new HubConnectionBuilder()
                .WithUrl("https://localhost:5001/chat", options =>
                {
                    options.AccessTokenProvider = token;
                })
                .Build();

            

            Console.WriteLine("Вы вошли в чат");
            hubConnection.On<string, string>("Receive", (user, message) =>
            {

                Console.WriteLine($"{user} пишет: {message}");
            });


            hubConnection.StartAsync();



            while (true)
            {
                Console.WriteLine("Введите сообщение: ");
                string message = Console.ReadLine();
                hubConnection.InvokeAsync("Send", userName, message);
                if (Console.ReadKey().Key == ConsoleKey.Escape)
                {
                    break;
                };
            }


            hubConnection.StopAsync();
        }

        /* static async Task GetUserById(int id)
         {
             HttpClient httpClient = new HttpClient();

             var responce = await httpClient.GetFromJsonAsync($"https://localhost:5001/user/{id}", typeof(User));

             if (responce is User user) 
             {
                 Console.WriteLine(user.Name);
             }


         }*/

        static async Task<User> Login(string login, string password) 
        {
            HttpClient httpClient = new HttpClient();

            User userForCheck = new()
            {
                Login= login,
                Password = password
            };

            var response = await httpClient.PostAsJsonAsync("https://localhost:5001/login", userForCheck);

            Console.WriteLine(response.ToString());


            return await response.Content.ReadFromJsonAsync<User>();
        }

        static async Task<string> myToken() 
        {
            return "";
        }

        
    }
}