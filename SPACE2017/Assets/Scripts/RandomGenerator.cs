using System;
using UnityEngine;

//Available here: http://blog.msevestre.ca/2010/12/how-to-generate-gaussian-random-numbers.html

public class RandomGenerator
{
    public static RandomGenerator Instance { get { return _instance; } }

    private static RandomGenerator _instance;

    private readonly System.Random _random;

    //indicates that an extra deviates was already calculated
    private bool _hasAnotherDeviate;

    //The other deviate calculated using the Box-Muller transformation
    private double _otherGaussianDeviate;


    public static void Init(int seed = 0)
    {
        _instance = new RandomGenerator(seed);
    }

    private RandomGenerator(int seed)
    {
        _random = new System.Random(seed);
        //?UnityEngine.Random.InitState(seed);
    }


    // returns a normally distributed deviate with zero mean and unit variance.
    // Adapted from Numerical Recipe page 289: Normal (Gaussian) Deviates
    public double NormalDeviate()
    {
        double rsq, v1, v2;
        if (_hasAnotherDeviate)
        {
            //we have an extra deviate handy. Reset the flag and return it
            _hasAnotherDeviate = false;
            return _otherGaussianDeviate;
        }
        do
        {
            v1 = UniformDeviate(-1, 1); //pick two uniform number
            v2 = UniformDeviate(-1, 1); //in the square extending from -1 to +1
            rsq = v1 * v1 + v2 * v2;    //see if they are in the unit circle
        } while (rsq >= 1.0 || rsq == 0.0);

        //now make the box-muller transformation to get two normal deviates.
        double fac = Math.Sqrt(-2.0 * Math.Log(rsq) / rsq);
        //Return one and save one for next time
        _otherGaussianDeviate = v1 * fac;
        _hasAnotherDeviate = true;
        return v2 * fac;
    }

    // Returns a uniformly distributed random number between min and max.
    public double UniformDeviate(double min, double max)
    {
        return (max - min) * NextDouble() + min;
    }
    
    // Returns a random number between 0 and 1
    public double NextDouble()
    {
        return _random.NextDouble();
    }

    public float Range(float min, float max)
    {
        return ((float)NextDouble() * (max - min)) + min;
    }
    
    public float NormalRandom(float mean, float std)
    {
        return (float)NormalDeviate() * std + mean;
    }
}

