using System;
using BepInEx;

using BepInEx.Configuration;
using HarmonyLib;

using System.Collections.Generic;
using System.Reflection.Emit;

using System.Linq;


namespace AssemblerVerticalConstruction
{


    public class AssemblerVerticalConstructionConfig
    {
        public int ID = 0;
        public UnityEngine.Vector3 LapJoint;
        public float ColliderDataOffset;
        public AssemblerVerticalConstructionConfig(int id, UnityEngine.Vector3 lapJoint, float colliderDataOffset)
        {
            this.ID = id;
            this.LapJoint = lapJoint;
            this.ColliderDataOffset = colliderDataOffset;
        }
    }
    [BepInPlugin("bifrom.com.DSP.AssemblerVerticalConstruction", "originally by 丰有珏, rewrite by NocturnalScream", "1.0.6")]

    public class AssemblerVerticalConstruction : BaseUnityPlugin
    {

        public static ConfigEntry<bool> IsResetNextIds;
        public static ConfigEntry<string> AssemblerVerticalConstructionJson;
        public static List<AssemblerVerticalConstructionConfig> AssemblerVerticalConstructionConfigs = new List<AssemblerVerticalConstructionConfig>();
        public static AssemblerComponentEx assemblerComponentEx = new AssemblerComponentEx();
        public static BepInEx.Logging.ManualLogSource mylog = BepInEx.Logging.Logger.CreateLogSource("AssemblerVerticalConstruction");
        ~AssemblerVerticalConstruction()
        {
  
            if (IsResetNextIds.Value == true)
            {
                IsResetNextIds.Value = false;
                Config.Save();
            }
        }

    /*    public string AssemblerVerticalConstructionConfigsToString()
        {
            string ret = "\n";
            for (int i = 0; i < AssemblerVerticalConstructionConfigs.Count; i++)
            {
                if (ret != "\n")
                {
                    ret += ",";
                }
                ret += AssemblerVerticalConstructionConfigs[i].ID + "|";
                ret += AssemblerVerticalConstructionConfigs[i].LapJoint.ToString()+"|";
                ret += AssemblerVerticalConstructionConfigs[i].ColliderDataOffset;
            }
            return ret;
        }*/

        void Start()
        {
          /*  AssemblerVerticalConstructionConfigs.Add(new AssemblerVerticalConstructionConfig(2303, new Vector3(0, 15.1f, 0), -3));
            AssemblerVerticalConstructionConfigs.Add(new AssemblerVerticalConstructionConfig(2304, new Vector3(0, 5.1f, 0), -3));
            AssemblerVerticalConstructionConfigs.Add(new AssemblerVerticalConstructionConfig(2305, new Vector3(0, 5.1f, 0), -3));
            AssemblerVerticalConstructionConfigs.Add(new AssemblerVerticalConstructionConfig(2302, new Vector3(0, 4.3f, 0), -3));
            AssemblerVerticalConstructionConfigs.Add(new AssemblerVerticalConstructionConfig(2309, new Vector3(0, 7.0f, 0), -3));
            AssemblerVerticalConstructionConfigs.Add(new AssemblerVerticalConstructionConfig(2308, new Vector3(0, 16.0f, 0), -3));
                 
            IsResetNextIds = Config.Bind("config", "IsResetNextIds", false, "在加载存档的时候重新计算建筑物的叠加关系 true需要重新计算 重新计算的时候会有一定的卡顿");
            AssemblerVerticalConstructionJson = Config.Bind("config", "AssemblerVerticalConstructionJson", AssemblerVerticalConstructionConfigsToString(), "建筑间隔信息 ");
*/          
            Harmony.CreateAndPatchAll(typeof(AssemblerVerticalConstruction));
            AssemblerComponentEx.Init();
        }
        

        

        [HarmonyPatch(typeof(VFPreload), nameof(VFPreload.InvokeOnLoadWorkEnded))]
        [HarmonyPostfix]
        public static void AfterLDBLoad()
        {
         //  CldPatch(); currently not used
        }

