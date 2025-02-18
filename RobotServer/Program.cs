using System.IO.Pipes;
using System.Text;

namespace RobotServer {
   internal class Program {
      static void Main (string[] args) {
         var builder = WebApplication.CreateBuilder (args);
         builder.WebHost.UseUrls ("http://localhost:9011");
         builder.Services.AddEndpointsApiExplorer ();
         var app = builder.Build ();
         //app.UseHttpsRedirection ();
         app.MapPost ("/api/RightAngle/App/GoHome", async () => {
            var ret = await Task.Run (() => SendMessage ("GoHome"));
            if (ret == "200") return Results.Ok ();
            else return Results.BadRequest ();
         });
         //app.MapPost ("GoHome/{mode}", async (string mode) => {
         //   var ret = await Task.Run (() => SendMessage ("GoHome", mode));
         //   if (ret == "200") return Results.Ok ();
         //   else return Results.BadRequest ();
         //});

         app.MapPost ("/api/RightAngle/App/RunProgram/{progName}", async (string progName) => {
            var ret = await Task.Run (() => SendMessage ("RunProgram", progName));
            if (ret == "200") return Results.Ok ();
            else return Results.BadRequest ();
         });
         app.Run ();
      }

      static string SendMessage (string message, string param = "") {
         var ret = "400";
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
               ret = Encoding.UTF8.GetString (buffer, 0, bytesRead);
               Console.WriteLine ("Server : messages read from client: " + ret);
            }
         } catch (Exception) {
            return ret;
         }
         return ret;
      }

      const string PipeName = "RAPIPE";
   }
}
