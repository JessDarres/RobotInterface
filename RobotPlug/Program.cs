using System.Diagnostics;
using System.IO.Pipes;
using System.Text;
using Petrel;

namespace RobotPlug {
   [Brick]
   internal class Program : IInitializable {
      public static void Main () {
      }

      public void Initialize () {
         lock (sLock) {
            Console.WriteLine ("Initialize RAPlug");
            ProcessStartInfo processStartInfo = new ProcessStartInfo {
               FileName = "RobotServer.exe",
               Arguments = "",
               UseShellExecute = true
            };
            try {
               Process.Start (startInfo: processStartInfo);
               Console.WriteLine ("Robot server started successfully.");
            } catch (Exception ex) {
               Console.WriteLine ($"Error: {ex.Message}");
            }
            Brick.DisableBrickMechanism = false;
            Brick.Verbose = false;
            while (mRAInvoker == null) {
               var inv = Brick.GetAll<IRightAngleInvoker> ().ToList ();
               if (inv != null) mRAInvoker = inv[0];
            }
            Task task = Task.Run (() => ListenForMessages ());
         }
      }

      void ListenForMessages () {
         while (true) {
            using var client = new NamedPipeClientStream (".", PipeName, PipeDirection.InOut);
            try {
               client.Connect ();
               byte[] buffer = new byte[256];
               int bytesRead;
               bytesRead = client.Read (buffer, 0, buffer.Length);
               if (bytesRead > 0) {
                  string message = Encoding.UTF8.GetString (buffer, 0, bytesRead);
                  var msgs = message.Split (',');
                  var ret = false;
                  if (mRAInvoker != null) {
                     switch (msgs[0].ToLower ()) {
                        case "gohome":
                           ret = mRAInvoker.GoHome ();
                           break;
                        case "runprogram":
                           ret = mRAInvoker.RunProgram (msgs[1]);
                           break;
                        case "ismachineinstartmode":
                           ret = mRAInvoker.IsMachineInStartMode;
                           break;
                        default:
                           break;
                     }
                  }
                  var messageBytes = Encoding.UTF8.GetBytes (ret ? "200" : "400");
                  client.Write (messageBytes);
                  client.WaitForPipeDrain ();
               }
            } catch (TimeoutException) {
               Console.WriteLine ("Timeout occurred while connecting to pipe.");
            } catch (IOException ex) {
               Console.WriteLine ("Error: " + ex.Message);
            }
         }
      }

      public void Uninitialize () {
         var proc = Process.GetProcessesByName ("RobotServer");
         if (proc.Length > 0) proc[0].Kill ();
         Console.WriteLine ("UnInitialize RAPlug!");
      }

      #region Private Data ------------------------------------------
      IRightAngleInvoker? mRAInvoker;
      static readonly object sLock = new ();
      const string PipeName = "RAPIPE";
      #endregion
   }
}
