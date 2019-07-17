/*
 * MIT License
 * 
 * Copyright (c) 2019 plexdata.de
 * 
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

using Moq;
using NUnit.Framework;
using Plexdata.LogWriter.Abstraction;
using Plexdata.QuickCopy.Builders;
using Plexdata.QuickCopy.Handlers;
using Plexdata.QuickCopy.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;

namespace QuickCopyTests.Builders
{
    [ExcludeFromCodeCoverage]
    [TestOf(nameof(PlaylistBuilder))]
    public class PlaylistBuilderTests
    {
        private String source;
        private Mock<ILogger> logger;
        private CancellationToken token;

        [SetUp]
        public void Setup()
        {
            this.logger = new Mock<ILogger>();
            this.token = new CancellationToken();
            this.source = null;
        }

        [TearDown]
        public void Cleanup()
        {
            if (this.source != null)
            {
                Directory.Delete(this.source, true);
            }
        }

        [Test]
        public void PlaylistBuilder_LoggerIsNull_ThrowsArgumentException()
        {
            this.logger = null;
            Assert.That(() => this.GetInstance(), Throws.ArgumentException);
        }

        [Test]
        public void Build_ArgumentsIsNull_ThrowsArgumentException()
        {
            Assert.That(() => this.GetInstance().Build(null), Throws.ArgumentException);
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        public void Build_ArgumentsWithFiles_ResultAsExpected(Int32 count)
        {
            List<PlaylistEntryHandler> actual = this.GetInstance().Build(this.GetFilesArguments(count));

            Assert.That(actual.Count, Is.EqualTo(count));
            for (Int32 index = 0; index < count; index++)
            {
                Assert.That(actual[index].Entry.Source, Is.EqualTo($"\\\\?\\c:\\source\\file-{index}.ext"));
                Assert.That(actual[index].Entry.Target, Is.EqualTo($"\\\\?\\c:\\target\\file-{index}.ext"));
                Assert.That(actual[index].Entry.IsMove, Is.True);
                Assert.That(actual[index].Entry.IsVerify, Is.True);
                Assert.That(actual[index].Entry.IsOverwrite, Is.True);
            }
        }

        [Test]
        [TestCase(1, true)]
        [TestCase(2, true)]
        [TestCase(3, true)]
        [TestCase(1, false)]
        [TestCase(2, false)]
        [TestCase(3, false)]
        public void Build_ArgumentsWithSource_ResultAsExpected(Int32 count, Boolean recursive)
        {
            List<PlaylistEntryHandler> actual = this.GetInstance().Build(this.GetSourceArguments(count, recursive));

            actual.Sort(ComparePlaylistEntryHandler);

            Assert.That(actual.Count, Is.EqualTo(recursive ? count * 3 : count));

            // WOW...

            for (Int32 index = 0; index < count; index++)
            {
                Assert.That(actual[index].Entry.Source, Is.EqualTo($"\\\\?\\{this.source}\\file-{index}.txt"));
                Assert.That(actual[index].Entry.Target, Is.EqualTo($"\\\\?\\c:\\target\\file-{index}.txt"));
                Assert.That(actual[index].Entry.IsMove, Is.True);
                Assert.That(actual[index].Entry.IsVerify, Is.True);
                Assert.That(actual[index].Entry.IsOverwrite, Is.True);
            }

            if (recursive)
            {
                for (Int32 index = count; index < count + count; index++)
                {
                    Assert.That(actual[index].Entry.Source, Is.EqualTo($"\\\\?\\{this.source}\\sub-folder-a\\file-{index - count}.txt"));
                    Assert.That(actual[index].Entry.Target, Is.EqualTo($"\\\\?\\c:\\target\\sub-folder-a\\file-{index - count}.txt"));
                    Assert.That(actual[index].Entry.IsMove, Is.True);
                    Assert.That(actual[index].Entry.IsVerify, Is.True);
                    Assert.That(actual[index].Entry.IsOverwrite, Is.True);
                }

                for (Int32 index = count + count; index < count + count + count; index++)
                {
                    Assert.That(actual[index].Entry.Source, Is.EqualTo($"\\\\?\\{this.source}\\sub-folder-b\\file-{index - (count + count)}.txt"));
                    Assert.That(actual[index].Entry.Target, Is.EqualTo($"\\\\?\\c:\\target\\sub-folder-b\\file-{index - (count + count)}.txt"));
                    Assert.That(actual[index].Entry.IsMove, Is.True);
                    Assert.That(actual[index].Entry.IsVerify, Is.True);
                    Assert.That(actual[index].Entry.IsOverwrite, Is.True);
                }
            }
        }

        private PlaylistBuilder GetInstance()
        {
            return new PlaylistBuilder(this.logger?.Object, this.token);
        }

        private Arguments GetFilesArguments(Int32 count)
        {
            String[] files = new String[count];

            for (Int32 index = 0; index < count; index++)
            {
                files[index] = $"c:\\source\\file-{index}.ext";
            }

            return new Arguments()
            {
                Target = @"c:\target",
                Files = files,
                IsMove = true,
                IsVerify = true,
                IsOverwrite = true,
            };
        }

        private Arguments GetSourceArguments(Int32 count, Boolean recursive)
        {
            this.source = Path.Combine(Path.GetTempPath(), "testing");
            String[] exts = new String[] { ".txt", ".tmp", ".xyz" };

            this.CreateSourceEnvironment(this.source, exts, count);
            if (recursive)
            {
                this.CreateSourceEnvironment(Path.Combine(this.source, "sub-folder-a"), exts, count);
                this.CreateSourceEnvironment(Path.Combine(this.source, "sub-folder-b"), exts, count);
            }

            return new Arguments()
            {
                Source = this.source,
                Target = @"c:\target",
                Pattern = "*.txt",
                IsMove = true,
                IsVerify = true,
                IsOverwrite = true,
                IsRecursive = recursive,
            };
        }

        private void CreateSourceEnvironment(String path, String[] exts, Int32 count)
        {
            if (Directory.Exists(path))
            {
                throw new InvalidOperationException();
            }

            DirectoryInfo info = Directory.CreateDirectory(path);

            for (Int32 index = 0; index < count; index++)
            {
                foreach (String ext in exts)
                {
                    File.CreateText(Path.Combine(path, $"file-{index}{ext}")).Dispose();
                }
            }
        }

        private static Int32 ComparePlaylistEntryHandler(PlaylistEntryHandler x, PlaylistEntryHandler y)
        {
            if (x == null || y == null)
            {
                throw new ArgumentNullException();
            }

            return String.CompareOrdinal(x.Entry.Source, y.Entry.Source);
        }
    }
}
