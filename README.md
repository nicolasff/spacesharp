# Spacesharp

A portable [Whitespace](https://en.wikipedia.org/wiki/Whitespace_(programming_language)) compiler written in C#, producing executable .NET binaries.

It uses [Lex and Yacc](http://dinosaur.compilertools.net/) to build its parser as a `.so` or `.dll` binary, and loads it with `DllImport`. I wrote it this way since I was very familiar with these tools and didn't have much experience with native .NET parsing libraries such as [ANTLR](https://www.antlr.org/).

The binaries are generated using .NET's own `System.Reflection.Emit` library. This means that the compiler produces portable `.exe` files which can run natively on Windows or with [Mono](https://www.mono-project.com/) on other platforms without needing to be recompiled.

## History

This GitHub repository was imported from [Google Code](https://code.google.com/archive/p/spacesharp/), where the project was originally hosted. With the [shutdown of Google Code](https://opensource.googleblog.com/2015/03/farewell-to-google-code.html) in January 2016, all projects were archived and only SVN snapshots were preserved, so the original commit history of this repository is lost.

Since this was written in August 2008 and hasn't been touched since, this project is obviously long dead. It is not accepting contributions and only hosted on GitHub for posterity.

## Usage with Mono

```
$ make
(produces wsc.exe)

$ mono wsc.exe
usage: wsc source.ws [-o bin.exe]

$ mono wsc.exe tests/ws/hworld.ws -o hworld.exe
saving to 'hworld.exe'

$ mono hworld.exe
Hello, world of spaces!
```

## Usage on Windows

Run `lex-yacc/build-parser.bat` in order to create the whitespace parser before compiling the project.

## Test programs

The directory `tests/ws` contains 17 different programs written in Whitespace which can be compiled to .NET executables. While some are pretty simple like `life.ws` which just prints `42` and only takes 17 bytes, others are much more complex.

### Life

```
$ xxd tests/ws/life.ws
00000000: 2020 2009 2009 2009 200a 090a 2009 0a0a     . . . ... ...
00000010: 0a                                       .

$ mono wsc.exe tests/ws/life.ws -o life.exe
saving to 'life.exe'

$ mono life.exe
42
```

Decoded:
- `space` `space`: Push (number)
- `space` `tab` `space` `tab` `space` `tab` `space` `LF`: The number `0101010` in binary, or 42
- `tab` `LF` `space` `tab`: Print the number at the top of the stack
- `LF` `LF` `LF`: End the program

### Factorial

```
$ wc -c tests/ws/fact.ws
    1757 tests/ws/fact.ws

$ mono wsc.exe tests/ws/fact.ws -o fact.exe
saving to 'fact.exe'

$ mono fact.exe
Enter a number: 12
12! = 479001600
```

### Quine 1

[A program that prints itself](https://en.wikipedia.org/wiki/Quine_(computing)) (obviously without reading its own source file). This is the longest of all the test programs.

```
$ wc -c tests/ws/quine.ws
   18862 tests/ws/quine.ws

$ shasum -a 1 tests/ws/quine.ws
f487e7131d08a4ff0c3c9a677e3f12f88114a1d6  tests/ws/quine.ws

$ mono wsc.exe tests/ws/quine.ws -o quine.exe
saving to 'quine.exe'

$ mono quine.exe | wc -c
   18862

$ mono quine.exe | shasum -a 1
f487e7131d08a4ff0c3c9a677e3f12f88114a1d6  -
```

### Quine 2

An even shorter quine:

```
$ wc -c tests/ws/quine-2.ws
   10675 tests/ws/quine-2.ws

$ shasum -a 1 tests/ws/quine-2.ws
887ac219835994077d70ee65efb6e334162497a5  tests/ws/quine-2.ws

$ mono wsc.exe tests/ws/quine-2.ws -o quine-2.exe
saving to 'quine-2.exe'

$ mono quine-2.exe | wc -c
   10675

$ mono quine-2.exe | shasum -a 1
887ac219835994077d70ee65efb6e334162497a5  -
```

### Towers of Hanoi

Prints the sequence of moves needed to solve the classic [Towers of Hanoi](https://en.wikipedia.org/wiki/Tower_of_Hanoi) problem, with any number of discs and 3 rods.
```
$ wc -c tests/ws/hanoi.ws
    2162 tests/ws/hanoi.ws

$ mono wsc.exe tests/ws/hanoi.ws -o hanoi.exe
saving to 'hanoi.exe'

$ mono hanoi.exe
Enter a number: 3
1 -> 3
1 -> 2
3 -> 2
1 -> 3
2 -> 1
2 -> 3
1 -> 3
```

## Trivia

The `-o` parameter configures the name of the executable produced by the compiler. It is optional, the default file name is `" .exe"`.
