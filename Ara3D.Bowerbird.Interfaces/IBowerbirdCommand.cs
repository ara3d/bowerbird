namespace Ara3D.Bowerbird.Interfaces
{
    public interface IBowerbirdCommand 
    {
        string Name { get; }
        void Execute(object argument);
    }
}    