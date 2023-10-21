using System.Runtime.InteropServices;
using UnityEngine;


namespace GamesTan.ECS.Game {
    public unsafe partial struct PClassA {
        [VirtualFunction(typeof(PClassA))]
        public static string FuncA_Virtual(PClassA* ptr) {
            return "PClassA::FuncA_Virtual" + ptr->Val1;
        }

        [VirtualFunction(typeof(PClassA))]
        public static string FuncB_Virtual(PClassA* ptr) {
            return "PClassA::FuncB_Virtual" + ptr->Val1;
        }
        
        [VirtualFunction(typeof(PClassA))]
        public static void FuncCParam_Virtual(PClassA* ptr,int a,float b,string c) {
        }
    }

    public unsafe partial struct SubClassA {
        [VirtualFunction(typeof(SubClassA))]
        public static string FuncA_Virtual(SubClassA* ptr) {
            return "SubClassA::FuncA_Virtual" + ptr->Val2;
        }
    }

    public unsafe partial struct SubClassB {
        [VirtualFunction(typeof(SubClassB))]
        public static string FuncA_Virtual(SubClassB* ptr) {
            return "SubClassB::FuncA_Virtual" + ptr->Val3;
        }

        [VirtualFunction(typeof(SubClassB))]
        public static string FuncB_Virtual(SubClassB* ptr) {
            return "SubClassB::FuncB_Virtual" + ptr->Val3;
        }        
        [VirtualFunction(typeof(SubClassB))]
        public static void FuncCParam_Virtual(SubClassB* ptr,int a,float b,string c) {
        }
    }

    public class TestStructVirtualFunctions : MonoBehaviour {
        unsafe void Start() {
            TestVirtualStruct();
        }

        unsafe void TestVirtualStruct() {
            PClassA dataP = new PClassA();
            SubClassA dataA = new SubClassA();
            SubClassB dataB = new SubClassB();
            dataP._SetEntityRef(new EntityRef() { _type = EntityIds.PClassA });
            dataA._SetEntityRef(new EntityRef() { _type = EntityIds.SubClassA });
            dataB._SetEntityRef(new EntityRef() { _type = EntityIds.SubClassB });

            dataP.Val1 = 1;
            dataA.Val1 = 2;
            dataB.Val1 = 3;

            dataA.Val2 = 4;
            dataB.Val3 = 5;

            Debug.Assert(dataP.FuncA() == "PClassA::FuncA_Virtual1");
            Debug.Assert(dataA.FuncA() == "SubClassA::FuncA_Virtual4");
            Debug.Assert(dataB.FuncA() == "SubClassB::FuncA_Virtual5");

            Debug.Assert(dataP.FuncB() == "PClassA::FuncB_Virtual1");
            Debug.Assert(dataA.FuncB() == "PClassA::FuncB_Virtual2");
            Debug.Assert(dataB.FuncB() == "SubClassB::FuncB_Virtual5");

            PClassA* ptrP = (PClassA*)&dataP;
            PClassA* ptrA = (PClassA*)&dataA;
            PClassA* ptrB = (PClassA*)&dataB;

            Debug.Assert(ptrP->FuncA() == "PClassA::FuncA_Virtual1");
            Debug.Assert(ptrA->FuncA() == "SubClassA::FuncA_Virtual4");
            Debug.Assert(ptrB->FuncA() == "SubClassB::FuncA_Virtual5");

            Debug.Assert(ptrP->FuncB() == "PClassA::FuncB_Virtual1");
            Debug.Assert(ptrA->FuncB() == "PClassA::FuncB_Virtual2");
            Debug.Assert(ptrB->FuncB() == "SubClassB::FuncB_Virtual5");
            Debug.Log("TestStructVirtualFunctions Done");
        }
    }
}