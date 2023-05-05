using System;
using System.Collections.Generic;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace WhatsUpToday.Core.Types;

/// <summary>
/// Generator for sequential GUIDs.
/// Based on https://github.com/SanderSade/SequentialGuid
/// </summary>
public class SequentialGuidGenerator
{
    private readonly byte _step;
    private volatile int _lockFlag;
    private GuidBytes _guidBytes;

    /// <summary>
    /// Creates a sequential GUID based on random GUID, optionally defining step
    /// <para>Step defaults to 1</para>
    /// </summary>
    public SequentialGuidGenerator(byte step = 1)
        : this(Guid.NewGuid(), step)
    {
    }

    /// <summary>
    /// Create sequential GUID from existing GUID, optionally defining step
    /// <para>Step defaults to 1</para>
    /// </summary>
    public SequentialGuidGenerator(Guid originalGuid, byte step = 1)
    {
        if (step == 0x00)
            throw new ArgumentOutOfRangeException(nameof(step), "Step cannot be 0!");

        Original = originalGuid;
        _step = step;
        _guidBytes.Guid = originalGuid;
    }

    public Guid GenerateGuid()
    {
        return Next();
    }

    /// <summary>
    /// Returns the original, initial GUID (first in sequence)
    /// </summary>
    public Guid Original { get; }

    /// <summary>
    /// Get the current value of the sequential GUID
    /// </summary>
    public Guid Current
    {
        get
        {
            SpinWait.SpinUntil(() => Interlocked.CompareExchange(ref _lockFlag, 1, 0) == 0);

            // we need to do this to avoid race issues
            var guid = _guidBytes.Guid;
            _lockFlag = 0;
            return guid;
        }
    }

    /// <summary>
    /// Return next sequential value of GUID
    /// </summary>
    public Guid Next()
    {
        // SpinWait is about 20% faster here than lock {}, but we need to
        // be aware of avoiding a race condition below
        SpinWait.SpinUntil(() => Interlocked.CompareExchange(ref _lockFlag, 1, 0) == 0);

        // this is really non-elegant, rethink this!
        if (!StepByte(ref _guidBytes.B15, _step) &&
            !StepByte(ref _guidBytes.B14) &&
            !StepByte(ref _guidBytes.B13) &&
            !StepByte(ref _guidBytes.B12) &&
            !StepByte(ref _guidBytes.B11) &&
            !StepByte(ref _guidBytes.B10) &&
            !StepByte(ref _guidBytes.B9) &&
            !StepByte(ref _guidBytes.B8) &&
            !StepByte(ref _guidBytes.B7) &&
            !StepByte(ref _guidBytes.B6) &&
            !StepByte(ref _guidBytes.B5) &&
            !StepByte(ref _guidBytes.B4) &&
            !StepByte(ref _guidBytes.B3) &&
            !StepByte(ref _guidBytes.B2) &&
            !StepByte(ref _guidBytes.B1) &&
            !StepByte(ref _guidBytes.B0))
            throw new OverflowException("Cannot increase the GUID anymore. Maximum value reached");

        // we need to do this to avoid race issues
        var guid = _guidBytes.Guid;
        _lockFlag = 0;
        return guid;
    }

    /// <summary>
    /// Try to add step to rightmost byte, and step up others in case of 0xFF
    /// Return false in case of overflow (next byte from right needs to be incremented by 1)
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private /*static*/ bool StepByte(ref byte currentByte, byte step = 1)
    {
        // result(1) means rollover occurred
        var result = (currentByte + step) > 0xff;
        currentByte = (byte)(currentByte + step);
        return !result;
    }
}

/// <summary>
/// Map GUID to individual bytes. May not be the most elegant approach, but is fast...
/// </summary>
[StructLayout(LayoutKind.Explicit)]
internal struct GuidBytes
{
    [FieldOffset(0)]
    internal Guid Guid;

    [FieldOffset(0)]
    internal byte B0;

    [FieldOffset(1)]
    internal byte B1;

    [FieldOffset(2)]
    internal byte B2;

    [FieldOffset(3)]
    internal byte B3;

    [FieldOffset(4)]
    internal byte B4;

    [FieldOffset(5)]
    internal byte B5;

    [FieldOffset(6)]
    internal byte B6;

    [FieldOffset(7)]
    internal byte B7;

    [FieldOffset(8)]
    internal byte B8;

    [FieldOffset(9)]
    internal byte B9;

    [FieldOffset(10)]
    internal byte B10;

    [FieldOffset(11)]
    internal byte B11;

    [FieldOffset(12)]
    internal byte B12;

    [FieldOffset(13)]
    internal byte B13;

    [FieldOffset(14)]
    internal byte B14;

    [FieldOffset(15)]
    internal byte B15;

