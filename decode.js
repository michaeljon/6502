#!/usr/bin/env node

const base =
  process.argv[2].substring(0, 2) == "0x" || process.argv[2].substring(0) == "$"
    ? 16
    : 10;
const val =
  process.argv[2].substring(0) == "$"
    ? process.argv[2].substring(1)
    : process.argv[2];

const ps = parseInt(val, base);

let result = "";

result += ps & 0x80 ? "N" : "n";
result += ps & 0x40 ? "V" : "v";
result += "-";
result += ps & 0x10 ? "B" : "b";
result += ps & 0x08 ? "D" : "d";
result += ps & 0x04 ? "I" : "i";
result += ps & 0x02 ? "Z" : "z";
result += ps & 0x01 ? "C" : "c";

console.log(`Decode ${ps} to ${result}`);
