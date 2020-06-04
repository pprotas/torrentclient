using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TorrentClient.Service;

namespace TorrentClient.Tests
{
    public class BEncodeTests
    {
        [Test]
        public void Decode_InputIsValidBEncodedNumber_ReturnsCorrectNumber()
        {
            Assert.That(BEncode.Decode(Encoding.UTF8.GetBytes("i5e")), Is.EqualTo(5));
        }

        [Test]
        public void Decode_InputIsValidBEncodedString_ReturnsCorrectString()
        {
            Assert.That(BEncode.Decode(Encoding.UTF8.GetBytes("5:hello")), Is.EqualTo("hello"));
        }

        [Test]
        public void Decode_InputIsValidBEncodedDictionary_ReturnsCorrectDictionary()
        {
            Dictionary<string, object> dictionary = new Dictionary<string, object>
            {
                { "key", "value" }
            };

            Assert.That(BEncode.Decode(Encoding.UTF8.GetBytes("d3:key5:valuee")), Is.EqualTo(dictionary));
        }

        [Test]
        public void Decode_InputIsValidBEncodedList_ReturnsCorrectList()
        {
            List<string> list = new List<string>
            {
                "ItemA",
                "ItemB"
            };

            Assert.That(BEncode.Decode(Encoding.UTF8.GetBytes("l5:ItemA5:ItemBe")), Is.EqualTo(list));
        }

        [Test]
        public void Decode_InputIsInvalidBEncodedNumber_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => BEncode.Decode(Encoding.UTF8.GetBytes("ire")));
        }

        [Test]
        public void Decode_InputIsOnlyStringLength_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => BEncode.Decode(Encoding.UTF8.GetBytes("5")));
        }

        [Test]
        public void Decode_InputIsStringLengthWithSeparator_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => BEncode.Decode(Encoding.UTF8.GetBytes("5:")));
        }

        [Test]
        public void Decode_InputIsStringWithInvalidLength_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => BEncode.Decode(Encoding.UTF8.GetBytes("5:four")));
        }

        [Test]
        public void Decode_InputIsDictionaryWithNoEnd_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => BEncode.Decode(Encoding.UTF8.GetBytes("d3:key5:value")));
        }
        [Test]
        public void Decode_InputIsListWithNoEnd_ThrowsFormatException()
        {
            Assert.Throws<FormatException>(() => BEncode.Decode(Encoding.UTF8.GetBytes("l5:ItemA5:ItemB")));
        }

        [Test]
        public void Encode_InputIsValidByteArray_ReturnsCorrectBEncodedString()
        {
            string input = "input";
            byte[] bytes = Encoding.UTF8.GetBytes(input);

            Assert.That(Encoding.UTF8.GetString(BEncode.Encode(bytes)), Is.EqualTo($"{input.Length}:{input}"));
        }

        [Test]
        public void Encode_InputIsValidString_ReturnsCorrectBEncodedString()
        {
            string input = "input";

            Assert.That(BEncode.Encode(input), Is.EqualTo($"{input.Length}:{input}"));
        }

        [Test]
        public void Encode_InputIsValidLong_ReturnsCorrectBEncodedNumber()
        {
            long input = 5;

            Assert.That(Encoding.UTF8.GetString(BEncode.Encode(input)), Is.EqualTo($"i{input}e"));
        }

        [Test]
        public void Encode_InputIsValidInteger_ReturnsCorrectBEncodedNumber()
        {
            int input = 5;

            Assert.That(Encoding.UTF8.GetString(BEncode.Encode(input)), Is.EqualTo($"i{input}e"));
        }

        [Test]
        public void Encode_InputIsValidList_ReturnsCorrectBEncodedList()
        {
            List<object> input = new List<object>() { "inputTest", (long)5 };

            Assert.That(Encoding.UTF8.GetString(BEncode.Encode(input)), Is.EqualTo($"l9:inputTesti5ee"));
        }

        [Test]
        public void Encode_InputIsValidDictionary_ReturnsCorrectBEncodedDictionary()
        {
            Dictionary<string, object> input = new Dictionary<string, object>() { 
                { "inputTest", (long)5 },
                { "inputTesr", (long)2 },
                { "inputTese", "hello" },
            };

            Assert.That(Encoding.UTF8.GetString(BEncode.Encode(input)), Is.EqualTo("d9:inputTese5:hello9:inputTesri2e9:inputTesti5ee")); // Gets sorted by key byte array (alphabetically most of the time)
        }
    }
}