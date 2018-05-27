using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

/// <summary>
/// Holds Wagons and provides options for merging trains. Never keep a reference to a Train, they may become invalid
/// due to merging or splitting. Use a reference to a Wagon instead whenever possible.
/// </summary>
public class Train
{
    public int Count { get { return Wagons.Count; } }
    public Wagon Master { get; private set; }
    public bool IsMaster(Wagon wagon) { return wagon == Master; }

    public List<Wagon> Wagons { get; private set; }

    public Train(Wagon wagon)
    {
        if(wagon == null)
        {
            throw new ArgumentNullException("wagon");
        }
        Wagons = new List<Wagon>();
        Wagons.Add(wagon);
        Master = wagon;
    }

    public bool IsBack(Wagon wagon)
    {
        return (wagon == Wagons[Wagons.Count - 1]);
    }

    public bool IsFront(Wagon wagon)
    {
        return (wagon == Wagons[0]);
    }

    /// <summary>
    /// Splits a train in half between the given wagons. Wagons MUST BE adjacent.
    /// </summary>
    /// <param name="a">Wagon a</param>
    /// <param name="b">Wagon b</param>
    /// <returns>Array containing 2 Trains, 0 is this containing the first half, 1 is new containing the other half</returns>
    public Train[] SplitBetween(Wagon a, Wagon b)
    {
        if(a == b || a == null || b == null || a.Train != b.Train)
        {
            throw new ArgumentException("Wagons are not in the same train");
        }

        // Find which wagon is first and split after that
        var idxA = Wagons.IndexOf(a);
        var idxB = Wagons.IndexOf(b);
        if(idxA == Math.Min(idxA, idxB))
        {
            return SplitAfter(a);
        }
        else
        {
            return SplitAfter(b);
        }
    }

    /// <summary>
    /// Splits a train in half after the given wagon
    /// </summary>
    /// <param name="wagon">The last wagon of the first half</param>
    /// <returns>Array containing 2 Trains, 0 is this containing the first half, 1 is new containing the other half</returns>
    public Train[] SplitAfter(Wagon wagon)
    {
        if(wagon == null)
        {
            throw new ArgumentNullException("wagon");
        }
        ExceptionIfNotInTrain(wagon);
        if(IsBack(wagon))
        {
            throw new ArgumentException("Can not split after last wagon in train");
        }

        // Split the train after the given wagon
        var splitIndex = Wagons.IndexOf(wagon);
        var firstHalf = Wagons.GetRange(0, splitIndex + 1);
        var secondHalf = Wagons.GetRange(splitIndex + 1, Wagons.Count - (splitIndex + 1));
        var trains = new Train[2]
        {
            this,
            new Train(Wagons[splitIndex + 1]),  // First wagon of second half will be the master
        };
        // Assign second half to the new train
        foreach(var w in secondHalf)
        {
            w.Train = trains[1];
        }
        this.Wagons = firstHalf;        // trains[0]
        trains[1].Wagons = secondHalf;
        return trains;
    }

    /// <summary>
    /// Joins two trains together
    /// </summary>
    /// <returns>Train containing the merged trains (the Train object from a)</returns>
    public static Train Join(Wagon a, Wagon b)
    {
        if(a == b || a == null || b == null || a.Train == b.Train || // Same train, invalid wagons
            (!a.Train.IsBack(a) && !a.Train.IsFront(a)) ||  // Wagons not on front or back
            (!b.Train.IsBack(b) && !b.Train.IsFront(b)))
        {
            throw new ArgumentException("Non valid train configurations for joining");
        }

        // Four options for joining:
        // Back of A - Back of B
        // Front of A - Front of B
        // Back of A - Front of B
        // Front of A - Back of B
        if(a.Train.IsBack(a) && b.Train.IsBack(b))
        {
            // B needs to be inverted and added to the back
            b.Train.Wagons.Reverse();               // We can modify in place since the Train won't be used again
            a.Train.Wagons.AddRange(b.Train.Wagons);
        }
        else if(a.Train.IsFront(a) && b.Train.IsFront(b))
        {
            // B needs to be inverted and added to the front
            b.Train.Wagons.Reverse();               // We can modify in place since the Train won't be used again
            a.Train.Wagons.InsertRange(0, b.Train.Wagons);
        }
        else if(a.Train.IsBack(a) && b.Train.IsFront(b))
        {
            // B just needs to be added to the back
            a.Train.Wagons.AddRange(b.Train.Wagons);
        }
        else
        {
            // B needs to be added to the front
            a.Train.Wagons.InsertRange(0, b.Train.Wagons);
        }


        // Ensure that the wagons have the new train reference
        // Must be done AFTER merging
        foreach(var w in b.Train.Wagons)
        {
            w.Train = a.Train;
        }

        return a.Train;
    }

    /// <summary>
    /// Adds a wagon to the front of the train
    /// </summary>
    /// <param name="wagon">The wagon to add</param>
    public void AddToFront(Wagon wagon)
    {
        if(wagon == null)
        {
            throw new ArgumentNullException("wagon");
        }
        ExceptionIfAlreadyInTrain(wagon);
        if(wagon.Train.Count > 1)
        {
            throw new NotImplementedException("Train merging not (yet) supported via AddToFront");
        }
        Wagons.Insert(0, wagon);
        wagon.Train = this;
    }

    public void AddToBack(Wagon wagon)
    {
        if(wagon == null)
        {
            throw new ArgumentNullException("wagon");
        }
        ExceptionIfAlreadyInTrain(wagon);
        if(wagon.Train.Count > 1)
        {
            throw new NotImplementedException("Train merging not (yet) supported via AddToBack");
        }
        Wagons.Add(wagon);
        wagon.Train = this;
    }

    public void SetMaster(Wagon wagon)
    {
        if(wagon == null)
        {
            throw new ArgumentNullException("wagon");
        }
        ExceptionIfNotInTrain(wagon);
        Master = wagon;
    }

    private void ExceptionIfNotInTrain(Wagon wagon)
    {
        if(!Wagons.Contains(wagon))
        {
            throw new Exception(string.Format("Wagon {0} is not part of this train",
                wagon.name));
        }
    }

    private void ExceptionIfAlreadyInTrain(Wagon wagon)
    {
        if(Wagons.Contains(wagon))
        {
            throw new Exception(string.Format("Wagon {0} is already part of this train",
                wagon.name));
        }
    }

    public void LogConsist()
    {
        var sb = new StringBuilder();
        sb.AppendFormat("Train Consist: {0}{1}", IsMaster(Wagons[0]) ? "(M) " : "", Wagons[0].name);
        for(int i = 1; i < Wagons.Count; i++)
        {
            sb.AppendFormat(" - {0}{1}", IsMaster(Wagons[i]) ? "(M) " : "", Wagons[i].name);
        }
        sb.AppendFormat(" - Master: {0}", Master.name);
        Debug.Log(sb.ToString());
    }
}
