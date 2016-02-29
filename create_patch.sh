#!/bin/bash

if [[ -z "$1" ]]; then
    echo "The first argument must be the name of the patch."
    echo "The recommended format is romID_shortGameName."
    exit -1
fi

cd desmume
svn diff > "../patch_$1.diff"
