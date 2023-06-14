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

using linerider.Utils;
using OpenTK.Graphics.OpenGL;
using System;

namespace linerider.Drawing
{
    internal static class SafeFrameBuffer
    {
        private enum Fbosupport
        {
            None = 0,
            Full,
            EXT,
            ARB
        }
        public static bool CanRecord => support != Fbosupport.None && blit != Fbosupport.None;
        private static Fbosupport support;
        private static Fbosupport renderbuffer;
        private static Fbosupport blit;
        public static void Initialize()
        {
            string glstr = GL.GetString(StringName.Extensions).ToUpperInvariant();
            Version _version = new Version();
            try
            {
                // This can fail for any number of reasons
                string[] version = GL.GetString(StringName.Version).Split('.');
                int major = int.Parse(version[0]);
                int minor = int.Parse(version[1]);
                _version = new Version(major, minor);
                System.Diagnostics.Debug.WriteLine("Using OpenGL Version " + _version);
            }
            catch
            {
                // Ignored
            }
            if (glstr.Contains("GL_ARB_FRAMEBUFFER_OBJECT") || _version >= new Version(3, 0))
            {
                support = Fbosupport.Full;
                renderbuffer = Fbosupport.Full;
                blit = Fbosupport.Full;
            }
            else
            {
                support = glstr.Contains("GL_EXT_FRAMEBUFFER_OBJECT") ? Fbosupport.EXT : Fbosupport.None;
                if (glstr.Contains("EXT_FRAMEBUFFER_MULTISAMPLE"))
                {
                    renderbuffer = Fbosupport.EXT;
                }
                if (glstr.Contains("EXT_FRAMEBUFFER_BLIT"))
                {
                    blit = Fbosupport.EXT;
                }
            }
        }

        public static void RenderbufferStorageMultisample(RenderbufferTarget target, int samples,
            RenderbufferStorage internalformat, int width, int height)
        {

            /*
            https://www.khronos.org/opengles/sdk/docs/man3/docbook4/xhtml/glRenderbufferStorageMultisample.xml
            samples must be must be less than or equal to
                        the maximum number of samples supported for internalformat.
                        (see glGetInternalformativ).
        */
            // GL.GetInternalformat( ImageTarget.Renderbuffer, SizedInternalFormat.Rgba8, InternalFormatParameter.Samples,1,out samples);
            // ErrorLog.WriteLine("sup" + support + "blit" + blit + "renderbuf" + renderbuffer + " samples" + samples);
            switch (renderbuffer)
            {
                case Fbosupport.Full:
                    GL.RenderbufferStorageMultisample(target, samples, internalformat, width, height);
                    return;
                case Fbosupport.EXT:
                    GL.Ext.RenderbufferStorageMultisample(target, samples, internalformat, width, height);
                    return;
                default:
                    RenderbufferStorage(target, internalformat, width, height);
                    break;
            }
            ErrorCode err = GL.GetError();
            if (err != ErrorCode.NoError)
            {
                ErrorLog.WriteLine("RenderBufferStorageMultisample Error " + err);
            }
        }
        public static void RenderbufferStorage(RenderbufferTarget target,
            RenderbufferStorage internalformat, int width, int height)
        {
            switch (support)
            {
                case Fbosupport.Full:
                    GL.RenderbufferStorage(target, internalformat, width, height);
                    return;
                case Fbosupport.EXT:
                    GL.Ext.RenderbufferStorage(target, internalformat, width, height);
                    return;
                default:
                    throw new Exception("render buffer not supported");
            }
        }

        public static void DeleteRenderbuffers(int n, int[] renderbuffers)
        {

            switch (support)
            {
                case Fbosupport.Full:
                    GL.DeleteRenderbuffers(n, renderbuffers);
                    return;
                case Fbosupport.EXT:
                    GL.Ext.DeleteRenderbuffers(n, renderbuffers);
                    return;
                default:
                    throw new Exception("render buffer not supported");
            }
        }

        public static void DeleteFramebuffer(int framebuffers)
        {
            switch (support)
            {
                case Fbosupport.Full:
                    GL.DeleteFramebuffer(framebuffers);
                    return;
                case Fbosupport.EXT:
                    GL.Ext.DeleteFramebuffer(framebuffers);
                    return;
                default:
                    throw new Exception("render buffer not supported");
            }
        }

        public static void BindRenderbuffer(RenderbufferTarget target, int renderbuffer)
        {
            switch (support)
            {
                case Fbosupport.Full:
                    GL.BindRenderbuffer(target, renderbuffer);
                    return;
                case Fbosupport.EXT:
                    GL.Ext.BindRenderbuffer(target, renderbuffer);
                    return;
                default:
                    throw new Exception("render buffer not supported");
            }
        }

        public static int GenRenderbuffer()
        {
            switch (support)
            {
                case Fbosupport.Full:
                    return GL.GenRenderbuffer();
                case Fbosupport.EXT:
                    return GL.Ext.GenRenderbuffer();
                default:
                    throw new Exception("render buffer not supported");
            }
        }

        public static void BlitFramebuffer(int srcX0, int srcY0, int srcX1, int srcY1, int dstX0, int dstY0,
            int dstX1, int dstY1, ClearBufferMask mask,
            BlitFramebufferFilter filter)
        {
            switch (blit)
            {
                case Fbosupport.Full:
                    GL.BlitFramebuffer(srcX0, srcY0, srcX1, srcY1, dstX0, dstY0, dstX1, dstY1, mask, filter);
                    return;
                case Fbosupport.EXT:
                    GL.Ext.BlitFramebuffer(srcX0, srcY0, srcX1, srcY1, dstX0, dstY0, dstX1, dstY1, mask, filter);
                    return;
                default:
                    throw new Exception("render buffer not supported");
            }
        }

        public static FramebufferErrorCode CheckFramebufferStatus(
            FramebufferTarget target)
        {

            switch (support)
            {
                case Fbosupport.Full:
                    return GL.CheckFramebufferStatus(target);
                case Fbosupport.EXT:
                    return GL.Ext.CheckFramebufferStatus(target);
                default:
                    throw new Exception("render buffer not supported");
            }
        }

        public static void FramebufferRenderbuffer(FramebufferTarget target,
            FramebufferAttachment attachment,
            RenderbufferTarget renderbuffertarget, int renderbuffer)
        {
            switch (support)
            {
                case Fbosupport.Full:
                    GL.FramebufferRenderbuffer(target, attachment, renderbuffertarget, renderbuffer);
                    break;
                case Fbosupport.EXT:
                    GL.Ext.FramebufferRenderbuffer(target, attachment, renderbuffertarget, renderbuffer);
                    break;
            }
        }

        public static int GenFramebuffer()
        {
            switch (support)
            {
                case Fbosupport.Full:
                    return GL.GenFramebuffer();
                case Fbosupport.EXT:
                    return GL.Ext.GenFramebuffer();
            }
            return 0;
        }
        public static void BindFramebuffer(FramebufferTarget target, int framebuffer)
        {
            switch (support)
            {
                case Fbosupport.Full:
                    GL.BindFramebuffer(target, framebuffer);
                    break;
                case Fbosupport.EXT:
                    GL.Ext.BindFramebuffer(target, framebuffer);
                    break;
            }
        }
    }
}
