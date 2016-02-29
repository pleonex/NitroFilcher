#!/bin/bash

if [[ -z "$1" ]] ; then
    echo "The first argument must be the relative path to the patch to apply."
    exit -1
fi

if [[ ! -d "desmume" ]] ; then
    # Checkout the desmume code if it doesn't exist
    svn checkout svn://svn.code.sf.net/p/desmume/code/trunk desmume
    cd desmume
else
    # Update the current repo otherwise
    cd desmume
    svn update
fi

# Apply our patch
patch -p0 -i ../"$1"

# Run the pre-steps
# Yes, there is a desmume folder inside the root folder
cd desmume
./autogen.sh

# Configure enabling debug
./configure --enable-debug --enable-developer

# Make
if [[ -z "$2" ]] ; then
    make
else
    make -j$2
fi

# We are not install but copying the executable
cd ../..
mkdir -p NitroFilcher/bin/Debug
mkdir -p NitroFilcher/bin/Release
cp desmume/desmume/src/gtk/desmume NitroFilcher/bin/Debug/desmume_nitrofilcher.exe
cp desmume/desmume/src/gtk/desmume NitroFilcher/bin/Release/desmume_nitrofilcher.exe
