using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using System.IO;
using System.Text;
using TorrentClient.Service;

namespace TorrentClient.Tests
{
    public class MemoryStreamExtensionsTests
    {
        [Test]
        public void Append_InputIsByte_AppendsByteToMemoryStream()
        {
            MemoryStream ms = new MemoryStream();
            byte b = Encoding.UTF8.GetBytes("5")[0];
            ms.Append(b);

            Assert.That(ms.ToArray(), Is.EqualTo(Encoding.UTF8.GetBytes("5")));
        }

        [Test]
        public void Append_InputIsByteArray_AppendsBytesToMemoryStream()
        {
            MemoryStream ms = new MemoryStream();
            byte[] b = Encoding.UTF8.GetBytes("53");
            ms.Append(b);

            Assert.That(ms.ToArray(), Is.EqualTo(Encoding.UTF8.GetBytes("53")));
        }
    }
}