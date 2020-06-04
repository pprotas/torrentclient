using NUnit.Framework;
using NUnit.Framework.Internal;
using System;
using TorrentClient.Service;

namespace TorrentClient.Tests
{
    public class FilteItemTest
    {
        [Test]
        public void FormattedSize_FileItemHasSize5_ReturnsCorrectString()
        {
            FileItem input = new FileItem { Size = 5 };

            Assert.That(input.FormattedSize, Is.EqualTo("5B"));
        }

        [Test]
        public void FormattedSize_FileItemHasSize1024_ReturnsCorrectString()
        {
            FileItem input = new FileItem { Size = 1024 };

            Assert.That(input.FormattedSize, Is.EqualTo("1KB"));
        }

        [Test]
        public void FormattedSize_FileItemHasSize1048576_ReturnsCorrectString()
        {
            FileItem input = new FileItem { Size = 1048576 };

            Assert.That(input.FormattedSize, Is.EqualTo("1MB"));
        }
    }
}