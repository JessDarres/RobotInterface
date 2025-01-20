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
            Brick.DisableBrickMechanism = false;
            Brick.Verbose = true;
            while (mRAInvoker == null) {
               var inv = Brick.GetAll<IRightAngleInvoker> ().ToList ();
               if (inv != null) mRAInvoker = inv[0];
            }
            var thr = new Thread (ListenForMessages);
            thr.Start ();
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
                  Console.WriteLine ("Received from pipe: " + message);
                  var msgs = message.Split (',');
                  var ret = false;
                  if (mRAInvoker != null) {
                     switch (msgs[0].ToLower ()) {
                        case "gohome":
                           int.TryParse (msgs[1], out int mode);
                           ret = mRAInvoker.GoHome (mode);
                           break;
                        case "runprogram":
                           ret = mRAInvoker.RunProgram (msgs[1]);
                           break;
                        default:
                           break;
                     }
                  }
                  var messageBytes = Encoding.UTF8.GetBytes (ret ? "200" : "400");
                  client.Write (messageBytes);
                  client.WaitForPipeDrain ();
                  Console.WriteLine ($"Client: cleint sent return code \"{ret}\" sucessfully");
               }
            } catch (TimeoutException) {
               Console.WriteLine ("Timeout occurred while connecting to pipe.");
            } catch (IOException ex) {
               Console.WriteLine ("Error: " + ex.Message);
            }
         }
      }

      public void Uninitialize () => Console.WriteLine ("UnInitialize RAPlug!");

      IRightAngleInvoker? mRAInvoker;
      static readonly object sLock = new ();
      const string PipeName = "RAPIPE";
   }
}
