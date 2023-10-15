# ECS_Game_Demo

---
<p align="center"><img src="https://github.com/JiepengTan/JiepengTan.github.io/blob/master/assets/img/blog/ECSGame/show_render.gif?raw=true" width="768"></p> 
<p align="center"><img src="https://github.com/JiepengTan/JiepengTan.github.io/blob/master/assets/img/blog/ECSGame/show_collision.gif?raw=true" width="768"></p> 

This repository is a 《Vampire Survivors》 3D tech demo 

## Quick Start
1. Clone this repository
2. Open it use Unity2023.1.7 or higher version
3. Open **Demo_URP** scene, run it, try to changed the value like below

<p align="center"><img src="https://github.com/JiepengTan/JiepengTan.github.io/blob/master/assets/img/blog/ECSGame/quick_start.jpg?raw=true" width="768"></p> 

## Technical detail
1. Animation : GPUSkin  
    - https://github.com/chengkehan/GPUSkinning
    - https://assetstore.unity.com/packages/tools/animation/gpu-ecs-animation-baker-250425  (recommend)

2. Indirect Renderer 
    - https://github.com/ellioman/Indirect-Rendering-With-Compute-Shaders

3. ECS
    - Entity Manager https://github.com/godotengine/godot/blob/4.1/core/templates/rid_owner.h
    - Memory Manager https://github.com/JiepengTan/LockstepECS

4. Scene Manager
    - Chunk, Grid, Region

## Blog (Chinese 中文)

https://zhuanlan.zhihu.com/p/660927213
