﻿// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System;
using System.IO;

namespace Roslyn.Test.Utilities
{
    public class TestStream : Stream
    {
        private readonly bool canRead, canSeek, canWrite;

        public TestStream(bool canRead = false, bool canSeek = false, bool canWrite = false)
        {
            this.canRead = canRead;
            this.canSeek = canSeek;
            this.canWrite = canWrite;
        }

        public override bool CanRead
        {
            get
            {
                return canRead;
            }
        }

        public override bool CanSeek
        {
            get
            {
                return canSeek;
            }
        }

        public override bool CanWrite
        {
            get
            {
                return canWrite;
            }
        }

        public override long Length
        {
            get
            {
                throw new NotSupportedException();
            }
        }

        public override long Position
        {
            get
            {
                throw new NotSupportedException();
            }

            set
            {
                throw new NotSupportedException();
            }
        }

        public override void Flush()
        {
            throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotSupportedException();
        }
    }
}
