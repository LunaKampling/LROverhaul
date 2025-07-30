// Replacement for System.Drawing.Color that can cast to SkiaSharp.Color
using SkiaSharp;

public struct Color
{
    byte red;
    byte green;
    byte blue;
    byte alpha;

    public Color(uint data)
    {
        var color = new SKColor(data);
        this.red = color.Red;
        this.green = color.Green;
        this.blue = color.Blue;
        this.alpha = color.Alpha;
    }

    public Color(int r, int g, int b, int a)
    {
        this.red = (byte)r;
        this.green = (byte)g;
        this.blue = (byte)b;
        this.alpha = (byte)a;
    }

    public byte A
    {
        get => alpha;
        set => alpha = value;
    }
    public byte R
    {
        get => red;
        set => red = value;
    }
    public byte G
    {
        get => green;
        set => green = value;
    }
    public byte B
    {
        get => blue;
        set => blue = value;
    }
    public float GetHue()
    {
        return ((SKColor)this).Hue;
    }
    public bool IsEmpty
    {
        get => Equals(Empty);
    }

    public static Color FromArgb(int r, int g, int b)
    {
        return new Color(r, g, b, 255);
    }
    public static Color FromArgb(int a, Color rgb)
    {
        return new Color(rgb.R, rgb.G, rgb.B, a);
    }
    public static Color FromArgb(int a, int r, int g, int b)
    {
        return new Color(r, g, b, a);
    }
    public static Color FromArgb(int data)
    {
        return new Color((uint)data);
    }

    public int ToArgb()
    {
        return B + G * 256 + R * 256 * 256 + A * 256 * 256 * 256;
    }

