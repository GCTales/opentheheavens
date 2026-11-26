using HarmonyLib;
using Mono.Cecil.Cil;
using MonoMod.Cil;
using RoR2;
using RoR2.Orbs;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace Open_The_Heavens
{
    public static class HexTweaks
    {
        private static float _ambientLevelsPerHex = 1.5f;

        public static float ambientLevelsPerHex
        {
            get {  return _ambientLevelsPerHex; }
            set { _ambientLevelsPerHex = value; }
        }
        private static Inventory YoinkedDeathInventory {  get; set; }
        private static HashSet<Interactor> CachedDeathInteractors  = new HashSet<Interactor>();

        internal static void Init()
        {
            IL.RoR2.Run.RecalculateDifficultyCoefficentInternal += HexAmbientLevelIncrease;
            On.EntityStates.HallowedCauldron.DrainingItems.TakeItemServer += CleanseHex;
            On.RoR2.PurchaseInteraction.OnEnable += DisableSaleStarReliquary;
            if (ConfigManager.EnableForceHexPickupGlobal.Value)
            {
                if (ConfigManager.EnableForceHexPickupCauldron.Value)
                {
                    IL.EntityStates.HallowedCauldron.GrantHexes.FixedUpdate += HallowedShrineFixedUpdate;
                    IL.EntityStates.HallowedCauldron.GrantHexes.SpawnHex += HexFromHallowedShrine;
                    IL.EntityStates.HallowedCauldron.GrantHexes.OnExit += KeepDeathOpen;
                    Stage.onStageStartGlobal += StageStartReset;
                    On.RoR2.GrantHexesController.OnInteractionBegin += YoinkDeathInteractor;
                    On.RoR2.GrantHexesController.GetInteractability += ForbidRepeatDeath;
                }
                On.RoR2.ChestBehavior.Open += MakeReliquarySpawnOrb;
                On.RoR2.PurchaseInteraction.OnInteractionBegin += CacheReliquaryInteractor;
            }
            On.RoR2.DrainItemsAndDropMultiChoicePotentialController.OnEnable += ReduceHallowedShrineDrain;
            ReadConfig_Gameplay();
        }

        private static void ReduceHallowedShrineDrain(On.RoR2.DrainItemsAndDropMultiChoicePotentialController.orig_OnEnable orig, DrainItemsAndDropMultiChoicePotentialController self)
        {

            orig(self);
            self.maxItemsToTake = 5;
        }

        private static void DisableSaleStarReliquary(On.RoR2.PurchaseInteraction.orig_OnEnable orig, PurchaseInteraction self)
        {
            orig(self);
            if (self.name == "HollowChestFree(Clone)"&& ConfigManager.DisableSaleStarReliquary.Value)
            {
                self.saleStarCompatible = false;
            }
        }

        internal static void LoadAssets()
        {
            if (ConfigManager.RestrictReliquaryPerStage.Value)
            {
                InteractableSpawnCard isc_hexReliquary = Addressables.LoadAssetAsync<InteractableSpawnCard>("RoR2/DLC4/HollowChest/iscHollowChest.asset").WaitForCompletion();
                isc_hexReliquary.maxSpawnsPerStage = 1;
            }
            //remove pages from Hallow and hallow from White/Green
            if (ConfigManager.RemoveErroneousCauldronTiers.Value)
            {
                BasicPickupDropTable bdt_handLife_hallow = Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/DLC4/bdtHallowedCauldron_HighGrade.asset").WaitForCompletion();
                bdt_handLife_hallow.pageWeight = 0;
                BasicPickupDropTable bdt_handLife_white = Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/DLC4/bdtHallowedCauldron_LowGrade.asset").WaitForCompletion();
                bdt_handLife_white.cursedWeight = 0;
                BasicPickupDropTable bdt_handLife_green = Addressables.LoadAssetAsync<BasicPickupDropTable>("RoR2/DLC4/bdtHallowedCauldron_MediumGrade.asset").WaitForCompletion();
                bdt_handLife_green.cursedWeight = 0;
            }
            
        }

        private static void CacheReliquaryInteractor(On.RoR2.PurchaseInteraction.orig_OnInteractionBegin orig, PurchaseInteraction self, Interactor activator)
        {
            if (!ConfigManager.EnableForceHexPickupReliquary.Value) { orig(self,activator); return; }
            if (self.name == "HollowChestFree(Clone)" && self.TryGetComponent<ChestBehavior>(out var chestBehaviour))
            {
                chestBehaviour.cachedInteractorForOrbTransfer = activator;
            }
            orig(self,activator);
        }

        private static void MakeReliquarySpawnOrb(On.RoR2.ChestBehavior.orig_Open orig, ChestBehavior self)
        {
            if(!ConfigManager.EnableForceHexPickupReliquary.Value) { orig(self);return; }
            if (self.name == "HollowChestFree(Clone)")
            {
                self.OpenAndGrantViaOrb(self.cachedInteractorForOrbTransfer);
                return;
            }
            orig(self);
        }

        private static void StageStartReset(Stage obj)
        {
            CachedDeathInteractors.Clear();
        }

        private static Interactability ForbidRepeatDeath(On.RoR2.GrantHexesController.orig_GetInteractability orig, GrantHexesController self, Interactor activator)
        {
            if (orig(self, activator) == Interactability.Available)
            {
                return CachedDeathInteractors.Contains(activator) ? Interactability.ConditionsNotMet : Interactability.Available;
            }
            return Interactability.Disabled;
        }

        private static void YoinkDeathInteractor(On.RoR2.GrantHexesController.orig_OnInteractionBegin orig, GrantHexesController self, Interactor activator)
        {
            YoinkedDeathInventory = activator.GetComponent<CharacterBody>().inventory;
            CachedDeathInteractors.Add(activator);
            orig(self, activator);
        }

        private static void KeepDeathOpen(ILContext il)
        {
            ILCursor c = new ILCursor(il);
            //stop hand of death from closing itself
            if (c.TryGotoNext( 
                //MoveType.After,
                x => x.MatchCallOrCallvirt<HallowingShrineSplitNode>(nameof(HallowingShrineSplitNode.TryCloseAllHands))))
            {
                //Instruction nextInstruction = c.Next;
                //c.Index--;
                //c.Emit<HallowingShrineSplitNode>(OpCodes.Call, nameof(HallowingShrineSplitNode.TryCloseOtherHands));
                //c.Emit(OpCodes.Br, nextInstruction);

                //c.Prev.Operand = AccessTools.Method(typeof(HallowingShrineSplitNode), nameof(HallowingShrineSplitNode.TryCloseOtherHands));

                c.Emit(OpCodes.Dup);
                c.Index++;
                c.Emit(OpCodes.Callvirt, AccessTools.Method(typeof(HallowingShrineSplitNode),nameof(HallowingShrineSplitNode.Open)));
            } else
            {
                Log.Error("Failed IL Hook: " + il.Method.Name + " - Failed to locate HallowingShrineSplitNode.TryCloseAllHands");
            }
        }


        private static void HexFromHallowedShrine(ILContext il)
        {
            //replace the pickup with ItemTransferOrb.DispatchItemTransferOrb()
            ILCursor c = new ILCursor(il);
            Instruction nextInstruction;
            if (c.TryGotoNext(MoveType.After,
                x => x.MatchCallOrCallvirt(AccessTools.Method(typeof(PickupDropletController),nameof(PickupDropletController.CreatePickupDroplet), new Type[] { typeof(GenericPickupController.CreatePickupInfo), typeof(Vector3), typeof(Vector3) }))))
            {
                c.Emit(OpCodes.Ldloc, 2);
                nextInstruction = c.Prev;
                c.Emit(OpCodes.Ldloc, 0);
                c.Emit(OpCodes.Call, AccessTools.PropertyGetter(typeof(HexTweaks), nameof(HexTweaks.YoinkedDeathInventory)));
                c.EmitDelegate<Func<Vector3, UniquePickup, Inventory, ItemTransferOrb>>((origin, pickup, cachedInventory) =>
                {
                    return ItemTransferOrb.DispatchItemTransferOrb(new ItemTransferOrb.DispatchArgs
                    {
                        origin = origin,
                        inventoryToGrantTo = cachedInventory,
                        itemIndex = PickupCatalog.GetPickupDef(pickup.pickupIndex).itemIndex,
                        permanentCount = 1,
                        isPickup = true,
                        travelDuration = 1f
                    });
                });
                c.Emit(OpCodes.Pop);
                if (c.TryGotoPrev(
                    x => x.MatchLdloca(5),
                    x => x.MatchInitobj("RoR2.GenericPickupController/CreatePickupInfo")))
                {
                    c.Emit(OpCodes.Br_S, nextInstruction);
                }else
                {
                    Log.Error("Failed IL Hook: " + il.Method.Name + " - Failed to locate ldloca.s 5 and initobj");
                }
            } else
            {
                Log.Error("Failed IL Hook: " + il.Method.Name + " - Failed to locate PickupDropletController.CreatePickupDroplet");
            }
        }

        private static void HallowedShrineFixedUpdate(ILContext il)
        {
            //Remove logic spawning hexes per player to instead spawn once each time.
            ILCursor c = new ILCursor(il);
            int startIndex = -1;
            Instruction nextInstruction;
            if (c.TryGotoNext(
                MoveType.After,
                x => x.MatchCallOrCallvirt(typeof(EntityStates.EntityState),nameof (EntityStates.EntityState.FixedUpdate))))
            {
                startIndex = c.Index;
                if (c.TryGotoNext(
                    x => x.MatchStloc(0)))
                {
                    c.Emit(OpCodes.Pop);
                    c.Emit(OpCodes.Ldc_I4_1);
                    nextInstruction = c.Prev;
                    c.Index = startIndex;
                    c.Emit(OpCodes.Br_S, nextInstruction);
                }

            } else
            {
                Log.Error("Failed IL Hook: " + il.Method.Name + " - Failed to locate base.OnExit");
            }
        }

        internal static void ReadConfig_Gameplay()
        {
            ambientLevelsPerHex = ConfigManager.HexLevelFactor.Value;
            Log.Info("Hex Level Factor set to " +  ambientLevelsPerHex);
        }
        private static void CleanseHex(On.EntityStates.HallowedCauldron.DrainingItems.orig_TakeItemServer orig, EntityStates.HallowedCauldron.DrainingItems self)
        {
            orig(self);
            if (ConfigManager.EnableHexAmbientLevel.Value)
            {
                RunTracker.teamCleansedHexes++;
            }
        }

        private static void HexAmbientLevelIncrease(ILContext il)
        {
            //increase ambient level by Ceil(RunTracker.teamCleansedHexes * HexTweaks.ambientLevelsPerHex
            ILCursor c = new ILCursor(il);
            if (c.TryGotoNext(
                //x => x.MatchCallOrCallvirt(AccessTools.Method(typeof(Mathf),nameof(Mathf.FloorToInt))),
                x => x.MatchCallOrCallvirt(AccessTools.PropertySetter(typeof(Run),nameof(Run.ambientLevelFloor)))))
            {
                c.Emit(OpCodes.Call, AccessTools.PropertyGetter(typeof(RunTracker),nameof(RunTracker.teamCleansedHexes)));
                c.Emit(OpCodes.Call, AccessTools.PropertyGetter(typeof(HexTweaks), nameof(HexTweaks.ambientLevelsPerHex)));
                c.Emit(OpCodes.Mul);
                c.Emit(OpCodes.Call, AccessTools.Method(typeof(Mathf), nameof(Mathf.CeilToInt)));
                c.Emit(OpCodes.Add);
            } else
            {
                Log.Error("Failed IL Hook: " + il.Method.Name + " - Failed to locate Run.set_ambientLevelFloor");
            }
        }
    }
}
