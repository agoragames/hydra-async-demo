#region License
/*
 * WsFrame.cs
 *
 * The MIT License
 *
 * Copyright (c) 2012-2013 sta.blockhead
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
 */
#endregion

using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace WebSocketSharp {

  internal class WsFrame : IEnumerable<byte>
  {
    #region Private Constructors

    private WsFrame()
    {
    }

    #endregion

    #region Public Constructors

    public WsFrame(Opcode opcode, PayloadData payloadData)
      : this(opcode, Mask.MASK, payloadData)
    {
    }

    public WsFrame(Opcode opcode, Mask mask, PayloadData payloadData)
      : this(Fin.FINAL, opcode, mask, payloadData)
    {
    }

    public WsFrame(Fin fin, Opcode opcode, Mask mask, PayloadData payloadData)
      : this(fin, opcode, mask, payloadData, false)
    {
    }

    public WsFrame(
      Fin fin, Opcode opcode, Mask mask, PayloadData payloadData, bool compressed)
    {
      Fin = fin;
      Rsv1 = isData(opcode) && compressed ? Rsv.ON : Rsv.OFF;
      Rsv2 = Rsv.OFF;
      Rsv3 = Rsv.OFF;
      Opcode = opcode;
      Mask = mask;

      /* PayloadLen */

      ulong dataLen = payloadData.Length;
      var payloadLen = dataLen < 126
                     ? (byte)dataLen
                     : dataLen < 0x010000
                       ? (byte)126
                       : (byte)127;

      PayloadLen = payloadLen;

      /* ExtPayloadLen */

      ExtPayloadLen = payloadLen < 126
                    ? new byte[]{}
                    : payloadLen == 126
                      ? ((ushort)dataLen).ToByteArray(ByteOrder.BIG)
                      : dataLen.ToByteArray(ByteOrder.BIG);

      /* MaskingKey */

      var masking = mask == Mask.MASK;
      var maskingKey = masking
                     ? createMaskingKey()
                     : new byte[]{};

      MaskingKey = maskingKey;

      /* PayloadData */

      if (masking)
        payloadData.Mask(maskingKey);

      PayloadData = payloadData;
    }

    #endregion

    #region Internal Properties

    internal bool IsBinary {
      get {
        return Opcode == Opcode.BINARY;
      }
    }

    internal bool IsClose {
      get {
        return Opcode == Opcode.CLOSE;
      }
    }

    internal bool IsCompressed {
      get {
        return Rsv1 == Rsv.ON;
      }
    }

    internal bool IsContinuation {
      get {
        return Opcode == Opcode.CONT;
      }
    }

    internal bool IsControl {
      get {
        var opcode = Opcode;
        return opcode == Opcode.CLOSE || opcode == Opcode.PING || opcode == Opcode.PONG;
      }
    }

    internal bool IsData {
      get {
        var opcode = Opcode;
        return opcode == Opcode.BINARY || opcode == Opcode.TEXT;
      }
    }

    internal bool IsFinal {
      get {
        return Fin == Fin.FINAL;
      }
    }

    internal bool IsFragmented {
      get {
        return Fin == Fin.MORE || Opcode == Opcode.CONT;
      }
    }

    internal bool IsMasked {
      get {
        return Mask == Mask.MASK;
      }
    }

    internal bool IsPerMessageCompressed {
      get {
        var opcode = Opcode;
        return (opcode == Opcode.BINARY || opcode == Opcode.TEXT) && Rsv1 == Rsv.ON;
      }
    }

    internal bool IsPing {
      get {
        return Opcode == Opcode.PING;
      }
    }

    internal bool IsPong {
      get {
        return Opcode == Opcode.PONG;
      }
    }

    internal bool IsText {
      get {
        return Opcode == Opcode.TEXT;
      }
    }

    internal ulong Length {
      get {
        return 2 + (ulong)(ExtPayloadLen.Length + MaskingKey.Length) + PayloadData.Length;
      }
    }

    #endregion

    #region Public Properties

    public Fin Fin { get; private set; }

    public Rsv Rsv1 { get; private set; }

    public Rsv Rsv2 { get; private set; }

    public Rsv Rsv3 { get; private set; }

    public Opcode Opcode { get; private set; }

    public Mask Mask { get; private set; }

    public byte PayloadLen { get; private set; }

    public byte[] ExtPayloadLen { get; private set; }

    public byte[] MaskingKey { get; private set; }

    public PayloadData PayloadData { get; private set; }

    #endregion

    #region Private Methods

    private static WsFrame createCloseFrame(CloseStatusCode code, string reason, Mask mask)
    {
      var data = ((ushort)code).Append(reason);
      return new WsFrame(Fin.FINAL, Opcode.CLOSE, mask, new PayloadData(data));
    }

    private static byte[] createMaskingKey()
    {
      var key = new byte[4];
      var rand = new Random();
      rand.NextBytes(key);

      return key;
    }

    private static void dump(WsFrame frame)
    {
      var len = frame.Length;
      var count = (long)(len / 4);
      var remainder = (int)(len % 4);

      int countDigit;
      string countFmt;
      if (count < 10000)
      {
        countDigit = 4;
        countFmt = "{0,4}";
      }
      else if (count < 0x010000)
      {
        countDigit = 4;
        countFmt = "{0,4:X}";
      }
      else if (count < 0x0100000000)
      {
        countDigit = 8;
        countFmt = "{0,8:X}";
      }
      else
      {
        countDigit = 16;
        countFmt = "{0,16:X}";
      }

      var spFmt = String.Format("{{0,{0}}}", countDigit);
      var headerFmt = String.Format(@"
 {0} 01234567 89ABCDEF 01234567 89ABCDEF
 {0}+--------+--------+--------+--------+", spFmt);
      var footerFmt = String.Format(" {0}+--------+--------+--------+--------+", spFmt);

      Func<Action<string, string, string, string>> linePrinter = () =>
      {
        long lineCount = 0;
        var lineFmt = String.Format(" {0}|{{1,8}} {{2,8}} {{3,8}} {{4,8}}|", countFmt);
        return (arg1, arg2, arg3, arg4) =>
        {
          Console.WriteLine(lineFmt, ++lineCount, arg1, arg2, arg3, arg4);
        };
      };
      var printLine = linePrinter();

      Console.WriteLine(headerFmt, String.Empty);

      var buffer = frame.ToByteArray();
      int i, j;
      for (i = 0; i <= count; i++)
      {
        j = i * 4;
        if (i < count)
          printLine(
            Convert.ToString(buffer[j],     2).PadLeft(8, '0'),
            Convert.ToString(buffer[j + 1], 2).PadLeft(8, '0'),
            Convert.ToString(buffer[j + 2], 2).PadLeft(8, '0'),
            Convert.ToString(buffer[j + 3], 2).PadLeft(8, '0'));
        else if (remainder > 0)
          printLine(
            Convert.ToString(buffer[j], 2).PadLeft(8, '0'),
            remainder >= 2 ? Convert.ToString(buffer[j + 1], 2).PadLeft(8, '0') : String.Empty,
            remainder == 3 ? Convert.ToString(buffer[j + 2], 2).PadLeft(8, '0') : String.Empty,
            String.Empty);
      }

      Console.WriteLine(footerFmt, String.Empty);
    }

    private static bool isBinary(Opcode opcode)
    {
      return opcode == Opcode.BINARY;
    }

    private static bool isClose(Opcode opcode)
    {
      return opcode == Opcode.CLOSE;
    }

    private static bool isContinuation(Opcode opcode)
    {
      return opcode == Opcode.CONT;
    }

    private static bool isControl(Opcode opcode)
    {
      return opcode == Opcode.CLOSE || opcode == Opcode.PING || opcode == Opcode.PONG;
    }

    private static bool isData(Opcode opcode)
    {
      return opcode == Opcode.TEXT || opcode == Opcode.BINARY;
    }

    private static bool isFinal(Fin fin)
    {
      return fin == Fin.FINAL;
    }

    private static bool isMasked(Mask mask)
    {
      return mask == Mask.MASK;
    }

    private static bool isPing(Opcode opcode)
    {
      return opcode == Opcode.PING;
    }

    private static bool isPong(Opcode opcode)
    {
      return opcode == Opcode.PONG;
    }

    private static bool isText(Opcode opcode)
    {
      return opcode == Opcode.TEXT;
    }

    private static WsFrame parse(byte[] header, Stream stream, bool unmask)
    {
      /* Header */

      // FIN
      var fin = (header[0] & 0x80) == 0x80 ? Fin.FINAL : Fin.MORE;
      // RSV1
      var rsv1 = (header[0] & 0x40) == 0x40 ? Rsv.ON : Rsv.OFF;
      // RSV2
      var rsv2 = (header[0] & 0x20) == 0x20 ? Rsv.ON : Rsv.OFF;
      // RSV3
      var rsv3 = (header[0] & 0x10) == 0x10 ? Rsv.ON : Rsv.OFF;
      // Opcode
      var opcode = (Opcode)(header[0] & 0x0f);
      // MASK
      var mask = (header[1] & 0x80) == 0x80 ? Mask.MASK : Mask.UNMASK;
      // Payload len
      var payloadLen = (byte)(header[1] & 0x7f);

      if (isControl(opcode) && payloadLen > 125)
        return createCloseFrame(CloseStatusCode.INCONSISTENT_DATA,
          "The payload length of a control frame must be 125 bytes or less.",
          Mask.UNMASK);

      var frame = new WsFrame {
        Fin = fin,
        Rsv1 = rsv1,
        Rsv2 = rsv2,
        Rsv3 = rsv3,
        Opcode = opcode,
        Mask = mask,
        PayloadLen = payloadLen
      };

      /* Extended Payload Length */

      var extLen = payloadLen < 126
                 ? 0
                 : payloadLen == 126
                   ? 2
                   : 8;

      var extPayloadLen = extLen > 0
                        ? stream.ReadBytesInternal(extLen)
                        : new byte[]{};

      if (extLen > 0 && extPayloadLen.Length != extLen)
        return createCloseFrame(CloseStatusCode.ABNORMAL,
          "'Extended Payload Length' of a frame cannot be read from the data stream.",
          Mask.UNMASK);

      frame.ExtPayloadLen = extPayloadLen;

      /* Masking Key */

      var masked = mask == Mask.MASK ? true : false;
      var maskingKey = masked
                     ? stream.ReadBytesInternal(4)
                     : new byte[]{};

      if (masked && maskingKey.Length != 4)
        return createCloseFrame(CloseStatusCode.ABNORMAL,
          "'Masking Key' of a frame cannot be read from the data stream.",
          Mask.UNMASK);

      frame.MaskingKey = maskingKey;

      /* Payload Data */

      ulong dataLen = payloadLen < 126
                    ? payloadLen
                    : payloadLen == 126
                      ? extPayloadLen.To<ushort>(ByteOrder.BIG)
                      : extPayloadLen.To<ulong>(ByteOrder.BIG);

      byte[] data = null;
      if (dataLen > 0)
      {
        if (payloadLen > 126 && dataLen > PayloadData.MaxLength)
        {
          var code = CloseStatusCode.TOO_BIG;
          return createCloseFrame(code, code.GetMessage(), Mask.UNMASK);
        }

        data = dataLen > 1024
             ? stream.ReadBytesInternal((long)dataLen, 1024)
             : stream.ReadBytesInternal((int)dataLen);

        if (data.LongLength != (long)dataLen)
          return createCloseFrame(CloseStatusCode.ABNORMAL,
            "'Payload Data' of a frame cannot be read from the data stream.",
            Mask.UNMASK);
      }
      else
      {
        data = new byte[]{};
      }

      var payloadData = new PayloadData(data, masked);
      if (masked && unmask)
      {
        payloadData.Mask(maskingKey);
        frame.Mask = Mask.UNMASK;
        frame.MaskingKey = new byte[]{};
      }

      frame.PayloadData = payloadData;
      return frame;
    }

    private static void print(WsFrame frame)
    {
      var len = frame.ExtPayloadLen.Length;
      var extPayloadLen = len == 2
                        ? frame.ExtPayloadLen.To<ushort>(ByteOrder.BIG).ToString()
                        : len == 8
                          ? frame.ExtPayloadLen.To<ulong>(ByteOrder.BIG).ToString()
                          : String.Empty;

      var masked = frame.IsMasked;
      var maskingKey = masked
                     ? BitConverter.ToString(frame.MaskingKey)
                     : String.Empty;

      var opcode = frame.Opcode;
      var payloadData = frame.PayloadData.Length == 0
                      ? String.Empty
                      : masked || frame.IsFragmented || frame.IsBinary || frame.IsClose
                        ? BitConverter.ToString(frame.PayloadData.ToByteArray())
                        : Encoding.UTF8.GetString(frame.PayloadData.ToByteArray());

      var format = @"
 FIN: {0}
 RSV1: {1}
 RSV2: {2}
 RSV3: {3}
 Opcode: {4}
 MASK: {5}
 Payload Len: {6}
 Extended Payload Len: {7}
 Masking Key: {8}
 Payload Data: {9}";

      Console.WriteLine(
        format, frame.Fin, frame.Rsv1, frame.Rsv2, frame.Rsv3, opcode, frame.Mask, frame.PayloadLen, extPayloadLen, maskingKey, payloadData);
    }

    #endregion

    #region Public Methods

    public IEnumerator<byte> GetEnumerator()
    {
      foreach (byte b in ToByteArray())
        yield return b;
    }

    public static WsFrame Parse(byte[] src)
    {
      return Parse(src, true);
    }

    public static WsFrame Parse(Stream stream)
    {
      return Parse(stream, true);
    }

    public static WsFrame Parse(byte[] src, bool unmask)
    {
      using (MemoryStream ms = new MemoryStream(src))
      {
        return Parse(ms, unmask);
      }
    }

    public static WsFrame Parse(Stream stream, bool unmask)
    {
      return Parse(stream, unmask, null);
    }

    public static WsFrame Parse(Stream stream, bool unmask, Action<Exception> error)
    {
      WsFrame frame = null;
      try
      {
        var header = stream.ReadBytesInternal(2);
        frame = header.Length == 2
              ? parse(header, stream, unmask)
              : createCloseFrame(CloseStatusCode.ABNORMAL,
                  "'Header' of a frame cannot be read from the data stream.",
                  Mask.UNMASK);
      }
      catch (Exception ex)
      {
        if (error != null)
          error(ex);
      }

      return frame;
    }

    public static void ParseAsync(Stream stream, Action<WsFrame> completed)
    {
      ParseAsync(stream, true, completed, null);
    }

    public static void ParseAsync(Stream stream, Action<WsFrame> completed, Action<Exception> error)
    {
      ParseAsync(stream, true, completed, error);
    }

    public static void ParseAsync(
      Stream stream, bool unmask, Action<WsFrame> completed, Action<Exception> error)
    {
      var header = new byte[2];
      AsyncCallback callback = ar =>
      {
        WsFrame frame = null;
        try
        {
          var readLen = stream.EndRead(ar);
          if (readLen == 1)
          {
            var tmp = stream.ReadByte();
            if (tmp > -1)
            {
              header[1] = (byte)tmp;
              readLen++;
            }
          }

          frame = readLen == 2
                ? parse(header, stream, unmask)
                : createCloseFrame(CloseStatusCode.ABNORMAL,
                    "'Header' of a frame cannot be read from the data stream.",
                    Mask.UNMASK);
        }
        catch (Exception ex)
        {
          if (error != null)
            error(ex);
        }
        finally
        {
          if (completed != null)
            completed(frame);
        }
      };

      stream.BeginRead(header, 0, 2, callback, null);
    }

    public void Print(bool dumped)
    {
      if (dumped)
        dump(this);
      else
        print(this);
    }

    public byte[] ToByteArray()
    {
      using (var buffer = new MemoryStream())
      {
        int header = (int)Fin;
        header = (header << 1) + (int)Rsv1;
        header = (header << 1) + (int)Rsv2;
        header = (header << 1) + (int)Rsv3;
        header = (header << 4) + (int)Opcode;
        header = (header << 1) + (int)Mask;
        header = (header << 7) + (int)PayloadLen;
        buffer.Write(((ushort)header).ToByteArray(ByteOrder.BIG), 0, 2);

        if (PayloadLen > 125)
          buffer.Write(ExtPayloadLen, 0, ExtPayloadLen.Length);

        if (Mask == Mask.MASK)
          buffer.Write(MaskingKey, 0, MaskingKey.Length);

        if (PayloadLen > 0)
        {
          var payload = PayloadData.ToByteArray();
          if (PayloadLen < 127)
            buffer.Write(payload, 0, payload.Length);
          else
            buffer.WriteBytes(payload);
        }

        buffer.Close();
        return buffer.ToArray();
      }
    }

    public override string ToString()
    {
      return BitConverter.ToString(ToByteArray());
    }

    #endregion

    #region Explicitly Implemented Interface Members

    IEnumerator IEnumerable.GetEnumerator()
    {
      return GetEnumerator();
    }

    #endregion
  }
}
