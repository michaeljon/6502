# 6502 Merlin-like grammar

A `line` is one of:

- Code: `^(label)?\s+([A-Z]{1,3})\s+(arg)?\s+(;comment)?`
- Comment: `^\*.*$^\s+;.*$`
- Data: `^(label)?\s+(DW|DB)\s+(value)`
- Directive: `^\s+ORG\s+(value)`
- Empty: `^\s*$^(label)\s*$`
- Equivalence: `^(label)\s+'(EQU|=)'\s+(value)`
