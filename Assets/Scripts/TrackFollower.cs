using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RailLib
{
    public class TrackFollower
    {
        public TrackSection TrackSection { get; private set; }
        public float Distance { get; private set; }
        public Vector3 Position { get { return TrackSection.GetPositionOnTrack(Distance); } }
        public Quaternion Rotation { get
            {
                var rot = TrackSection.GetRotationOnTrack(Distance);
                return Inverted ? Helper.InvertTrackRotation(rot) : rot;
            } }
        public bool Inverted { get; private set; }

        public TrackFollower(TrackSection section, float startDistance, bool startInverted)
        {
            TrackSection = section;
            Distance = startDistance;
            Inverted = startInverted;
        }

        public bool Move(float distanceDelta)
        {
            float distanceLeft = Math.Abs(distanceDelta);
            while(distanceLeft > 0)
            {
                // Get distance relative to current track orientation
                float trackRelativeDistance = Math.Sign(distanceDelta) * distanceLeft;
                if(Inverted)
                {
                    trackRelativeDistance = -trackRelativeDistance;
                }

                if(trackRelativeDistance < 0)
                {
                    // We move towards the previous section
                    if(Distance + trackRelativeDistance < 0)
                    {
                        // We have to jump to the previous section
                        if(TrackSection.Previous == null)
                        {
                            // Ran out of track to finish this move!
                            return false;
                        }
                        // We can move Distance on this section, so subtract that from the total
                        distanceLeft -= Distance;

                        if(PreviousIsInverted(TrackSection))
                        {
                            // If it's inverted compared to our current section our invertion state needs to be flipped as well
                            // And we must start at that section's start point (Distance = 0) instead of at the end
                            Inverted = !Inverted;
                            Distance = 0;
                        }
                        else
                        {
                            // Go to the end of the previous section
                            Distance = TrackSection.Previous.Length;
                        }
                        TrackSection = TrackSection.Previous;
                    }
                    else
                    {
                        // Our goal is on this section
                        distanceLeft = 0;
                        Distance += trackRelativeDistance;
                    }
                }
                else
                {
                    // We move towards the next section
                    if(Distance + trackRelativeDistance > TrackSection.Length)
                    {
                        // We have to jump to the next section
                        if(TrackSection.Next == null)
                        {
                            // Ran out of track to finish this move!
                            return false;
                        }
                        // We can move Length - Distance on this section, so subtract that from the total
                        distanceLeft -= TrackSection.Length - Distance;

                        if(NextIsInverted(TrackSection))
                        {
                            // If it's inverted compared to our current section our invertion state needs to be flipped as well
                            // And we must start at that section's end point instead of at the start
                            Inverted = !Inverted;
                            Distance = TrackSection.Next.Length;
                        }
                        else
                        {
                            // Go to the start of the next section
                            Distance = 0;
                        }
                        TrackSection = TrackSection.Next;
                    }
                    else
                    {
                        // Our goal is in this section
                        distanceLeft = 0;
                        Distance += trackRelativeDistance;
                    }
                }
            }
            return true;
        }

        private bool NextIsInverted(TrackSection trackSection)
        {
            if(trackSection.Next != null && Vector3.Distance(trackSection.EndPosition, trackSection.Next.Position) > 0.05f)
            {
                return true;
            }
            return false;
        }

        private bool PreviousIsInverted(TrackSection trackSection)
        {
            if(trackSection.Previous != null && Vector3.Distance(trackSection.Previous.EndPosition, trackSection.Position) > 0.05f)
            {
                return true;
            }
            return false;
        }
    }
}
