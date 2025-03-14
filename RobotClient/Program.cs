using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;

namespace RobotClient {
   internal class Program {
      //static async Task Main (string[] args) {
      //   var handler = new HttpClientHandler ();
      //   handler.DefaultProxyCredentials = CredentialCache.DefaultCredentials;
      //   using var client = new HttpClient (handler);
      //   client.BaseAddress = new Uri ("http://localhost:9011/");
      //   client.DefaultRequestHeaders.Accept.Add (new MediaTypeWithQualityHeaderValue ("application/json"));
      //   for (; ; ) {
      //      Console.ForegroundColor = ConsoleColor.Gray;
      //      //Console.WriteLine ("gohome -> Goto the entered mode in home page");
      //      Console.WriteLine ("gohome -> Goto home page");
      //      Console.WriteLine ("runprogram -> Run program");
      //      Console.WriteLine ("1 -> Goto home page");
      //      Console.WriteLine ("2 -> Run p8 program");
      //      var input = Console.ReadLine ();
      //      var message = "http://localhost:9011/api/RightAngle/App/";
      //      switch (input?.ToLower ()) {
      //         //case "gohome":
      //         //   Console.WriteLine ("Please enter the mode number :");
      //         //   var modeNum = Console.ReadLine ();
      //         //   message += $"GoHome/{modeNum}";
      //         //   break;
      //         case "runprogram":
      //            Console.WriteLine ("Please enter the program name :");
      //            var name = Console.ReadLine ();
      //            message += $"RunProgram/{name}";
      //            break;
      //         case "gohome":
      //         case "1":
      //            message += "GoHome";
      //            break;
      //         case "2":
      //            message += "RunProgram/p8";
      //            break;
      //      }
      //      var response = await client.PostAsync (message, null);
      //      Console.ForegroundColor = response.StatusCode == System.Net.HttpStatusCode.OK ? ConsoleColor.Green : ConsoleColor.Red;
      //      Console.WriteLine (response.StatusCode);
      //   }
      //}

      #region JSON file as a input
      //static async Task Main (string[] args) {
      //   // Path to the JSON file you want to send
      //   string filePath = "data.json"; // Make sure this file exists in your client application directory

      //   // Call the SendJsonFileToServer method to send the file
      //   await SendJsonFileToServer (filePath);
      //}

      //static async Task SendJsonFileToServer (string filePath) {
      //   try {
      //      // Create HttpClient instance
      //      using (var client = new HttpClient ()) {
      //         // Prepare the MultipartFormDataContent to send the file
      //         var formData = new MultipartFormDataContent ();

      //         // Read the file content into a byte array
      //         byte[] fileBytes = await File.ReadAllBytesAsync (filePath);
      //         var fileContent = new ByteArrayContent (fileBytes);

      //         // Set the appropriate content type for the file
      //         fileContent.Headers.ContentType = new MediaTypeHeaderValue ("application/json");

      //         // Add the file content to the form data (the server expects a field name "file")
      //         formData.Add (fileContent, "file", Path.GetFileName (filePath));

      //         // Define the server URL (change to match your server URL)
      //         string serverUrl = "http://localhost:9011/api/RightAngle/App/";

      //         // Send the POST request with the file
      //         var response = await client.PostAsync (serverUrl, formData);

      //         // Check if the response is successful
      //         if (response.IsSuccessStatusCode) {
      //            Console.WriteLine ("File successfully uploaded!");
      //            string responseContent = await response.Content.ReadAsStringAsync ();
      //            Console.WriteLine ("Server Response: " + responseContent);
      //         } else {
      //            Console.WriteLine ($"Failed to upload file. Status Code: {response.StatusCode}");
      //         }
      //      }
      //   } catch (Exception ex) {
      //      Console.WriteLine ($"An error occurred: {ex.Message}");
      //   }
      //}
      #endregion

      #region JSON body as a input
      static async Task Main () {
         Console.WriteLine ("Sending POST request...");
         for (; ; ) {
            Console.WriteLine ("1 - GoHome, 2 - RunProgram, 3 - IsMachineInStartMode, e - Exit");
            var ip = Console.ReadLine ();
            if (ip == "e") return;
            await SendPostRequest (ip);
         }
      }

      static async Task SendPostRequest (string name) {
         try {
            using (HttpClient client = new HttpClient ()) {
               var value = "";
               if (name == "2") {
                  Console.WriteLine ("Enter program name");
                  value = Console.ReadLine ();
               }

               var send = new {
                  nodeName = name switch {
                     "1" => "GoHome",
                     "2" => "RunProgram",
                     "3" => "IsMachineInStartMode",
                  },
                  nodeValue = value,
                  updateTime = DateTime.UtcNow
               };
               var nodeval = name == "2" ? send.nodeValue : "";
               Console.WriteLine ($"Method name : {send.nodeName} {send.nodeValue}");
               string json = JsonSerializer.Serialize (send);
               var content = new StringContent (json, Encoding.UTF8, "application/json");
               string serverUrl = "http://localhost:9011/api/RightAngle/App/";
               HttpResponseMessage response = await client.PostAsync (serverUrl, content);
               response.EnsureSuccessStatusCode ();

               string result = await response.Content.ReadAsStringAsync ();
               Console.WriteLine ($"POST Response: {result}\n");
            }
         } catch (Exception ex) {
            Console.WriteLine ("POST request failed: " + ex.Message);
         }
      }
      #endregion

      public class RobotRequest {
         public string nodeName { get; set; }
         public string nodeValue { get; set; }
         public DateTime updateTime { get; set; }
      }
   }
}
