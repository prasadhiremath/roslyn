﻿// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using System.Collections.Immutable;
using System.Linq;
using System.Runtime.Serialization;

namespace Roslyn.Utilities
{
    internal static class SerializationInfoExtensions
    {
        public static void AddArray<T>(this SerializationInfo info, string name, ImmutableArray<T> value) where T : class
        {
            // we will copy the content into an array and serialize the copy
            // we could serialize elementwise, but that would require serializing
            // name and type for every serialized element which seems worse than creating a copy.
            info.AddValue(name, value.ToArray(), typeof(T[]));
        }

        public static ImmutableArray<T> GetArray<T>(this SerializationInfo info, string name) where T : class
        {
            var arr = (T[])info.GetValue(name, typeof(T[]));
            return ImmutableArray.Create<T>(arr);
        }

        public static void AddByteArray(this SerializationInfo info, string name, ImmutableArray<byte> value)
        {
            // we will copy the content into an array and serialize the copy
            // we could serialize elementwise, but that would require serializing
            // name and type for every serialized element which seems worse than creating a copy.
            info.AddValue(name, value.IsDefault ? null : value.ToArray(), typeof(byte[]));
        }

        public static ImmutableArray<byte> GetByteArray(this SerializationInfo info, string name)
        {
            var arr = (byte[])info.GetValue(name, typeof(byte[]));
            return ImmutableArray.Create<byte>(arr);
        }
    }
}
