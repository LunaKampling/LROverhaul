
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
using OpenTK;
using System;
using System.Threading;
//using System.Windows.Forms;

namespace linerider.UI
{
    public class PlatformImpl : Gwen.Platform.Neutral.PlatformImplementation
    {
        public MouseCursor CurrentCursor = MouseCursor.Default;
        private readonly MainWindow game;
        public PlatformImpl(MainWindow game)
        {
            this.game = game;
        }
        public override bool SetClipboardText(string text)
        {
            bool ret = false;
            Thread staThread = new Thread(
                () =>
                {
                    try
                    {
                        if (OperatingSystem.IsWindows()) {
                            /* TODO replace, maybe glfw clipboard api   
                            Clipboard.SetText(text);
                            */
                        }
                        ret = true;
                    }
                    catch (Exception)
                    {
                        return;
                    }
                });
            if (OperatingSystem.IsWindows()) staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
            // At this point either you have clipboard data or an exception
            return ret;
        }
        public override string GetClipboardText()
        {
            // Code from http://forums.getpaint.net/index.php?/topic/13712-trouble-accessing-the-clipboard/page__view__findpost__p__226140
            string ret = string.Empty;
            Thread staThread = new Thread(
                () =>
                {
                    try
                    {
                        if (OperatingSystem.IsWindows()) {
                            /* TODO replace, maybe glfw clipboard api   
                            if (!Clipboard.ContainsText())
                                return;
                            ret = Clipboard.GetText();
                            */
                        }
                    }
                    catch (Exception)
                    {
                        return;
                    }
                });
            if (OperatingSystem.IsWindows()) staThread.SetApartmentState(ApartmentState.STA);
            staThread.Start();
            staThread.Join();
            // At this point either you have clipboard data or an exception
            return ret;
        }
        private void SetGameCursor(MouseCursor cursor)
        {
            if (game.Cursor != cursor)
            {
                CurrentCursor = cursor;
                game.UpdateCursor();
            }
        }
        public override void SetCursor(Gwen.Cursor c)
        {
            switch (c.Name)
            {
                default:
                case "Default":
                    SetGameCursor(game.Cursors.List[CursorsHandler.Type.Default]);
                    return;
                case "SizeWE":
                    SetGameCursor(game.Cursors.List[CursorsHandler.Type.SizeWE]);
                    break;
                case "SizeNWSE":
                    SetGameCursor(game.Cursors.List[CursorsHandler.Type.SizeNWSE]);
                    break;
                case "SizeNS":
                    SetGameCursor(game.Cursors.List[CursorsHandler.Type.SizeNS]);
                    break;
                case "SizeNESW":
                    SetGameCursor(game.Cursors.List[CursorsHandler.Type.SizeSWNE]);
                    break;
                case "IBeam":
                    SetGameCursor(game.Cursors.List[CursorsHandler.Type.Beam]);
                    break;
                case "Hand":
                    SetGameCursor(game.Cursors.List[CursorsHandler.Type.Hand]);
                    break;
                case "SizeAll":
                case "Help":
                case "No":
                    SetGameCursor(game.Cursors.List[CursorsHandler.Type.Default]);
                    break;
            }
        }
    }
}