﻿/* Copyright 2010-2014 MongoDB Inc.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

using System;
using System.IO;

namespace MongoDB.Bson.IO
{
    /// <summary>
    /// Represents a slice of a byte buffer.
    /// </summary>
    public class ByteBufferSlice : IByteBuffer
    {
        private readonly IByteBuffer _buffer;
        private bool _disposed;
        private readonly int _length;
        private readonly int _offset;

        /// <summary>
        /// Initializes a new instance of the <see cref="ByteBufferSlice"/> class.
        /// </summary>
        /// <param name="buffer">The byte buffer.</param>
        /// <param name="offset">The offset of the slice.</param>
        /// <param name="length">The length of the slice.</param>
        public ByteBufferSlice(IByteBuffer buffer, int offset, int length)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException("buffer");
            }
            if (!buffer.IsReadOnly)
            {
                throw new ArgumentException("The buffer is not read only.", "buffer");
            }
            if (offset < 0 || offset > buffer.Length)
            {
                throw new ArgumentOutOfRangeException("offset");
            }
            if (length < 0)
            {
                throw new ArgumentException("The length is negative.", "length");
            }
            if (offset + length > buffer.Length)
            {
                throw new ArgumentException("The length extends beyond the end of the buffer.", "length");
            }

            _buffer = buffer;
            _offset = offset;
            _length = length;
        }

        /// <inheritdoc/>
        public int Capacity
        {
            get
            {
                ThrowIfDisposed();
                return _length;
            }
        }

        /// <inheritdoc/>
        public bool IsReadOnly
        {
            get
            {
                ThrowIfDisposed();
                return true;
            }
        }

        /// <inheritdoc/>
        public int Length
        {
            get
            {
                ThrowIfDisposed();
                return _length;
            }
            set
            {
                throw new NotSupportedException();
            }
        }

        /// <inheritdoc/>
        public ArraySegment<byte> AccessBackingBytes(int position)
        {
            EnsureValidPosition(position);
            ThrowIfDisposed();

            return _buffer.AccessBackingBytes(position + _offset);
        }

        /// <inheritdoc/>
        public void Clear(int position, int count)
        {
            EnsureValidPositionAndCount(position, count);
            ThrowIfDisposed();

            _buffer.Clear(position + _offset, count);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            if (!_disposed)
            {
                _disposed = true;
                _buffer.Dispose();
            }
        }

        /// <inheritdoc/>
        public void EnsureCapacity(int capacity)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public IByteBuffer GetSlice(int position, int length)
        {
            EnsureValidPositionAndLength(position, length);
            ThrowIfDisposed();

            return _buffer.GetSlice(position + _offset, length);
        }

        /// <inheritdoc/>
        public void LoadFrom(Stream stream, int position, int count)
        {
            EnsureValidPositionAndCount(position, count);
            ThrowIfDisposed();

            throw new NotSupportedException();
        }

        /// <inheritdoc/>
        public void MakeReadOnly()
        {
            ThrowIfDisposed();
        }

        /// <inheritdoc/>
        public byte ReadByte(int position)
        {
            EnsureValidPosition(position);
            ThrowIfDisposed();

            return _buffer.ReadByte(position + _offset);
        }

        /// <inheritdoc/>
        public void ReadBytes(int position, byte[] destination, int offset, int count)
        {
            EnsureValidPositionAndCount(position, count);
            ThrowIfDisposed();

            _buffer.ReadBytes(position + _offset, destination, offset, count);
        }

        /// <inheritdoc/>
        public void WriteByte(int position, byte value)
        {
            EnsureValidPosition(position);
            ThrowIfDisposed();

            _buffer.WriteByte(position + _offset, value);
        }

        /// <inheritdoc/>
        public void WriteBytes(int position, byte[] source, int offset, int count)
        {
            EnsureValidPositionAndCount(position, count);
            ThrowIfDisposed();

            _buffer.WriteBytes(position + _offset, source, offset, count);
        }

        /// <inheritdoc/>
        public void WriteTo(Stream stream, int position, int count)
        {
            EnsureValidPositionAndCount(position, count);
            ThrowIfDisposed();

            _buffer.WriteTo(stream, position + _offset, count);
        }

        private void EnsureValidPosition(int position)
        {
            if (position < 0 || position > _length)
            {
                throw new ArgumentOutOfRangeException("position");
            }
        }

        private void EnsureValidPositionAndCount(int position, int count)
        {
            EnsureValidPosition(position);
            if (count < 0)
            {
                throw new ArgumentException("Count is negative.", "count");
            }
            if (position + count > _length)
            {
                throw new ArgumentException("Count extends beyond the end of the buffer.", "count");
            }
        }

        private void EnsureValidPositionAndLength(int position, int length)
        {
            EnsureValidPosition(position);
            if (length < 0)
            {
                throw new ArgumentException("Length is negative.", "length");
            }
            if (position + length > _length)
            {
                throw new ArgumentException("Length extends beyond the end of the buffer.", "length");
            }
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(GetType().FullName);
            }
        }
    }
}