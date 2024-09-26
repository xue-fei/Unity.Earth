using System;

public class GPSUtil
{
    /// <summary>
    /// Π
    /// 圆周率
    /// </summary>
    private const double PI = 3.14159265358979324;
    private const double X_PI = 3.14159265358979324 * 3000.0 / 180.0;
    private const double A = 6378245.0;
    private const double EE = 0.00669342162296594323;
    private const double LON_BOUNDARY_MIN = 72.004;
    private const double LAT_BOUNDARY_MIN = 0.8293;
    private const double LON_BOUNDARY_MAX = 137.8347;
    private const double LAT_BOUNDARY_MAX = 55.8271;

    /// <summary>
    /// 是否中国境内坐标
    /// </summary>
    /// <param name="gpsLat"></param>
    /// <param name="gpsLng"></param>
    /// <returns></returns>
    private static bool OutOfChina(double gpsLat, double gpsLng)
    {
        if (gpsLng < LON_BOUNDARY_MIN || gpsLng > LON_BOUNDARY_MAX)
        {
            return true;
        }

        if (gpsLat < LAT_BOUNDARY_MIN || gpsLat > LAT_BOUNDARY_MAX)
        {
            return true;
        }

        return false;
    }

    #region WGS坐标系与GCJ02坐标系互转

    /// <summary>
    /// WGS84坐标系转GCJ02坐标系
    /// </summary>
    /// <param name="wgsLat">WGS坐标，纬度</param>
    /// <param name="wgsLng">WGS坐标，经度</param>
    /// <param name="gcjLat">GCJ02坐标，纬度</param>
    /// <param name="gcjLng">GCJ02坐标，经度</param>
    public static void WGS84_to_GCJ02(double wgsLat, double wgsLng, out double gcjLat, out double gcjLng)
    {
        if (OutOfChina(wgsLat, wgsLng))
        {
            gcjLat = wgsLat;
            gcjLng = wgsLng;
        }
        else
        {
            double dLat = TransformLat(wgsLng - 105.0, wgsLat - 35.0);
            double dLon = TransformLon(wgsLng - 105.0, wgsLat - 35.0);
            double radLat = wgsLat / 180.0 * PI;
            double magic = Math.Sin(radLat);
            magic = 1 - EE * magic * magic;
            double sqrtMagic = Math.Sqrt(magic);
            dLat = (dLat * 180.0) / ((A * (1 - EE)) / (magic * sqrtMagic) * PI);
            dLon = (dLon * 180.0) / (A / sqrtMagic * Math.Cos(radLat) * PI);
            gcjLat = wgsLat + dLat;
            gcjLng = wgsLng + dLon;
        }
    }

    public static void GCJ02_to_WGS84(double gcjLat, double gcjLng, out double wgsLat, out double wgsLng)
    {
        WGS84_to_GCJ02(gcjLat, gcjLng, out wgsLat, out wgsLng);

        wgsLng = gcjLng * 2 - wgsLng;
        wgsLat = gcjLat * 2 - wgsLat;
    }

    private static double TransformLat(double x, double y)
    {
        double ret = -100.0 + 2.0 * x + 3.0 * y + 0.2 * y * y + 0.1 * x * y + 0.2 * Math.Sqrt(Math.Abs(x));
        ret += (20.0 * Math.Sin(6.0 * x * PI) + 20.0 * Math.Sin(2.0 * x * PI)) * 2.0 / 3.0;
        ret += (20.0 * Math.Sin(y * PI) + 40.0 * Math.Sin(y / 3.0 * PI)) * 2.0 / 3.0;
        ret += (160.0 * Math.Sin(y / 12.0 * PI) + 320 * Math.Sin(y * PI / 30.0)) * 2.0 / 3.0;
        return ret;
    }

    private static double TransformLon(double x, double y)
    {
        double ret = 300.0 + x + 2.0 * y + 0.1 * x * x + 0.1 * x * y + 0.1 * Math.Sqrt(Math.Abs(x));
        ret += (20.0 * Math.Sin(6.0 * x * PI) + 20.0 * Math.Sin(2.0 * x * PI)) * 2.0 / 3.0;
        ret += (20.0 * Math.Sin(x * PI) + 40.0 * Math.Sin(x / 3.0 * PI)) * 2.0 / 3.0;
        ret += (150.0 * Math.Sin(x / 12.0 * PI) + 300.0 * Math.Sin(x / 30.0 * PI)) * 2.0 / 3.0;
        return ret;
    }

    #endregion

    #region 火星坐标系 (GCJ-02) 与百度坐标系 (BD-09) 的互转

    public static void GCJ02_to_Bd09(double gcjLat, double gcjLng, out double bdLat, out double bdLng)
    {
        double z = Math.Sqrt(gcjLng * gcjLng + gcjLat * gcjLat) + 0.00002 * Math.Sin(gcjLat * PI);
        double theta = Math.Atan2(gcjLat, gcjLng) + 0.000003 * Math.Cos(gcjLng * PI);
        bdLng = z * Math.Cos(theta) + 0.0065;
        bdLat = z * Math.Sin(theta) + 0.006;
    }


    public static void BD09_to_GCJ02(double bdLat, double bdLng, out double gcjLat, out double gcjLng)
    {
        double x = bdLng - 0.0065, y = bdLat - 0.006;
        double z = Math.Sqrt(x * x + y * y) - 0.00002 * Math.Sin(y * PI);
        double theta = Math.Atan2(y, x) - 0.000003 * Math.Cos(x * PI);
        gcjLng = z * Math.Cos(theta);
        gcjLat = z * Math.Sin(theta);
    }

    #endregion
}