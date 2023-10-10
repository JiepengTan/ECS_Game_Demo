#!/bin/bash
dir="$(cd $(dirname ${BASH_SOURCE[0]}) && pwd)"
cd $dir/CodeGenerator/Bin

pwd
echo "gen codes..."
./GamesTan.Game.CodeGenerator.exe

pwd
echo "done"
# copy to unity 
# rm -rf ../../Assets/LockstepECS/__DllSourceFiles/Game.Model/Src/__UnsafeECS/Generated/
# mkdir -p ../../Assets/LockstepECS/__DllSourceFiles/Game.Model/Src/__UnsafeECS/Generated/
# cp -rf ../Src/Tools.UnsafeECS.ECSOutput/Src/Generated/Model/* ../../Assets/LockstepECS/__DllSourceFiles/Game.Model/Src/__UnsafeECS/Generated/

read