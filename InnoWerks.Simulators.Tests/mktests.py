for t in range(0, 256):
    test_id = "{:02x}".format(t)

    print("[TestMethod]")
    print("public void RunIndividual6502Test" + test_id.upper() + "()")
    print("{")
    print(
        '    if (ignored[byte.Parse("' + test_id + '", NumberStyles.HexNumber, CultureInfo.InvariantCulture)] == true)'
    )
    print("    {")
    print('        Assert.Inconclusive($"Test ' + test_id + ' is marked as ignored");')
    print("    }")
    print("    else")
    print("    {")
    print('        RunNamedBatch("' + test_id + '");')
    print("    }")
    print("}")
    print()
