﻿using System;

namespace WhatsUpToday.Core.Types;

/// <summary>
/// Unique Guid generator.
/// Taken from
/// https://github.com/nhibernate/nhibernate-core/blob/master/src/NHibernate/Id/GuidCombGenerator.cs
/// </summary>
public static class GuidCombGenerator
{
    private static readonly long BaseDateTicks = new DateTime(1900, 1, 1).Ticks;

    /// <summary>
    /// Generate a new <see cref="Guid"/> using the comb algorithm.
    /// </summary>
    public static Guid GenerateComb()
    {
        byte[] guidArray = Guid.NewGuid().ToByteArray();

        DateTime now = DateTime.UtcNow;

        // Get the days and milliseconds which will be used to build the byte string 
        TimeSpan days = new TimeSpan(now.Ticks - BaseDateTicks);
        TimeSpan msecs = now.TimeOfDay;

        // Convert to a byte array 
        // Note that SQL Server is accurate to 1/300th of a millisecond so we divide by 3.333333 
        byte[] daysArray = BitConverter.GetBytes(days.Days);
        byte[] msecsArray = BitConverter.GetBytes((long)(msecs.TotalMilliseconds / 3.333333));

        // Reverse the bytes to match SQL Servers ordering 
        Array.Reverse(daysArray);
        Array.Reverse(msecsArray);

        // Copy the bytes into the guid 
        Array.Copy(daysArray, daysArray.Length - 2, guidArray, guidArray.Length - 6, 2);
        Array.Copy(msecsArray, msecsArray.Length - 4, guidArray, guidArray.Length - 4, 4);

        return new Guid(guidArray);
    }
}
