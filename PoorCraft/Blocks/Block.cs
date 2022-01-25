using OpenTK.Mathematics;

namespace PoorCraft.Blocks
{
    public abstract class Block
    {
        public virtual int Length { get; }
        public virtual float[] Data { get; }
        public virtual Vector3 Position { get; }
    }
}
