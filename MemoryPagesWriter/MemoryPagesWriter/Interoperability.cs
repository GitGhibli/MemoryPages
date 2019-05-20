namespace ECC.Opsu.Gis3D.Contract
{
    interface Interoperability
    {
        void SetLayersAndObjects(Layer[] layers, Gis3DObject[] objects);

        void GoToLocation(float x, float y);
    }
}
