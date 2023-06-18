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
using linerider.UI;
using OpenTK;
using System.Drawing;

namespace linerider.Tools
{
    public class PanTool : Tool
    {
        private Vector2d CameraStart;
        private float ZoomStart;
        private Vector2d CameraTarget;
        private Vector2d startposition;
        private Vector2d lastposition;
        private bool zoom = false;

        public override string Name => "Hand Tool";
        public override Bitmap Icon => GameResources.icon_tool_pan.Bitmap;
        public override MouseCursor Cursor => Active
                    ? zoom ? game.Cursors.List[CursorsHandler.Type.Zoom] : game.Cursors.List[CursorsHandler.Type.DragActive]
                    : game.Cursors.List[CursorsHandler.Type.DragInactive];

        public PanTool() : base()
        {
        }

        public override void Cancel()
        {
            if (Active)
            {
                Active = false;
                game.Track.Camera.SetFrameCenter(CameraStart);
                game.Track.Zoom = ZoomStart;
            }
        }
        public override void OnMouseRightDown(Vector2d pos)
        {
            zoom = true;
            Active = true;
            startposition = pos;
            lastposition = startposition;
            CameraStart = game.Track.Camera.GetCenter();
            CameraTarget = ScreenToGameCoords(pos);
            ZoomStart = game.Track.Zoom;
            game.Invalidate();
            game.UpdateCursor();
            base.OnMouseRightDown(pos);
        }
        public override void OnMouseDown(Vector2d pos)
        {
            zoom = false;
            Active = true;
            startposition = pos;
            CameraStart = game.Track.Camera.GetCenter();
            ZoomStart = game.Track.Zoom;
            game.Invalidate();
            base.OnMouseDown(pos);
        }

        public override void OnMouseMoved(Vector2d pos)
        {
            if (Active)
            {
                if (zoom)
                {
                    game.Track.ZoomBy((float)(0.01 * (lastposition.Y - pos.Y)));
                    lastposition = pos;
                }
                else
                {
                    Vector2d newcenter =
                        CameraStart -
                        (pos / game.Track.Zoom -
                        startposition / game.Track.Zoom);
                    game.Track.Camera.SetFrameCenter(newcenter);
                }
                game.Invalidate();
            }
            base.OnMouseMoved(pos);
        }
        public override void OnMouseRightUp(Vector2d pos)
        {
            Active = false;
            base.OnMouseRightUp(pos);
        }
        public override void OnMouseUp(Vector2d pos)
        {
            Active = false;
            base.OnMouseUp(pos);
        }

        public override void Stop()
        {
            Active = false;
            base.Stop();
        }
    }
}