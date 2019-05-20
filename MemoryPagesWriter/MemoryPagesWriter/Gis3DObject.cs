namespace ECC.Opsu.Gis3D.Contract
{
    public class Gis3DObject
    {
        public int Id { get; set; }
        public int LayerId { get; set; }

        public string ShortName { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }

        public float X { get; set; }
        public float Y { get; set; }
        public float Height { get; set; }
    }
}