        public static void CldPatch()
        {
            List<int> IDs = new List<int>() {2303,2304,2305};
            var ratio = 0.3f;
            foreach(var ID in IDs)
            {
                var AssemblerProto = LDB.items.Select(ID);
                var clds = AssemblerProto.prefabDesc.colliders;
                var bdclds = AssemblerProto.prefabDesc.buildColliders;
                AssemblerProto.prefabDesc.buildCollider.ext.x *= ratio;
                AssemblerProto.prefabDesc.buildCollider.ext.y *= ratio;
                AssemblerProto.prefabDesc.buildCollider.ext.z *= ratio;
                for (int j = 0; j < bdclds.Length; j++)
                {
                    bdclds[j].ext.x *= ratio;
                    bdclds[j].ext.y *= ratio;
                    bdclds[j].ext.z *= ratio;
                }
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(ItemProto), "Preload")]
        private static bool PreloadPatch(ItemProto __instance, int _index)
         {
            ModelProto modelProto = LDB.models.modelArray[__instance.ModelIndex];
            if (modelProto != null && modelProto.prefabDesc != null && modelProto.prefabDesc.isAssembler == true)
             {
                UnityEngine.Vector3 lapJoint = UnityEngine.Vector3.zero;
                if (__instance.ID == 2303 || __instance.ID == 2304 || __instance.ID == 2305)
                {
                    lapJoint = new UnityEngine.Vector3(0, 5.35f, 0);
                }
/*                 else if (__instance.ID == 2302)
                {
                    lapJoint = new Vector3(0, 4.3f, 0);
                }
                else if (__instance.ID == 2309)
                {
                    lapJoint = new Vector3(0, 7.0f, 0);
                }else if (__instance.ID == 2308)
                {
                    lapJoint = new Vector3(0, 16.0f, 0);
                    
                } */
                if (lapJoint != UnityEngine.Vector3.zero)
                {
                    LDB.models.modelArray[__instance.ModelIndex].prefabDesc.multiLevel = true;
                    LDB.models.modelArray[__instance.ModelIndex].prefabDesc.multiLevelAllowPortsOrSlots = true;
                    LDB.models.modelArray[__instance.ModelIndex].prefabDesc.lapJoint = lapJoint;
                }
            }
            return true;
         }

