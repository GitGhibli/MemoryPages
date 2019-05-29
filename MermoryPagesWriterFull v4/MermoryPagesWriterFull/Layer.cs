using System.Drawing;

namespace ECC.Opsu.Gis3D.Contract
{
    public class Layer
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Name { get; set; }
        public Color Color { get; set; }
    }
}
