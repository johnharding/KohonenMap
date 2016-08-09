using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Rhino.Geometry;
using Kohonen.Friends;

namespace Kohonen.Component
{
    class InputItem
    {
        public VectorND Synaptic;
        public Point3d pos; // Location in map space

        public InputItem(int Dimensions, Random myRandom)
        {
            // N random dimensions
            Synaptic = new VectorND(Dimensions, myRandom);
        }

        public InputItem(List<double> values)
        {
            // N random dimensions
            Synaptic = new VectorND(values);

        }

    }
}
