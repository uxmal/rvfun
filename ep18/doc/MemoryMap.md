# Old memory layout
This was the old memory layout, prior to Ep 14:
```
+----------------+ 00000000
| code + data    | RWX
|                |
| stack          |
+----------------+
```

# New memory layout
This is the layout after Ep 14:
```
.................. 00000000
.                . unmapped
.                .
..................
+----------------+ 00400000
|                | RW (not executable)
|                |
| stack          |
+----------------+
| code           | 00800000
|                | RX (not writeable)
+----------------+
| data           | 00800000 + code size (RW)
+----------------+
```