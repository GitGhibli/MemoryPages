using ECC.Opsu.Gis3D.Contract;
using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace MemoryPagesWriterFull
{
    class Program
    {
        static object ConsoleLock = new object();
        
        [DllImport("Kernel32")]
        private static extern bool SetConsoleCtrlHandler(EventHandler handler, bool add);

        private delegate bool EventHandler(CtrlType sig);
        static EventHandler _handler;

        private static Thread ReceiverThread;

        private enum CtrlType
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT = 1,
            CTRL_CLOSE_EVENT = 2,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT = 6
        }

        private static bool Handler(CtrlType signal)
        {
            switch (signal)
            {
                case CtrlType.CTRL_BREAK_EVENT:
                case CtrlType.CTRL_C_EVENT:
                case CtrlType.CTRL_LOGOFF_EVENT:
                case CtrlType.CTRL_SHUTDOWN_EVENT:
                case CtrlType.CTRL_CLOSE_EVENT:
                    Console.WriteLine("Closing");
                    FeedbackMessageReceiver.Run = false;
                    ReceiverThread.Join();
                    Environment.Exit(0);
                    return false;

                default:
                    return false;
            }
        }

        static void Main(string[] args)
        {
            ReceiverThread = new FeedbackMessageReceiver(ConsoleLock).Listen();

            // Register the handler
            _handler += new EventHandler(Handler);
            SetConsoleCtrlHandler(_handler, true);
            
            Layer[] layers = new Layer[100];
            for (int i = 0; i < layers.Length; i++)
            {
                layers[i] = new Layer { Name = "ÁČĎÉĚÍŇÓŘŠŤÚŮÝŽáčďéěíňóřšťúůýž", Color = Color.Red, Id = i, ParentId = 9000 + i };
            }

            Gis3DObject[] gis3DObjects = new Gis3DObject[500];
            for (var j = 0; j < gis3DObjects.Length; j++)
            {
                gis3DObjects[j] =
                new Gis3DObject { Id = j, Name = "Gis3dObject " + j, Description = "1A,Á,B,C,Č,D,Ď,E,É,Ě,F,G,H,Ch,I,Í,J,K,L,M,N,Ň,O,Ó,P,Q,R,Ř,S,Š,T,Ť,U,Ú,Ů,V,W,X,Y,Ý,Z,Ž, 1A,Á,B,C,Č,D,Ď,E,É,Ě,F,G,H,Ch,I,Í,J,K,L,M,N,Ň,O,Ó,P,Q,R,Ř,S,Š,T,Ť,U,Ú,Ů,V,W,X,Y,Ý,Z,Ž,1A,Á,B,C,Č,D,Ď,E,É,Ě,F,G,H,Ch,I,Í,J,K,L,M,N,Ň,O,Ó,P,Q,R,Ř,S,Š,T,Ť,U,Ú,Ů,V,W,X,Y,Ý,Z,Ž, 1A,Á,B,C,Č,D,Ď,E,É,Ě,F,G,H,Ch,I,Í,J,K,L,M,N,Ň,O,Ó,P,Q,R,Ř,S,Š,T,Ť,U,Ú,Ů,V,W,X,Y,Ý,Z,Ž, 1A,Á,B,C,Č,D,Ď,E,É,Ě,F,G,H,Ch,I,Í,J,K,L,M,N,Ň,O,Ó,P,Q,R,Ř,S,Š,T,Ť,U,Ú,Ů,V,W,X,Y,Ý,Z,Ž, 1A,Á,B,C,Č,D,Ď,E,É,Ě,F,G,H,Ch,I,Í,J,K,L,M,N,Ň,O,Ó,P,Q,R,Ř,S,Š,T,Ť,U,Ú,Ů,V,W,X,Y,Ý,Z,Ž,1A,Á,B,C,Č,D,Ď,E,É,Ě,F,G,H,Ch,I,Í,J,K,L,M,N,Ň,O,Ó,P,Q,R,Ř,S,Š,T,Ť,U,Ú,Ů,V,W,X,Y,Ý,Z,Ž, 1A,Á,B,C,Č,D,Ď,E,É,Ě,F,G,H,Ch,I,Í,J,K,L,M,N,Ň,O,Ó,P,Q,R,Ř,S,Š,T,Ť,U,Ú,Ů,V,W,X,Y,Ý,Z,Ž", ShortName = "o1", LayerId = 9027, X = 90.9F, Y = 27.27F, Height = 6.6F };
            }

            SetLayersAndObjects(layers, gis3DObjects);
            GoToLocationCycle();
        }

        private static void GoToLocationCycle()
        {
            int index = 1;
            float x = 1, y = 1;
            float floatingPart = 0.5F;

            using (var writer = new GoToLocationMMFWriter())
            {
                while (true)
                {
                    Console.WriteLine("To send 'Go to location message' press any key");
                    Console.ReadKey();
                    writer.Write(x * index + floatingPart, y * index * 2 + floatingPart);
                    Console.WriteLine("Message processed");
                    index++;
                }
            }
        }

        private static void SetLayersAndObjects(Layer[] layers, Gis3DObject[] gis3DObjects)
        {
            var sender = new InitialMessageSender();
            Console.WriteLine("To send initialization message press any key");
            Console.ReadKey();
            Console.WriteLine(String.Format("Sending {0} layers and {1} gis objects", layers.Length, gis3DObjects.Length));
            sender.Send(layers, gis3DObjects);
            Console.WriteLine("Message processed");
        }

    }
}
