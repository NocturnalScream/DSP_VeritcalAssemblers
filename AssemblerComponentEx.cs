using System;
using System.Collections.Generic;
using HarmonyLib;
namespace AssemblerVerticalConstruction
{
    public class AssemblerComponentEx
    {
        public Dictionary<int, Dictionary<int, HashSet<int>>> assemblerStacks = new();
        public int[][] assemblerStackMembers = new int[64*6][]; //stores the assembler stack member id and the id of the root assembler in the stack [index][assemblerStackMembers] = rootAssemblerId
        public uint[][] assemblerRootSignTypes = new uint[64*6][]; //stores the signType of the rootAssembler
        public int assemblerStackMembersCapacity = 64*6;
        public void SetArrayCapacity(int newCapacity, ref int[][] targetArray, ref int arrayCapacity)
        {
            var array = targetArray;
            targetArray = new int[newCapacity][];
            if (array != null)
            {
                Array.Copy(array, targetArray, (newCapacity <= arrayCapacity) ? newCapacity : arrayCapacity);
            }
            arrayCapacity = newCapacity;
        }

        public static void Init()
        {
            Dictionary<int, Dictionary<int, HashSet<int>>> assemblerStacks = new();
            int[][] assemblerStackMembers = new int[64*6][]; //stores the assembler stack member id and the id of the root assembler in the stack [index][assemblerStackMembers] = rootAssemblerId
            int[][] assemblerRootSignTypes = new int[64*6][]; //stores the signType of the rootAssembler
        }

         public int GetNextId(int index, int assemblerId){

           if(index >= assemblerStackMembers.Length)
           {
               return 0;
           }
           if (this.assemblerStackMembers[index] == null || assemblerId >= this.assemblerStackMembers[index].Length)
           {
                return 0;
           }
           return this.assemblerStackMembers[index][assemblerId];
        } 

        public void RecalcIdsOnLoad()
        {
            assemblerStacks = new();
            for (int i = 0; i < GameMain.data.factories.Length; i++)
            {
                if (GameMain.data.factories[i] == null)
                {
                    continue;
                }
                var _this = GameMain.data.factories[i].factorySystem;
                if (_this == null)
                {
                    continue;
                }
                if (!assemblerStacks.ContainsKey(i))
                {
                    assemblerStacks[i] = new();
                } 
                var assemblerCapacity = Traverse.Create(_this).Field("assemblerCapacity").GetValue<int>();
                for (int j = 1; j < assemblerCapacity; j++)
                {
                    traceStack(GameMain.data.factories[i].factorySystem, j);
                }
                List<int> keysList = new List<int>(AssemblerVerticalConstruction.assemblerComponentEx.assemblerStacks[i].Keys);
                foreach (var rootAssembler in keysList)
                {
                    traceStackUpAndBuild(GameMain.data.factories[i].factorySystem, rootAssembler);
                }
            }
        }     

        public void RecalcIds(FactorySystem factorySystem)
        {
            var index = factorySystem.factory.index;
            if (GameMain.data.factories[index] == null || factorySystem == null)
                {
                    return;
                }           
            if (!this.assemblerStacks.ContainsKey(index))
            {
                assemblerStacks[index].Clear();
            }
            if (this.assemblerStackMembers[index] != null)
            {
                this.assemblerStackMembers[index].Initialize();
            }
            if (this.assemblerRootSignTypes[index] != null)
            {
                this.assemblerRootSignTypes[index].Initialize();
            }

            var assemblerCapacity = Traverse.Create(factorySystem).Field("assemblerCapacity").GetValue<int>();
            for (int j = 1; j < assemblerCapacity; j++)
            {
                traceStack(GameMain.data.factories[index].factorySystem, j);
            }
            List<int> keysList = new List<int>(AssemblerVerticalConstruction.assemblerComponentEx.assemblerStacks[index].Keys);
            foreach (var rootAssembler in keysList)
            {
                traceStackUpAndBuild(GameMain.data.factories[index].factorySystem, rootAssembler);
            }
        }


        public void SaveRootIdSignType(FactorySystem factorySystem, int assemblerId, uint signType)
        {
            var index = factorySystem.factory.index;

            if (this.assemblerRootSignTypes[index] == null || assemblerId >= this.assemblerRootSignTypes[index].Length)
            {
                 var array = this.assemblerRootSignTypes[index];
               
                var newCapacity = assemblerId * 2;
                newCapacity = newCapacity > 256 ? newCapacity : 256;
                this.assemblerRootSignTypes[index] = new uint[newCapacity];
                 if (array != null)
                 {
                    var len = array.Length;
                    Array.Copy(array, this.assemblerRootSignTypes[index], (newCapacity <= len) ? newCapacity : len);
                 }
            }
            if (assemblerId != 0)
            {
                this.assemblerRootSignTypes[index][assemblerId] = signType;
            }
        }
            