    public bool Equals(Color c) => c.R == R && c.G == G && c.B == B && c.A == A;
    public override bool Equals(object o)
    {
        if (o is Color)
            return Equals((Color)o);
        return false;
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool operator ==(Color left, Color right) => left.Equals(right);
    public static bool operator !=(Color left, Color right) => !left.Equals(right);

    public static implicit operator SKColor(Color c) => new(c.R, c.G, c.B, c.A);
    public static implicit operator Color(SKColor s) => new(s.Red, s.Green, s.Blue, s.Alpha);

    // named colors copied from SkiaSharp.SkColors

    //
    // Summary:
    //     Gets the predefined color of alice blue, or #FFF0F8FF.
    public static Color AliceBlue = new(4293982463u);

    //
    // Summary:
    //     Gets the predefined color of antique white, or #FFFAEBD7.
    public static Color AntiqueWhite = new(4294634455u);

    //
    // Summary:
    //     Gets the predefined color of aqua, or #FF00FFFF.
    public static Color Aqua = new(4278255615u);

    //
    // Summary:
    //     Gets the predefined color of aquamarine, or #FF7FFFD4.
    public static Color Aquamarine = new(4286578644u);

    //
    // Summary:
    //     Gets the predefined color of azure, or #FFF0FFFF.
    public static Color Azure = new(4293984255u);

    //
    // Summary:
    //     Gets the predefined color of beige, or #FFF5F5DC.
    public static Color Beige = new(4294309340u);

    //
    // Summary:
    //     Gets the predefined color of bisque, or #FFFFE4C4.
    public static Color Bisque = new(4294960324u);

    //
    // Summary:
    //     Gets the predefined color of black, or #FF000000.
    public static Color Black = new(4278190080u);

    //
    // Summary:
    //     Gets the predefined color of blanched almond, or #FFFFEBCD.
    public static Color BlanchedAlmond = new(4294962125u);

    //
    // Summary:
    //     Gets the predefined color of blue, or #FF0000FF.
    public static Color Blue = new(4278190335u);

    //
    // Summary:
    //     Gets the predefined color of blue violet, or #FF8A2BE2.
    public static Color BlueViolet = new(4287245282u);

    //
    // Summary:
    //     Gets the predefined color of brown, or #FFA52A2A.
    public static Color Brown = new(4289014314u);

    //
    // Summary:
    //     Gets the predefined color of burly wood, or #FFDEB887.
    public static Color BurlyWood = new(4292786311u);

    //
    // Summary:
    //     Gets the predefined color of cadet blue, or #FF5F9EA0.
    public static Color CadetBlue = new(4284456608u);

    //
    // Summary:
    //     Gets the predefined color of chartreuse, or #FF7FFF00.
    public static Color Chartreuse = new(4286578432u);

    //
    // Summary:
    //     Gets the predefined color of chocolate, or #FFD2691E.
    public static Color Chocolate = new(4291979550u);

    //
    // Summary:
    //     Gets the predefined color of coral, or #FFFF7F50.
    public static Color Coral = new(4294934352u);

    //
    // Summary:
    //     Gets the predefined color of cornflower blue, or #FF6495ED.
    public static Color CornflowerBlue = new(4284782061u);

    //
    // Summary:
    //     Gets the predefined color of cornsilk, or #FFFFF8DC.
    public static Color Cornsilk = new(4294965468u);

    //
    // Summary:
    //     Gets the predefined color of crimson, or #FFDC143C.
    public static Color Crimson = new(4292613180u);

    //
    // Summary:
    //     Gets the predefined color of cyan, or #FF00FFFF.
    public static Color Cyan = new(4278255615u);

    //
    // Summary:
    //     Gets the predefined color of dark blue, or #FF00008B.
    public static Color DarkBlue = new(4278190219u);

    //
    // Summary:
    //     Gets the predefined color of dark cyan, or #FF008B8B.
    public static Color DarkCyan = new(4278225803u);

    //
    // Summary:
    //     Gets the predefined color of dark goldenrod, or #FFB8860B.
    public static Color DarkGoldenrod = new(4290283019u);

    //
    // Summary:
    //     Gets the predefined color of dark gray, or #FFA9A9A9.
    public static Color DarkGray = new(4289309097u);

    //
    // Summary:
    //     Gets the predefined color of dark green, or #FF006400.
    public static Color DarkGreen = new(4278215680u);

    //
    // Summary:
    //     Gets the predefined color of dark khaki, or #FFBDB76B.
    public static Color DarkKhaki = new(4290623339u);

    //
    // Summary:
    //     Gets the predefined color of dark magenta, or #FF8B008B.
    public static Color DarkMagenta = new(4287299723u);

    //
    // Summary:
    //     Gets the predefined color of dark olive green, or #FF556B2F.
    public static Color DarkOliveGreen = new(4283788079u);

    //
    // Summary:
    //     Gets the predefined color of dark orange, or #FFFF8C00.
    public static Color DarkOrange = new(4294937600u);

    //
    // Summary:
    //     Gets the predefined color of dark orchid, or #FF9932CC.
    public static Color DarkOrchid = new(4288230092u);

    //
    // Summary:
    //     Gets the predefined color of dark red, or #FF8B0000.
    public static Color DarkRed = new(4287299584u);

    //
    // Summary:
    //     Gets the predefined color of dark salmon, or #FFE9967A.
    public static Color DarkSalmon = new(4293498490u);

    //
    // Summary:
    //     Gets the predefined color of dark sea green, or #FF8FBC8B.
    public static Color DarkSeaGreen = new(4287609995u);

    //
    // Summary:
    //     Gets the predefined color of dark slate blue, or #FF483D8B.
    public static Color DarkSlateBlue = new(4282924427u);

    //
    // Summary:
    //     Gets the predefined color of dark slate gray, or #FF2F4F4F.
    public static Color DarkSlateGray = new(4281290575u);

    //
    // Summary:
    //     Gets the predefined color of dark turquoise, or #FF00CED1.
    public static Color DarkTurquoise = new(4278243025u);

    //
    // Summary:
    //     Gets the predefined color of dark violet, or #FF9400D3.
    public static Color DarkViolet = new(4287889619u);

    //
    // Summary:
    //     Gets the predefined color of deep pink, or #FFFF1493.
    public static Color DeepPink = new(4294907027u);

    //
    // Summary:
    //     Gets the predefined color of deep sky blue, or #FF00BFFF.
    public static Color DeepSkyBlue = new(4278239231u);

    //
    // Summary:
    //     Gets the predefined color of dim gray, or #FF696969.
    public static Color DimGray = new(4285098345u);

    //
    // Summary:
    //     Gets the predefined color of dodger blue, or #FF1E90FF.
    public static Color DodgerBlue = new(4280193279u);

    //
    // Summary:
    //     Gets the predefined color of firebrick, or #FFB22222.
    public static Color Firebrick = new(4289864226u);

    //
    // Summary:
    //     Gets the predefined color of floral white, or #FFFFFAF0.
    public static Color FloralWhite = new(4294966000u);

    //
    // Summary:
    //     Gets the predefined color of forest green, or #FF228B22.
    public static Color ForestGreen = new(4280453922u);

    //
    // Summary:
    //     Gets the predefined color of fuchsia, or #FFFF00FF.
    public static Color Fuchsia = new(4294902015u);

    //
    // Summary:
    //     Gets the predefined color of gainsboro, or #FFDCDCDC.
    public static Color Gainsboro = new(4292664540u);

    //
    // Summary:
    //     Gets the predefined color of ghost white, or #FFF8F8FF.
    public static Color GhostWhite = new(4294506751u);

    //
    // Summary:
    //     Gets the predefined color of gold, or #FFFFD700.
    public static Color Gold = new(4294956800u);

    //
    // Summary:
    //     Gets the predefined color of goldenrod, or #FFDAA520.
    public static Color Goldenrod = new(4292519200u);

    //
    // Summary:
    //     Gets the predefined color of gray, or #FF808080.
    public static Color Gray = new(4286611584u);

    //
    // Summary:
    //     Gets the predefined color of green, or #FF008000.
    public static Color Green = new(4278222848u);

    //
    // Summary:
    //     Gets the predefined color of green yellow, or #FFADFF2F.
    public static Color GreenYellow = new(4289593135u);

    //
    // Summary:
    //     Gets the predefined color of honeydew, or #FFF0FFF0.
    public static Color Honeydew = new(4293984240u);

    //
    // Summary:
    //     Gets the predefined color of hot pink, or #FFFF69B4.
    public static Color HotPink = new(4294928820u);

    //
    // Summary:
    //     Gets the predefined color of indian red, or #FFCD5C5C.
    public static Color IndianRed = new(4291648604u);

    //
    // Summary:
    //     Gets the predefined color of indigo, or #FF4B0082.
    public static Color Indigo = new(4283105410u);

    //
    // Summary:
    //     Gets the predefined color of ivory, or #FFFFFFF0.
    public static Color Ivory = new(4294967280u);

    //
    // Summary:
    //     Gets the predefined color of khaki, or #FFF0E68C.
    public static Color Khaki = new(4293977740u);

    //
    // Summary:
    //     Gets the predefined color of lavender, or #FFE6E6FA.
    public static Color Lavender = new(4293322490u);

    //
    // Summary:
    //     Gets the predefined color of lavender blush, or #FFFFF0F5.
    public static Color LavenderBlush = new(4294963445u);

    //
    // Summary:
    //     Gets the predefined color of lawn green, or #FF7CFC00.
    public static Color LawnGreen = new(4286381056u);

    //
    // Summary:
    //     Gets the predefined color of lemon chiffon, or #FFFFFACD.
    public static Color LemonChiffon = new(4294965965u);

    //
    // Summary:
    //     Gets the predefined color of light blue, or #FFADD8E6.
    public static Color LightBlue = new(4289583334u);

    //
    // Summary:
    //     Gets the predefined color of light coral, or #FFF08080.
    public static Color LightCoral = new(4293951616u);

    //
    // Summary:
    //     Gets the predefined color of light cyan, or #FFE0FFFF.
    public static Color LightCyan = new(4292935679u);

    //
    // Summary:
    //     Gets the predefined color of light goldenrod yellow, or #FFFAFAD2.
    public static Color LightGoldenrodYellow = new(4294638290u);

    //
    // Summary:
    //     Gets the predefined color of light gray, or #FFD3D3D3.
    public static Color LightGray = new(4292072403u);

    //
    // Summary:
    //     Gets the predefined color of light green, or #FF90EE90.
    public static Color LightGreen = new(4287688336u);

    //
    // Summary:
    //     Gets the predefined color of light pink, or #FFFFB6C1.
    public static Color LightPink = new(4294948545u);

    //
    // Summary:
    //     Gets the predefined color of light salmon, or #FFFFA07A.
    public static Color LightSalmon = new(4294942842u);

    //
    // Summary:
    //     Gets the predefined color of light sea green, or #FF20B2AA.
    public static Color LightSeaGreen = new(4280332970u);

    //
    // Summary:
    //     Gets the predefined color of light sky blue, or #FF87CEFA.
    public static Color LightSkyBlue = new(4287090426u);

    //
    // Summary:
    //     Gets the predefined color of light slate gray, or #FF778899.
    public static Color LightSlateGray = new(4286023833u);

    //
    // Summary:
    //     Gets the predefined color of light steel blue, or #FFB0C4DE.
    public static Color LightSteelBlue = new(4289774814u);

    //
    // Summary:
    //     Gets the predefined color of light yellow, or #FFFFFFE0.
    public static Color LightYellow = new(4294967264u);

    //
    // Summary:
    //     Gets the predefined color of lime, or #FF00FF00.
    public static Color Lime = new(4278255360u);

    //
    // Summary:
    //     Gets the predefined color of lime green, or #FF32CD32.
    public static Color LimeGreen = new(4281519410u);

    //
    // Summary:
    //     Gets the predefined color of linen, or #FFFAF0E6.
    public static Color Linen = new(4294635750u);

    //
    // Summary:
    //     Gets the predefined color of magenta, or #FFFF00FF.
    public static Color Magenta = new(4294902015u);

    //
    // Summary:
    //     Gets the predefined color of maroon, or #FF800000.
    public static Color Maroon = new(4286578688u);

    //
    // Summary:
    //     Gets the predefined color of medium aquamarine, or #FF66CDAA.
    public static Color MediumAquamarine = new(4284927402u);

    //
    // Summary:
    //     Gets the predefined color of medium blue, or #FF0000CD.
    public static Color MediumBlue = new(4278190285u);

    //
    // Summary:
    //     Gets the predefined color of medium orchid, or #FFBA55D3.
    public static Color MediumOrchid = new(4290401747u);

    //
    // Summary:
    //     Gets the predefined color of medium purple, or #FF9370DB.
    public static Color MediumPurple = new(4287852763u);

    //
    // Summary:
    //     Gets the predefined color of medium sea green, or #FF3CB371.
    public static Color MediumSeaGreen = new(4282168177u);

    //
    // Summary:
    //     Gets the predefined color of medium slate blue, or #FF7B68EE.
    public static Color MediumSlateBlue = new(4286277870u);

    //
    // Summary:
    //     Gets the predefined color of medium spring green, or #FF00FA9A.
    public static Color MediumSpringGreen = new(4278254234u);

    //
    // Summary:
    //     Gets the predefined color of medium turquoise, or #FF48D1CC.
    public static Color MediumTurquoise = new(4282962380u);

    //
    // Summary:
    //     Gets the predefined color of medium violet red, or #FFC71585.
    public static Color MediumVioletRed = new(4291237253u);

    //
    // Summary:
    //     Gets the predefined color of midnight blue, or #FF191970.
    public static Color MidnightBlue = new(4279834992u);

    //
    // Summary:
    //     Gets the predefined color of mint cream, or #FFF5FFFA.
    public static Color MintCream = new(4294311930u);

    //
    // Summary:
    //     Gets the predefined color of misty rose, or #FFFFE4E1.
    public static Color MistyRose = new(4294960353u);

    //
    // Summary:
    //     Gets the predefined color of moccasin, or #FFFFE4B5.
    public static Color Moccasin = new(4294960309u);

    //
    // Summary:
    //     Gets the predefined color of navajo white, or #FFFFDEAD.
    public static Color NavajoWhite = new(4294958765u);

    //
    // Summary:
    //     Gets the predefined color of navy, or #FF000080.
    public static Color Navy = new(4278190208u);

    //
    // Summary:
    //     Gets the predefined color of old lace, or #FFFDF5E6.
    public static Color OldLace = new(4294833638u);

    //
    // Summary:
    //     Gets the predefined color of olive, or #FF808000.
    public static Color Olive = new(4286611456u);

    //
    // Summary:
    //     Gets the predefined color of olive drab, or #FF6B8E23.
    public static Color OliveDrab = new(4285238819u);

    //
    // Summary:
    //     Gets the predefined color of orange, or #FFFFA500.
    public static Color Orange = new(4294944000u);

    //
    // Summary:
    //     Gets the predefined color of orange red, or #FFFF4500.
    public static Color OrangeRed = new(4294919424u);

    //
    // Summary:
    //     Gets the predefined color of orchid, or #FFDA70D6.
    public static Color Orchid = new(4292505814u);

    //
    // Summary:
    //     Gets the predefined color of pale goldenrod, or #FFEEE8AA.
    public static Color PaleGoldenrod = new(4293847210u);

    //
    // Summary:
    //     Gets the predefined color of pale green, or #FF98FB98.
    public static Color PaleGreen = new(4288215960u);

    //
    // Summary:
    //     Gets the predefined color of pale turquoise, or #FFAFEEEE.
    public static Color PaleTurquoise = new(4289720046u);

    //
    // Summary:
    //     Gets the predefined color of pale violet red, or #FFDB7093.
    public static Color PaleVioletRed = new(4292571283u);

    //
    // Summary:
    //     Gets the predefined color of papaya whip, or #FFFFEFD5.
    public static Color PapayaWhip = new(4294963157u);

    //
    // Summary:
    //     Gets the predefined color of peach puff, or #FFFFDAB9.
    public static Color PeachPuff = new(4294957753u);

    //
    // Summary:
    //     Gets the predefined color of peru, or #FFCD853F.
    public static Color Peru = new(4291659071u);

    //
    // Summary:
    //     Gets the predefined color of pink, or #FFFFC0CB.
    public static Color Pink = new(4294951115u);

    //
    // Summary:
    //     Gets the predefined color of plum, or #FFDDA0DD.
    public static Color Plum = new(4292714717u);

    //
    // Summary:
    //     Gets the predefined color of powder blue, or #FFB0E0E6.
    public static Color PowderBlue = new(4289781990u);

    //
    // Summary:
    //     Gets the predefined color of purple, or #FF800080.
    public static Color Purple = new(4286578816u);

    //
    // Summary:
    //     Gets the predefined color of red, or #FFFF0000.
    public static Color Red = new(4294901760u);

    //
    // Summary:
    //     Gets the predefined color of rosy brown, or #FFBC8F8F.
    public static Color RosyBrown = new(4290547599u);

    //
    // Summary:
    //     Gets the predefined color of royal blue, or #FF4169E1.
    public static Color RoyalBlue = new(4282477025u);

    //
    // Summary:
    //     Gets the predefined color of saddle brown, or #FF8B4513.
    public static Color SaddleBrown = new(4287317267u);

    //
    // Summary:
    //     Gets the predefined color of salmon, or #FFFA8072.
    public static Color Salmon = new(4294606962u);

    //
    // Summary:
    //     Gets the predefined color of sandy brown, or #FFF4A460.
    public static Color SandyBrown = new(4294222944u);

    //
    // Summary:
    //     Gets the predefined color of sea green, or #FF2E8B57.
    public static Color SeaGreen = new(4281240407u);

    //
    // Summary:
    //     Gets the predefined color of sea shell, or #FFFFF5EE.
    public static Color SeaShell = new(4294964718u);

    //
    // Summary:
    //     Gets the predefined color of sienna, or #FFA0522D.
    public static Color Sienna = new(4288696877u);

    //
    // Summary:
    //     Gets the predefined color of silver, or #FFC0C0C0.
    public static Color Silver = new(4290822336u);

    //
    // Summary:
    //     Gets the predefined color of sky blue, or #FF87CEEB.
    public static Color SkyBlue = new(4287090411u);

    //
    // Summary:
    //     Gets the predefined color of slate blue, or #FF6A5ACD.
    public static Color SlateBlue = new(4285160141u);

    //
    // Summary:
    //     Gets the predefined color of slate gray, or #FF708090.
    public static Color SlateGray = new(4285563024u);

    //
    // Summary:
    //     Gets the predefined color of snow, or #FFFFFAFA.
    public static Color Snow = new(4294966010u);

    //
    // Summary:
    //     Gets the predefined color of spring green, or #FF00FF7F.
    public static Color SpringGreen = new(4278255487u);

    //
    // Summary:
    //     Gets the predefined color of steel blue, or #FF4682B4.
    public static Color SteelBlue = new(4282811060u);

    //
    // Summary:
    //     Gets the predefined color of tan, or #FFD2B48C.
    public static Color Tan = new(4291998860u);

    //
    // Summary:
    //     Gets the predefined color of teal, or #FF008080.
    public static Color Teal = new(4278222976u);

    //
    // Summary:
    //     Gets the predefined color of thistle, or #FFD8BFD8.
    public static Color Thistle = new(4292394968u);

    //
    // Summary:
    //     Gets the predefined color of tomato, or #FFFF6347.
    public static Color Tomato = new(4294927175u);

    //
    // Summary:
    //     Gets the predefined color of turquoise, or #FF40E0D0.
    public static Color Turquoise = new(4282441936u);

    //
    // Summary:
    //     Gets the predefined color of violet, or #FFEE82EE.
    public static Color Violet = new(4293821166u);

    //
    // Summary:
    //     Gets the predefined color of wheat, or #FFF5DEB3.
    public static Color Wheat = new(4294303411u);

    //
    // Summary:
    //     Gets the predefined color of white, or #FFFFFFFF.
    public static Color White = new(uint.MaxValue);

    //
    // Summary:
    //     Gets the predefined color of white smoke, or #FFF5F5F5.
    public static Color WhiteSmoke = new(4294309365u);

    //
    // Summary:
    //     Gets the predefined color of yellow, or #FFFFFF00.
    public static Color Yellow = new(4294967040u);

    //
    // Summary:
    //     Gets the predefined color of yellow green, or #FF9ACD32.
    public static Color YellowGreen = new(4288335154u);

    //
    // Summary:
    //     Gets the predefined color of white transparent, or #00FFFFFF.
    public static Color Transparent = new(16777215u);

    //
    // Summary:
    //     Gets the predefined empty color (black transparent), or #00000000.
    public static Color Empty => new(0u);
}