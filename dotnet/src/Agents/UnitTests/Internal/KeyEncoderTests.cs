﻿// Copyright (c) Microsoft. All rights reserved.
using System;
using System.Linq;
using Microsoft.SemanticKernel.Agents.Internal;
using Xunit;

namespace SemanticKernel.Agents.UnitTests.Internal;

/// <summary>
/// Unit testing of <see cref="KeyEncoder"/>.
/// </summary>
public class KeyEncoderTests
{
    /// <summary>
    /// Validate the production of unique and consistent hashes.
    /// </summary>
    [Fact]
    public void VerifyKeyEncoderUniqueness()
    {
        this.VerifyHashEquivalancy(Array.Empty<string>());
        this.VerifyHashEquivalancy(nameof(KeyEncoderTests));
        this.VerifyHashEquivalancy(nameof(KeyEncoderTests), "http://localhost", "zoo");
    }

    private void VerifyHashEquivalancy(params string[] keys)
    {
        string hash1 = KeyEncoder.GenerateHash(keys);
        string hash2 = KeyEncoder.GenerateHash(keys);
        string hash3 = KeyEncoder.GenerateHash(keys.Concat(["another"]));

        Assert.Equal(hash1, hash2);
        Assert.NotEqual(hash1, hash3);
    }
}