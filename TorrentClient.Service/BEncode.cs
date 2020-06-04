using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace TorrentClient.Service
{
    public class BEncode
    {
        private static readonly byte DictionaryStart = Encoding.UTF8.GetBytes("d")[0];  // 100
        private static readonly byte DictionaryEnd = Encoding.UTF8.GetBytes("e")[0];    // 101
        private static readonly byte ListStart = Encoding.UTF8.GetBytes("l")[0];        // 108
        private static readonly byte ListEnd = Encoding.UTF8.GetBytes("e")[0];          // 101
        private static readonly byte NumberStart = Encoding.UTF8.GetBytes("i")[0];      // 105
        private static readonly byte NumberEnd = Encoding.UTF8.GetBytes("e")[0];        // 101
        private static readonly byte ByteArrayDivider = Encoding.UTF8.GetBytes(":")[0]; //  58

        public static object Decode(byte[] bytes)
        {
            IEnumerator<byte> enumerator = ((IEnumerable<byte>)bytes).GetEnumerator();
            enumerator.MoveNext();

            return DecodeNextObject(enumerator);
        }

        private static object DecodeNextObject(IEnumerator<byte> enumerator)
        {
            if (enumerator.Current == DictionaryStart)
            {
                return DecodeDictionary(enumerator);
            }

            else if (enumerator.Current == ListStart)
            {
                return DecodeList(enumerator);
            }

            else if (enumerator.Current == NumberStart)
            {
                return DecodeNumber(enumerator);
            }

            return DecodeByteArray(enumerator);
        }

        public static object DecodeFile(string path)
        {
            if (!File.Exists(path))
            {
                var ex = new FileNotFoundException("Unable to find file: " + path);
                Log.LogException(ex);
                throw ex;
            }

            byte[] bytes = File.ReadAllBytes(path);

            return Decode(bytes);
        }

        private static long DecodeNumber(IEnumerator<byte> enumerator)
        {
            List<byte> bytes = new List<byte>();

            while (enumerator.MoveNext())
            {
                if (enumerator.Current == NumberEnd)
                {
                    break;
                }

                bytes.Add(enumerator.Current);
            }

            string numAsString = Encoding.UTF8.GetString(bytes.ToArray());

            return long.Parse(numAsString);
        }

        private static byte[] DecodeByteArray(IEnumerator<byte> enumerator)
        {
            List<byte> lengthBytes = new List<byte>();

            bool separatorPassed = false;

            do
            {
                if (enumerator.Current == ByteArrayDivider)
                {
                    separatorPassed = true;
                    break;
                }

                lengthBytes.Add(enumerator.Current);
            }
            while (enumerator.MoveNext());

            if (!separatorPassed)
            {
                var ex = new FormatException("No separator found in string");
                Log.LogException(ex);
                throw ex;
            }

            string lengthString = Encoding.UTF8.GetString(lengthBytes.ToArray());

            if (!int.TryParse(lengthString, out int length))
            {
                var ex = new FormatException("Unable to parse length of byte array");
                Log.LogException(ex);
                throw ex;
            }

            List<byte> bytes = new List<byte>();

            for (int i = 0; i < length; i++)
            {
                if (enumerator.MoveNext())
                {
                    bytes.Add(enumerator.Current);
                }
            }

            return bytes.ToArray();
        }

        private static List<object> DecodeList(IEnumerator<byte> enumerator)
        {
            List<object> list = new List<object>();

            bool listEnded = false;
            while (enumerator.MoveNext())
            {
                if (enumerator.Current == ListEnd)
                {
                    listEnded = true;
                    break;
                }

                list.Add(DecodeNextObject(enumerator));
            }

            if (!listEnded)
            {
                var ex = new FormatException("The list has no specified end");
                Log.LogException(ex);
                throw ex;
            }

            return list;
        }

        private static Dictionary<string, object> DecodeDictionary(IEnumerator<byte> enumerator)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            List<string> keys = new List<string>();

            bool dictionaryEnded = false;
            while (enumerator.MoveNext())
            {
                if (enumerator.Current == DictionaryEnd)
                {
                    dictionaryEnded = true;
                    break;
                }

                string key = Encoding.UTF8.GetString(DecodeByteArray(enumerator));
                enumerator.MoveNext();
                object val = DecodeNextObject(enumerator);

                keys.Add(key);
                dict.Add(key, val);
            }

            if (!dictionaryEnded)
            {
                var ex = new FormatException("The dictionary has no specified end");
                Log.LogException(ex);
                throw ex;
            }

            var sortedKeys = keys.OrderBy(x => BitConverter.ToString(Encoding.UTF8.GetBytes(x)));
            if (!keys.SequenceEqual(sortedKeys))
            {
                var ex = new Exception("Error loading dictionary: keys not sorted");
                Log.LogException(ex);
                throw ex;
            }

            return dict;
        }

        public static byte[] Encode(object obj)
        {
            MemoryStream buffer = new MemoryStream();

            EncodeNextObject(buffer, obj);

            return buffer.ToArray();
        }

        public static void EncodeToFile(object obj, string path)
        {
            File.WriteAllBytes(path, Encode(obj));
        }

        private static void EncodeNextObject(MemoryStream buffer, object obj)
        {
            if (obj is byte[])
                EncodeByteArray(buffer, (byte[])obj);
            else if (obj is string)
                EncodeString(buffer, (string)obj);
            else if (obj is long)
                EncodeNumber(buffer, (long)obj);
            else if (obj is int)
                EncodeNumber(buffer, (int)obj);
            else if (obj.GetType() == typeof(List<object>))
                EncodeList(buffer, (List<object>)obj);
            else if (obj.GetType() == typeof(Dictionary<string, object>))
                EncodeDictionary(buffer, (Dictionary<string, object>)obj);
            else
            {
                var ex = new FormatException("Unable to encode type " + obj.GetType());
                Log.LogException(ex);
                throw ex;
            }
        }

        private static void EncodeNumber(MemoryStream buffer, long input)
        {
            buffer.Append(NumberStart);
            buffer.Append(Encoding.UTF8.GetBytes(Convert.ToString(input)));
            buffer.Append(NumberEnd);
        }

        private static void EncodeByteArray(MemoryStream buffer, byte[] body)
        {
            buffer.Append(Encoding.UTF8.GetBytes(Convert.ToString(body.Length)));
            buffer.Append(ByteArrayDivider);
            buffer.Append(body);
        }

        private static void EncodeString(MemoryStream buffer, string input)
        {
            EncodeByteArray(buffer, Encoding.UTF8.GetBytes(input));
        }

        private static void EncodeList(MemoryStream buffer, List<object> input)
        {
            buffer.Append(ListStart);
            foreach (object item in input)
            {
                EncodeNextObject(buffer, item);
            }

            buffer.Append(ListEnd);
        }

        private static void EncodeDictionary(MemoryStream buffer, Dictionary<string, object> input)
        {
            buffer.Append(DictionaryStart);

            // we need to sort the keys by their raw bytes, not the string
            var sortedKeys = input.Keys.ToList().OrderBy(x => BitConverter.ToString(Encoding.UTF8.GetBytes(x)));

            foreach (string key in sortedKeys)
            {
                EncodeString(buffer, key);
                EncodeNextObject(buffer, input[key]);
            }
            buffer.Append(DictionaryEnd);
        }
    }
}