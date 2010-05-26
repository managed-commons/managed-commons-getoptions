all: mcat.exe

Commons.GetOptions.dll: 
	ln -s ../bin/Commons.GetOptions.dll

mcat.exe: mcat.cs Makefile Commons.GetOptions.dll
	gmcs /nowarn:169 /r:Commons.GetOptions.dll /t:exe mcat.cs

run: mcat.exe
	mono mcat.exe

clean:
	rm -rf mcat.exe Commons.GetOptions.dll
