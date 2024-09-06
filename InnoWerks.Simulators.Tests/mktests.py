for t in range(0, 256):
    test_id = "{:02x}".format(t)

    print("[TestMethod]")

    # 6502 expected failures
    if test_id.upper() in ["02", "12", "22", "32", "42", "52", "62", "72", "92", "B2", "D2", "F2"]:
        print("[ExpectedException(typeof(IllegalOpCodeException))]")
    print("public void RunIndividual65C02Test" + test_id.upper() + "()")
    print("{")
    print('    RunNamedBatch(CpuClass.WDC65C02, "' + test_id + '");')
    print("}")
    print()
