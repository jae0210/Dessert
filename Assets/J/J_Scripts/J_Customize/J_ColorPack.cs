using UnityEngine;

public static class J_ColorPack
{
    public static int Pack(Color c)
    {
        Color32 cc = c;
        return (cc.r << 24) | (cc.g << 16) | (cc.b << 8) | cc.a;
    }

    public static Color Unpack(int packed)
    {
        byte r = (byte)((packed >> 24) & 0xFF);
        byte g = (byte)((packed >> 16) & 0xFF);
        byte b = (byte)((packed >> 8) & 0xFF);
        byte a = (byte)(packed & 0xFF);
        return new Color32(r, g, b, a);
    }
}
