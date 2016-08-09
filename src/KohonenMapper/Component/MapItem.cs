using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Kohonen.Friends;
using Rhino.Geometry;
using Grasshopper.Kernel;
using Grasshopper.Kernel.Types;

namespace Kohonen.Component
{
    class MapItem
    {
        public VectorND Synaptic;
        public VectorND limboSynaptic;
        public Point3d pos; // Location in map space

        public MapItem(int Dimensions, double Space, int I, int J, Random myRandom)
        {
            // N random dimensions
            Synaptic = new VectorND(Dimensions, myRandom);
            limboSynaptic = new VectorND(Synaptic);
            pos = new Point3d(I * Space, J * Space, 0.0);
        }
        
        /// <summary>
        /// Updates the values of the map to whatever the limbo is
        /// </summary>
        public void Update()
        {
            Synaptic.CopyData(limboSynaptic);
        }

        /// <summary>
        /// Resets the limbo vector
        /// </summary>
        public void ResetLimbo()
        {
            limboSynaptic = new VectorND(Synaptic);
        }

    }
}
