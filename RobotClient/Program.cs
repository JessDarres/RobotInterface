using System.Net.Http.Headers;

namespace RobotClient {
   internal class Program {
      static async Task Main (string[] args) {
         using HttpClient client = new HttpClient ();
         client.BaseAddress = new Uri ("http://localhost:9011/RunProgram/p8.fx");
         client.DefaultRequestHeaders.Accept.Add (new MediaTypeWithQualityHeaderValue ("application/json"));
         for (; ; ) {
            Console.ForegroundColor = ConsoleColor.Gray;
            //Console.WriteLine ("gohome -> Goto the entered mode in home page");
            Console.WriteLine ("gohome -> Goto home page");
            Console.WriteLine ("runprogram -> Run program");
            Console.WriteLine ("1 -> Goto home page");
            Console.WriteLine ("2 -> Run p8 program");
            var input = Console.ReadLine ();
            var message = "http://localhost:9011/";
            switch (input?.ToLower ()) {
               //case "gohome":
               //   Console.WriteLine ("Please enter the mode number :");
               //   var modeNum = Console.ReadLine ();
               //   message += $"GoHome/{modeNum}";
               //   break;
               case "runprogram":
                  Console.WriteLine ("Please enter the program name :");
                  var name = Console.ReadLine ();
                  message += $"RunProgram/{name}";
                  break;
               case "gohome":
               case "1":
                  message += "GoHome/0";
                  break;
               case "2":
                  message += "RunProgram/p8";
                  break;
            }
            var response = await client.PostAsync (message, null);
            Console.ForegroundColor = response.StatusCode == System.Net.HttpStatusCode.OK ? ConsoleColor.Green : ConsoleColor.Red;
            Console.WriteLine (response.StatusCode);
         }
      }
   }
}
