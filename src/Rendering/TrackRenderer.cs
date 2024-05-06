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

using linerider.Drawing;
using linerider.Game;
using linerider.Utils;
using OpenTK.Graphics.OpenGL;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;

namespace linerider.Rendering
{
    internal class TrackRenderer : GameService
    {
        private enum LineActionType
        {
            Add,
            Change,
            Remove
        }
        public bool RequiresUpdate = true;

        private readonly LineDecorator _decorator;
        private readonly LineRenderer _physvbo;
        private readonly LineRenderer _sceneryvbo;
        /// <summary>
        /// A dictionary of [line id] -> [index of first vertex]
        /// </summary>
        private Dictionary<int, int> _physlines;

        /// <summary>
        /// A dictionary of [line id] -> [index of first IBO index]
        /// </summary>
        /// <remarks>
        /// We have a seperate scenery vbo for rendering it under the sim one.
        /// </remarks>
        private Dictionary<int, int> _scenerylines;

        private static LayerContainer<Layer> _layers;

        /// <summary>
        /// We use an action queue system instead of instantly adding to the renderer
        /// for the sake of multithreading safety
        /// </summary>
        private readonly Queue<Tuple<LineActionType, GameLine>> _lineactions;
        private readonly ResourceSync _sync;
        private const int linesize = 6;
        public TrackRenderer()
        {
            _sync = new ResourceSync();
            _lineactions = new Queue<Tuple<LineActionType, GameLine>>();
            _physlines = new Dictionary<int, int>();
            _scenerylines = new Dictionary<int, int>();
            _decorator = new LineDecorator();
            _physvbo = new LineRenderer(Shaders.LineShader)
            {
                OverrideColor = Settings.Colors.ExportLine
            };

            _sceneryvbo = new LineRenderer(Shaders.LineShader)
            {
                OverrideColor = Color.Black
            };
        }
        public void Render(DrawOptions options, bool recording = false)
        {
            using (new GLEnableCap(EnableCap.Texture2D))
            {
                UpdateBuffers();
                GL.BlendFunc(
                    BlendingFactorSrc.SrcAlpha,
                    BlendingFactorDest.OneMinusSrcAlpha);
                GameDrawingMatrix.Enter();
                _physvbo.Scale = options.Zoom;
                _physvbo.KnobState = options.KnobState;

                _sceneryvbo.Scale = options.Zoom;
                // Green lines dont get lifelock
                _sceneryvbo.KnobState = options.KnobState != KnobState.Hidden ? KnobState.Shown : KnobState.Hidden;

                if ((Settings.PreviewMode || recording) && !(recording && !Settings.Recording.EnableColorTriggers))
                {
                    _sceneryvbo.OverrideColor = game.Track.Timeline.GetFrameLineColor(game.Track.Offset);
                    _physvbo.OverrideColor = game.Track.Timeline.GetFrameLineColor(game.Track.Offset);
                }
                else
                {
                    _sceneryvbo.OverrideColor = Settings.Computed.LineColor;
                    _physvbo.OverrideColor = Settings.Computed.LineColor;
                }

                if (options.LineColors)
                {
                    _sceneryvbo.OverrideColor = Settings.Colors.SceneryLine;
                    _sceneryvbo.OverridePriority = 1;
                    _physvbo.OverridePriority = 1;
                }
                else
                {
                    _sceneryvbo.OverridePriority = 255; // Force override
                    _physvbo.OverridePriority = 255;
                }
                _physvbo.Overlay = options.Overlay;
                _sceneryvbo.Overlay = options.Overlay;
                _sceneryvbo.Draw();
                _decorator.DrawUnder(options);
                _physvbo.Draw();
                GameDrawingMatrix.Exit();
            }
        }
        /// <summary>
        /// Clears the renderer and initializes it with new lines.
        /// </summary>
        public void InitializeTrack(Track track)
        {
            AutoArray<GameLine> scenery = new AutoArray<GameLine>(track.SceneryLines);
            scenery.UnsafeSetCount(track.SceneryLines);
            AutoArray<GameLine> phys = new AutoArray<GameLine>(track.BlueLines + track.RedLines);
            GameLine[] sorted = track.GetSortedLines();
            using (_sync.AcquireWrite())
            {
                _lineactions.Clear();
                _physvbo.Clear();
                _sceneryvbo.Clear();
                _layers = track._layers;
                _decorator.Clear();
                _physlines.Clear();
                _scenerylines.Clear();
                _physlines.Clear();
                RequiresUpdate = true;

                int scenerycount = 0;
                for (int i = 0; i < sorted.Length; i++)
                {
                    GameLine line = sorted[i];
                    if (line.Type == LineType.Scenery)
                    {
                        scenery.unsafe_array[scenery.Count - (1 + scenerycount++)] = line;
                    }
                    else
                    {
                        phys.Add(line);
                    }
                }
                Debug.Assert(scenerycount == scenery.Count,
                    "Predicted scenery count was not accurate");
                if (scenery.Count != 0)
                {
                    LineVertex[] sceneryverts = new LineVertex[scenery.Count * linesize];
                    _ = System.Threading.Tasks.Parallel.For(0, scenery.Count, (index) =>
                    {
                        int counter = 0;
                        LineVertex[] verts = GenerateLine(scenery[index], false);
                        foreach (LineVertex vert in verts)
                        {
                            sceneryverts[index * linesize + counter++] = vert;
                        }
                    });
                    _scenerylines = _sceneryvbo.AddLines(
                        scenery,
                        sceneryverts);
                }
                if (phys.Count != 0)
                {
                    LineVertex[] physverts = new LineVertex[phys.Count * linesize];
                    _ = System.Threading.Tasks.Parallel.For(0, phys.Count, (index) =>
                      {
                          int counter = 0;
                          LineVertex[] verts = GenerateLine(phys[index], false);
                          foreach (LineVertex vert in verts)
                          {
                              physverts[index * linesize + counter++] = vert;
                          }
                      });
                    _physlines = _physvbo.AddLines(
                        phys,
                        physverts);
                    _decorator.Initialize(phys);
                }
                RequiresUpdate = true;
            }
        }
        public void AddLine(GameLine line)
        {
            RequiresUpdate = true;
            using (_sync.AcquireWrite())
            {
                _lineactions.Enqueue(
                    new Tuple<LineActionType, GameLine>(
                        LineActionType.Add,
                        line));
            }
        }
        public void LineChanged(GameLine line)
        {
            RequiresUpdate = true;
            using (_sync.AcquireWrite())
            {
                _lineactions.Enqueue(
                    new Tuple<LineActionType, GameLine>(
                        LineActionType.Change,
                        line));
            }
        }
        public void RemoveLine(GameLine line)
        {
            RequiresUpdate = true;
            using (_sync.AcquireWrite())
            {
                _lineactions.Enqueue(
                    new Tuple<LineActionType, GameLine>(
                        LineActionType.Remove,
                        line));
            }
        }
        private void UpdateBuffers()
        {
            using (_sync.AcquireWrite())
            {
                RequiresUpdate = false;
                while (_lineactions.Count != 0)
                {
                    Tuple<LineActionType, GameLine> dequeued = _lineactions.Dequeue();
                    GameLine line = dequeued.Item2;
                    switch (dequeued.Item1)
                    {
                        case LineActionType.Add:
                            if (line.Type == LineType.Scenery)
                            {
                                AddLine(
                                    line,
                                    _sceneryvbo,
                                    _scenerylines);
                            }
                            else
                            {
                                AddLine(
                                    line,
                                    _physvbo,
                                    _physlines);
                                _decorator.AddLine((StandardLine)line);
                            }
                            break;
                        case LineActionType.Remove:
                            if (line.Type == LineType.Scenery)
                            {
                                RemoveLine(
                                    line,
                                    _sceneryvbo,
                                    _scenerylines);
                            }
                            else
                            {
                                RemoveLine(
                                    line,
                                    _physvbo,
                                    _physlines);
                                _decorator.RemoveLine((StandardLine)line);
                            }
                            break;
                        case LineActionType.Change:
                            if (line.Type == LineType.Scenery)
                            {
                                LineChanged(
                                    line,
                                    _sceneryvbo,
                                    _scenerylines,
                                    false);
                            }
                            else
                            {
                                bool hit = Settings.Editor.HitTest && game.Track.Timeline.IsLineHit(line.ID);
                                LineChanged(
                                    line,
                                    _physvbo,
                                    _physlines,
                                    hit);
                                _decorator.LineChanged((StandardLine)line, hit);
                            }
                            break;
                    }
                }
                _lineactions.Clear();
            }
        }
        private void AddLine(
            GameLine line,
            LineRenderer renderer,
            Dictionary<int, int> lookup)
        {
            if (line.layer == null)
            {
                line.layer = _layers.currentLayer;
                _layers.currentLayer.lines.Add(line.ID, line);
                Debug.WriteLine("line added to layer");
            }
            if (lookup.ContainsKey(line.ID))
            {
                LineChanged(line, renderer, lookup, false);
                return;
            }
            LineVertex[] lineverts = GenerateLine(line, false);
            int start = renderer.AddLine(lineverts);
            lookup.Add(line.ID, start);
        }
        private void LineChanged(
            GameLine line,
            LineRenderer renderer,
            Dictionary<int, int> lookup,
            bool hit)
        {
            LineVertex[] lineverts = GenerateLine(line, hit);
            renderer.ChangeLine(lookup[line.ID], lineverts);
        }
        private void RemoveLine(
            GameLine line,
            LineRenderer renderer,
            Dictionary<int, int> lookup)
        {
            int start = lookup[line.ID];
            renderer.RemoveLine(start);
            // Preserve the id in the lookup in event of undo
        }
        private static LineVertex[] GenerateLine(GameLine line, bool hit)
        {
            Layer layer = line.layer;
            int color = Utility.ColorToRGBA_LE(layer.GetColor());
            if (line is StandardLine stl)
            {
                if (hit)
                {
                    color = Utility.ColorToRGBA_LE(line.Color);
                }
                //else if (stl.Trigger != null)
                //{
                //    int trigger = Utility.ColorToRGBA_LE(
                //        Constants.TriggerLineColor);
                //    color = Utility.ChangeAlpha(trigger, 254);
                //}
            }
            LineVertex[] lineverts = LineRenderer.CreateTrackLine(
                line.Position1,
                line.Position2,
                2 * line.Width,
                color,
                (byte)line.SelectionState);
            return lineverts;
        }
    }
}