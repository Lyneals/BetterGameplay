using BepInEx;
using RoR2;
using UnityEngine;

namespace BetterGameplay
{
    [BepInPlugin(GUID, Name, Version)]
    public class BetterGameplayPlugin : BaseUnityPlugin
    {
        public const string GUID = "com.xoxfaby.BetterGameplay";
        public const string Name = "BetterGameplay";
        public const string Version = "1.1.3";

        static void MapZone_Awake(On.RoR2.MapZone.orig_Awake orig, MapZone self)
        {
            orig(self);
            if (self.zoneType == MapZone.ZoneType.OutOfBounds) self.gameObject.layer = 29;
        }

        static void MapZone_TryZoneStart(On.RoR2.MapZone.orig_TryZoneStart orig, MapZone self, Collider collider)
        {
            if (self.zoneType == MapZone.ZoneType.OutOfBounds)
            {
                if (collider.GetComponent<PickupDropletController>() || collider.GetComponent<GenericPickupController>())
                {
                    SpawnCard spawnCard = ScriptableObject.CreateInstance<SpawnCard>();
                    spawnCard.hullSize = HullClassification.Human;
                    spawnCard.nodeGraphType = RoR2.Navigation.MapNodeGroup.GraphType.Ground;
                    spawnCard.prefab = Resources.Load<GameObject>("SpawnCards/HelperPrefab");
                    GameObject gameObject = DirectorCore.instance.TrySpawnObject(new DirectorSpawnRequest(spawnCard, new DirectorPlacementRule
                    {
                        placementMode = DirectorPlacementRule.PlacementMode.NearestNode,
                        position = collider.transform.position
                    }, RoR2Application.rng));
                    if (gameObject)
                    {
                        TeleportHelper.TeleportGameObject(collider.gameObject, gameObject.transform.position);
                        UnityEngine.Object.Destroy(gameObject);
                    }
                    UnityEngine.Object.Destroy(spawnCard);
                }
            }

            orig(self, collider);
        }
        static bool EquipmentSlot_ExecuteIfReady(On.RoR2.EquipmentSlot.orig_ExecuteIfReady orig, EquipmentSlot self)
        {
            if ((self.inventory != null) && self.inventory.GetItemCount(ItemCatalog.FindItemIndex("AutoCastEquipment")) > 0)
            {
                if (SceneInfo.instance.sceneDef.nameToken == "MAP_BAZAAR_TITLE")
                {
                    return false;
                }
            }
            return orig(self);
        }
        static void Inventory_UpdateEquipment(On.RoR2.Inventory.orig_UpdateEquipment orig, Inventory self)
            {

                if (self.GetItemCount(ItemCatalog.FindItemIndex("AutoCastEquipment")) > 0)
                {
                    if (SceneInfo.instance.sceneDef.nameToken == "MAP_BAZAAR_TITLE")
                    {
                        for(int i = 0; i<self.equipmentStateSlots.Length; i++)
                        {
                            self.equipmentStateSlots[i].chargeFinishTime.t += Time.deltaTime;
                        }
                        return;
                    }
                }
                orig(self);
            }
        public void Awake()
        {
            Log.Init(Logger);
            for (int i = 0; i < 32; i++)
            {
                Physics.IgnoreLayerCollision(29, i, Physics.GetIgnoreLayerCollision(15, i)) ;
            }
            Physics.IgnoreLayerCollision(29, 8, false);
            Physics.IgnoreLayerCollision(29, 13, false);

            On.RoR2.MapZone.Awake += MapZone_Awake;
            On.RoR2.MapZone.TryZoneStart += MapZone_TryZoneStart;
            On.RoR2.EquipmentSlot.ExecuteIfReady += EquipmentSlot_ExecuteIfReady;
            On.RoR2.Inventory.UpdateEquipment += Inventory_UpdateEquipment;
        }
    }
}
