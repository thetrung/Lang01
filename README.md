### Lang-01
Toy Programming Language Reseach/Playground.

### Intro
I used to write dozen of languages intepreter & compiler in the last decade ( 2010 ), so now I'm back to prototype even more things and make sure I have fun instead of worrying about serious usage and performance. The target platform, as usual, is dotNET, for easy library work, portability and can run with powerful game engine like Unity.

### Goal

- The language aim to be scriptable, simple like Lua but use Stack like Forth. 
- Assembly-like structure, where `Label` and `Jump` are still popular for loop.
- The intepreter also maintain its own stack, that user can resize on init.
- It will only have data types: lua-like Table, Boolean, Double, String.
- Bytecode translate on parsing before intepreting.
- Most important: bring back the FLOW state to programming.

### Mechanism

- Source > ByteCode > Cached Stack > Eval Loop
