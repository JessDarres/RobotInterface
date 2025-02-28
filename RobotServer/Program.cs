using System.IO.Pipes;
using System.Text;
using System.Text.Json;


namespace RobotServer {
   internal class Program {
      static void Main (string[] args) {
         var builder = WebApplication.CreateBuilder (args);
         builder.WebHost.UseUrls ("http://localhost:9011");
         builder.Services.AddEndpointsApiExplorer ();
         mApp = builder.Build ();
         mApp.MapPost ("/api/RightAngle/App/", async context => {
            using var request = new StreamReader (context.Request.Body);
            string content = await request.ReadToEndAsync ();
            var robotRequest = JsonSerializer.Deserialize<RobotRequest> (content, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            // Validate request data
            if (robotRequest == null || string.IsNullOrEmpty (robotRequest.nodeName)) {
               context.Response.StatusCode = 400; // Bad Request
               await context.Response.WriteAsync ("Invalid JSON body");
               return;
            }

            (string name, string value) = (robotRequest.nodeName, robotRequest.nodeValue ?? "");
            var ret = await Task.Run (() => SendMessage (name, value));
            context.Response.StatusCode = ret;
            await context.Response.WriteAsync ($"Status code {ret}");
         });
         mApp.Run ();
      }

      static int SendMessage (string message, string param = "") {
         int ret = 400;
         using var stm = new NamedPipeServerStream (PipeName, PipeDirection.InOut);
         try {
            stm.WaitForConnection ();
            var sendMsg = string.Join (',', [message, param]);
            var msgBytes = Encoding.UTF8.GetBytes (sendMsg);
            stm.Write (msgBytes);
            stm.WaitForPipeDrain ();
            byte[] buffer = new byte[256];
            int bytesRead;
            bytesRead = stm.Read (buffer, 0, buffer.Length);
            if (bytesRead > 0) {
               ret = Convert.ToInt16 (Encoding.UTF8.GetString (buffer, 0, bytesRead));
               Console.WriteLine ("Server : messages read from client: " + ret);
            }
         } catch (Exception) {
            return ret;
         }
         return ret;
      }

      const string PipeName = "RAPIPE";
      static WebApplication mApp;
   }

   public class RobotRequest {
      public string nodeName { get; set; }
      public string nodeValue { get; set; }
      public DateTime updateTime { get; set; }
   }
}
