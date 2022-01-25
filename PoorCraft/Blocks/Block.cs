using OpenTK.Mathematics;
using System;
using System.Collections.Generic;

namespace PoorCraft.Blocks
{
    public abstract class Block
    {
        public virtual int Length { get; }
        public virtual float[] Data { get; }
        public virtual Vector3 Position { get; }

        public override bool Equals(object obj)
        {
            return obj is Block block &&
                   Length == block.Length &&
                   EqualityComparer<float[]>.Default.Equals(Data, block.Data) &&
                   Position.Equals(block.Position);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Length, Data, Position);
        }
    }
}
