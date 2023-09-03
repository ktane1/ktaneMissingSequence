using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

public class sequencePatterns : MonoBehaviour
{
    //Primes
    public static List<int> primes = new List<int>() { 2, 3, 5, 7, 11, 13, 17, 19, 23, 29, 31, 37, 41, 43, 47, 53, 59, 61, 67, 71, 73, 79, 83, 89, 97, 101, 103, 107, 109, 113, 127, 131, 137, 139, 149, 151, 157, 163, 167, 173, 179, 181, 191, 193, 197, 199, 211, 223, 227, 229, 233, 239, 241, 251, 257, 263, 269, 271, 277, 281, 283, 293, 307, 311, 313, 317, 331, 337, 347, 349, 353, 359, 367, 373, 379, 383, 389, 397, 401, 409, 419, 421, 431, 433, 439, 443, 449, 457, 461, 463, 467, 479, 487, 491, 499, 503, 509, 521, 523, 541, 547, 557, 563, 569, 571, 577, 587, 593, 599, 601, 607, 613, 617, 619, 631, 641, 643, 647, 653, 659, 661, 673, 677, 683, 691, 701, 709, 719, 727, 733, 739, 743, 751, 757, 761, 769, 773, 787, 797, 809, 811, 821, 823, 827, 829, 839, 853, 857, 859, 863, 877, 881, 883, 887, 907, 911, 919, 929, 937, 941, 947, 953, 967, 971, 977, 983, 991, 997 };

    //Fibonacci terms
    public static List<int> fibonacciTerms = new List<int>() { 0, 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987 };

    //Perfect squares
    public static List<int> squares = new List<int>() { 0, 1, 4, 9, 16, 25, 36, 49, 64, 81, 100, 121, 144, 169, 196, 225, 256, 289, 324, 361, 400, 441, 484, 529, 576, 625, 676, 729, 784, 841, 900, 961, 1024, 1089, 1156, 1225, 1296, 1369, 1444, 1521, 1600, 1681, 1764, 1849, 1936};

    //Perfect cubes
    public static List<int> cubes = new List<int>() { 0, 1, 8, 27, 64, 125, 216, 343, 512, 729, 1000, 1331, 1728, 2197, 2744, 3375, 4096, 4913, 5832, 6859, 8000 };

    //Arithmetic Progressions
    public static List<int> AP(int start, int offset, int termNo)
    {
        List<int> results = new List<int>();
        for (int i = 0; i < termNo; i++)
        {
            results.Add(start + offset * i);
        }
        return results;
    }

    //Geometric Progressions
    public static List<int> GP(int start, int offset, int termNo)
    {
        List<int> results = new List<int>();
        for (int i = 0; i < termNo; i++)
        {
            results.Add(start * Convert.ToInt32((Math.Pow(Convert.ToDouble(offset), Convert.ToDouble(i)))));
        }
        return results;
    }


    //APGP Combinations
    public static List<int> APAPOff(int start, int offStart, int offset, int termNo)
    {
        List<int> results = new List<int>();
        List<int> offResults = AP(offStart, offset, termNo - 1);
        for (int i = 0; i < termNo; i++)
        {
            if (i == 0) { results.Add(start); }
            else
            {
                results.Add(results[i-1] + offResults[i-1]);
            }
        }
        return results;
    }
    /* Mainly this always returns a super large value so it's omitted
    public static List<int> APGPOff(int start, int offStart, int offset, int termNo)
    {
        List<int> results = new List<int>();
        List<int> offResults = AP(offStart, offset, termNo - 1);
        for (int i = 0; i < termNo; i++)
        {
            if (i == 0) { results.Add(start); }
            else
            {
                results.Add(results[i - 1] * offResults[i - 1]);
            }
        }
        return results;
    }*/

    public static List<int> GPAPOff(int start, int offStart, int offset, int termNo)
    {
        List<int> results = new List<int>();
        List<int> offResults = GP(offStart, offset, termNo - 1);
        for (int i = 0; i < termNo; i++)
        {
            if (i == 0) { results.Add(start); }
            else
            {
                results.Add(results[i - 1] + offResults[i - 1]);
            }
        }
        return results;
    }

    /* Mainly this also always returns a super large value so it's omitted
    public static List<int> GPGPOff(int start, int offStart, int offset, int termNo)
    {
        List<int> results = new List<int>();
        List<int> offResults = GP(offStart, offset, termNo - 1);
        for (int i = 0; i < termNo; i++)
        {
            if (i == 0) { results.Add(start); }
            else
            {
                results.Add(results[i - 1] * offResults[i - 1]);
            }
        }
        return results;
    }
    */

