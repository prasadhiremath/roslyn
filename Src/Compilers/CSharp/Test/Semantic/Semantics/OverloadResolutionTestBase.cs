﻿// Copyright (c) Microsoft Open Technologies, Inc.  All Rights Reserved.  Licensed under the Apache License, Version 2.0.  See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.CSharp.Symbols;
using Microsoft.CodeAnalysis.CSharp.Test.Utilities;
using Microsoft.CodeAnalysis.CSharp.UnitTests;
using ProprietaryTestResources = Microsoft.CodeAnalysis.Test.Resources.Proprietary;
using Roslyn.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Roslyn.Test.Utilities;

namespace Microsoft.CodeAnalysis.CSharp.UnitTests
{
    public abstract class OverloadResolutionTestBase : CompilingTestBase
    {
        internal void TestOverloadResolutionWithDiff(string source, MetadataReference[] additionalRefs = null)
        {
            // The mechanism of this test is: we build the bound tree for the code passed in and then extract
            // from it the nodes that describe the method symbols. We then compare the description of
            // the symbols given to the comment that follows the call.

            var mscorlibRef = new MetadataImageReference(ProprietaryTestResources.NetFX.v4_0_30316_17626.mscorlib.AsImmutableOrNull(), display: "mscorlib");
            var references = new[] { mscorlibRef }.Concat(additionalRefs ?? SpecializedCollections.EmptyArray<MetadataReference>());

            var compilation = CreateCompilation(source, references, TestOptions.Dll.WithAllowUnsafe(true));

            var method = (SourceMethodSymbol)compilation.GlobalNamespace.GetTypeMembers("C").Single().GetMembers("M").Single();
            var diagnostics = new DiagnosticBag();
            var block = Compiler.BindMethodBody(method, diagnostics);
            var tree = BoundTreeDumperNodeProducer.MakeTree(block);
            var results = string.Join("\n", tree.PreorderTraversal().Select(edge => edge.Value)
                .Where(x => x.Text == "method" && x.Value != null)
                .Select(x => x.Value)
                .ToArray());

            // var r = string.Join("\n", tree.PreorderTraversal().Select(edge => edge.Value).ToArray();

            var expected = string.Join("\n", source
                .Split(new[] { "\r\n" }, System.StringSplitOptions.RemoveEmptyEntries)
                .Where(x => x.Contains("//-"))
                .Select(x => x.Substring(x.IndexOf("//-") + 3))
                .ToArray());

            AssertEx.Equal(expected, results);
        }
    }
}
