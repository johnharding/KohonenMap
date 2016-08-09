using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grasshopper.Kernel.Types;

namespace Kohonen.Friends
{
    /// <summary>
    /// N-Dimensional Vector
    /// </summary>
    class VectorND : IGH_Goo
    {
        private List<double> Item = new List<double>();
        public int Count { get; set; }

        /// <summary>
        /// Creates a vector from a list of values
        /// </summary>
        /// <param name="values"></param>
        public VectorND(List<double> values)
        {
            Item.Clear();
            for (int i = 0; i < values.Count; i++)
            {
                Item.Add(values[i]);
            }
        }




        /// <summary>
        /// Default constructor for making a zeroed N dimensional vector
        /// </summary>
        /// <param name="Dimensions"></param>
        public VectorND(int Dimensions)
        {
            // Add some random doubles between 0.0 and 1.0
            Item.Clear();
            for (int i = 0; i < Dimensions; i++)
                Item.Add(0.0);
            Count = Dimensions;
        }

        /// <summary>
        /// Constructor for making a random N dimensional vector
        /// </summary>
        /// <param name="Dimensions"></param>
        /// <param name="myRandom"></param>
        public VectorND(int Dimensions, Random myRandom)
        {
            // Add some random doubles between 0.0 and 1.0
            Item.Clear();
            for (int i = 0; i < Dimensions; i++)
                Item.Add(myRandom.NextDouble());
            Count = Dimensions;
        }

        /// <summary>
        /// Cloning constructor
        /// </summary>
        /// <param name="otherVector"></param>
        public VectorND(VectorND otherVector)
        {
            Item.Clear();
            for (int i = 0; i < otherVector.Item.Count; i++)
                Item.Add(otherVector.Item[i]);
        }

        /// <summary>
        /// Returns a particular value at the given dimension. 
        /// Returns zero if the required dimension is larger than the vector dimension (as you would expect!)
        /// </summary>
        /// <param name="Index"></param>
        /// <returns></returns>
        public double GetItem(int Index)
        {
            if (Index < Item.Count)
                return Item[Index];
            else
                return 0.0;
        }

        /// <summary>
        /// Returns the list of item values
        /// </summary>
        /// <returns></returns>
        public List<double> GetItemList()
        {
            return Item;
        }

        /// <summary>
        /// Returns the list of item values but of Grasshopper Number Type.
        /// </summary>
        /// <returns></returns>
        public List<GH_Number> GetGHList()
        {
            List<GH_Number> newItems = new List<GH_Number>();
            foreach (double mrpotato in Item)
            {
                GH_Number ghNumber = new GH_Number(mrpotato);
                newItems.Add(ghNumber);
            }
            return newItems;
        }

        /// <summary>
        /// Returns the distance to another N-Dimensional vector. 
        /// TODO: Handle lower/higher dimensions by assuming zeroes.
        /// </summary>
        /// <param name="otherVector"></param>
        /// <returns></returns>
        public double DistanceTo(VectorND otherVector)
        {
            if (Item.Count != otherVector.Item.Count)
            {
                return 0.0;
            }
            else
            {
                double sum = 0.0;
                for (int i = 0; i < Item.Count; i++)
                {
                    sum += Math.Pow(otherVector.Item[i] - Item[i], 2);
                }
                return Math.Sqrt(sum);
            }
                
        }

        /// <summary>
        /// Adds two vectors together and returns the result (sum)
        /// </summary>
        /// <param name="otherVector"></param>
        /// <returns></returns>
        public void Sum(VectorND otherVector)
        {
            for (int i = 0; i < Item.Count; i++)
            {
                Item[i] += otherVector.Item[i];
            }

        }


        /// <summary>
        /// subtract
        /// </summary>
        /// <param name="otherVector"></param>
        public void Subtract(VectorND otherVector)
        {
             for (int i = 0; i < Item.Count; i++)
             {
                 Item[i] -= otherVector.Item[i];
             }
        }

        /// <summary>
        /// scale
        /// </summary>
        /// <param name="ScaleFactor"></param>
        public void Scale(double ScaleFactor)
        {
            for (int i = 0; i < Item.Count; i++)
            {
                Item[i] *= ScaleFactor;
            }
        }

        /// <summary>
        /// Replaces all the values with those from another vector.
        /// If the other vector is of lower dimension, it will only replace the values up to that dimension
        /// </summary>
        /// <param name="otherVector"></param>
        /// <returns></returns>
        public void CopyData(VectorND otherVector)
        {

            if (Item.Count == otherVector.Item.Count)
            {
                for (int i = 0; i < Item.Count; i++)
                {
                    if(i<otherVector.Item.Count)
                        Item[i] = otherVector.GetItem(i);
                }
            }
        }


        public void Zero()
        {
            for (int i = 0; i < Item.Count; i++)
            {
                Item[i] = 0.0;
            }
        }


        /// <summary>
        /// Returns a string representation of the state (value) of this instance.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            //if (this.Value == 0) { return "False"; }
            //if (this.Value > 0) { return "True"; }
            return this.Item.ToString();
        }

        public Grasshopper.Kernel.Types.IGH_GooProxy EmitProxy()
        {
            return null;
        }

        public bool CastFrom(object source)
        {
            return false;
        }

        public bool CastTo<T>(out T target)
        {
            target = default(T);
            return false;
        }

        public bool Read(GH_IO.Serialization.GH_IReader reader)
        {
            return false;
        }

        public bool Write(GH_IO.Serialization.GH_IWriter writer)
        {
            return false;
        }

        public Grasshopper.Kernel.Types.IGH_Goo Duplicate()
        {
            VectorND N = new VectorND(this);
            return N;
        }

        public object ScriptVariable()
        {
            throw new NotImplementedException();
        }

        public bool IsValid
        {
            get { return true; }
        }

        public string IsValidWhyNot
        {
            get { return "The brain cannot understand itself"; }
        }

        public string TypeDescription
        {
            get { return "N-Dimensional Vector DataType"; }
        }

        public string TypeName
        {
            get { return "VectorND"; }
        }


    }
}
