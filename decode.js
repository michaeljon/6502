#!/usr/bin/env node

const ps = parseInt(process.argv[2], 16);

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
