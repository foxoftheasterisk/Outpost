using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Xna.Framework;

namespace OutpostLibrary.Navigation
{
    public struct ChunkAddress
    {
        public string world;
        public IntVector3 position;

        public override string ToString()
        {
            return world + ": " + position.X + ", " + position.Y + ", " + position.Z;
        }

        /// <summary>
        /// Creates a ChunkAddress at the given chunk coordinates
        /// For worldspace positions, use the Vector3 constructor.
        /// </summary>
        /// <param name="_world">The world the chunk is in</param>
        /// <param name="_position">The coordinates of the chunk</param>
        public ChunkAddress(string _world, IntVector3 _position)
        {
            world = _world;
            position = _position;
        }

        /// <summary>
        /// Creates a ChunkAddress that contains the given position
        /// THIS IS WORLDSPACE - DO NOT PASS CHUNK COORDINATES IN THIS CONSTRUCTOR
        /// </summary>
        /// <param name="_world">The world the chunk is in</param>
        /// <param name="_position">A worldspace position contained within the chunk</param>
        public ChunkAddress(string _world, Vector3 _position)
        {
            world = _world;
            _position = _position / Sizes.ChunkSize;
            position = new IntVector3((int)Math.Floor(_position.X), (int)Math.Floor(_position.Y), (int)Math.Floor(_position.Z));
        }

        public Vector3 getWorldspacePosition()
        {
            return (Vector3)position * Sizes.ChunkSize;
        }

        public static ChunkAddress operator +(ChunkAddress ca, IntVector3 iv)
        {
            return new ChunkAddress(ca.world, ca.position + iv);
        }

        public static ChunkAddress operator -(ChunkAddress ca, IntVector3 iv)
        {
            return new ChunkAddress(ca.world, ca.position - iv);
        }
    }
}