        public void traceStackUpAndBuild(FactorySystem factorySystem, int assemblerId)
        {
            int entityId = factorySystem.assemblerPool[assemblerId].entityId;
            while(true)
            {
                int assemberId = factorySystem.factory.entityPool[entityId].assemblerId;
                int upperEntityId = 0;
                int num3;
                int num4;
                int upperAssemblerId = 0;
                bool flag;
                factorySystem.factory.ReadObjectConn(entityId, 15, out flag, out num3, out num4);
                if (num3 != 0 && num4 == 14)
                {
                    upperEntityId = num3;
                }
                if (upperEntityId >= 0 && upperEntityId < factorySystem.factory.entityPool.Length)
                {
                    upperAssemblerId = factorySystem.factory.entityPool[upperEntityId].assemblerId;
                }
                else 
                {
                    break;
                }    
               // upperAssemblerId = factorySystem.factory.entityPool[upperEntityId].assemblerId;
                if (upperAssemblerId != 0)
                {                   
                    addAssemblerToStack(factorySystem.factory ,assemberId, upperEntityId);
                    SyncAssemblerFunctions(factorySystem, assemberId);
                    entityId = upperEntityId;
                }
                else
                {
                    break;
                }
            }
        }
//upperEntityId > 0 && 
        public void traceStack(FactorySystem factorySystem, int assemblerId)
        {
            int entityId = factorySystem.assemblerPool[assemblerId].entityId;
            int index = factorySystem.factory.index;
            do
            {
                bool flag;
                int num3;
                int num4;
                int upperEntityId = 0;
                int lowerEntityId = 0;
                int upperAssemblerId = 0;
                factorySystem.factory.ReadObjectConn(entityId, 15, out flag, out num3, out num4);
                if (num3 != 0 && num4 == 14)
                {
                    upperEntityId = num3;
                }
                factorySystem.factory.ReadObjectConn(entityId, 14, out flag, out num3, out num4); 
                if (num3 != 0 && num4 == 15)
                {
                    lowerEntityId = num3;
                }   
                     if (upperEntityId >= 0 && upperEntityId < factorySystem.factory.entityPool.Length)
                    {
                        upperAssemblerId = factorySystem.factory.entityPool[upperEntityId].assemblerId;
                    }
                else
                {
                    break;    
                }     
                //upperAssemblerId = factorySystem.factory.entityPool[upperEntityId].assemblerId;
                if (lowerEntityId == 0 && upperAssemblerId != 0) //check if assembler is the lowest one of a stack by checking bottom and top connections
                {
                    traceStackUpAndBuild(factorySystem, factorySystem.factory.entityPool[entityId].assemblerId);
                    break;
                }
                entityId = upperEntityId;
            }
            while (entityId != 0);
        }

        public void addAssemblerToStack(PlanetFactory __instance, int assemblerId, int nextEntityId)
        {
            var index = __instance.factorySystem.factory.index;
            if (index >= assemblerStackMembers.Length)
            {
                this.SetArrayCapacity(assemblerStackMembersCapacity * 2, ref assemblerStackMembers, ref assemblerStackMembersCapacity);
            }
           
            if (assemblerId != 0 && __instance.factorySystem.assemblerPool[assemblerId].id == assemblerId)
            {
                if (nextEntityId == 0)
                {
                    this.assemblerStacks[index][assemblerStackMembers[index][assemblerId]].Remove(assemblerId);
                    this.assemblerStackMembers[__instance.index][assemblerId] = 0;
                 //   if (this.assemblerStacks[index][assemblerStackMembers[index][assemblerId]].Count == 0)
                 //   {
                 //       this.assemblerStacks[index].Remove(assemblerStackMembers[index][assemblerId]);
                 //   }                   
                    return;
                } 
            
                var nextAssemblerId = __instance.entityPool[nextEntityId].assemblerId;       
                bool flag1 = true;
                if (!this.assemblerStacks.ContainsKey(index))
                {
                    this.assemblerStacks[index] = new(); 
                    this.assemblerStacks[index][assemblerId] = new();
                }

                foreach (var k in this.assemblerStacks[index].Keys)
                {
                    if (this.assemblerStacks[index][k].Contains(assemblerId) && nextAssemblerId != 0)
                    {
                        if (!this.assemblerStacks[index][k].Contains(nextAssemblerId))
                        {    
                            this.assemblerStacks[index][k].Add(nextAssemblerId);
                        }
                        if (this.assemblerStackMembers[index][nextAssemblerId] != k)
                        {
                            this.assemblerStackMembers[index][nextAssemblerId] = k;
                        }
                        flag1 = false;
                        this.SyncAssemblerFunctions(__instance.factorySystem, assemblerId);
                        break;
                    }
                }
                if (flag1)
                {
                    this.assemblerStacks[index][assemblerId] = new HashSet<int>();
                    this.assemblerStacks[index][assemblerId].Add(nextAssemblerId);
                    this.assemblerStackMembers[index][nextAssemblerId] = assemblerId;                 
                    this.SyncAssemblerFunctions(__instance.factorySystem, assemblerId);
                }                   
            
            }
        }

