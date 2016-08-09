using System;
using System.Drawing;
using Grasshopper.Kernel;
using Kohonen.Properties;
using System.Collections.Generic;
using Kohonen.Friends;
using Grasshopper.Kernel.Data;
using Grasshopper.Kernel.Types;

// TODO: Custom attributes

namespace Kohonen.Component
{
    public class Kohonen : GH_Component
    {
        private int xMap, yMap, featureDimension, mySeed;
        private double winLearn, learn, neigh, space, winLearnDecay, learnDecay, neighDecay, neighRad, radius;
        private bool converged;
        private Random myRandom;
        
        private int[] winner;
        private MapItem[,] som;
        private List<InputItem> inputs;
        private GH_Structure<GH_Number> tree = new GH_Structure<GH_Number>();

        int counter;

        public Kohonen() : base("Kohonen Map", "Kohonen Map", "Dimensionality Reduction", "Extra", "ANN")
        {
            counter = 0;
            inputs = new List<InputItem>();
            winner = new int[2];
        }

        protected override void RegisterInputParams(GH_Component.GH_InputParamManager pManager)
        {
            pManager.AddBooleanParameter("Reset", "Reset", "Set to true to reset the map", GH_ParamAccess.item);
            pManager.AddIntegerParameter("XMap", "XMap", "Horizontal size of map", GH_ParamAccess.item, 30);
            pManager.AddIntegerParameter("YMap", "YMap", "Vertical size of map", GH_ParamAccess.item, 30);
            pManager.AddNumberParameter("Space", "Space", "Space", GH_ParamAccess.item, 1.0);
            pManager.AddNumberParameter("Inputs", "Inputs", "Inputs", GH_ParamAccess.tree, 1.0);
            pManager.AddNumberParameter("WinLearn", "WinLearn", "The initial winner learning rate", GH_ParamAccess.item, 0.95);
            pManager.AddNumberParameter("Learn", "Learn", "The initial learning rate for other losing nodes", GH_ParamAccess.item, 0.90);
            pManager.AddNumberParameter("WinLearnDecay", "WinLearnDecay", "The decay constant for WinLearn", GH_ParamAccess.item, 0.9980);
            pManager.AddNumberParameter("LearnDecay", "LearnDecay", "The decay constant for Learn", GH_ParamAccess.item, 0.9975);
            pManager.AddNumberParameter("NeighDecay", "NeighDecay", "The decay constant for the neighbourhood radius", GH_ParamAccess.item, 0.99);
            pManager.AddNumberParameter("NeighRad", "NeighRad", "Initial neighbourhood radius (as a factor of map size)", GH_ParamAccess.item, 0.50);
            pManager.AddIntegerParameter("Seed", "Seed", "Optional random number seed for initial map setup. Set at zero for a time based pseudorandom", GH_ParamAccess.item, 0);
        }

