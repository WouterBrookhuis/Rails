using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RailLib
{
    public class TrackLayer
    {
        struct State
        {
            public TrackSection section;
            public bool inverted;
        }

        public TrackSection CurrentSection { get; set; }
        public bool Inverted { get; set; }

        private Stack<State> previousStates;

        public TrackLayer()
        {
            previousStates = new Stack<State>();
        }

        public void ClearHistory()
        {
            previousStates = new Stack<State>();
        }

        /// <summary>
        /// Starts a new straight track at the given position and rotation
        /// </summary>
        /// <param name="position">Track start position</param>
        /// <param name="rotation">Track start rotation</param>
        /// <param name="length">Length of track</param>
        /// <param name="inverted">If the track piece should be inverted</param>
        /// <returns></returns>
        public TrackSection StartTrack(Vector3 position, Quaternion rotation, float length, bool inverted = false)
        {
            if(CurrentSection != null)
            {
                PushState();
            }

            CurrentSection = new TrackSection()
            {
                Length = length,
                Position = position,
                Rotation = rotation,
            };

            if(inverted)
            {
                CurrentSection.Flip();
            }

            Inverted = inverted;
            return CurrentSection;
        }

        public TrackSection StartTrack(Vector3 position, Quaternion rotation, float length, float angle, bool inverted = false)
        {
            if(CurrentSection != null)
            {
                PushState();
            }

            CurrentSection = new TrackSection()
            {
                Length = length,
                Position = position,
                Rotation = rotation,
                Curve = angle,
                Curved = true,
            };

            if(inverted)
            {
                CurrentSection.Flip();
            }

            Inverted = inverted;
            return CurrentSection;
        }

        /// <summary>
        /// Repositions the track layer on the given track
        /// </summary>
        /// <param name="section"></param>
        /// <param name="inverted"></param>
        public void Reposition(TrackSection section, bool inverted = false)
        {
            ClearHistory();
            CurrentSection = section;
            Inverted = inverted;
        }

        /// <summary>
        /// Moves the tracklayer back to the previous track section
        /// </summary>
        public void MoveBack()
        {
            if(previousStates.Count > 0)
            {
                var state = previousStates.Pop();
                CurrentSection = state.section;
                Inverted = state.inverted;
            }
        }

        public TrackSection PlaceTrack(float length, Quaternion relativeRotation, bool inverted = false)
        {
            var track = PlaceTrack(length, inverted);
            track.Rotation = track.Rotation * relativeRotation;
            return track;
        }

        /// <summary>
        /// Places a new section of straight track after the current section
        /// </summary>
        /// <param name="length">The length of the new section</param>
        /// <param name="inverted">If the new section is inverted</param>
        public TrackSection PlaceTrack(float length, bool inverted = false)
        {
            if(CurrentSection == null)
            {
                throw new InvalidOperationException("Use StartTrack to start a new track");
            }

            var track = new TrackSection()
            {
                Length = length,
                Previous = inverted ? null : CurrentSection,
                Next = inverted ? CurrentSection : null,
            };

            // Rotation MUST be calculated first so position offset computation functions correctly
            track.Rotation = CalculateNewTrackRotation(track, inverted);
            track.Position = CalculateNewTrackPosition(track, inverted);

            // Check to see if we are supposed to hook this up to the front or back of the current section
            if(Inverted)
            {
                CurrentSection.Previous = track;
            }
            else
            {
                CurrentSection.Next = track;
            }

            // Store state
            PushState();

            // Update state for next calls
            Inverted = inverted;
            CurrentSection = track;

            return CurrentSection;
        }

        /// <summary>
        /// Places a new section of straight track after the current section
        /// </summary>
        /// <param name="length">The length of the new section</param>
        /// <param name="curve">The curve in degrees</param>
        /// <param name="inverted">If the new section is inverted</param>
        public TrackSection PlaceTrack(float length, float curve, bool inverted = false)
        {
            if(CurrentSection == null)
            {
                throw new InvalidOperationException("Use StartTrack to start a new track");
            }

            var track = new TrackSection()
            {
                Length = length,
                Previous = inverted ? null : CurrentSection,
                Next = inverted ? CurrentSection : null,
                Curved = true,
                Curve = curve
            };

            // Rotation MUST be calculated first so position offset computation functions correctly
            track.Rotation = CalculateNewTrackRotation(track, inverted);
            track.Position = CalculateNewTrackPosition(track, inverted);

            // Check to see if we are supposed to hook this up to the front or back of the current section
            if(Inverted)
            {
                CurrentSection.Previous = track;
            }
            else
            {
                CurrentSection.Next = track;
            }

            // Store state
            PushState();

            // Update state for next calls
            Inverted = inverted;
            CurrentSection = track;

            return CurrentSection;
        }

        private Vector3 CalculateNewTrackPosition(TrackSection newPiece, bool invertNewPiece)
        {
            // Get the current track's attach position
            var anchor = this.Inverted ? CurrentSection.Position : CurrentSection.EndPosition;
            var offset = invertNewPiece ? newPiece.EndPosition : Vector3.zero;
            return anchor + offset;
        }

        private Quaternion CalculateNewTrackRotation(TrackSection newPiece, bool invertNewPiece)
        {
            Quaternion anchorRotation = Inverted ? Helper.InvertTrackRotation(CurrentSection.Rotation) : CurrentSection.GetRotationOnTrack(CurrentSection.Length);
            //Debug.LogFormat("Anchor rotation: {0}, Inverted: {1}", anchorRotation.eulerAngles, Inverted);
            Quaternion newRotation = invertNewPiece ? Helper.InvertTrackRotation(newPiece.GetRotationOnTrack(newPiece.Length)) : Quaternion.identity;
            //Debug.LogFormat("New piece rotation: {0}, Inverted: {1}", newRotation.eulerAngles, invertNewPiece);
            //Debug.LogFormat("Total rotation: {0}", (anchorRotation * newRotation).eulerAngles);
            return anchorRotation * newRotation;
        }

        private void PushState()
        {
            previousStates.Push(new State
            {
                section = CurrentSection,
                inverted = Inverted
            });
        }
    }
}