        public void SyncAssemblerFunctions(FactorySystem factorySystem, int assemblerId)
        {

            var index = factorySystem.factory.index;
            if (factorySystem.assemblerPool[assemblerId].entityId == 0)
            {
                return;
            }
            if (! this.assemblerStacks.ContainsKey(index))
            {
                this.assemblerStacks[index] = new();
                this.assemblerStacks[index][assemblerId] = new();
                return;
            }
            
            if(this.assemblerStackMembers[index][assemblerId] != 0 || this.assemblerStacks[index].ContainsKey(assemblerId))
            {

                int rootAssemblerId = 0;
                if(this.assemblerStackMembers[index][assemblerId] != 0)
                {
                    rootAssemblerId = this.assemblerStackMembers[index][assemblerId];
                }            
                else
                {
                    rootAssemblerId = assemblerId;
                }
                int recpId = factorySystem.assemblerPool[rootAssemblerId].recipeId;
                var recipeProto = LDB.recipes.Select(1);
                if (recpId > 0)
		        {
			        recipeProto = LDB.recipes.Select(recpId);
		        }
                
                
                int rootAssemblerEntityId = factorySystem.assemblerPool[rootAssemblerId].entityId;
                var height = this.assemblerStacks[index][rootAssemblerId].Count + 1;
                var assemblerPrefab = LDB.models.Select(factorySystem.factory.entityPool[rootAssemblerEntityId].modelIndex).prefabDesc;
                factorySystem.assemblerPool[rootAssemblerId].timeSpend = recipeProto.TimeSpend* 10000 / height;
                factorySystem.assemblerPool[rootAssemblerId].extraTimeSpend = recipeProto.TimeSpend * 100000 / height;
                factorySystem.factory.powerSystem.consumerPool[factorySystem.factory.entityPool[rootAssemblerEntityId].powerConId].workEnergyPerTick = assemblerPrefab.workEnergyPerTick * height;
                factorySystem.factory.powerSystem.consumerPool[factorySystem.factory.entityPool[rootAssemblerEntityId].powerConId].idleEnergyPerTick = assemblerPrefab.idleEnergyPerTick * height;
                foreach (int stackAssemblerId in this.assemblerStacks[index][rootAssemblerId])
                {
                    var StackAssemblerEntityId = factorySystem.assemblerPool[stackAssemblerId].entityId;
                    //factorySystem.assemblerPool[stackAssemblerId].SetRecipe(recpId, factorySystem.factory.entitySignPool);
                    SaveRootIdSignType(factorySystem, assemblerId, factorySystem.factory.entitySignPool[rootAssemblerEntityId].signType);
                    factorySystem.factory.entitySignPool[StackAssemblerEntityId].iconId0 = factorySystem.factory.entitySignPool[rootAssemblerEntityId].iconId0;
                    factorySystem.factory.entitySignPool[StackAssemblerEntityId].iconType = factorySystem.factory.entitySignPool[rootAssemblerEntityId].iconType;
                    factorySystem.factory.entitySignPool[StackAssemblerEntityId].signType = assemblerRootSignTypes[factorySystem.factory.index][rootAssemblerId];
                    factorySystem.factory.powerSystem.consumerPool[factorySystem.factory.entityPool[StackAssemblerEntityId].powerConId].workEnergyPerTick = 0;
                    factorySystem.factory.powerSystem.consumerPool[factorySystem.factory.entityPool[StackAssemblerEntityId].powerConId].idleEnergyPerTick = 0;                   
                    factorySystem.factory.entityPool[StackAssemblerEntityId].powerConId = 0;
                    factorySystem.factory.powerSystem.RemoveConsumerComponent(factorySystem.factory.powerSystem.consumerPool[factorySystem.assemblerPool[stackAssemblerId].pcId].id);
                    
                }
            }
        }
    }
}


