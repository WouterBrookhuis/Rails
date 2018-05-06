using System;
using System.Collections.Generic;
using System.Linq;

namespace RailLib
{
    public class Junction
    {
        public TrackSection Entry { get; private set; }
        public TrackSection Left { get; private set; }
        public TrackSection Right { get; private set; }
        public bool GoLeft { get; private set; }

        /// <summary>
        /// Creates a new junction from the given sections. Note that they may not be inverted.
        /// </summary>
        /// <param name="entry">The entry track of the junction</param>
        /// <param name="left">The left branch</param>
        /// <param name="right">The right branch</param>
        public Junction(TrackSection entry, TrackSection left, TrackSection right)
        {
            Entry = entry;
            Left = left;
            Right = right;

            // Hook up pieces
            Left.Previous = Entry;
            Right.Previous = Entry;
            Toggle();
        }

        public void Toggle()
        {
            GoLeft = !GoLeft;
            if(GoLeft)
            {
                // We now go left
                Entry.Next = Left;
            }
            else
            {
                // We now go right
                Entry.Next = Right;
            }
        }
    }
}
