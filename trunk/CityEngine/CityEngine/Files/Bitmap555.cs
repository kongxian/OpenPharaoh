﻿// -----------------------------------------------------------------------
// <copyright file="Bitmap555.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace CityEngine.Files
{
    using System;
    using System.Collections.Generic;
    using System.Drawing;
    using System.IO;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public static class Bitmap555
    {
        public static Bitmap Load(Bitmap555Type type, string filename, ushort width, ushort height, uint offset)
        {
            switch (type)
            {
                case Bitmap555Type.Isometric:
                    return LoadIsometricBitmap(filename, width, height, offset);
                case Bitmap555Type.PlainCompressed256:
                case Bitmap555Type.PlainCompressed257:
                case Bitmap555Type.PlainCompressed276:
                    return LoadPlainCompressedBitmap(filename, width, height, offset);
                case Bitmap555Type.Plain:
                case Bitmap555Type.Plain1:
                case Bitmap555Type.Plain10:
                case Bitmap555Type.Plain12:
                case Bitmap555Type.Plain13:
                    return LoadPlainBitmap(filename, width, height, offset);
                default:
                    throw new NotSupportedException("Unknown image type: " + ((uint)type).ToString("X"));
            }
        }

        public static Bitmap LoadPlainBitmap(string filename, ushort width, ushort height, uint offset)
        {
            var b = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
            var bd = b.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);

            int PixelSize = 2;

            using (var f = new FileStream(filename, FileMode.Open))
            {
                f.Seek(offset, SeekOrigin.Begin);

                unsafe
                {
                    for (int y = 0; y < height; y++)
                    {
                        byte* row = (byte*)bd.Scan0 + (y * bd.Stride);

                        for (int x = 0; x < width; x++)
                        {
                            var b1 = (byte)f.ReadByte();
                            var b2 = (byte)f.ReadByte();

                            row[(x * PixelSize) + 0] = b1;
                            row[(x * PixelSize) + 1] = b2;
                        }
                    }
                }
            }

            b.UnlockBits(bd);
            b.MakeTransparent(System.Drawing.Color.FromArgb(247, 0, 255));
            return b;
        }

        public static Bitmap LoadPlainCompressedBitmap(string filename, ushort width, ushort height, uint offset)
        {
            var b = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
            var bd = b.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);

            int PixelSize = 2;

            using (var f = new FileStream(filename, FileMode.Open))
            {
                f.Seek(offset, SeekOrigin.Begin);

                unsafe
                {
                    for (int y = 0; y < height; y++)
                    {
                        byte* row = (byte*)bd.Scan0 + (y * bd.Stride);

                        var x = 0;
                        while (x < width)
                        {
                            var countPixel = (byte)f.ReadByte();

                            if (countPixel == 0xFF)
                            {
                                var countSkip = (byte)f.ReadByte();

                                for (int i = 0; i < countSkip; i++)
                                {
                                    // fill with 0xf81f /Magenta/
                                    row[(x * PixelSize) + 0] = 0x1f;
                                    row[(x * PixelSize) + 1] = 0xf8;

                                    x++;
                                }
                            }
                            else
                            {
                                for (int i = 0; i < countPixel; i++)
                                {
                                    var b1 = (byte)f.ReadByte();
                                    var b2 = (byte)f.ReadByte();

                                    row[(x * PixelSize) + 0] = b1;
                                    row[(x * PixelSize) + 1] = b2;

                                    x++;
                                }
                            }
                        }
                    }
                }
            }

            b.UnlockBits(bd);
            b.MakeTransparent(System.Drawing.Color.FromArgb(247, 0, 255));
            return b;
        }

        public static Bitmap LoadPlainCompressedBitmap(string filename, ushort width, ushort height, uint offset, uint length)
        {
            var b = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
            var bd = b.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);

            int PixelSize = 2;

            using (var f = new FileStream(filename, FileMode.Open))
            {
                f.Seek(offset, SeekOrigin.Begin);

                unsafe
                {
                    uint position = 0;

                    for (int y = 0; y < height; y++)
                    {
                        byte* row = (byte*)bd.Scan0 + (y * bd.Stride);

                        var x = 0;
                        while (x < width)
                        {
                            if (position >= length)
                            {
                                row[(x * PixelSize) + 0] = 0x1f;
                                row[(x * PixelSize) + 1] = 0xf8;
                                x++;
                                continue;
                            }

                            var countPixel = (byte)f.ReadByte();
                            position += 1;

                            if (countPixel == 0xFF)
                            {
                                var countSkip = (byte)f.ReadByte();
                                position += 1;

                                for (int i = 0; i < countSkip; i++)
                                {
                                    // fill with 0xf81f /Magenta/
                                    row[(x * PixelSize) + 0] = 0x1f;
                                    row[(x * PixelSize) + 1] = 0xf8;

                                    x++;
                                }
                            }
                            else
                            {
                                for (int i = 0; i < countPixel; i++)
                                {
                                    var b1 = (byte)f.ReadByte();
                                    var b2 = (byte)f.ReadByte();
                                    position += 2;

                                    row[(x * PixelSize) + 0] = b1;
                                    row[(x * PixelSize) + 1] = b2;

                                    x++;
                                }
                            }
                        }
                    }
                }
            }

            b.UnlockBits(bd);
            b.MakeTransparent(Color.FromArgb(247, 0, 255));
            return b;
        }

        public static ushort TILE_WIDTH = 58;
        public static ushort TILE_HEIGHT = 30;
        public static uint TILE_BYTES = 1800;

        public static Bitmap LoadIsometricBitmap(string filename, ushort width, ushort height, uint offset)
        {
            // no matter what are width and height, the isometric tile is always:
            // 1800 bytes, 58x30

            width = TILE_WIDTH;
            height = TILE_HEIGHT;

            var b = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);
            var bd = b.LockBits(new Rectangle(0, 0, width, height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format16bppRgb555);

            int PixelSize = 2;

            using (var f = new FileStream(filename, FileMode.Open))
            {
                f.Seek(offset, SeekOrigin.Begin);

                unsafe
                {
                    var startX = (width / 2) - 1;
                    var endX = (width / 2) + 1;

                    for (int y = 0; y < height; y++)
                    {
                        byte* row = (byte*)bd.Scan0 + (y * bd.Stride);

                        for (int x = 0; x < startX; x++)
                        {
                            // fill with 0xf81f /Magenta/
                            row[(x * PixelSize) + 0] = 0x1f;
                            row[(x * PixelSize) + 1] = 0xf8;
                        }

                        for (int x = startX; x < endX; x++)
                        {
                            var b1 = (byte)f.ReadByte();
                            var b2 = (byte)f.ReadByte();

                            row[(x * PixelSize) + 0] = b1;
                            row[(x * PixelSize) + 1] = b2;
                        }

                        for (int x = endX; x < width; x++)
                        {
                            // fill with 0xf81f /Magenta/
                            row[(x * PixelSize) + 0] = 0x1f;
                            row[(x * PixelSize) + 1] = 0xf8;
                        }

                        if (y >= (height / 2))
                        {
                            startX += 2;
                            endX -= 2;
                        }
                        else
                        {
                            if (startX != 0)
                            {
                                startX -= 2;
                                endX += 2;
                            }
                        }
                    }
                }
            }

            b.UnlockBits(bd);
            b.MakeTransparent(Color.FromArgb(247, 0, 255));
            return b;
        }
    }
}