        [HarmonyPrefix, HarmonyPatch(typeof(FactorySystem), "SetAssemblerCapacity")]
        private static bool SetAssemblerCapacityPatch(FactorySystem __instance, int newCapacity)
        {

            var _this = __instance;
            var index = _this.factory.index;
            if (index > assemblerComponentEx.assemblerStackMembers.Length)
            {
                assemblerComponentEx.SetArrayCapacity(assemblerComponentEx.assemblerStackMembersCapacity * 2, ref assemblerComponentEx.assemblerStackMembers, ref assemblerComponentEx.assemblerStackMembersCapacity);
            }
            var assemblerCapacity = _this.assemblerCapacity;
            int[] array = assemblerComponentEx.assemblerStackMembers[index];
            assemblerComponentEx.assemblerStackMembers[index] = new int[newCapacity];
            if (array != null)
            {
                Array.Copy(array, assemblerComponentEx.assemblerStackMembers[index], (newCapacity <= assemblerCapacity) ? newCapacity : assemblerCapacity);
            }
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PlanetFactory), "ApplyInsertTarget")]
        public static bool ApplyInsertTargetPatch(PlanetFactory __instance, ref int entityId, ref int insertTarget, int slotId, int offset)
        {
            var _this = __instance;
            if (entityId == 0)
            {
                return true ;
            }
            if (insertTarget < 0)
            {
                Assert.CannotBeReached();
                insertTarget = 0;
            }
            int assemblerId = _this.entityPool[entityId].assemblerId;
            if (assemblerId > 0 && _this.entityPool[insertTarget].assemblerId > 0)
            {
                assemblerComponentEx.RecalcIds(__instance.factorySystem); 
                //assemblerComponentEx.addAssemblerToStack(__instance, assemblerId, insertTarget);
                //assemblerComponentEx.traceStack(__instance.factorySystem,insertTarget);
            }
             if (assemblerComponentEx.assemblerStackMembers[__instance.factorySystem.factory.index][__instance.entityPool[insertTarget].assemblerId] != 0)
            {
                insertTarget = __instance.factorySystem.assemblerPool[assemblerComponentEx.assemblerStackMembers[__instance.factorySystem.factory.index][__instance.entityPool[insertTarget].assemblerId]].entityId;
            } 
            return true;
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PlanetFactory), nameof(PlanetFactory.ApplyPickTarget))]
        public static void ApplyPickTargetPatch(PlanetFactory __instance, int entityId, ref int pickTarget, int slotId, int offset)
        {
             if (assemblerComponentEx.assemblerStackMembers[__instance.factorySystem.factory.index][__instance.entityPool[pickTarget].assemblerId] != 0)
            {
                pickTarget = __instance.factorySystem.assemblerPool[assemblerComponentEx.assemblerStackMembers[__instance.factorySystem.factory.index][__instance.entityPool[pickTarget].assemblerId]].entityId;
            } 
        }

        [HarmonyPrefix, HarmonyPatch(typeof(PlanetFactory), "ApplyEntityDisconnection")]
        public static bool ApplyEntityDisconnectionPatch(PlanetFactory __instance, int otherEntityId, int removingEntityId, int otherSlotId, int removingSlotId)
        {
            if (otherEntityId == 0)
            {
                return true;
            }
            var _this = __instance;
            int assemblerId = _this.entityPool[otherEntityId].assemblerId;
            if (assemblerId > 0)
            {
                int assemblerId2 = _this.entityPool[removingEntityId].assemblerId;
                if (assemblerId > 0 && assemblerId2 > 0)
                {
                  assemblerComponentEx.RecalcIds(__instance.factorySystem);                  
                  //  var rootId = assemblerComponentEx.assemblerStackMembers[__instance.index][assemblerId2];
                  //  assemblerComponentEx.addAssemblerToStack(__instance, assemblerId2, 0);
                  //  assemblerComponentEx.SyncAssemblerFunctions(__instance.factorySystem, rootId);
                }
            }
            return true;
        }

    [HarmonyTranspiler]
    [HarmonyPatch(typeof(FactorySystem), nameof(FactorySystem.GameTick), typeof(long), typeof(bool), typeof(int), typeof(int), typeof(int))]
    [HarmonyPatch(typeof(FactorySystem), nameof(FactorySystem.GameTick), typeof(long), typeof(bool))]
    static IEnumerable<CodeInstruction> 
    FactorySystemGameTickPatch(IEnumerable<CodeInstruction> code, ILGenerator generator)
    {
        var matcher = new CodeMatcher(code, generator);

        matcher.MatchForward(false
            , new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(AssemblerComponent), nameof(FactorySystem.assemblerPool)))
            , new CodeMatch(OpCodes.Ldloc_S)
            , new CodeMatch(OpCodes.Ldelema)
            , new CodeMatch(OpCodes.Ldfld, AccessTools.Field(typeof(AssemblerComponent), nameof(AssemblerComponent.id))));

        matcher
            .RemoveInstructions(4)
            .Advance(1)
            .InsertAndAdvance(Transpilers.EmitDelegate<Func<FactorySystem,int,bool>>((_this, j) =>
            {
                return FactorySystemGameTickPatch(_this, j);
           
            }))
            .SetOpcodeAndAdvance(OpCodes.Brfalse);

        return matcher.InstructionEnumeration();
    }

