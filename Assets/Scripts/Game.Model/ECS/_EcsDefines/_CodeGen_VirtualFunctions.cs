
//#define DONT_USE_GENERATE_CODE                                                                
                                                                                                
using System.Collections.Generic;                                                               
using GamesTan.UnsafeECSDefine;                                                                 
using System;                                                                                   
namespace GamesTan.UnsafeECSDefine  {                                                           
public partial class PClassA{ 
 public virtual  string FuncA_Virtual() { return default; }
public virtual  string FuncB_Virtual() { return default; }
public virtual  void FuncCParam_Virtual(int a,float b,string c) {  }
 
 }
public partial class SubClassA{ 
 public override  string FuncA_Virtual() { return default; }
 
 }
public partial class SubClassB{ 
 public override  string FuncA_Virtual() { return default; }
public override  string FuncB_Virtual() { return default; }
public override  void FuncCParam_Virtual(int a,float b,string c) {  }
 
 }
                                                                                        
}                                                                                               