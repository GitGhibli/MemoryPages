using ECC.Opsu.Gis3D.Contract;
using System;
using System.Drawing;

namespace MemoryPagesWriterFull
{
    class Program
    {
        static object ConsoleLock = new object();

        static void Main(string[] args)
        {
            new FeedbackMessageReceiver(ConsoleLock).Listen();

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

        private static void WriteToConsole(string message)
        {
            //lock (ConsoleLock)
            //{
            //    Console.WriteLine(message);
            //}

            Console.WriteLine(message);
        }

        private static ConsoleKeyInfo ReadKeyFromConsole()
        {
            //lock (ConsoleLock)
            //{
            //    return Console.ReadKey();
            //}

            return Console.ReadKey();
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
                    WriteToConsole("To send 'Go to location message' press any key");
                    ReadKeyFromConsole();
                    writer.Write(x * index + floatingPart, y * index * 2 + floatingPart);
                    WriteToConsole("Message processed");
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
