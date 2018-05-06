using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RailLib
{
    [Serializable]
    public class TrackSection
    {
        public TrackSectionComponent Component { get; set; }
        public UInt32 UniqueID { get; set; }
        public TrackSection Next { get; set; }
        public TrackSection Previous { get; set; }
        public Vector3 Position { get; set; }
        public Quaternion Rotation { get; set; }
        private float _length;

        public float Length
        {
            get
            {
                return _length;
            }
            set
            {
                if(value > 0)
                {
                    _length = value;
                }
                else
                {
                    throw new ArgumentOutOfRangeException("Length can not be 0 or negative");
                }
            }
        }
        public bool Curved { get; set; }
        public float Curve { get; set; }
        public Vector3 EndPosition
        {
            get { return GetPositionOnTrack(Length); }
        }
        public Quaternion EndRotation
        {
            get { return GetRotationOnTrack(Length); }
        }

        /// <summary>
        /// Gets the global position at the given distance on this track section
        /// </summary>
        /// <param name="distanceFromStart">Distance from the start of this track section</param>
        /// <returns>World position</returns>
        public Vector3 GetPositionOnTrack(float distanceFromStart)
        {
            if(Curved)
            {
                //Calculate for curved section
                var angleRadians = Curve * Mathf.Deg2Rad;
                //Curve radius in meters
                var r = Length / angleRadians;
                //Angle to go along the circle in radians
                var a = angleRadians * (distanceFromStart / Length);
                var x = Mathf.Sin(a) * r;
                var y = r - (Mathf.Cos(a) * r);

                return Rotation * new Vector3(y, 0, x) + Position;
            }

            var end = Rotation * Vector3.forward * Length + Position;
            return Vector3.LerpUnclamped(Position, end, distanceFromStart / Length);
        }

        public Vector3 GetLocalPositionOnTrack(float distanceFromStart)
        {
            if(Curved)
            {
                //Calculate for curved section
                var angleRadians = Curve * Mathf.Deg2Rad;
                //Curve radius in meters
                var r = Length / angleRadians;
                //Angle to go along the circle in radians
                var a = angleRadians * (distanceFromStart / Length);
                var x = Mathf.Sin(a) * r;
                var y = r - (Mathf.Cos(a) * r);

                return new Vector3(y, 0, x);
            }

            var end = Vector3.forward * Length;
            return Vector3.LerpUnclamped(Vector3.zero, end, distanceFromStart / Length);
        }

        public Quaternion GetRotationOnTrack(float distanceFromStart)
        {
            return Rotation * GetLocalRotationOnTrack(distanceFromStart);
        }

        public Quaternion GetLocalRotationOnTrack(float distanceFromStart)
        {
            if(Curved)
            {
                return Quaternion.Euler(0, Curve * (distanceFromStart / Length), 0);
            }

            return Quaternion.identity;
        }
        
        /// <summary>
        /// Creates a copy of this TrackSection
        /// </summary>
        /// <param name="connections">When true connections are copied as well</param>
        /// <returns>Copy</returns>
        public TrackSection Clone(bool connections = false)
        {
            var ts = new TrackSection();
            if(connections)
            {
                ts.Next = Next;
                ts.Previous = Previous;
            }
            ts.Position = Position;
            ts.Rotation = Rotation;
            ts.Length = Length;
            ts.Curved = Curved;
            ts.Curve = Curve;
            return ts;
        }

        /// <summary>
        /// Flips the track section in place, swapping Start and End points
        /// </summary>
        public void Flip()
        {
            if(Curved)
            {
                throw new NotImplementedException();
            }

            var end = EndPosition;
            Rotation = Helper.InvertTrackRotation(Rotation);
            Position = end;
        }
    }
}