public static bool FactorySystemGameTickPatch(FactorySystem _this, int j)
    {

        if (assemblerComponentEx.assemblerStacks.ContainsKey(_this.factory.index) && assemblerComponentEx.assemblerStacks[_this.factory.index].ContainsKey(j))
        { 
            PowerSystem powerSystem = _this.factory.powerSystem;         
            FactoryProductionStat factoryProductionStat = GameMain.statistics.production.factoryStatPool[_this.factory.index];
            AnimData[] entityAnimPool = _this.factory.entityAnimPool;
            PowerConsumerComponent[] consumerPool = powerSystem.consumerPool;
            SignData[] entitySignPool = _this.factory.entitySignPool;
            int[] productRegister = factoryProductionStat.productRegister;
            int[] consumeRegister = factoryProductionStat.consumeRegister;
            int[][] entityNeeds = _this.factory.entityNeeds;
            int entityId2 = _this.assemblerPool[j].entityId;
            float num = 0.016666668f;
            uint num15 = 0U;
            float [] networkServes = powerSystem.networkServes;
            float num16 = networkServes[consumerPool[_this.assemblerPool[j].pcId].networkId];
            if (_this.assemblerPool[j].recipeId != 0)
            {
                int num2 = _this.assemblerPool[j].requires.Length;
                int height = 3*assemblerComponentEx.assemblerStacks[_this.factory.index][j].Count;
                _this.assemblerPool[j].needs[0] = ((0 < num2 && _this.assemblerPool[j].served[0] < _this.assemblerPool[j].requireCounts[0] * height) ? _this.assemblerPool[j].requires[0] : 0);
                _this.assemblerPool[j].needs[1] = ((1 < num2 && _this.assemblerPool[j].served[1] < _this.assemblerPool[j].requireCounts[1] * height) ? _this.assemblerPool[j].requires[1] : 0);
                _this.assemblerPool[j].needs[2] = ((2 < num2 && _this.assemblerPool[j].served[2] < _this.assemblerPool[j].requireCounts[2] * height) ? _this.assemblerPool[j].requires[2] : 0);
                _this.assemblerPool[j].needs[3] = ((3 < num2 && _this.assemblerPool[j].served[3] < _this.assemblerPool[j].requireCounts[3] * height) ? _this.assemblerPool[j].requires[3] : 0);
                _this.assemblerPool[j].needs[4] = ((4 < num2 && _this.assemblerPool[j].served[4] < _this.assemblerPool[j].requireCounts[4] * height) ? _this.assemblerPool[j].requires[4] : 0);
                _this.assemblerPool[j].needs[5] = ((5 < num2 && _this.assemblerPool[j].served[5] < _this.assemblerPool[j].requireCounts[5] * height) ? _this.assemblerPool[j].requires[5] : 0);
                num15 = _this.assemblerPool[j].InternalUpdate(num16, productRegister, consumeRegister);
            }
            entityAnimPool[entityId2].Step(num15, num * num16);
            entityAnimPool[entityId2].power = num16;
            entityNeeds[entityId2] = _this.assemblerPool[j].needs;
            if (entitySignPool[entityId2].signType == 0U || entitySignPool[entityId2].signType > 3U)
            {
                uint signType = ((_this.assemblerPool[j].recipeId == 0) ? 4U : ((num15 > 0U) ? 0U : 6U));
                assemblerComponentEx.SaveRootIdSignType(_this, j, signType);
                entitySignPool[entityId2].signType = signType;
            }
                return false;
        }
                else if (assemblerComponentEx.assemblerStackMembers[_this.factory.index]!= null && assemblerComponentEx.assemblerStackMembers[_this.factory.index][j] != 0)
        {
            AnimData[] entityAnimPool = _this.factory.entityAnimPool;
            int entityId2 = _this.assemblerPool[j].entityId;
            int rootAssemblerId = assemblerComponentEx.assemblerStackMembers[_this.factory.index][j];
            int entityRootId = _this.assemblerPool[rootAssemblerId].entityId;

            uint rootIdSignType = assemblerComponentEx.assemblerRootSignTypes[_this.factory.index][rootAssemblerId];
            _this.factory.entitySignPool[entityId2].signType = rootIdSignType; 

            entityAnimPool[entityId2].state = entityAnimPool[entityRootId].state;
            entityAnimPool[entityId2].power = entityAnimPool[entityRootId].power;
            entityAnimPool[entityId2].time = entityAnimPool[entityRootId].time;
            
            return false;
        }
        return _this.assemblerPool[j].id == j;
    }            
                
        [HarmonyPostfix, HarmonyPatch(typeof(UIAssemblerWindow), "OnRecipeResetClick")]
        public static void OnRecipeResetClickPatch(UIAssemblerWindow __instance)
        {
            if (__instance.assemblerId == 0 || __instance.factory == null)
            {
                return;
            }
            AssemblerComponent assemblerComponent = __instance.factorySystem.assemblerPool[__instance.assemblerId];
            if (assemblerComponent.id != __instance.assemblerId)
            {
                return;
            }
            if (assemblerComponentEx.assemblerStacks[__instance.factory.index].ContainsKey(assemblerComponent.id))
            {
                assemblerComponentEx.SyncAssemblerFunctions(__instance.factorySystem, __instance.assemblerId);
            }
            ;
            
        }

        [HarmonyPostfix, HarmonyPatch(typeof(PlanetFactory), "PasteBuildingSetting")]
        public static void PasteBuildingSettingPatch(PlanetFactory __instance, int objectId)
        {
            if (objectId <= 0)
            {
                return;
            }
            BuildingParameters clipboard = BuildingParameters.clipboard;
            int assemblerId = __instance.entityPool[objectId].assemblerId;
            
            if (assemblerId != 0 && clipboard.type == BuildingType.Assembler && __instance.factorySystem.assemblerPool[assemblerId].recipeId == clipboard.recipeId)
            {
                ItemProto itemProto = LDB.items.Select((int)__instance.entityPool[objectId].protoId);
                if (itemProto != null && itemProto.prefabDesc != null)
                {
                    assemblerComponentEx.SyncAssemblerFunctions(__instance.factorySystem,  assemblerId);
                }
            }
            
        }
        [HarmonyPrefix, HarmonyPatch(typeof(UIAssemblerWindow), nameof(UIAssemblerWindow.OnAssemblerIdChange))]
        public static void OnAssemblerIdChangePatch(UIAssemblerWindow __instance)
        {
            var _this = __instance;
            if (_this.active)
            {
                if (_this.assemblerId == 0 || _this.factory == null)
                {
                    _this._Close();
                    return;
                }
                AssemblerComponent assemblerComponent = _this.factorySystem.assemblerPool[_this.assemblerId];
                if (assemblerComponent.id != _this.assemblerId)
                {
                    _this._Close();
                    return;
                }
                
                var rootAssemblerId = assemblerComponentEx.assemblerStackMembers[_this.factorySystem.factory.index][_this.assemblerId];
                if (rootAssemblerId != 0)
                {
                    _this._assemblerId = rootAssemblerId;
                }
            }
        }

        [HarmonyPrefix, HarmonyPatch(typeof(EntityBriefInfo), nameof(EntityBriefInfo.SetBriefInfo))]
        public static void SetBriefInfoPatch(PlanetFactory _factory,ref int _entityId)
        {
        if (_factory == null)
		{
			return;
		}
		if (_entityId == 0)
		{
			return;
		}
        if (_factory.entityPool[_entityId].assemblerId != 0 && assemblerComponentEx.assemblerStackMembers[_factory.index][_factory.entityPool[_entityId].assemblerId] != 0)
        {
            _entityId = _factory.factorySystem.assemblerPool[assemblerComponentEx.assemblerStackMembers[_factory.index][_factory.entityPool[_entityId].assemblerId]].entityId;
        }
        }

        [HarmonyPostfix, HarmonyPatch(typeof(UIAssemblerWindow), nameof(UIAssemblerWindow.OnRecipePickerReturn))]
        public static void OnRecipePickerReturnPatch(UIAssemblerWindow __instance)
        {
            if (__instance.assemblerId == 0 || __instance.factory == null)
            {
                return;
            }
            if (! assemblerComponentEx.assemblerStacks.ContainsKey(__instance.factory.index))
            {
                assemblerComponentEx.assemblerStacks[__instance.factory.index] = new();
                assemblerComponentEx.assemblerStacks[__instance.factory.index][__instance.assemblerId] = new();
            }
            AssemblerComponent assemblerComponent = __instance.factorySystem.assemblerPool[__instance.assemblerId];
            if (assemblerComponent.id != __instance.assemblerId)
            {
                return;
            }
            if (assemblerComponentEx.assemblerStacks[__instance.factory.index].ContainsKey(assemblerComponent.id))
            {
                assemblerComponentEx.SyncAssemblerFunctions(__instance.factorySystem, __instance.assemblerId);
            }         
        }

        [HarmonyPostfix, HarmonyPatch(typeof(BuildingParameters), nameof(BuildingParameters.ApplyPrebuildParametersToEntity))]
        public static void ApplyPrebuildParametersToEntityPatch(BuildingParameters[] __instance, int entityId, int recipeId, int filterId, int[] parameters, PlanetFactory factory)
        {
            FactorySystem factorySystem = factory.factorySystem;
            int assemblerId = factory.entityPool[entityId].assemblerId;

            if (assemblerComponentEx.assemblerStacks.ContainsKey(factory.index) && assemblerComponentEx.assemblerStacks[factory.index].ContainsKey(assemblerId))
            {
                    assemblerComponentEx.SyncAssemblerFunctions(factorySystem, assemblerId);           
            }
        }

        [HarmonyTranspiler] 
        [HarmonyPatch(typeof(BuildTool_Click), nameof(BuildTool_Click.CheckBuildConditions))]
        [HarmonyPatch(typeof(BuildTool_BlueprintPaste), nameof(BuildTool_BlueprintPaste.CheckBuildConditions))]
        public static IEnumerable<CodeInstruction> CheckBuildConditionsPatch(IEnumerable<CodeInstruction> instructions)
        {
            var codes = new List<CodeInstruction>(instructions);

            // Find the index of the line we want to match
            int index = codes.FindIndex(ins => ins.opcode == OpCodes.Ldfld && ins.operand.ToString().Contains("desc.isSplitter"));

            if (index >= 0)
            {
                // Insert the new condition after the matched line
                codes.Insert(index + 1, new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BuildPreview), nameof(PrefabDesc.isAssembler))));
                codes.Insert(index + 2, new CodeInstruction(OpCodes.Or));
            }

            return codes.AsEnumerable();
        }
      
      
      
       /*  static IEnumerable<CodeInstruction> CheckBuildConditionsPatch(IEnumerable<CodeInstruction> instructions)
        {
            List<CodeInstruction> instructionList = new List<CodeInstruction>(instructions);

            for (int i = 0; i < instructionList.Count; i++)
            {
                CodeInstruction instruction = instructionList[i];
                if (instruction.opcode == OpCodes.Brtrue
                    && instruction.operand is Label label
                    && instructionList[i + 1].opcode == OpCodes.Ldarg_0
                    && instructionList[i + 2].opcode == OpCodes.Ldfld
                    && instructionList[i + 2].operand is FieldInfo fieldInfo
                    && fieldInfo.Name == "desc"
                    && instructionList[i + 3].opcode == OpCodes.Ldfld
                    && instructionList[i + 3].operand is FieldInfo nestedFieldInfo
                    && nestedFieldInfo.Name == "isSplitter")
                {
                    instructionList[i + 3] = new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(BuildPreview), nameof(PrefabDesc.isAssembler)));
                    instructionList.Insert(i + 4, new CodeInstruction(OpCodes.Brtrue, label));
                    break;
                }
            }
            return instructionList;
        } */
     // needed later to check for Illegal blueprints due to not researched stack technology - not yet implemented

 /*        [HarmonyPrefix, HarmonyPatch(typeof(GameHistoryData), "get_buildMaxHeight")]
        public static bool get_buildMaxHeightPatch(GameHistoryData __instance, ref float __result)
        {
        __result = (float)(__instance.labLevel * 4 + 12);
        return false;
        } */ //needed if level height needs to be adjusted

        
        [HarmonyPostfix, HarmonyPatch(typeof(FactorySystem), nameof(FactorySystem.Import))]
        public static void FactorySystemImportPostfixPatch(FactorySystem __instance)
        {
            assemblerComponentEx.RecalcIdsOnLoad();
            for (int k = 1; k < __instance.inserterCursor; k++)
		    {

                if (assemblerComponentEx.assemblerStackMembers[__instance.factory.index] != null)
                {
                    if (assemblerComponentEx.assemblerStackMembers[__instance.factory.index][__instance.factory.entityPool[__instance.inserterPool[k].insertTarget].assemblerId] != 0)
                    {
                        __instance.inserterPool[k].insertTarget = __instance.assemblerPool[assemblerComponentEx.assemblerStackMembers[__instance.factory.index][__instance.factory.entityPool[__instance.inserterPool[k].insertTarget].assemblerId]].entityId;
                    }
                    if (assemblerComponentEx.assemblerStackMembers[__instance.factory.index][__instance.factory.entityPool[__instance.inserterPool[k].pickTarget].assemblerId] != 0)
                    {
                        __instance.inserterPool[k].pickTarget = __instance.assemblerPool[assemblerComponentEx.assemblerStackMembers[__instance.factory.index][__instance.factory.entityPool[__instance.inserterPool[k].pickTarget].assemblerId]].entityId;
                    }
                }
            } 
        } 
    }
}