    /// <summary>
    /// Get "Microsoft" byte position
    /// </summary>
    internal byte GetByteAt(int position)
    {
        switch (position)
        {
            case 0: return B0;
            case 1: return B1;
            case 2: return B2;
            case 3: return B3;
            case 4: return B4;
            case 5: return B5;
            case 6: return B6;
            case 7: return B7;
            case 8: return B8;
            case 9: return B9;
            case 10: return B10;
            case 11: return B11;
            case 12: return B12;
            case 13: return B13;
            case 14: return B14;
            case 15: return B15;
            default:
                throw new IndexOutOfRangeException($"Position must be between 0 and 15, received {position}");
        }
    }
}

/// <summary>
/// GUID conversion class
/// </summary>
internal static class GuidConverter
{
    internal static unsafe Guid DecimalToGuid(decimal dec) =>
        *(Guid*)(void*)&dec;

    /// <summary>
    /// From https://stackoverflow.com/a/49380620/3248515
    /// </summary>
    internal static unsafe void GuidToInt64(Guid value, out long x, out long y)
    {
        var ptr = (long*)&value;
        x = *ptr++;
        y = *ptr;
    }

    /// <summary>
    /// From https://stackoverflow.com/a/49380620/3248515
    /// </summary>
    internal static unsafe Guid GuidFromInt64(long x, long y)
    {
        var ptr = stackalloc long[2];
        ptr[0] = x;
        ptr[1] = y;
        return *(Guid*)ptr;
    }
}

/// <summary>
/// Useful helper methods for GUID
/// </summary>
public static class GuidHelper
{
    /// <summary>
    ///     Opposite of Guid.Empty - returns ffffffff-ffff-ffff-ffff-ffffffffffff
    /// </summary>
    public static Guid MaxValue => new Guid("ffffffffffffffffffffffffffffffff");

    /// <summary>
    ///     Get GUID from BigInteger.
    ///     <para>
    ///         Note that it is possible for BigInteger not to fit to GUID, ffffffff-ffff-ffff-ffff-ffffffffffff + 1 will
    ///         overflow GUID and return 00000000-00..., ffffffff-ffff-ffff-ffff-ffffffffffff + 2 returns 00000001-00...
    ///         and so forth
    ///     </para>
    ///     <para>
    ///         Defaults to isCompliant = true, as this is the more common use outside Microsoft/.NET.
    ///         E.g. compatible with Python or Java UUID, and http://guid-convert.appspot.com.
    ///         See also https://stackoverflow.com/questions/9195551/why-does-guid-tobytearray-order-the-bytes-the-way-it-does
    ///     </para>
    /// </summary>
    public static Guid FromBigInteger(BigInteger integer, bool isCompliant = true)
    {
        var bytes = integer.ToByteArray();

        //GUID can only be initialized from 16-byte array,
        //but BigInteger can be a single byte
        //or more than 16 bytes - see also comment on ToBigInteger()
        if (bytes.Length != 16)
            Array.Resize(ref bytes, 16);

        return isCompliant ? FromCompliantByteArray(bytes) : new Guid(bytes);
    }

    /// <summary>
    /// Convert decimal to Guid
    /// </summary>
    public static Guid FromDecimal(decimal dec) =>
        GuidConverter.DecimalToGuid(dec);

    /// <summary>
    /// Convert two longs to Guid
    /// <para>
    /// Note that two longs cannot hold GUID larger than
    /// ffffffff-ffff-7fff-ffff-ffffffffff7f, but that shouldn't be
    /// issue in most realistic use cases
    /// </para>
    /// </summary>
    public static Guid FromLongs(long first, long second) =>
        GuidConverter.GuidFromInt64(first, second);

    /// <summary>
    /// Create GUID from byte array compatible with Python and Java.
    /// <para>
    /// See http://guid-convert.appspot.com and
    /// https://stackoverflow.com/questions/9195551/why-does-guid-tobytearray-order-the-bytes-the-way-it-does
    /// </para>
    /// </summary>
    public static Guid FromCompliantByteArray(byte[] bytes)
    {
        if (bytes?.Length != 16)
            throw new ArgumentOutOfRangeException(nameof(bytes), "Byte array must contain 16 bytes!");

        var byteArray = new byte[16];

        byteArray[0] = bytes[12];
        byteArray[1] = bytes[13];
        byteArray[2] = bytes[14];
        byteArray[3] = bytes[15];
        byteArray[4] = bytes[10];
        byteArray[5] = bytes[11];
        byteArray[6] = bytes[8];
        byteArray[7] = bytes[9];
        byteArray[8] = bytes[7];
        byteArray[9] = bytes[6];
        byteArray[10] = bytes[5];
        byteArray[11] = bytes[4];
        byteArray[12] = bytes[3];
        byteArray[13] = bytes[2];
        byteArray[14] = bytes[1];
        byteArray[15] = bytes[0];

        return new Guid(byteArray);
    }
}

