using linerider.Game;
using OpenTK.Mathematics;
using System.Collections.Generic;

namespace linerider.Tools
{
    /// <summary>
    /// Represents a selected line with a clone of the line before any change
    /// and Line being the current line state
    /// </summary>
    public class LineSelection
    {
        public GameLine line;
        public GameLine clone;
        /// <summary>
        /// Is selection snapped to Position
        /// </summary>
        public bool joint1;
        /// <summary>
        /// Is selection snapped to Position2
        /// </summary>
        public bool joint2;
        /// <summary>
        /// Are both joints snapped?
        /// </summary>
        public bool BothJoints => joint1 == joint2;
        /// <summary>
        /// Optional list of lines that are snapped this selection
        /// </summary>
        public List<LineSelection> snapped;
        public LineSelection()
        {
        }
        /// <summary>
        /// Initialize a lineselection automatically generating clone and applying bothjoints
        /// </summary>
        public LineSelection(GameLine Line, bool bothjoints)
        {
            line = Line;
            clone = line.Clone();
            joint1 = bothjoints;
            joint2 = bothjoints;
            snapped = [];
        }
        /// <summary>
        /// Initialize a lineselection automatically generating clone and applying bothjoints
        /// </summary>
        public LineSelection(GameLine Line, bool bothjoints, List<LineSelection> Snapped)
        {
            line = Line;
            clone = line.Clone();
            joint1 = bothjoints;
            joint2 = bothjoints;
            snapped = Snapped;
        }
        /// <summary>
        /// Initialize a lineselection automatically generating clone and generating joint snap
        /// </summary>
        public LineSelection(GameLine Line, Vector2d snapjoint)
        {
            line = Line;
            clone = line.Clone();
            joint1 = line.Position1 == snapjoint;
            joint2 = line.Position2 == snapjoint;
            snapped = [];
        }

        public LineType GetLineType() => line.Type;
    }
}