        protected override void RegisterOutputParams(GH_Component.GH_OutputParamManager pManager)
        {
            pManager.AddBooleanParameter("Coverged", "Coverged", "Has the map converged?", GH_ParamAccess.item);
            pManager.AddIntegerParameter("Counter", "Counter", "Cycles", GH_ParamAccess.item);
            pManager.AddGenericParameter("Locations", "Locations", "Position in R2 of the neuron", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Synaptics", "Synaptics", "Synaptic N-Dimensional vector for that neuron", GH_ParamAccess.tree);
            pManager.AddGenericParameter("Neigh", "Neigh", "Neighbourhood learning radius", GH_ParamAccess.item);
            pManager.AddGenericParameter("InputLocations", "InputLocations", "", GH_ParamAccess.tree);
        }

        protected override void SolveInstance(IGH_DataAccess DA)
        {

            bool reset = false;
            DA.GetData("Reset", ref reset);
            if (reset)
            {
                counter = 0;
                return;
            }

            // All the things to do first
            if(counter == 0){
                
                // 0. Reset things
                inputs.Clear();

                // 1. Map things
                if (!DA.GetData("XMap", ref xMap)) { return; }
                if (!DA.GetData("YMap", ref yMap)) { return; }
                if (!DA.GetData("Space", ref space)) { return; }
                
                som = new MapItem[xMap, yMap];

                // Reset all the parameters now we have the size of the map (we need this for the initial learning radius)
                resetParams();
                converged = false;                                         // Flags whether learning has finished

                if (!DA.GetData("WinLearn", ref winLearn)) { return; }
                if (!DA.GetData("Learn", ref learn)) { return; }
                if (!DA.GetData("WinLearnDecay", ref winLearnDecay)) { return; }
                if (!DA.GetData("LearnDecay", ref learnDecay)) { return; }
                if (!DA.GetData("NeighDecay", ref neighDecay)) { return; }
                if (!DA.GetData("NeighRad", ref neighRad)) { return; }
                if (!DA.GetData("Seed", ref mySeed)) { return; } 
                //winLearn = 0.9;                                            // Winner learning rate
                //learn = 0.8;                                               // Others learning rate

                if(mySeed==0)
                {
                    myRandom = new Random();
                }
                else
                {
                    myRandom = new Random(mySeed);
                }

                radius = Math.Sqrt(Math.Pow(xMap, 2) + Math.Pow(yMap, 2)) * neighRad;   // Initial topological Radius
                neigh = radius;

                // 2. Input things
                List<double> tempInputs = new List<double>();
                if (!DA.GetDataTree<GH_Number>("Inputs", out tree)) { return; } // These are just dummies at the moment...

                featureDimension = tree.get_Branch(0).Count;

                // TODO: Fix for non-random inputs
                for(int i=0; i<tree.Branches.Count; i++)
                {
                    // Set up a feature vector of doubles
                    List<double> featureVector = new List<double>();
                    for (int j = 0; j < tree.get_Branch(i).Count; j++)
                    {
                        double myDouble;
                        GH_Convert.ToDouble(tree.get_Branch(i)[j], out myDouble, GH_Conversion.Primary);
                        featureVector.Add(myDouble);
                    }
                    inputs.Add(new InputItem(featureVector));
                }
                
                // 3. Initialise the map
                for (int i=0; i<xMap; i++){
                  for (int j=0; j<yMap; j++){
                    som[i,j] = new MapItem(featureDimension, space, i, j, myRandom);
                  }
                }

            } // end of counter=0 events



            if(!converged)
            {
                // Now the bits we want to do every iteration
                for (int i=0; i<inputs.Count; i++){
                    FindWinner(i);
                    OrganiseMap(i);
                }
  
                UpdateMap();

                counter ++;
            }


            // Export data
            DA.SetData("Coverged", converged);
            DA.SetData("Counter", counter);
            
            GH_Structure<GH_Point> locations = new GH_Structure<GH_Point>();
            GH_Structure<GH_Point> inputLocations = new GH_Structure<GH_Point>();
            GH_Structure<VectorND> synaptics = new GH_Structure<VectorND>();
            GH_Structure<GH_Number> synumbers = new GH_Structure<GH_Number>();

            for (int i=0; i<xMap; i++){
                  //List<GH_Point> myTarget = new List<GH_Point>();
                GH_Path myPath = new GH_Path(i);
                for (int j=0; j<yMap; j++)
                {
                    // Deal with the location first
                    GH_Point myPoint = new GH_Point(som[i,j].pos);
                    locations.Append(myPoint, myPath.AppendElement(j));

                    // Now the ND Vector
                    List<GH_Number> myList = new List<GH_Number>();

                    for (int k = 0; k < som[i, j].Synaptic.Count; k++)
                    {
                        GH_Number myGHNumber = new GH_Number(som[i, j].Synaptic.GetItem(k));
                        myList.Add(myGHNumber);
                    }
                    synumbers.AppendRange(myList, myPath.AppendElement(j));
                }
            }

            // Output the input locations
            for (int i=0; i<inputs.Count; i++){
                GH_Point myPoint = new GH_Point(inputs[i].pos);
                inputLocations.Append(myPoint);
            }
            

            DA.SetDataTree(2, locations);
            DA.SetDataTree(3, synumbers);
            DA.SetData(4, neigh);
            DA.SetDataTree(5, inputLocations);

        } // end of SolveInstance


        /// <summary>
        /// Find the MapItem with the closest Euclidean distance to the input
        /// </summary>
        /// <param name="muk"></param>
        private void FindWinner(int muk){

            // Global check (slow)
            double mindis = 1000000.0;
  
            // Get the reference of the winning map item
            for (int i=0; i<xMap; i++){
              for (int j=0; j<yMap; j++){
                double dis = som[i,j].Synaptic.DistanceTo(inputs[muk].Synaptic);
                if (dis < mindis){
                    mindis = dis; 
                    winner[0] = i; 
                    winner[1] = j;
                } 
              }
            }

            // Move the input so that it is located at the best map item's location
            // Fine to point
            inputs[muk].pos = som[winner[0], winner[1]].pos;

        }

        /// <summary>
        /// This is where the learning takes place
        /// </summary>
        /// <param name="muk"></param>
        private void OrganiseMap(int muk){

            
            for (int i=0; i<xMap; i++){
                for (int j=0; j<yMap; j++){

                    //Make a copy of the input synaptic vector
                    VectorND dd = new VectorND(inputs[muk].Synaptic);

                    // If you are the winner then...
                    if(i == winner[0] && j == winner[1])
                    {
                        //Console.Beep(500, 100);
                        // The winner uses the winLearn rate to learn
                        dd.Subtract(som[i,j].Synaptic);
                        dd.Scale(winLearn);
                        som[i,j].limboSynaptic.Sum(dd);
                        //som[i, j].limboSynaptic.Scale(4);
                    }
                    else
                    {
                        // Do some learning, even if you are not the winner
                        // Note: Topological distance is calculated using the map topology
                        double rad = Math.Sqrt(Math.Pow((i - winner[0]), 2) + Math.Pow((j - winner[1]), 2));
                        
                        if(rad<=neigh)
                        {
                            dd.Subtract(som[i,j].Synaptic);
                            dd.Scale(learn/rad);
                            som[i,j].limboSynaptic.Sum(dd);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Updates the feature map in 2d space using the limbo states
        /// </summary>
        private void UpdateMap(){
  
            // Thanks to Christian Derix for these
            winLearn *= (1 - ((double)counter * (1-winLearnDecay)));
            learn *= (1 - ((double)counter * (1 - learnDecay)));
            neigh = radius * (1 - ((double)counter * (1-neighDecay)));


            //winLearn *= (1 - ((double)counter / 600));
            //learn *= (1 - ((double)counter / 400));
            //neigh = radius * ((1 - (double)counter / 100));

            if(winLearn < 0.5) converged=true;
            
            for (int i=0; i<xMap; i++){
              for (int j=0; j<yMap; j++){
                som[i,j].Update();
                som[i,j].ResetLimbo(); //Resets the limbo to the current SOM map values
              }
            }
        }


        private void resetParams()
        {
            converged = false;                                          // Flags whether learning has finished
            featureDimension = 0;                                       // Dimension of the input samples
            winLearn = 0.0;                                             // Winner learning rate
            learn = 0.0;                                                // Others learning rate
            neigh = Math.Sqrt(Math.Pow(xMap, 2) + Math.Pow(yMap, 2)) * neighRad;    // Initial topological Radius
            winLearnDecay = 0.0;
            learnDecay = 0.0;
            neighDecay = 0.0;
        }


        public override Guid ComponentGuid
        {
            //generated at http://www.newguid.com/
            get { return new Guid("c571217d-a91c-4a51-8f0b-226222cedc8c"); }
        }

        protected override Bitmap Icon
        {
            get
            {
                return Properties.Resources.Kohonen;
            }
        }
    }
}

