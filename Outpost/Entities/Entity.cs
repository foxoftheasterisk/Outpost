using Microsoft.Xna.Framework;
using OutpostLibrary.Navigation;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutpostEngine.Entities
{
    public class Entity
    {

        public ChunkAddress Chunk { get; protected set; }
        protected Vector3 posInChunk;
        public Vector3 PosInChunk => posInChunk;

        public Vector3 GetPosition()
        {
            return Chunk.getWorldspacePosition() + PosInChunk;
        }


        protected float pitch; //i.e. vertical rotation
        protected float yaw; //i.e. horizontal rotation

        //TODO:
        /*
        public getFacing()
        {

        }
        //*/
    }
}
