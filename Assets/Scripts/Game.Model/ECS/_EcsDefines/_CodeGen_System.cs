
//#define DONT_USE_GENERATE_CODE                                                                
                                                                                                
using System.Collections.Generic;                                                               
using GamesTan.UnsafeECSDefine;                                                                 
using System;                                                                                   
namespace GamesTan.UnsafeECSDefine  {                                                           
public partial class SysTestBulletAwake :ISystemWithSpecifyEntity{ 
 public Bullet Entity; 

 
 }
public partial class SysTestBulletUpdateCollision :ISystemWithSpecifyEntity{ 
 public Bullet Entity; 

 
 }
public partial class SysTestBulletUpdatePos :ISystemWithSpecifyEntity{ 
 public Bullet Entity; 

 
 }
public partial class SysTestEnemyAwake :ISystemWithSpecifyEntity{ 
 public Enemy Entity; 

 
 }
public partial class SysTestEnemyUpdateAI :ISystemWithSpecifyEntity{ 
 public Enemy Entity; 

 
 }
public partial class SysTestEnemyUpdateAnimation :ISystemWithSpecifyEntity{ 
 public Enemy Entity; 

 
 }
public partial class SysTestUpdateMeshRender :IPureSystemWithEntity{ 
 public AssetData AssetData; 
public Transform3D Transform3D; 
public MeshRenderData MeshRenderData; 

 
 }
public partial class SysTestUpdateSkinRender :IPureSystemWithEntity{ 
 public AssetData AssetData; 
public Transform3D Transform3D; 
public AnimRenderData AnimRenderData; 

 
 }
                                                                                        
}                                                                                               