    //Digital Roots
    public static int DR(int k)
    {
        if (k == 0) { return 0; }
        else if (k % 9 == 0) { return 9; }
        else { return k % 9; }
    }

    //Sum of digits
    public static int SumOfDigits(int k)
    {
        int sum = 0;
        while (k > 0)
        {
            sum += k % 10;
            k = k / 10;
        }
        return sum;
    }

    //Product of digits
    public static int ProdOfDigits(int k)
    {
        int prod = 1;
        while (k > 0)
        {
            prod *= k % 10;
            k = k / 10;
        }
        return prod;
    }

    //Factorisations
    public static List<int> Factor(int k)
    {
        List<int> factors = new List<int>();
        for (int i = 2; k > 1; i++)
        {
            while (k % i == 0)
            {
                k /= i;
                factors.Add(i);
            }
        }
        return factors;
    }

    //Least Prime Factor
    public static int LeastPF(int k)
    {
        return Factor(k).Min();
    }

    //Largest Prime Factor
    public static int LargestPF(int k)
    {
        return Factor(k).Max();
    }

    //Special arithmetic offsets (primes, perfect squares, properties of numbers etc.)
    public static List<int> Special(int start, int rnd, int rnd2, int termNo)
    {
        List<int> results = new List<int>();
        int k = 0;
        switch (rnd)
        {
            case 0://Digital root
                for (int i = 0; i < termNo; i++)
                {
                    if (i == 0) { results.Add(start); }
                    else
                    {
                        results.Add(results[i - 1] + DR(results[i - 1]));//Addition (subtraction always causes ambiguity)
                    }
                }
                break;

            case 1://Sum of digits
                for (int i = 0; i < termNo; i++)
                {
                    if (i == 0) { results.Add(start); }
                    else
                    {
                        results.Add(results[i - 1] + SumOfDigits(results[i - 1]));//Addition
                    }
                }
                break;

            case 2://Product of digits
                for (int i = 0; i < termNo; i++)
                {
                    if (i == 0) { results.Add(start); }
                    else
                    {
                        if (rnd2 == 0) { results.Add(results[i - 1] + ProdOfDigits(results[i - 1])); }//Addition
                        else if (rnd2 == 1) { results.Add(results[i - 1] - ProdOfDigits(results[i - 1])); }//Subtraction
                        else if (rnd2 == 2)//Alternate between addition and subtraction
                        {
                            if (i % 2 == 0)
                            {
                                results.Add(results[i - 1] + ProdOfDigits(results[i - 1]));
                            }
                            else
                            {
                                results.Add(results[i - 1] - ProdOfDigits(results[i - 1]));
                            }
                        }
                    }
                }
                break;

            case 3://Primes
                k = UnityEngine.Random.Range(0, primes.Count - 6);
                for (int i = 0; i < termNo; i++)
                {
                    if (i == 0) { results.Add(start); }
                    else
                    {
                        if (rnd2 == 0) { results.Add(results[i - 1] + primes[k + i]); }//Addition
                        else if (rnd2 == 1) { results.Add(results[i - 1] - primes[k + i]); }//Subtraction
                        else if (rnd2 == 2)//Alternate between addition and subtraction
                        {
                            if (i % 2 == 0)
                            {
                                results.Add(results[i - 1] + primes[k + i]);
                            }
                            else
                            {
                                results.Add(results[i - 1] - primes[k + i]);
                            }
                        }
                    }
                }
                break;

            case 4://Perfect Squares
                k = UnityEngine.Random.Range(0, squares.Count - 6);
                for (int i = 0; i < termNo; i++)
                {
                    if (i == 0) { results.Add(start); }
                    else
                    {
                        if (rnd2 == 0) { results.Add(results[i - 1] + squares[k + i]); }//Addition
                        else if (rnd2 == 1) { results.Add(results[i - 1] - squares[k + i]); }//Subtraction
                        else if (rnd2 == 2)//Alternate between addition and subtraction
                        {
                            if (i % 2 == 0)
                            {
                                results.Add(results[i - 1] + squares[k + i]);
                            }
                            else
                            {
                                results.Add(results[i - 1] - squares[k + i]);
                            }
                        }
                    }
                }
                break;

            case 5://Perfect Cubes
                k = UnityEngine.Random.Range(0, cubes.Count - 10);
                for (int i = 0; i < termNo; i++)
                {
                    if (i == 0) { results.Add(start); }
                    else
                    {
                        if (rnd2 == 0) { results.Add(results[i - 1] + cubes[k + i]); }//Addition
                        else if (rnd2 == 1) { results.Add(results[i - 1] - cubes[k + i]); }//Subtraction
                        else if (rnd2 == 2)//Alternate between addition and subtraction
                        {
                            if (i % 2 == 0)
                            {
                                results.Add(results[i - 1] + cubes[k + i]);
                            }
                            else
                            {
                                results.Add(results[i - 1] - cubes[k + i]);
                            }
                        }
                    }
                }
                break;

            case 6://Largest Prime Factor
                for (int i = 0; i < termNo; i++)
                {
                    if (i == 0) { results.Add(start); }
                    else
                    {
                        results.Add(results[i - 1] + LargestPF(results[i - 1]));
                    }
                }
                break;

            case 7://Sum of prime factors
                for (int i = 0; i < termNo; i++)
                {
                    if (i == 0) { results.Add(start); }
                    else
                    {
                        results.Add(results[i - 1] + Factor(results[i - 1]).Sum()); 
                    }
                }
                break;

        }
        return results;
    }

