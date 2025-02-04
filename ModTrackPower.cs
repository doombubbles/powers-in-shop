using Il2CppAssets.Scripts.Models.Audio;
using Il2CppAssets.Scripts.Models.GenericBehaviors;
using Il2CppAssets.Scripts.Models.Map;
using Il2CppAssets.Scripts.Models.Powers.Effects;
using Il2CppAssets.Scripts.Models.Towers;
using Il2CppAssets.Scripts.Models.Towers.Behaviors;
using Il2CppAssets.Scripts.Models.Towers.Projectiles;
using Il2CppAssets.Scripts.Simulation.Towers;
using Il2CppAssets.Scripts.Simulation.Track;
using Il2CppAssets.Scripts.Unity;
using Il2CppAssets.Scripts.Unity.UI_New;
using Il2CppAssets.Scripts.Unity.UI_New.InGame;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.RightMenu;
using BTD_Mod_Helper.Extensions;
using HarmonyLib;
using Il2CppAssets.Scripts;
using Il2CppAssets.Scripts.Models;
using Il2CppAssets.Scripts.Models.Towers.Behaviors.Emissions;
using Il2CppAssets.Scripts.Unity.UI_New.InGame.StoreMenu;
using Il2CppNinjaKiwi.Common.ResourceUtils;
using UnityEngine;
using UnityEngine.UI;
using CreateEffectOnExpireModel = Il2CppAssets.Scripts.Models.Towers.Behaviors.CreateEffectOnExpireModel;
using Vector2 = Il2CppAssets.Scripts.Simulation.SMath.Vector2;

namespace PowersInShop;

public abstract class ModTrackPower : ModPowerTower
{
    protected abstract int Pierce { get; }

    public override string BaseTower => TowerType.NaturesWardTotem;

    public override void ModifyBaseTowerModel(TowerModel towerModel)
    {
        var powerModel = Game.instance.model.GetPowerWithName(Name);

        towerModel.footprint = new CircleFootprintModel("", 0, true, false, true);
        towerModel.radius = 3;
        towerModel.range = 0;
        towerModel.showPowerTowerBuffs = false;

        towerModel.GetBehavior<CreateEffectOnExpireModel>().effectModel =
            powerModel.GetBehavior<CreateEffectOnPowerModel>().effectModel;

        var assetId = powerModel.GetBehavior<CreateSoundOnPowerModel>().sound.assetId;
        var createSoundOnTowerPlaceModel = towerModel.GetBehavior<CreateSoundOnTowerPlaceModel>();
        createSoundOnTowerPlaceModel.sound1.assetId = assetId;
        createSoundOnTowerPlaceModel.sound2.assetId = assetId;
        createSoundOnTowerPlaceModel.heroSound1 = new SoundModel("", new AudioClipReference(""));
        createSoundOnTowerPlaceModel.heroSound2 = new SoundModel("", new AudioClipReference(""));

        towerModel.display = towerModel.GetBehavior<DisplayModel>().display = CreatePrefabReference("");

        var projectile = powerModel.GetDescendant<ProjectileModel>().Duplicate();

        towerModel.AddBehavior(new CreateProjectileOnTowerDestroyModel("", projectile,
            new SingleEmissionModel("", null), true, false, false));

        towerModel.RemoveBehaviors<SlowBloonsZoneModel>();
        towerModel.RemoveBehaviors<SavedSubTowerModel>();
    }

    public override void ModifyTowerModelForMatch(TowerModel towerModel, GameModel gameModel)
    {
        var basePowerProj = Game.instance.model.GetPowerWithName(Name).GetDescendant<ProjectileModel>();
        var currentPowerProj = gameModel.GetPowerWithName(Name).GetDescendant<ProjectileModel>();
        var createProjectile = towerModel.GetDescendant<CreateProjectileOnTowerDestroyModel>();
        var projectile = createProjectile.projectileModel;

        var newProjectile = currentPowerProj.Duplicate();

        newProjectile.pierce = Pierce * currentPowerProj.pierce / basePowerProj.pierce;

        createProjectile.RemoveChildDependant(projectile);
        createProjectile.projectileModel = newProjectile;
        createProjectile.AddChildDependant(newProjectile);
    }

    internal static void OnUpdate()
    {
        if (InGame.instance == null) return;

        var inputManager = InGame.instance.inputManager;
        var towerModel = inputManager?.towerModel;

        if (towerModel != null && inputManager!.inPlacementMode && towerModel.GetModTower() is ModTrackPower)
        {
            var map = InGame.instance.bridge.simulation.Map;
            InGameObjects.instance.IconUpdate(InputManager.GetCursorPosition(),
                map.CanPlace(new Vector2(inputManager.cursorPositionWorld), towerModel));
        }
    }

    [HarmonyPatch(typeof(InputManager), nameof(InputManager.EnterPlacementMode), typeof(TowerModel),
        typeof(InputManager.PositionDelegate), typeof(ObjectId), typeof(bool), typeof(int))]
    internal class InputManager_EnterPlacementMode
    {
        [HarmonyPostfix]
        internal static void Postfix(TowerModel forTowerModel)
        {
            if (forTowerModel.GetModTower() is ModTrackPower trackPower)
            {
                var image = ShopMenu.instance
                    .GetTowerButtonFromBaseId(trackPower.Id)
                    .gameObject.transform
                    .Find("Icon").GetComponent<Image>();

                InGameObjects.instance.IconUpdate(new UnityEngine.Vector2(-3000, 0), false);
                InGameObjects.instance.IconStart(image.sprite);
            }
        }
    }

    [HarmonyPatch(typeof(InputManager), nameof(InputManager.ExitPlacementMode))]
    internal class InputManager_ExitPlacementMode
    {
        [HarmonyPostfix]
        internal static void Postfix()
        {
            if (InGameObjects.instance.powerIcon != null)
            {
                InGameObjects.instance.IconEnd();
            }
        }
    }

    [HarmonyPatch(typeof(Map), nameof(Map.CanPlace))]
    internal class Map_CanPlace
    {
        [HarmonyPostfix]
        internal static void Patch(Vector2 at, TowerModel tm, ref bool __result)
        {
            if (tm.GetModTower() is ModTrackPower trackPower)
            {
                __result = InGame.Bridge.CheckPowerLocation(at.ToUnity(), trackPower.PowerModel);
            }
        }
    }

    [HarmonyPatch(typeof(Tower), nameof(Tower.OnDestroy))]
    public class Tower_OnDestroy
    {
        [HarmonyPrefix]
        public static void Prefix(Tower __instance)
        {
            if (__instance.towerModel?.GetModTower() is ModTrackPower trackPower &&
                (!InGame.instance.IsCoop || __instance.owner == Game.instance.GetNkGI().PeerID) &&
                (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
            {
                ShopMenu.instance.GetTowerButtonFromBaseId(trackPower.Id).gameObject
                    .GetComponent<TowerPurchaseButton>()
                    .ButtonActivated();
            }
        }
    }
}