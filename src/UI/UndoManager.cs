//  Author:
//       Noah Ablaseau <nablaseau@hotmail.com>
//
//  Copyright (c) 2017 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using linerider.Game;
using System;
using System.Collections.Generic;
namespace linerider
{
    public class UndoManager : GameService
    {
        private class act : GameService
        {
            public object UserHint = null;
            public List<GameLine> States;
            public act()
            {
                States = [];
            }
            private bool DoAction(TrackWriter track, GameLine beforeact, GameLine afteract)
            {
                if (beforeact == null && afteract == null)
                    throw new ArgumentNullException(
                        "undo action with no values");
                // Remove line
                if (afteract == null)
                {
                    track.RemoveLine(beforeact);
                }
                // Add line
                else if (beforeact == null)
                {
                    track.AddLine(afteract.Clone());
                }
                // Replace line
                else if (beforeact.Type != afteract.Type)
                {
                    track.RemoveLine(beforeact);
                    track.AddLine(afteract);
                }
                // Move action
                else
                {
                    track.ReplaceLine(beforeact, afteract.Clone());
                }
                return !(beforeact is SceneryLine);

            }
            /// <summary>
            /// Undo previous action, returns true if physics are changed
            /// </summary>
            public void Undo(TrackWriter track)
            {
                bool physchanged = false;
                for (int i = States.Count - 1; i > 0; i -= 2)
                {
                    physchanged |= DoAction(track, States[i], States[i - 1]);
                }

                if (physchanged)
                    track.NotifyTrackChanged();
            }

            public void Redo(TrackWriter track)
            {
                bool physchanged = false;
                for (int i = 0; i < States.Count - 1; i += 2)
                {
                    physchanged |= DoAction(track, States[i], States[i + 1]);
                }
                if (physchanged)
                    track.NotifyTrackChanged();
            }
            public override string ToString()
            {
                if (States != null && States.Count != 0)
                {
                    string ret = "";
                    foreach (GameLine state in States)
                    {
                        ret += state.ToString() + "|";
                    }
                    return ret;
                }
                return base.ToString();
            }
        }
        public int ActionCount { get; private set; }
        private int _position = 0;
        private readonly List<act> _actions = [];
        private act _currentaction;
        private const int MaximumBufferSize = 10000;
        public UndoManager()
        {
        }
        public void SetActionUserHint(object hint)
        {
            if (_currentaction == null)
                throw new Exception("UndoManager current action null");
            _currentaction.UserHint = hint;
        }
        /// <summary>
        /// After calling beginaction the current state will be added tothe action
        /// </summary>
        public void AddChange(GameLine before, GameLine after)
        {
            if (_currentaction == null)
                throw new Exception("UndoManager current action null");
            _currentaction.States.Add(before?.Clone());
            _currentaction.States.Add(after?.Clone());
        }
        public void BeginAction()
        {
            if (_currentaction != null)
                throw new Exception("Attempt to overwrite current undo state");
            _currentaction = new act();
        }
        public void CancelAction()
        {
            if (_currentaction == null)
                throw new Exception("UndoManager current action null");
            DoAction(_currentaction, true);
            _currentaction = null;
        }
        public void EndAction()
        {
            if (_currentaction == null)
                throw new Exception("UndoManager current action null");
            if (_position != _actions.Count)
            {
                _actions.RemoveRange(_position, _actions.Count - _position);
            }
            if (_actions.Count > MaximumBufferSize)
            {
                _actions.RemoveRange(0, _actions.Count - MaximumBufferSize / 2);
            }
            _actions.Add(_currentaction);
            _position = _actions.Count;
            ActionCount++;
            _currentaction = null;
        }
        public object Undo()
        {
            if (_actions.Count > 0 && _position > 0)
            {
                _position--;
                ActionCount--;
                act action = _actions[_position];
                DoAction(action, true);
                return action.UserHint;
            }
            return null;
        }

        public object Redo()
        {
            if (_actions.Count > 0 && _position < _actions.Count)
            {
                act action = _actions[_position];
                _position++;
                ActionCount++;
                DoAction(action, false);
                return action.UserHint;
            }
            return null;
        }
        private void DoAction(act action, bool undo)
        {
            using (TrackWriter trk = game.Track.CreateTrackWriter())
            {
                trk.DisableUndo();
                trk.DisableExtensionUpdating();
                if (undo)
                    action.Undo(trk);
                else
                    action.Redo(trk);
            }
            game.Track.NotifyTrackChanged();
            game.Track.Invalidate();
        }
    }
}