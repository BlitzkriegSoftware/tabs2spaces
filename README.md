# tabs2spaces #
Little Utility to Convert Tabs To Spaces

## Build ##

From BASH run either:

```BASH
./publish-linux.sh
```
or

```BASH
./publish-windows.sh
```

## Use ##

Set Environment Variable to the number of spaces to replace each TAB with:

```BASH
export PAD=3
```

Read from a file
```BASH
t2s infile.txt > outfile.txt
```
or

Read from STDIN
```BASH
t2s < infile.txt > outfile.txt
```