/// <summary>
///     Extension methods for GUID
/// </summary>
public static class GuidExtensions
{
    /// <summary>
    ///     Convert GUID to BigInteger
    ///     <para>
    ///         Defaults to isCompliant = true, as this is the more common use outside Microsoft/.NET.
    ///         E.g. compatible with Python or Java UUID, and http://guid-convert.appspot.com.
    ///         See also https://stackoverflow.com/questions/9195551/why-does-guid-tobytearray-order-the-bytes-the-way-it-does
    ///     </para>
    /// </summary>
    public static BigInteger ToBigInteger(this Guid guid, bool isCompliant = true)
    {
        var bytes = isCompliant ? ToCompliantByteArray(guid) : guid.ToByteArray();

        //[...] the most significant bit of the last element in the byte array.
        //This bit is set (the value of the byte is 0xFF) if the array is created from a negative BigInteger value. The bit is not set (the value of the byte is zero)
        // To prevent positive values from being misinterpreted as negative values, you can add a zero-byte value to the end of the array.
        //from https://docs.microsoft.com/en-us/dotnet/api/system.numerics.biginteger.-ctor?view=netframework-4.7.2#System_Numerics_BigInteger__ctor_System_Byte___
        if (bytes[15] == 0xFF)
        {
            Array.Resize(ref bytes, 17);
            bytes[16] = 0x00;
        }

        return new BigInteger(bytes);
    }

    /// <summary>
    ///     Convert GUID to pair of Int64s, sometimes used in languages without native GUID/UUID implementation (Javascript)
    ///     <para>
    ///         Note that two longs cannot hold GUID larger than ffffffff-ffff-7fff-ffff-ffffffffff7f,
    ///         but that shouldn't be an issue in most realistic use cases
    ///     </para>
    /// </summary>
    public static (long, long) ToLongs(this Guid guid)
    {
        GuidConverter.GuidToInt64(guid, out var first, out var second);
        return (first, second);
    }

    /// <summary>
    ///     Get the hex character at the specified position (0..31).
    ///     Character is returned in lowercase
    ///     <para>Far faster and uses less memory than using guid-to-string</para>
    /// </summary>
    public static char GetCharacterAt(this Guid guid, int position)
    {
        if (position < 0 || position > 31)
            throw new ArgumentOutOfRangeException(nameof(position), position,
                $"Position must be between 0 and 31, but received {position}");

        //remap position to internal byte order, as characters 0..13 do not match the byte order
        var remap = position >> 1;
        switch (remap)
        {
            case 0:
                remap = 3;
                break;
            case 1:
                remap = 2;
                break;
            case 2:
                remap = 1;
                break;
            case 3:
                remap = 0;
                break;
            case 4:
                remap = 5;
                break;
            case 5:
                remap = 4;
                break;
            case 6:
                remap = 7;
                break;
            case 7:
                remap = 6;
                break;
        }

        var guidByte = new GuidBytes { Guid = guid }.GetByteAt(remap);

        //logic similar to https://github.com/dotnet/corefx/blob/7622efd2dbd363a632e00b6b95be4d990ea125de/src/Common/src/CoreLib/System/Guid.cs#L989,
        //but we're using nibble and not full byte
        var nibbleByte = (position % 2 == 0 ? (guidByte & 0xF0) >> 4 : guidByte & 0x0F) & 0xf;
        return (char)(nibbleByte > 9 ? nibbleByte + 87 : nibbleByte + 48);
    }

    /// <summary>
    ///     Get byte from GUID without converting GUID to byte array. Position is the native position of the byte in GUID
    ///     structure on Windows/.NET (0..15)
    ///     <para>This is very slightly faster than using Guid.ToByteArray()[position], but uses far less memory</para>
    /// </summary>
    public static byte GetByteAt(this Guid guid, int position) =>
        new GuidBytes { Guid = guid }.GetByteAt(position);

    /// <summary>
    ///     Get GUID as byte array compatible with common use outside Microsoft/.NET.
    ///     <para>
    ///         E.g. compatible with Python or Java UUID, and http://guid-convert.appspot.com.
    ///         See also https://stackoverflow.com/questions/9195551/why-does-guid-tobytearray-order-the-bytes-the-way-it-does
    ///     </para>
    /// </summary>
    public static byte[] ToCompliantByteArray(this Guid guid)
    {
        var converter = new GuidBytes { Guid = guid };
        return new[]
        {
            converter.B15,
            converter.B14,
            converter.B13,
            converter.B12,
            converter.B11,
            converter.B10,
            converter.B9,
            converter.B8,
            converter.B6,
            converter.B7,
            converter.B4,
            converter.B5,
            converter.B0,
            converter.B1,
            converter.B2,
            converter.B3
        };
    }
}