    //Special terms with a certain offset
    public static List<int> SpecialTerms(int start, int rnd, int offset, int termNo)
    {
        List<int> results = new List<int>();
        for (int i = 0; i < termNo; i++)
        {
            switch (rnd)
            {
                case 0://Primes
                    results.Add(primes[start + i] + offset);
                    break;
                case 1://Perfect squares
                    results.Add(squares[start + i] + offset);
                    break;
                case 2://Perfect cubes
                    results.Add(cubes[start + i] + offset);
                    break;
            }
        }
        return results;
    }

    //Fibonacci progression
    public static List<int> Fib(int first, int second, int termNo)
    {
        List<int> results = new List<int>();
        for (int i = 0; i < termNo; i++)
        {
            if (i == 0) { results.Add(first); }
            else if (i == 1) { results.Add(second); }
            else { results.Add(results[i - 1] + results[i - 2]); }
        }
        return results;
    }

    //Recursive in the form a-b
    public static List<int> RecursiveEz(int first, int second, int termNo)
    {
        List<int> results = new List<int>();
        for (int i = 0; i < termNo; i++)
        {
            if (i == 0) { results.Add(first); }
            else if (i == 1) { results.Add(second); }
            else { results.Add(results[i - 2] - results[i - 1]); }
        }
        return results;
    }

    //Recursive in the form na+b or a+nb
    public static List<int> RecursiveMed(int first, int second, int offset, int termNo, int version)
    {
        List<int> results = new List<int>();
        for (int i = 0; i < termNo; i++)
        {
            if (version == 0)
            {
                if (i == 0) { results.Add(first); }
                else if (i == 1) { results.Add(second); }
                else { results.Add(offset * results[i - 2] + results[i - 1]); }
            }
            else
            {
                if (i == 0) { results.Add(first); }
                else if (i == 1) { results.Add(second); }
                else { results.Add(results[i - 2] + offset * results[i - 1]); }
            }
        }
        return results;
    }

    //Recursive in the form a+b-ab
    public static List<int> RecursiveProd(int first, int second, int termNo)
    {
        List<int> results = new List<int>();
        for (int i = 0; i < termNo; i++)
        {
            if (i == 0) { results.Add(first); }
            else if (i == 1) { results.Add(second); }
            else { results.Add(results[i - 2] + results[i - 1] - (results[i - 2] * results[i - 1])); }
        }
        return results;
    }

    //Recursive in the form a+b+n
    public static List<int> RecursiveAdd(int first, int second, int offset, int termNo)
    {
        List<int> results = new List<int>();
        for (int i = 0; i < termNo; i++)
        {
            if (i == 0) { results.Add(first); }
            else if (i == 1) { results.Add(second); }
            else { results.Add(results[i - 2] + results[i - 1] + offset); }
        }
        return results;
    }

    //Combination of two sequences (multiplication)
    public static List<int> Comb(List<int> first, List<int> second, int termNo)
    {
        List<int> results = new List<int>();
        for (int i = 0; i < termNo; i++)
        {
            results.Add(first[i] * second[i]);
        }
        return results;
    }

    //Combination of two sequences (addition)
    /* Omitted because it's nearly impossible to deduce
    public static List<int> Comb(List<int> first, List<int> second, int termNo)
    {
        List<int> results = new List<int>();
        for (int i = 0; i < termNo; i++)
        {
            results.Add(first + second);
        }
        return results;
    }
    */
}
