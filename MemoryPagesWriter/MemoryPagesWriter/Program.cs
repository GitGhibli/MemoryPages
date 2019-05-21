using ECC.Opsu.Gis3D.Contract;
using System;
using System.Drawing;

namespace MemoryPagesWriter
{
    class Program
    {
        static void Main(string[] args)
        {
            Layer[] layers = new Layer[] {
                new Layer { Name = "Layer Name 1", Color = Color.Red, Id = 9027, ParentId = 8611 },
                new Layer {Name = "Layer Name 2", Color = Color.Green, Id = 7, ParentId = 9611},
                new Layer {Name = "Layer Name 2", Color = Color.Green, Id = 7, ParentId = 9611},
                new Layer {Name = "Layer Name 2", Color = Color.Green, Id = 7, ParentId = 9611},
                new Layer {Name = "Layer Name 2", Color = Color.Green, Id = 7, ParentId = 9611}
            };

            Gis3DObject[] gis3DObjects = new Gis3DObject[]
            {
                new Gis3DObject {Id = 1, Name = "Gis3dObject 1", Description = "Description of Gis3dObject1", ShortName = "o1", LayerId=9027, X = 90.9F, Y = 27.27F, Height = 6.6F},
                new Gis3DObject {Id = 2, Name = "Gis3dObject 2", Description = "Description of Gis3dObject2", ShortName = "o2", LayerId=5, X = 10.1F, Y = 27.27F, Height = 11.11F},
                new Gis3DObject {Id = 2, Name = "Gis3dObject 2", Description = "Description of Gis3dObject2", ShortName = "o2", LayerId=5, X = 10.1F, Y = 27.27F, Height = 11.11F},
                new Gis3DObject {Id = 2, Name = "Gis3dObject 2", Description = "Description of Gis3dObject2", ShortName = "o2", LayerId=5, X = 10.1F, Y = 27.27F, Height = 11.11F},
                new Gis3DObject {Id = 2, Name = "Gis3dObject 2", Description = "Description of Gis3dObject2", ShortName = "o2", LayerId=5, X = 10.1F, Y = 27.27F, Height = 11.11F},
                new Gis3DObject {Id = 2, Name = "Gis3dObject 2", Description = "Description of Gis3dObject2", ShortName = "o2", LayerId=5, X = 10.1F, Y = 27.27F, Height = 11.11F},
                new Gis3DObject {Id = 2, Name = "Gis3dObject 2", Description = "Description of Gis3dObject2", ShortName = "o2", LayerId=5, X = 10.1F, Y = 27.27F, Height = 11.11F},
                new Gis3DObject {Id = 2, Name = "Gis3dObject 2", Description = "Description of Gis3dObject2", ShortName = "o2", LayerId=5, X = 10.1F, Y = 27.27F, Height = 11.11F},
            };

            SetLayersAndObjects(layers, gis3DObjects);
            GoToLocationCycle();
        }

        private static void GoToLocationCycle()
        {
            int index = 1;
            float x = 1, y = 1;
            float floatingPart = 0.5F;

            using(var writer = new GoToLocationMMFWriter())
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
            using (var writer = new InitialMemoryPagesWriter(10000))
            {
                Console.WriteLine("To send initialization message press any key");
                Console.ReadKey();
                writer.Write(layers, gis3DObjects);
                Console.WriteLine("Message processed");
            }
        }

    }
}
