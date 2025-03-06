# Command line


The command line arguments to a program are provided as a vector of pointers
to strings. A typical C `main` program looks like this:
```C
int main(int argc, char **argv)

```
          argv[0]   argv[1]   argv[2]
argv --> +---------+---------+---------+
         |    *    |    *    |   null  |
         +----|----+----|----+---------+
            |         V
            v         world!\0
            hello\0
```

If we type `rvfun hello world!`, the arguments we receive need to be packed
as nul-terminated UTF-8 strings. But where? We pack the pointers at the top
of the stack area, which is readable and writeable!

```
                +---------------+ StackAddress
                |               |
                |               |
                    ...
                |               |       <- x2, stack ptr
             ,--+ ptr1   ptr2   +--,    <- x11, argv
             `->| h e l l o _ w |  |
                | o r l d ! _   |<-'
                +---------------+ StackAddress
```

