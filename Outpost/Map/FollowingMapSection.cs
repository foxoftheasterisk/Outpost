using OutpostEngine.Entities;
using OutpostLibrary.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutpostEngine.Map
{
    public class FollowingMapSection : MapSection
    {
        Entity following;
        int allowedStray;

        public FollowingMapSection(ChunkAddress _center, int _radius, LoadState _targetState, Entity _following, int _allowedStray) : base(_center, _radius, _targetState)
        {
            following = _following;
            allowedStray = _allowedStray;
        }

        public FollowingMapSection(string world, OutpostLibrary.IntVector3 _center, int _radius, LoadState _targetState, Entity _following, int _allowedStray)
            : this(new ChunkAddress(world, _center), _radius, _targetState, _following, _allowedStray)
        { }

        public FollowingMapSection(string world, Microsoft.Xna.Framework.Vector3 _center, int _radius, LoadState _targetState, Entity _following, int _allowedStray)
            : this(new ChunkAddress(world, _center), _radius, _targetState, _following, _allowedStray)
        { }

        public void Update()
        {
            if (following.Chunk.world != center.world)
                Move(following.Chunk);
            if (Math.Abs(following.Chunk.position.X - center.position.X) > allowedStray ||
                Math.Abs(following.Chunk.position.Y - center.position.Y) > allowedStray ||
                Math.Abs(following.Chunk.position.Z - center.position.Z) > allowedStray)
                Move(following.Chunk);
        }
    }
}
