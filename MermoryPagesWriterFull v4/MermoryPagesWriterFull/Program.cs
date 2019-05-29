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

            Layer[] layers = new Layer[1000];
            for (int i = 0; i < layers.Length; i++)
            {
                if (i<4) layers[i] = new Layer { Name = "Vrstva " + i, Color = Color.Red, Id = i, ParentId = -1 }; 
                else layers[i] = new Layer { Name = "Vrstva "+i, Color = Color.Red, Id = i, ParentId = i/20 };
            }

            Gis3DObject[] gis3DObjects = new Gis3DObject[10000];
            for (var j = 0; j < gis3DObjects.Length; j++)
            {
                gis3DObjects[j] =
                new Gis3DObject { Id = j, Name = "Gis3dObject " + j, Description = "Dlouhý popis objektu " + j + ". Dlouhý popis objektu " + j+". Dlouhý popis toho objektu. Dlouhý popis toho objektu. Dlouhý popis toho objektu.", ShortName = "o"+j, LayerId = 80+j%10, X = 300000.0F+100*j, Y = 5550000.0F - (j%10)*10000F, Height = 0F };
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
            Console.WriteLine(String.Format("Sending {0} layers and {1} gis objects", layers.Length, gis3DObjects.Length));
            sender.Send(layers, gis3DObjects);
            Console.WriteLine("Message processed");
        }

    }
}
