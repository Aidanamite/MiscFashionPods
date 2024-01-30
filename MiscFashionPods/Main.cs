using HarmonyLib;
using SRML;
using SRML.Console;
using SRML.SR;
using SRML.SR.Translation;
using SRML.Utils.Enum;
using AssetsLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Console = SRML.Console.Console;
using Object = UnityEngine.Object;
using static AssetsLib.TextureUtils;

namespace MiscFashionPods
{
    public class Main : ModEntryPoint
    {
        internal static Assembly modAssembly = Assembly.GetExecutingAssembly();
        internal static string modName = $"{modAssembly.GetName().Name}";
        internal static string modDir = $"{System.Environment.CurrentDirectory}\\SRML\\Mods\\{modName}";
        internal static GameObject _fp;
        internal static GameObject FashionPrefab
        {
            get
            {
                if (!_fp)
                    _fp = GameContext.Instance.LookupDirector.GetPrefab(Identifiable.Id.CLIP_ON_FASHION);
                return _fp;
            }
        }
        internal static GameObject _fpp;
        internal static GameObject FashionPodPrefab
        {
            get
            {
                if (!_fpp)
                    _fpp = GameContext.Instance.LookupDirector.GetGadgetDefinition(Gadget.Id.FASHION_POD_CLIP_ON).prefab;
                return _fpp;
            }
        }
        public static Dictionary<Fashion.Slot, (string, SlimeAppearance.SlimeBone)> customSlotAttachments = new Dictionary<Fashion.Slot, (string, SlimeAppearance.SlimeBone)>();
        public static Dictionary<Identifiable.Id, Func<AttachFashions, Vector3>> customFashionOffset = new Dictionary<Identifiable.Id, Func<AttachFashions, Vector3>>();

        internal static List<(string, Gadget.Id, Identifiable.Id, string, string, SRModLoader.LoadingStep, Func<(Fashion.Slot, Sprite, Color, GameObject, SudoGadgetDefinition)>)> fashions =
            new List<(string, Gadget.Id, Identifiable.Id, string, string, SRModLoader.LoadingStep, Func<(Fashion.Slot, Sprite, Color, GameObject, SudoGadgetDefinition)>)>
        {
            ("BEATRIX",default,default,"Beatrix Fashion Pod",null, SRModLoader.LoadingStep.LOAD, () => {
                var p = Resources.FindObjectsOfTypeAll<SkinnedMeshRenderer>().First((y) => y.name == "hair");
                var prefab = new GameObject("").CreatePrefab();
                prefab.transform.localScale = Vector3.one * 3f;
                prefab.name = "fashion_hair";
                prefab.AddComponent<MeshFilter>().sharedMesh = p.sharedMesh;
                prefab.AddComponent<MeshRenderer>().sharedMaterials = p.sharedMaterials;
                return (
                FashionSlotIds.BODY,
                CreateFashionIcon(Resources.FindObjectsOfTypeAll<Texture2D>().First((y) => y.name == "tut_vac1")).CreateSprite(),
                new Color(0.5f,0.75f,0.75f),
                prefab,
                new SudoGadgetDefinition() { craftCosts = new [] {
                    (Identifiable.Id.PINK_PLORT, 25),
                    (Identifiable.Id.TABBY_PLORT, 25),
                    (Identifiable.Id.DEEP_BRINE_CRAFT, 25),
                    (Identifiable.Id.SPIRAL_STEAM_CRAFT, 12),
                    (Identifiable.Id.STRANGE_DIAMOND_CRAFT, 2)
                }, blueprintCost = 1500, availableDelegate = (x) => (y) => y.CreateBasicLock(x,Gadget.Id.NONE, ProgressDirector.ProgressType.UNLOCK_LAB, 8) });
            }),
            ("DRONE_KEY",default,default,"Drone Key Fashion Pod",null, SRModLoader.LoadingStep.LOAD, () => {
                var prefab = Resources.FindObjectsOfTypeAll<Drone>().First((y) => y.name == "slimeDrone").transform.Find("prefab_slimeBase/Body/Pivot Key").CreatePrefab().gameObject;
                prefab.name = "fashion_drone_key";
                return (
                Fashion.Slot.TOP,
                CreateFashionIcon(Resources.FindObjectsOfTypeAll<Sprite>().First((y) => y.name == "iconGadgetDrone").texture).CreateSprite(),
                new Color(0.5f,0.75f,0.75f),
                prefab,
                new SudoGadgetDefinition() { craftCosts = new [] {
                    (Identifiable.Id.PINK_PLORT, 25),
                    (Identifiable.Id.QUANTUM_PLORT, 25),
                    (Identifiable.Id.WILD_HONEY_CRAFT, 25),
                    (Identifiable.Id.HEXACOMB_CRAFT, 12),
                    (Identifiable.Id.LAVA_DUST_CRAFT, 2)
                }, startAvailable = true, blueprintCost = 1500, availableDelegate = (x) => (y) => y.CreateBasicLock(x,Gadget.Id.DRONE,0) });
            }),
            ("SLIME_KEY",default,default,"Slime Key Fashion Pod",null, SRModLoader.LoadingStep.LOAD, () => {
                var topBone = GameContext.Instance.LookupDirector.GetPrefab(Identifiable.Id.KEY).transform.Find("bone_key");
                var meshEdge = topBone.Find("mesh_key").GetComponent<MeshRenderer>();
                var meshTeeth = topBone.Find("bone_keyTeeth/mesh_keyTeeth").GetComponent<MeshRenderer>();
                var prefab = new GameObject("").CreatePrefab();
                prefab.name = "fashion_slimeKey";
                var prefabChild1 = new GameObject("rim");
                prefabChild1.transform.SetParent(prefab.transform,false);
                prefabChild1.AddComponent<MeshRenderer>().sharedMaterials = meshEdge.sharedMaterials;
                prefabChild1.AddComponent<MeshFilter>().sharedMesh = meshEdge.GetComponent<MeshFilter>().sharedMesh;
                prefabChild1.transform.localPosition = Vector3.right * -0.55f;
                prefabChild1.transform.localRotation = Quaternion.Euler(0,90,0);
                var prefabChild2 = new GameObject("teeth");
                prefabChild2.transform.SetParent(prefab.transform,false);
                prefabChild2.AddComponent<MeshRenderer>().sharedMaterials = meshTeeth.sharedMaterials;
                prefabChild2.AddComponent<MeshFilter>().sharedMesh = meshTeeth.GetComponent<MeshFilter>().sharedMesh;
                prefabChild2.transform.localPosition = Vector3.right * 0.45f;
                prefabChild2.transform.localRotation = Quaternion.Euler(0,-90,0);
                return (
                FashionSlotIds.BODY,
                CreateFashionIcon(SceneContext.Instance.PediaDirector.entryDict[PediaDirector.Id.KEYS].icon.texture).CreateSprite(),
                new Color(0.5f,0.75f,0.75f),
                prefab,
                new SudoGadgetDefinition() { craftCosts = new [] {
                    (Identifiable.Id.PINK_PLORT, 25),
                    (Identifiable.Id.CRYSTAL_PLORT, 25),
                    (Identifiable.Id.JELLYSTONE_CRAFT, 25),
                    (Identifiable.Id.SLIME_FOSSIL_CRAFT, 12),
                    (Identifiable.Id.STRANGE_DIAMOND_CRAFT, 2)
                }, startAvailable = true, blueprintCost = 1500, availableDelegate = (x) => (y) => y.CreateBasicLock(x,Gadget.Id.NONE,ProgressDirector.ProgressType.SLIME_DOORS,0) });
            }),
            ("RAD_SMALL",default,default,"Small Rad Fashion Pod",null, SRModLoader.LoadingStep.LOAD, () => {
                var p = Identifiable.Id.RAD_SLIME.GetAppearance(SlimeAppearance.AppearanceSaveSet.CLASSIC).Structures[1];
                var prefab = new GameObject("").CreatePrefab();
                prefab.name = "fashion_radSmall";
                prefab.AddComponent<MeshFilter>().sharedMesh = p.Element.Prefabs[0].GetComponent<MeshFilter>().sharedMesh;
                prefab.AddComponent<MeshRenderer>().materials = p.DefaultMaterials;
                prefab.AddComponent<MaterialColorCopier>();
                return (
                FashionSlotIds.AURA,
                CreateFashionIcon(LoadImage("rad.png")).CreateSprite(),
                new Color(0.5f,0.75f,0.75f),
                prefab,
                new SudoGadgetDefinition() { craftCosts = new [] {
                    (Identifiable.Id.PINK_PLORT, 25),
                    (Identifiable.Id.RAD_PLORT, 25),
                    (Identifiable.Id.INDIGONIUM_CRAFT, 25),
                    (Identifiable.Id.GLASS_SHARD_CRAFT, 12),
                    (Identifiable.Id.LAVA_DUST_CRAFT, 2)
                }, startAvailable = true, blueprintCost = 1500, availableDelegate = (x) => (y) => y.CreateBasicLock(x,Gadget.Id.NONE, ProgressDirector.ProgressType.UNLOCK_QUARRY,12) });
            }),
            ("RAD",default,default,"Rad Fashion Pod",null, SRModLoader.LoadingStep.LOAD, () => {
                var p = Identifiable.Id.RAD_SLIME.GetAppearance(SlimeAppearance.AppearanceSaveSet.CLASSIC).Structures;
                var prefab = new GameObject("").CreatePrefab();
                prefab.name = "fashion_rad";
                prefab.AddComponent<MeshFilter>().sharedMesh = p[1].Element.Prefabs[0].GetComponent<MeshFilter>().sharedMesh;
                prefab.AddComponent<MeshRenderer>().materials = p[1].DefaultMaterials;
                prefab.AddComponent<MaterialColorCopier>();
                prefab.transform.localScale = Vector3.one * 3;
                var child = new GameObject("rad_core");
                child.transform.SetParent(prefab.transform,false);
                child.AddComponent<MeshFilter>().sharedMesh = p[2].Element.Prefabs[0].GetComponent<MeshFilter>().sharedMesh;
                child.AddComponent<MeshRenderer>().sharedMaterials = p[2].DefaultMaterials;
                child.AddComponent<CameraFacingBillboard>();
                return (
                FashionSlotIds.AURA,
                CreateFashionIcon(LoadImage("rad.png")).CreateSprite(),
                new Color(0.5f,0.75f,0.75f),
                prefab,
                new SudoGadgetDefinition() { craftCosts = new [] {
                    (Identifiable.Id.PINK_PLORT, 25),
                    (Identifiable.Id.RAD_PLORT, 25),
                    (Identifiable.Id.INDIGONIUM_CRAFT, 25),
                    (Identifiable.Id.GLASS_SHARD_CRAFT, 12),
                    (Identifiable.Id.LAVA_DUST_CRAFT, 2)
                }, startAvailable = true, blueprintCost = 1500, availableDelegate = (x) => (y) => y.CreateBasicLock(x,Gadget.Id.NONE, ProgressDirector.ProgressType.UNLOCK_QUARRY,12) });
            }),
            ("SILVER_CREST",default,default,"Silver Crest Fashion Pod",null, SRModLoader.LoadingStep.LOAD, () => {
                var p = Identifiable.Id.QUICKSILVER_SLIME.GetAppearance(SlimeAppearance.AppearanceSaveSet.CLASSIC).Structures[1];
                var prefab = new GameObject("").CreatePrefab();
                prefab.name = "fashion_silverCrest";
                prefab.AddComponent<MeshFilter>().sharedMesh = p.Element.Prefabs[0].GetComponent<SkinnedMeshRenderer>().sharedMesh;
                prefab.AddComponent<MeshRenderer>().materials = p.DefaultMaterials;
                prefab.AddComponent<MaterialColorCopier>();
                return (
                Fashion.Slot.TOP,
                CreateFashionIcon(LoadImage("quicksilver.png")).CreateSprite(),
                new Color(0.5f,0.75f,0.75f),
                prefab,
                new SudoGadgetDefinition() { craftCosts = new [] {
                    (Identifiable.Id.PINK_PLORT, 25),
                    (SRModLoader.IsModPresent("frostys_quicksilver_rancher") ? Identifiable.Id.QUICKSILVER_PLORT : Identifiable.Id.FIRE_PLORT, 25),
                    (Identifiable.Id.BUZZ_WAX_CRAFT, 25),
                    (Identifiable.Id.SILKY_SAND_CRAFT, 12),
                    (Identifiable.Id.LAVA_DUST_CRAFT, 2)
                }, startAvailable = true, blueprintCost = 1500, availableDelegate = (x) => (y) => y.CreateBasicLock(x,Gadget.Id.NONE,ProgressDirector.ProgressType.ENTER_ZONE_MOCHI_RANCH,0) });
            }),
            ("FLOWER",default,default,"Flower Fashion Pod",null, SRModLoader.LoadingStep.LOAD, () => {
                var p = Identifiable.Id.TANGLE_SLIME.GetAppearance(SlimeAppearance.AppearanceSaveSet.CLASSIC).Structures[1];
                var prefab = new GameObject("").CreatePrefab();
                prefab.name = "fashion_flower";
                prefab.AddComponent<MeshFilter>().sharedMesh = p.Element.Prefabs[0].GetComponent<MeshFilter>().sharedMesh;
                prefab.AddComponent<MeshRenderer>().materials = p.DefaultMaterials;
                prefab.AddComponent<MaterialColorCopier>().editBrightness = false;
                return (
                Fashion.Slot.TOP,
                CreateFashionIcon(LoadImage("flower.png")).CreateSprite(),
                new Color(0.5f,0.75f,0.75f),
                prefab,
                new SudoGadgetDefinition() { craftCosts = new [] {
                    (Identifiable.Id.PINK_PLORT, 25),
                    (Identifiable.Id.TANGLE_PLORT, 25),
                    (Identifiable.Id.WILD_HONEY_CRAFT, 25),
                    (Identifiable.Id.HEXACOMB_CRAFT, 12),
                    (Identifiable.Id.ROYAL_JELLY_CRAFT, 2)
                }, startAvailable = true, blueprintCost = 1500, availableDelegate = (x) => (y) => y.CreateBasicLock(x,Gadget.Id.NONE,ProgressDirector.ProgressType.UNLOCK_DESERT,48) });
            }),
            ("GLIMMER",default,default,"Glimmer Fashion Pod",null, SRModLoader.LoadingStep.LOAD, () => {
                var p = Identifiable.Id.MOSAIC_SLIME.GetAppearance(SlimeAppearance.AppearanceSaveSet.CLASSIC).GlintAppearance.suspendedGlintPrefab.transform.Find("model").GetComponent<MeshFilter>();
                var prefab = new GameObject("").CreatePrefab();
                prefab.name = "fashion_glimmer";
                var prefab2 = new GameObject("").CreatePrefab();
                prefab2.name = "sparkle";
                prefab2.AddComponent<MeshFilter>().sharedMesh = p.sharedMesh;
                prefab2.AddComponent<MeshRenderer>().sharedMaterials = p.GetComponent<MeshRenderer>().sharedMaterials;
                prefab2.GetComponent<MeshRenderer>().shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                var life = prefab2.AddComponent<LifetimeScaleAnimator>();
                life.min = Vector3.zero;
                life.max = Vector3.one;
                life.animationPeriod = 0.3f;
                var spawn = prefab.AddComponent<SimpleSpawner>();
                spawn.objects.Add(GameObjectUtils.CreateScriptableObject<SpawnObject>((x)=>{ x.prefab = life;x.maxRadius = 3;x.maxLifetime = 2;x.minLifetime = 0.5f; }));
                return (
                FashionSlotIds.AURA,
                CreateFashionIcon(LoadImage("glitter.png")).CreateSprite(),
                new Color(0.5f,0.75f,0.75f),
                prefab,
                new SudoGadgetDefinition() { craftCosts = new [] {
                    (Identifiable.Id.PINK_PLORT, 25),
                    (Identifiable.Id.MOSAIC_PLORT, 25),
                    (Identifiable.Id.PRIMORDY_OIL_CRAFT, 25),
                    (Identifiable.Id.SPIRAL_STEAM_CRAFT, 12),
                    (Identifiable.Id.STRANGE_DIAMOND_CRAFT, 2)
                }, startAvailable = true, blueprintCost = 1500, availableDelegate = (x) => (y) => y.CreateBasicLock(x,Gadget.Id.NONE,ProgressDirector.ProgressType.UNLOCK_DESERT,24) });
            }),
            ("FIRE",default,default,"Fire Fashion Pod",null, SRModLoader.LoadingStep.LOAD, () => {
                var p = Identifiable.Id.FIRE_SLIME.GetAppearance(SlimeAppearance.AppearanceSaveSet.CLASSIC).Structures[1].Element.Prefabs[0].transform.Find("FireQuad").GetComponent<MeshFilter>();
                var prefab = new GameObject("").CreatePrefab();
                prefab.name = "fashion_fire";
                prefab.AddComponent<MeshFilter>().sharedMesh = p.sharedMesh;
                prefab.AddComponent<MeshRenderer>().sharedMaterials = p.GetComponent<MeshRenderer>().sharedMaterials;
                prefab.transform.localScale = Vector3.one * 2.5f;
                return (
                FashionSlotIds.BODY,
                CreateFashionIcon(LoadImage("fire.png")).CreateSprite(),
                new Color(0.5f,0.75f,0.75f),
                prefab,
                new SudoGadgetDefinition() { craftCosts = new [] {
                    (Identifiable.Id.PINK_PLORT, 25),
                    (Identifiable.Id.FIRE_PLORT, 25),
                    (Identifiable.Id.PRIMORDY_OIL_CRAFT, 25),
                    (Identifiable.Id.PEPPER_JAM_CRAFT, 12),
                    (Identifiable.Id.LAVA_DUST_CRAFT, 2)
                }, startAvailable = true, blueprintCost = 1500, availableDelegate = (x) => (y) => y.CreateBasicLock(x,Gadget.Id.NONE,ProgressDirector.ProgressType.UNLOCK_DESERT,8) });
            }),
            ("FERAL",default,default,"Feral Fashion Pod",null, SRModLoader.LoadingStep.LOAD, () => {
                var prefab = new GameObject("").CreatePrefab();
                prefab.name = "fashion_feral";
                prefab.AddComponent<SlimeExpressionForcer>().eyes = SlimeFace.SlimeExpression.Feral;
                prefab.AddComponent<FakeFeralAura>().auraPrefab = Resources.FindObjectsOfTypeAll<GameObject>().First((x) => x.name == "FX auraFeral");
                return (
                FashionSlotIds.FACE,
                CreateFashionIcon(SceneContext.Instance.PediaDirector.entryDict[PediaDirector.Id.FERAL_SLIME].icon.texture).CreateSprite(),
                new Color(0.5f,0.75f,0.75f),
                prefab,
                new SudoGadgetDefinition() { craftCosts = new [] {
                    (Identifiable.Id.PINK_PLORT, 25),
                    (Identifiable.Id.HUNTER_PLORT, 25),
                    (Identifiable.Id.JELLYSTONE_CRAFT, 25),
                    (Identifiable.Id.PEPPER_JAM_CRAFT, 12),
                    (Identifiable.Id.ROYAL_JELLY_CRAFT, 2)
                }, startAvailable = true, blueprintCost = 1500, availableDelegate = (x) => (y) => y.CreateBasicLock(x,Gadget.Id.NONE,ProgressDirector.ProgressType.UNLOCK_MOSS,24) });
            }),
            ("SPORES",default,default,"Spores Fashion Pod",null, SRModLoader.LoadingStep.LOAD, () => {
                var prefab = new GameObject("").CreatePrefab();
                prefab.name = "fashion_spores";
                Object.Instantiate(GameObjectUtils.FindFX("FXPool Pollen"), prefab.transform, false);
                return (
                FashionSlotIds.AURA,
                CreateFashionIcon(LoadImage("spores.png")).CreateSprite(),
                new Color(0.5f,0.75f,0.75f),
                prefab,
                new SudoGadgetDefinition() { craftCosts = new [] {
                    (Identifiable.Id.PINK_PLORT, 25),
                    (Identifiable.Id.HONEY_PLORT, 25),
                    (Identifiable.Id.BUZZ_WAX_CRAFT, 25),
                    (Identifiable.Id.SILKY_SAND_CRAFT, 12),
                    (Identifiable.Id.ROYAL_JELLY_CRAFT, 2)
                }, startAvailable = true, blueprintCost = 1500, availableDelegate = (x) => (y) => y.CreateBasicLock(x,Gadget.Id.NONE,8) });
            }),
            ("BOTTLE",default,default,"Bottle Fashion Pod",null, SRModLoader.LoadingStep.LOAD, () => {
                var p = GameContext.Instance.LookupDirector.GetPrefab(Identifiable.Id.DEEP_BRINE_CRAFT).GetComponentInChildren<MeshFilter>();
                var prefab = new GameObject("").CreatePrefab();
                prefab.name = "fashion_bottle";
                prefab.AddComponent<MeshFilter>().sharedMesh = p.sharedMesh;
                prefab.AddComponent<MeshRenderer>().sharedMaterials = p.GetComponent<MeshRenderer>().sharedMaterials;
                prefab.transform.localScale = Vector3.one * 2;
                return (
                FashionSlotIds.BODY,
                CreateFashionIcon(GameContext.Instance.LookupDirector.GetIcon(Identifiable.Id.DEEP_BRINE_CRAFT).texture).CreateSprite(),
                new Color(0.5f,0.75f,0.75f),
                prefab,
                new SudoGadgetDefinition() { craftCosts = new [] {
                    (Identifiable.Id.PINK_PLORT, 25),
                    (Identifiable.Id.MOSAIC_PLORT, 25),
                    (Identifiable.Id.DEEP_BRINE_CRAFT, 25),
                    (Identifiable.Id.GLASS_SHARD_CRAFT, 12),
                    (Identifiable.Id.STRANGE_DIAMOND_CRAFT, 2)
                }, startAvailable = true, blueprintCost = 1500, availableDelegate = (x) => (y) => y.CreateBasicLock(x,Gadget.Id.NONE,ProgressDirector.ProgressType.UNLOCK_LAB,48) });
            }),
            ("WINGS_HEXA",default,default,"Hexa Wings Fashion Pod",null, SRModLoader.LoadingStep.LOAD, () => {
                var p = GameContext.Instance.LookupDirector.GetPrefab(Identifiable.Id.HEXACOMB_CRAFT).GetComponentInChildren<MeshFilter>();
                var prefab = new GameObject("").CreatePrefab();
                prefab.name = "fashion_wingsHexa";
                var wing = prefab.AddComponent<WingAttach>();
                wing.prefabRight = new GameObject("").CreatePrefab();
                wing.prefabRight.name = "wing_hexa_right";
                wing.prefabRight.AddComponent<MeshFilter>().sharedMesh = p.sharedMesh;
                wing.prefabRight.AddComponent<MeshRenderer>().sharedMaterials = p.GetComponent<MeshRenderer>().sharedMaterials;
                wing.prefabRight.transform.localScale = new Vector3(1.1f,1.1f,0.6f);
                wing.prefabRight.transform.localRotation = Quaternion.Euler(-8, 88, 74);
                wing.prefabRight.transform.localPosition = new Vector3(0, -0.1f, -0.1f);
                wing.prefabLeft = wing.prefabRight.CreatePrefab();
                wing.prefabLeft.name = "wing_hexa_left";
                wing.prefabLeft.transform.localRotation = Quaternion.Euler(8, -88, 74);
                wing.prefabLeft.transform.localPosition = new Vector3(0, -0.1f, 0.1f);
                return (
                FashionSlotIds.WINGS,
                CreateFashionIcon(GameContext.Instance.LookupDirector.GetIcon(Identifiable.Id.HEXACOMB_CRAFT).texture).CreateSprite(),
                new Color(0.5f,0.75f,0.75f),
                prefab,
                new SudoGadgetDefinition() { craftCosts = new [] {
                    (Identifiable.Id.PINK_PLORT, 25),
                    (Identifiable.Id.HONEY_PLORT, 25),
                    (Identifiable.Id.JELLYSTONE_CRAFT, 25),
                    (Identifiable.Id.HEXACOMB_CRAFT, 12),
                    (Identifiable.Id.ROYAL_JELLY_CRAFT, 2)
                }, startAvailable = true, blueprintCost = 1500, availableDelegate = (x) => (y) => y.CreateBasicLock(x,Gadget.Id.NONE,ProgressDirector.ProgressType.UNLOCK_LAB,96) });
            }),
            ("DERVISH_RING",default,default,"Disc Fashion Pod",null, SRModLoader.LoadingStep.LOAD, () => {
                var p = Identifiable.Id.DERVISH_SLIME.GetAppearance(SlimeAppearance.AppearanceSaveSet.CLASSIC).Structures[1];
                var prefab = new GameObject("").CreatePrefab();
                prefab.name = "fashion_dervishRing";
                prefab.AddComponent<MaterialColorCopier>().editSaturation = false;
                var tilt = new GameObject("tilt").transform;
                tilt.SetParent(prefab.transform,false);
                tilt.localRotation = Quaternion.Euler(20,0,0);
                var child = new GameObject("ring");
                child.transform.SetParent(tilt,false);
                child.transform.localScale = Vector3.one * 1.1f;
                child.AddComponent<MeshFilter>().sharedMesh = p.Element.Prefabs[0].GetComponent<MeshFilter>().sharedMesh;
                child.AddComponent<MeshRenderer>().materials = p.DefaultMaterials;
                var rotate = child.AddComponent<RotateObject>();
                rotate.axis = Vector3.up;
                rotate.speed = 180;
                return (
                FashionSlotIds.BODY,
                CreateFashionIcon(LoadImage("disc.png")).CreateSprite(),
                new Color(0.5f,0.75f,0.75f),
                prefab,
                new SudoGadgetDefinition() { craftCosts = new [] {
                    (Identifiable.Id.PINK_PLORT, 25),
                    (Identifiable.Id.DERVISH_PLORT, 25),
                    (Identifiable.Id.BUZZ_WAX_CRAFT, 25),
                    (Identifiable.Id.SPIRAL_STEAM_CRAFT, 12),
                    (Identifiable.Id.ROYAL_JELLY_CRAFT, 2)
                }, startAvailable = true, blueprintCost = 1500, availableDelegate = (x) => (y) => y.CreateBasicLock(x,Gadget.Id.NONE,ProgressDirector.ProgressType.UNLOCK_DESERT,12) });
            }),
            ("WINGS_TARR",default,default,"Tarr Wings Fashion Pod",null, SRModLoader.LoadingStep.LOAD, () => {
                var prefab = new GameObject("").CreatePrefab();
                prefab.name = "fashion_wingsTarr";
                var wing = prefab.AddComponent<WingAttach>();
                wing.prefabRight = new GameObject("").CreatePrefab();
                wing.prefabRight.name = "wing_tarr_right";
                var m = GameContext.Instance.LookupDirector.GetPrefab(Identifiable.Id.JELLYSTONE_CRAFT).GetComponentInChildren<MeshFilter>().sharedMesh;
                var min = new Vector2(float.PositiveInfinity,float.PositiveInfinity);
                var max = new Vector2(float.NegativeInfinity,float.NegativeInfinity);
                var verts = m.vertices;
                for (int i = 0; i < verts.Length; i++)
                {
                    if (min.x > verts[i].x)
                        min.x = verts[i].x;
                    if (min.y > verts[i].y)
                        min.y = verts[i].y;
                    if (max.x < verts[i].x)
                        max.x = verts[i].x;
                    if (max.y < verts[i].y)
                        max.y = verts[i].y;
                }
                max -= min;
                var uv = m.uv;
                for (int i = 0; i < verts.Length; i++)
                    uv[i] = new Vector2((verts[i].x - min.x) / max.x, (verts[i].y - min.y) / max.y);

                var m2 = new MeshData(verts,uv,m.triangles);
                m2.RemoveDuplicateVertices();
                wing.prefabRight.AddComponent<MeshFilter>().sharedMesh = m2;
                wing.prefabRight.GetComponent<MeshFilter>().sharedMesh.name = "tarr_wing_model";
                wing.prefabRight.AddComponent<MeshRenderer>().sharedMaterial = Identifiable.Id.TARR_SLIME.GetAppearance(SlimeAppearance.AppearanceSaveSet.CLASSIC).Structures[0].DefaultMaterials[0];
                wing.prefabRight.transform.localScale = new Vector3(1.5f,1.5f,0.6f);
                wing.prefabRight.transform.localRotation = Quaternion.Euler(-7, 83, -60);
                wing.prefabRight.transform.localPosition = new Vector3(0.03f, 0, 0.4f);
                wing.prefabLeft = wing.prefabRight.CreatePrefab();
                wing.prefabLeft.name = "wing_tarr_left";
                wing.prefabLeft.transform.localRotation = Quaternion.Euler(-7, 97, 110);
                wing.prefabLeft.transform.localPosition = new Vector3(0.03f, 0, -0.4f);
                return (
                FashionSlotIds.WINGS,
                CreateFashionIcon(LoadImage("tarr_wing.png")).CreateSprite(),
                new Color(0.5f,0.75f,0.75f),
                prefab,
                new SudoGadgetDefinition() { craftCosts = new [] {
                    (Identifiable.Id.PINK_PLORT, 25),
                    (Enum.TryParse("TARR_PLORT",out Identifiable.Id id) ? id : Identifiable.Id.PUDDLE_PLORT, 25),
                    (Identifiable.Id.PRIMORDY_OIL_CRAFT, 25),
                    (Identifiable.Id.SLIME_FOSSIL_CRAFT, 12),
                    (Identifiable.Id.STRANGE_DIAMOND_CRAFT, 2)
                }, startAvailable = true, blueprintCost = 1500, availableDelegate = (x) => (y) => y.CreateBasicLock(x,Gadget.Id.NONE,ProgressDirector.ProgressType.EXCHANGE_VIKTOR,8) });
            }),
            ("GLITCH_TRAIL",default,default,"Glitch Trail Fashion Pod",null, SRModLoader.LoadingStep.LOAD, () => {
                var p = Identifiable.Id.GLITCH_SLIME.GetAppearance(SlimeAppearance.AppearanceSaveSet.CLASSIC).Structures[1];
                var prefab = new GameObject("").CreatePrefab();
                prefab.name = "fashion_glitch_trail";
                var trail = prefab.AddComponent<TrailRenderer>();
                trail.CopyFields(p.Element.Prefabs[0].GetComponent<TrailRenderer>());
                trail.material = p.DefaultMaterials[0];
                prefab.AddComponent<MaterialColorCopier>();
                return (
                FashionSlotIds.BODY,
                CreateFashionIcon(LoadImage("glitch_trail.png")).CreateSprite(),
                new Color(0.5f,0.75f,0.75f),
                prefab,
                new SudoGadgetDefinition() { craftCosts = new [] {
                    (Identifiable.Id.PINK_PLORT, 25),
                    (Identifiable.Id.QUANTUM_PLORT, 25),
                    (Identifiable.Id.INDIGONIUM_CRAFT, 25),
                    (Identifiable.Id.SILKY_SAND_CRAFT, 12),
                    (Identifiable.Id.ROYAL_JELLY_CRAFT, 2)
                }, startAvailable = true, blueprintCost = 1500, availableDelegate = (x) => (y) => y.CreateBasicLock(x,Gadget.Id.NONE,ProgressDirector.ProgressType.ENTER_ZONE_VIKTOR_LAB,12) });
            }),
            ("TRAIL",default,default,"Trail Fashion Pod",null, SRModLoader.LoadingStep.LOAD, () => {
                var prefab = Resources.FindObjectsOfTypeAll<Drone>().First((y) => y.name == "slimeDrone").transform.Find("prefab_slimeBase/Body/FX Trail").CreatePrefab().gameObject;
                prefab.name = "fashion_trail";
                prefab.AddComponent<MaterialColorCopier>();
                return (
                FashionSlotIds.BODY,
                CreateFashionIcon(LoadImage("trail.png")).CreateSprite(),
                new Color(0.5f,0.75f,0.75f),
                prefab,
                new SudoGadgetDefinition() { craftCosts = new [] {
                    (Identifiable.Id.PINK_PLORT, 25),
                    (Identifiable.Id.PHOSPHOR_PLORT, 25),
                    (Identifiable.Id.WILD_HONEY_CRAFT, 25),
                    (Identifiable.Id.PEPPER_JAM_CRAFT, 12),
                    (Identifiable.Id.STRANGE_DIAMOND_CRAFT, 2)
                }, startAvailable = true, blueprintCost = 1500, availableDelegate = (x) => (y) => y.CreateBasicLock(x,Gadget.Id.DRONE,8) });
            }),
            ("WINGS_ANCIENT",default,default,"Ancient Wings Fashion Pod",null, SRModLoader.LoadingStep.LOAD, () => {
                var p = Identifiable.Id.MAGIC_WATER_LIQUID.GetPrefab();
                var mats = p.transform.Find("Sphere").GetComponent<MeshRenderer>().sharedMaterials;
                var mats2 = p.transform.Find("Sphere/Sphere").GetComponent<MeshRenderer>().sharedMaterials;
                var mes = GameContext.Instance.LookupDirector.GetPrefab(Identifiable.Id.STRANGE_DIAMOND_CRAFT).GetComponentInChildren<MeshFilter>().sharedMesh;
                var prefab = new GameObject("").CreatePrefab();
                prefab.name = "fashion_wingsAncient";
                var wing = prefab.AddComponent<WingAttach>();
                wing.prefabRight = new GameObject("").CreatePrefab();
                wing.prefabRight.name = "wing_ancient_right";
                wing.prefabRight.AddComponent<MeshFilter>().sharedMesh = mes;
                wing.prefabRight.AddComponent<MeshRenderer>().sharedMaterials = mats;
                wing.prefabRight.transform.localScale = new Vector3(1.5f,1.5f,0.6f);
                wing.prefabRight.transform.localRotation = Quaternion.Euler(0, -90, 90);
                wing.prefabRight.transform.localPosition = new Vector3(-0.03f, 0, 0.4f);
                var overlay = new GameObject("field");
                overlay.transform.SetParent(wing.prefabRight.transform,false);
                overlay.AddComponent<MeshFilter>().sharedMesh = mes;
                overlay.AddComponent<MeshRenderer>().sharedMaterials = mats2;
                overlay.transform.localScale = Vector3.one * 1.3f;
                for (int i = 0; i < 4; i++)
                {
                    var trailObj = new GameObject("trail" + (i == 0 ? "" : $" ({i + 1})"));
                    trailObj.transform.SetParent(wing.prefabRight.transform,false);
                    trailObj.transform.localPosition = Vector3.up * (0.1f - 0.11f * i);
                    var trail = trailObj.AddComponent<TrailRenderer>();
                    trail.sharedMaterials = mats;
                    trail.startWidth = 0.25f;
                    trail.endWidth = 0;
                    trail.minVertexDistance = 0.2f;
                    trail.time = 0.2f;
                }
                for (int i = 0; i < 4; i++)
                {
                    var trailObj = new GameObject("trailField" + (i == 0 ? "" : $" ({i + 1})"));
                    trailObj.transform.SetParent(wing.prefabRight.transform,false);
                    trailObj.transform.localPosition = Vector3.up * (0.1f - 0.11f * i);
                    var trail = trailObj.AddComponent<TrailRenderer>();
                    trail.sharedMaterials = mats2;
                    trail.startWidth = 0.35f;
                    trail.endWidth = 0.1f;
                    trail.minVertexDistance = 0.2f;
                    trail.time = 0.6f;
                }
                wing.prefabLeft = wing.prefabRight.CreatePrefab();
                wing.prefabLeft.name = "wing_ancient_left";
                wing.prefabLeft.transform.localRotation = Quaternion.Euler(180, -90, -90);
                wing.prefabLeft.transform.localPosition = new Vector3(0.03f, 0, -0.4f);
                return (
                FashionSlotIds.WINGS,
                CreateFashionIcon(GameContext.Instance.LookupDirector.GetIcon(Identifiable.Id.MAGIC_WATER_LIQUID).texture).CreateSprite(),
                new Color(0.5f,0.75f,0.75f),
                prefab,
                new SudoGadgetDefinition() { craftCosts = new [] {
                    (Identifiable.Id.PINK_PLORT, 25),
                    (Identifiable.Id.GOLD_PLORT, 5),
                    (Identifiable.Id.DEEP_BRINE_CRAFT, 25),
                    (Identifiable.Id.SLIME_FOSSIL_CRAFT, 12),
                    (Identifiable.Id.LAVA_DUST_CRAFT, 2)
                }, startAvailable = true, blueprintCost = 2000, availableDelegate = (x) => (y) => y.CreateBasicLock(x,Gadget.Id.NONE,ProgressDirector.ProgressType.UNLOCK_DESERT,96) });
            })
        };

        public static void RegisterFashionPod(string IdName, (Gadget.Id, Identifiable.Id)? Ids, string Name, string Desc, SRModLoader.LoadingStep PrefabCreationStep, Func<(Fashion.Slot, Sprite, Color, GameObject, SudoGadgetDefinition)> PrefabCreator)
        {
            if (preloaded)
            {
                if (Ids == null)
                    Ids = InitFashionIds(IdName);
                InitFashsionStuff(Ids.Value, Name, Desc);
                if (PrefabCreationStep == SRModLoader.LoadingStep.PRELOAD)
                {
                    var p = PrefabCreator();
                    CreateFashionPod(Ids.Value.Item1, Ids.Value.Item2, p.Item1, p.Item2, p.Item3, p.Item4, p.Item5);
                }
            }
            else if (SRModLoader.CurrentLoadingStep > SRModLoader.LoadingStep.PRELOAD)
                throw new AccessViolationException("Method cannot be used after the PRELOAD");
            var i = Ids ?? default;
            fashions.Add((IdName, i.Item1, i.Item2, Name, Desc, PrefabCreationStep, PrefabCreator));
        }

        public static Texture2D CreateFashionIcon(Texture2D mainImage)
        {
            var back = LoadImage("fashion.png");
            if (!mainImage.isReadable)
                mainImage = mainImage.GetReadable();
            back.ModifyTexturePixels((c, x, y) => c.Overlay(mainImage.GetPixelBilinear(x, y)));
            return back;
        }

        static bool preloaded = false;
        public override void PreLoad()
        {
            HarmonyInstance.PatchAll();
            customSlotAttachments.Add(FashionSlotIds.BODY, ("/bone_root/bone_slime/bone_core", SlimeAppearance.SlimeBone.Core));
            customSlotAttachments.Add(FashionSlotIds.BACK, ("/bone_root/bone_slime/bone_core/bone_jiggle_fro/bone_skin_fro", SlimeAppearance.SlimeBone.JiggleFront));
            customSlotAttachments.Add(FashionSlotIds.AURA, ("/bone_root/bone_slime/bone_core", SlimeAppearance.SlimeBone.Core));
            customSlotAttachments.Add(FashionSlotIds.FACE, ("/bone_root/bone_slime/bone_core/bone_jiggle_bac/bone_skin_bac", SlimeAppearance.SlimeBone.JiggleBack));
            customSlotAttachments.Add(FashionSlotIds.WINGS, ("/bone_root", SlimeAppearance.SlimeBone.Root));
            for (var i = 0; i < fashions.Count; i++)
            {
                var t = fashions[i];
                if (t.Item3 == default)
                    (t.Item2, t.Item3) = InitFashionIds(t.Item1);
                InitFashsionStuff(t.Item2, t.Item3, t.Item4, t.Item5);
                fashions[i] = t;
            }
            foreach (var t in fashions)
                if (t.Item6 == SRModLoader.LoadingStep.PRELOAD)
                {
                    var p = t.Item7();
                    CreateFashionPod(t.Item2, t.Item3, p.Item1, p.Item2, p.Item3, p.Item4, p.Item5);
                }
            preloaded = true;
            customFashionOffset.Add(fashions.First((x) => x.Item1 == "BEATRIX").Item3, (x) => Identifiable.MEAT_CLASS.Contains(x?.GetComponent<Identifiable>()?.id ?? Identifiable.Id.NONE) ? new Vector3(0, -6.7f, -0.8f) : new Vector3(0, -6.8f, -0.2f));
            customFashionOffset.Add(fashions.First((x) => x.Item1 == "DRONE_KEY").Item3, (x) => Identifiable.MEAT_CLASS.Contains(x?.GetComponent<Identifiable>()?.id ?? Identifiable.Id.NONE) ? new Vector3(0, 0.6f, -0.8f) : new Vector3(0, -0.1f, -0.1f));
            customFashionOffset.Add(fashions.First((x) => x.Item1 == "SILVER_CREST").Item3, (x) => Identifiable.MEAT_CLASS.Contains(x?.GetComponent<Identifiable>()?.id ?? Identifiable.Id.NONE) ? new Vector3(0, -0.4f, -0.6f) : new Vector3(0, -1, -0.1f));
            customFashionOffset.Add(fashions.First((x) => x.Item1 == "SLIME_KEY").Item3, (x) => Identifiable.MEAT_CLASS.Contains(x?.GetComponent<Identifiable>()?.id ?? Identifiable.Id.NONE) ? new Vector3(0, 0, -0.5f) : new Vector3(0, 0, 0));
            customFashionOffset.Add(fashions.First((x) => x.Item1 == "RAD").Item3, (x) => Identifiable.MEAT_CLASS.Contains(x?.GetComponent<Identifiable>()?.id ?? Identifiable.Id.NONE) ? new Vector3(0, 0, -0.8f) : new Vector3(0, 0, 0));
            customFashionOffset.Add(fashions.First((x) => x.Item1 == "RAD_SMALL").Item3, (x) => Identifiable.MEAT_CLASS.Contains(x?.GetComponent<Identifiable>()?.id ?? Identifiable.Id.NONE) ? new Vector3(0, 0, -0.8f) : new Vector3(0, 0, 0));
            customFashionOffset.Add(fashions.First((x) => x.Item1 == "FLOWER").Item3, (x) => Identifiable.MEAT_CLASS.Contains(x?.GetComponent<Identifiable>()?.id ?? Identifiable.Id.NONE) ? new Vector3(0, 0.4f, -0.2f) : new Vector3(0, -0.1f, 0));
            customFashionOffset.Add(fashions.First((x) => x.Item1 == "FIRE").Item3, (x) => Identifiable.MEAT_CLASS.Contains(x?.GetComponent<Identifiable>()?.id ?? Identifiable.Id.NONE) ? new Vector3(0, 0, -0.8f) : new Vector3(0, 0, 0));
            customFashionOffset.Add(fashions.First((x) => x.Item1 == "SPORES").Item3, (x) => Identifiable.MEAT_CLASS.Contains(x?.GetComponent<Identifiable>()?.id ?? Identifiable.Id.NONE) ? new Vector3(0, 0, -0.8f) : new Vector3(0, 0, 0));
            customFashionOffset.Add(fashions.First((x) => x.Item1 == "BOTTLE").Item3, (x) => Identifiable.MEAT_CLASS.Contains(x?.GetComponent<Identifiable>()?.id ?? Identifiable.Id.NONE) ? new Vector3(0, -0.6f, -0.7f) : new Vector3(0, -0.7f, 0));
            customFashionOffset.Add(fashions.First((x) => x.Item1 == "DERVISH_RING").Item3, (x) => Identifiable.MEAT_CLASS.Contains(x?.GetComponent<Identifiable>()?.id ?? Identifiable.Id.NONE) ? new Vector3(0, 0, -0.8f) : new Vector3(0, 0, 0));
            customFashionOffset.Add(fashions.First((x) => x.Item1 == "GLITCH_TRAIL").Item3, (x) => Identifiable.MEAT_CLASS.Contains(x?.GetComponent<Identifiable>()?.id ?? Identifiable.Id.NONE) ? new Vector3(0, 0, -0.8f) : new Vector3(0, 0, 0));
            customFashionOffset.Add(fashions.First((x) => x.Item1 == "TRAIL").Item3, (x) => Identifiable.MEAT_CLASS.Contains(x?.GetComponent<Identifiable>()?.id ?? Identifiable.Id.NONE) ? new Vector3(0, -0.1f, -1.3f) : new Vector3(0, -0.2f, -0.3f));
        }
        public override void Load()
        {
            foreach (var t in fashions)
                if (t.Item6 == SRModLoader.LoadingStep.LOAD)
                {
                    var p = t.Item7();
                    CreateFashionPod(t.Item2, t.Item3, p.Item1, p.Item2, p.Item3, p.Item4, p.Item5);
                }
        }
        public override void PostLoad()
        {
            foreach (var t in fashions)
                if (t.Item6 == SRModLoader.LoadingStep.POSTLOAD)
                {
                    var p = t.Item7();
                    CreateFashionPod(t.Item2, t.Item3, p.Item1, p.Item2, p.Item3, p.Item4, p.Item5);
                }
        }
        internal static void TriggerLoad(string name)
        {
            foreach (var t in fashions)
            {
                if (t.Item1 == name && t.Item6 == SRModLoader.LoadingStep.FINISHED)
                {
                    var p = t.Item7();
                    CreateFashionPod(t.Item2, t.Item3, p.Item1, p.Item2, p.Item3, p.Item4, p.Item5);
                }
            }

        }
        public static void Log(string message) => Console.Log($"[{modName}]: " + message);
        public static void LogError(string message) => Console.LogError($"[{modName}]: " + message);
        public static void LogWarning(string message) => Console.LogWarning($"[{modName}]: " + message);
        public static void LogSuccess(string message) => Console.LogSuccess($"[{modName}]: " + message);

        public static (Gadget.Id, Identifiable.Id) InitFashionIds(string idName) => ((Gadget.Id)EnumPatcher.AddEnumValue(typeof(Gadget.Id), "FASHION_POD_" + idName), (Identifiable.Id)EnumPatcher.AddEnumValue(typeof(Identifiable.Id), idName + "_FASHION"));
        public static void InitFashsionStuff(Gadget.Id gadgetId, Identifiable.Id identId, string name, string desc)
        {
            Identifiable.GADGET_NAME_DICT[identId] = gadgetId;
            var t = gadgetId.GetTranslation();
            if (name != null)
                t.SetNameTranslation(name);
            if (desc != null)
                t.SetDescriptionTranslation(desc);
        }
        public static void InitFashsionStuff((Gadget.Id, Identifiable.Id) ids, string name, string desc) => InitFashsionStuff(ids.Item1, ids.Item2, name, desc);
        public static void CreateFashionPod(Gadget.Id gadgetId, Identifiable.Id identId, Fashion.Slot slot, Sprite icon, Color vacColor, GameObject attachPrefab, SudoGadgetDefinition definition = default)
        {
            var prefab = FashionPrefab.CreatePrefab();
            prefab.GetComponent<Identifiable>().id = identId;
            var f = prefab.GetComponent<Fashion>();
            f.slot = slot;
            f.attachPrefab = attachPrefab;
            prefab.transform.Find("Icon Pivot/ShopIconUI/Image").GetComponent<Image>().sprite = icon;
            LookupRegistry.RegisterIdentifiablePrefab(prefab);
            LookupRegistry.RegisterVacEntry(GameObjectUtils.CreateScriptableObject<VacItemDefinition>((x) => { x.color = vacColor; x.icon = icon; x.id = identId; }));
            AmmoRegistry.RegisterAmmoPrefab(PlayerState.AmmoMode.DEFAULT, prefab);

            var gadgetPrefab = FashionPodPrefab.CreatePrefab();
            gadgetPrefab.GetComponent<Gadget>().id = gadgetId;
            gadgetPrefab.GetComponent<FashionPod>().fashionId = identId;
            gadgetPrefab.transform.Find("model_fashionPod").GetComponent<MeshRenderer>().material.mainTexture = icon.texture;
            LookupRegistry.RegisterGadget(GameObjectUtils.CreateScriptableObject<GadgetDefinition>((x) => {
                x.pediaLink = PediaDirector.Id.CURIOS;
                x.icon = icon;
                x.id = gadgetId;
                x.prefab = gadgetPrefab;
                if (definition.blueprintCost != null)
                    x.blueprintCost = definition.blueprintCost.Value;
                if (definition.buyCountLimit != null)
                    x.buyCountLimit = definition.buyCountLimit.Value;
                if (definition.destroyOnRemoval != null)
                    x.destroyOnRemoval = definition.destroyOnRemoval.Value;
                if (definition.craftCosts != null)
                {
                    var c = new GadgetDefinition.CraftCost[definition.craftCosts.Length];
                    for (int i = 0; i < c.Length; i++)
                        c[i] = new GadgetDefinition.CraftCost() { id = definition.craftCosts[i].Item1, amount = definition.craftCosts[i].Item2 };
                    x.craftCosts = c;
                }
            }));
            if (definition.startUnlocked)
                GadgetRegistry.RegisterDefaultBlueprint(gadgetId);
            else if (definition.startAvailable)
                GadgetRegistry.RegisterDefaultAvailableBlueprint(gadgetId);
            else if (definition.availableDelegate != null)
                GadgetRegistry.RegisterBlueprintLock(gadgetId, definition.availableDelegate(gadgetId));
        }
        public static void CreateFashionPod((Gadget.Id, Identifiable.Id) ids, Fashion.Slot slot, Sprite icon, Color vacColor, GameObject attachPrefab, SudoGadgetDefinition definition = default) => CreateFashionPod(ids.Item1, ids.Item2, slot, icon, vacColor, attachPrefab, definition);
    }

    public static class ExtentionMethods
    {
        public static int CountItems(this IEnumerable collection)
        {
            int c = 0;
            if (collection != null)
                foreach (var o in collection)
                    c++;
            return c;
        }
    }

    public class SlimeExpressionForcer : MonoBehaviour
    {
        public SlimeFace.SlimeExpression eyes;
        public SlimeFace.SlimeExpression mouth;
        public bool ignoreBlink = false;
        void Start()
        {
            var a = GetComponentInParent<SlimeAppearanceApplicator>();
            if (a)
                a.SetExpression(a.SlimeExpression);
        }
        void OnDestroy()
        {
            enabled = false;
            var a = GetComponentInParent<SlimeAppearanceApplicator>();
            if (a)
                a.SetExpression(a.SlimeExpression);
        }
    }

class WingAttach : MonoBehaviour
{
    public GameObject prefabLeft;
    public GameObject prefabRight;
    GameObject[] created;
    List<Transform> dummyWings = new List<Transform>();
    void Start()
    {
        Transform[] p = null;
        if (Identifiable.MEAT_CLASS.Contains(GetComponentInParent<Identifiable>()?.id ?? Identifiable.Id.NONE))
        {
            p = new[] { new GameObject("dummy_wing_bone_left").transform, new GameObject("dummy_wing_bone_right").transform };
            p[0].SetParent(transform.parent.parent.parent.Find("bone_spine/bone_l_wing1"), false);
            p[1].SetParent(transform.parent.parent.parent.Find("bone_spine/bone_r_wing1"), false);
            p[0].localPosition = new Vector3(0, 0.1f, -0.05f);
            p[1].localPosition = new Vector3(0, -0.1f, 0.05f);
            p[0].localRotation = Quaternion.Euler(20, 75, 80);
            p[1].localRotation = Quaternion.Euler(20, 75, -100);
            dummyWings.AddRange(p);
        }
        else
            p = new[] { transform.parent?.Find("bone_slime/bone_wing_left"), transform.parent?.Find("bone_slime/bone_wing_right") };
        if (Identifiable.IsGordo(GetComponentInParent<GordoIdentifiable>()?.id ?? Identifiable.Id.NONE))
        {
            if (p == null)
                p = new Transform[2];
            if (p.Length > 0 && !p[0]) {
                p[0] = new GameObject("dummy_wing_bone_left").transform;
                p[0].SetParent(transform.parent?.Find("bone_slime"),false);
                p[0].localPosition = new Vector3(-0.3f, 0.8f, -0.3f);
                p[0].localRotation = Quaternion.Euler(30, 45, 30);
                dummyWings.Add(p[0]);
            }
            if (p.Length > 1 && !p[1])
            {
                p[1] = new GameObject("dummy_wing_bone_right").transform;
                p[1].SetParent(transform.parent?.Find("bone_slime"), false);
                p[1].localPosition = new Vector3(0.3f, 0.8f, -0.3f);
                p[1].localRotation = Quaternion.Euler(-150, -45, -150);
                dummyWings.Add(p[1]);
            }
        }
        if (GetComponentInParent<Drone>())
        {
            if (p == null)
                p = new Transform[2];
            if (p.Length > 0 && !p[0])
            {
                p[0] = new GameObject("dummy_wing_bone_left").transform;
                p[0].SetParent(transform.parent?.Find("bone_slime"), false);
                p[0].localPosition = new Vector3(-0.4f, 0.4f, -0.2f);
                p[0].localRotation = Quaternion.Euler(30, 45, 30);
            }
            if (p.Length > 1 && !p[1])
            {
                p[1] = new GameObject("dummy_wing_bone_right").transform;
                p[1].SetParent(transform.parent?.Find("bone_slime"), false);
                p[1].localPosition = new Vector3(0.4f, 0.4f, -0.2f);
                p[1].localRotation = Quaternion.Euler(-150, -45, -150);
            }
        }
        if (p != null)
        {
            created = new GameObject[p.Length];
            for (int i = 0; i < p.Length; i++)
                if ((i % 2) == 0 ? prefabLeft : prefabRight)
                    created[i] = Instantiate((i % 2) == 0 ? prefabLeft : prefabRight, p[i]);
        }
    }
    void OnDestroy()
    {
        if (created != null)
            foreach (var o in created)
                Destroy(o);
        foreach (var o in dummyWings)
            Destroy(o.gameObject);
    }
}

    /*[HarmonyPatch(typeof(SlimeAppearanceApplicator), "SetExpression")]
    class Patch_SetSlimeExpression
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();
            var ind = code.FindIndex((x) => x.opcode == OpCodes.Stloc_0)-1;
            code.InsertRange(ind, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(Patch_SetSlimeExpression),"TweakExpression"))
            });
            return code;
        }
        public static SlimeExpressionFace TweakExpression(SlimeExpressionFace face, SlimeAppearanceApplicator applicator)
        {
            foreach (var f in applicator.GetComponentsInChildren<SlimeExpressionForcer>())
            {
                if (f.eyes != null && (face.SlimeExpression != SlimeFace.SlimeExpression.Blink || f.ignoreBlink))
                {
                    var e = applicator.Appearance.Face.GetExpressionFace(f.eyes.Value);
                    if (e.Eyes)
                        face.Eyes = e.Eyes;
                }
                if (f.mouth != null)
                {
                    var e = applicator.Appearance.Face.GetExpressionFace(f.mouth.Value);
                    if (e.Mouth)
                        face.Mouth = e.Mouth;
                }
            }
            return face;
        }
    }*/

    [HarmonyPatch(typeof(SlimeFace),"GetExpressionFace")]
    class Patch_Face
    {
        static bool patch = true;
        static void Postfix(ref SlimeExpressionFace __result)
        {
            if (patch)
            {
                var a = GetCalling();
                if (a)
                    __result = TweakExpression(__result, a);
            }
        }
        static SlimeAppearanceApplicator GetCalling() => Patch_ApplicatorApply.calling ?? Patch_FaceStateApply.calling;
        public static SlimeExpressionFace TweakExpression(SlimeExpressionFace face, SlimeAppearanceApplicator applicator)
        {
            patch = false;
            foreach (var f in applicator.GetComponentsInChildren<SlimeExpressionForcer>())
                if (f.enabled)
                {
                    if (f.eyes != SlimeFace.SlimeExpression.None && (face.SlimeExpression != SlimeFace.SlimeExpression.Blink || f.ignoreBlink))
                    {
                        var e = applicator.Appearance.Face.GetExpressionFace(f.eyes);
                        if (e.Eyes)
                            face.Eyes = e.Eyes;
                    }
                    if (f.mouth != SlimeFace.SlimeExpression.None)
                    {
                        var e = applicator.Appearance.Face.GetExpressionFace(f.mouth);
                        if (e.Mouth)
                            face.Mouth = e.Mouth;
                    }
                }
            patch = true;
            return face;
        }
    }

    [HarmonyPatch(typeof(SlimeAppearanceApplicator), "ApplyAppearance")]
    class Patch_ApplicatorApply
    {
        public static SlimeAppearanceApplicator calling;
        static void Prefix(SlimeAppearanceApplicator __instance) => calling = __instance;
        static void Postfix() => calling = null;
    }

    [HarmonyPatch(typeof(SlimeFaceAnimator.State), "ApplyFacialExpression")]
    class Patch_FaceStateApply
    {
        public static SlimeAppearanceApplicator calling;
        static void Prefix(SlimeFaceAnimator.State __instance) => calling = __instance.anim?.appearanceApplicator;
        static void Postfix() => calling = null;
    }

    class FakeFeralAura : MonoBehaviour
    {
        public GameObject auraPrefab;
        public GameObject created;
        void Start() => created = Instantiate(auraPrefab, GetComponentInParent<AttachFashions>().transform);
        void OnDestroy() => Destroy(created);
    }

    public abstract class LifetimeAnimator : MonoBehaviour
    {
        public float endOfLife;
        float life;
        public float animationPeriod;
        protected abstract void UpdateProgress(float progress);
        float lastProgress;
        void Awake() => UpdateProgress(0);
        void Update()
        {
            life += Time.deltaTime;
            float progress;
            if (life < endOfLife / 2)
                progress = life / animationPeriod;
            else
                progress = (endOfLife - life) / animationPeriod;
            progress = Mathf.Clamp01(progress);
            if (progress != lastProgress)
            {
                lastProgress = progress;
                UpdateProgress(lastProgress);
            }
            if (life > endOfLife)
                Destroy(gameObject);
        }
    }

    public class LifetimeScaleAnimator : LifetimeAnimator
    {
        public Vector3 min;
        public Vector3 max;
        protected override void UpdateProgress(float progress) => transform.localScale = Vector3.Lerp(min, max, progress);
    }

    public class SpawnObject : ScriptableObject
    {
        public LifetimeAnimator prefab;
        public GameObject fxPrefab;
        public float minRadius;
        public float maxRadius;
        public float minLifetime;
        public float maxLifetime;
    }

    public class SimpleSpawner : SRBehaviour
    {
        public List<SpawnObject> objects = new List<SpawnObject>();
        public float spawnInterval = 0.2f;
        float time;
        void Update()
        {
            if (objects.Count == 0)
                return;
            var c = 1;
            if (spawnInterval > 0)
            {
                time += Time.deltaTime;
                c = (int)(time / spawnInterval);
                time %= spawnInterval;
            }
            if (c > 0)
            {

                var pos = transform.position;
                while (c > 0)
                {
                    c--;
                    var d = new Vector3(Randoms.SHARED.GetInRange(-1f, 1), Randoms.SHARED.GetInRange(-1f, 1), Randoms.SHARED.GetInRange(-1f, 1)).normalized;
                    var o = Randoms.SHARED.Pick(objects, default(SpawnObject));
                    if (o.prefab)
                        Instantiate(o.prefab, pos + d * Randoms.SHARED.GetInRange(o.minRadius, o.maxRadius), default).endOfLife = Randoms.SHARED.GetInRange(o.minLifetime, o.maxLifetime);
                    if (o.fxPrefab)
                    {
                        var g = SpawnAndPlayFX(o.fxPrefab, pos + d * Randoms.SHARED.GetInRange(o.minRadius, o.maxRadius), default);
                        if (o.maxLifetime > 0 || o.minLifetime > 0)
                            Destroy(g, Randoms.SHARED.GetInRange(o.minLifetime, o.maxLifetime));
                    }
                }
            }
        }
    }

    public class MaterialColorCopier : MonoBehaviour
    {
        static Dictionary<SlimeAppearance, Color> colorCache = new Dictionary<SlimeAppearance, Color>();
        public Color? color;
        public bool editHue = true;
        public bool editSaturation = true;
        public bool editBrightness = true;
        public bool started { get; private set; }
        void Start()
        {
            started = true;
            var applicator = GetComponentInParent<SlimeAppearanceApplicator>();
            if (!applicator || applicator.Appearance)
                UpdateColor();
            if (applicator)
                applicator.OnAppearanceChanged += OnAppearanceChanged;
        }
        void OnDestroy()
        {
            var applicator = GetComponentInParent<SlimeAppearanceApplicator>();
            if (applicator)
                applicator.OnAppearanceChanged -= OnAppearanceChanged;
        }

        void OnAppearanceChanged(SlimeAppearance appearance) => UpdateColor();

        public void UpdateColor()
        {
            if (!editHue && !editSaturation && !editBrightness)
                return;
                Color? c = null;
            if (color != null)
                c = color.Value;
            else
            {
                var applicator = GetComponentInParent<SlimeAppearanceApplicator>();
                if (applicator?.Appearance)
                {
                    if (colorCache.TryGetValue(applicator.Appearance, out var c3))
                        c = c3;
                    else
                    {
                        var c2 = Color.clear;
                        var count = 0;
                        foreach (var s in applicator.Appearance.Structures)
                            foreach (var m in s.DefaultMaterials)
                                for (int p = 0; p < m.shader.GetPropertyCount(); p++)
                                    if (m.shader.GetPropertyType(p) == UnityEngine.Rendering.ShaderPropertyType.Color)
                                    {
                                        count++;
                                        c2 += m.GetColor(m.shader.GetPropertyName(p));
                                    }
                        if (count > 0)
                        {
                            c2 /= count;
                            colorCache[applicator.Appearance] = c2;
                            c = c2;
                        }
                    }
                    if (c == null)
                    {
                        var palette = applicator.Appearance.ColorPalette;
                        c = (palette.Bottom + palette.Middle + palette.Top) / 3;
                    }
                }
                if (c == null)
                {
                    var identifiable = GetComponentInParent<Identifiable>();
                    if (identifiable)
                    {
                        SlimeAppearance appearance = null;
                        SceneContext.Instance?.SlimeAppearanceDirector?.appearanceSelections?.selections.TryGetValue(identifiable.id, out appearance);
                        if (appearance)
                        {

                            var c2 = Color.clear;
                            var count = 0;
                            foreach (var s in appearance.Structures)
                                foreach (var m in s.DefaultMaterials)
                                    for (int p = 0; p < m.shader.GetPropertyCount(); p++)
                                        if (m.shader.GetPropertyType(p) == UnityEngine.Rendering.ShaderPropertyType.Color)
                                        {
                                            count++;
                                            c2 += m.GetColor(m.shader.GetPropertyName(p));
                                        }
                            if (count > 0)
                            {
                                c2 /= count;
                                colorCache[appearance] = c2;
                                c = c2;
                            }
                            if (c == null)
                            {
                                var palette = appearance.ColorPalette;
                                c = (palette.Bottom + palette.Middle + palette.Top) / 3;
                            }
                        }
                        if (c == null)
                        {
                            VacItemDefinition def = null;
                            GameContext.Instance?.LookupDirector?.vacItemDict.TryGetValue(identifiable.id, out def);
                            if (def)
                                c = def.color;
                        }
                    }
                    if (c == null)
                    {
                        var colorizer = GetComponentInParent<Colorizer>();
                        if (colorizer)
                            c = colorizer.TintColor;
                    }
                    if (c == null)
                    {
                        var fashion = GetComponentInParent<AttachFashions>();
                        if (fashion)
                        {
                            var c2 = Color.clear;
                            var count = 0;
                            foreach (var r in fashion.GetComponentsInChildren<Renderer>())
                                foreach (var m in r.materials)
                                    for (int p = 0; p < m.shader.GetPropertyCount(); p++)
                                        if (!r.GetComponentInParent<MaterialColorCopier>() && m.shader.GetPropertyType(p) == UnityEngine.Rendering.ShaderPropertyType.Color)
                                        {
                                            count++;
                                            c2 += m.GetColor(m.shader.GetPropertyName(p));
                                        }
                            if (count > 0)
                                c = c2 / count;
                        }
                    }
                }
            }
            if (c == null)
                c = new Color(0, 0.5f, 0.05f);
            else
            {
                Color.RGBToHSV(c.Value, out var h, out var s, out var v);
                c = Color.HSVToRGB(h, Mathf.Sqrt(s), v);
            }
            foreach (var r in GetComponentsInChildren<Renderer>())
                foreach (var m in r.materials)
                    for (int p = 0; p < m.shader.GetPropertyCount(); p++)
                        if (m.shader.GetPropertyType(p) == UnityEngine.Rendering.ShaderPropertyType.Color)
                        {
                            if (!editHue || !editSaturation || !editBrightness)
                            {
                                var c2 = m.GetColor(m.shader.GetPropertyName(p));
                                var a = c2.a;
                                Color.RGBToHSV(c2, out var h, out var s, out var v);
                                Color.RGBToHSV(c.Value, out var h2, out var s2, out var v2);
                                c2 = Color.HSVToRGB(editHue ? h2 : h, editSaturation ? s2 : s, editBrightness ? v2 : v);
                                c2.a = a;
                                m.SetColor(m.shader.GetPropertyName(p), c2);
                            }
                            else
                                m.SetColor(m.shader.GetPropertyName(p), new Color(c.Value.r, c.Value.g, c.Value.b, m.GetColor(m.shader.GetPropertyName(p)).a));
                        }
        }
    }

    public struct SudoGadgetDefinition
    {
        public int? blueprintCost;
        public int? buyCountLimit;
        public (Identifiable.Id, int)[] craftCosts;
        public bool? destroyOnRemoval;
        public bool startUnlocked;
        public bool startAvailable;
        public Func<Gadget.Id, GadgetRegistry.BlueprintLockCreateDelegate> availableDelegate;
    }

    [EnumHolder]
    public static class FashionSlotIds
    {
        public static readonly Fashion.Slot BODY;
        public static readonly Fashion.Slot BACK;
        public static readonly Fashion.Slot AURA;
        public static readonly Fashion.Slot FACE;
        public static readonly Fashion.Slot WINGS;
    }

    [HarmonyPatch(typeof(AttachFashions),"GetParentForSlot")]
    static class Patch_SlotParent
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var code = instructions.ToList();
            var ind = code.FindLastIndex((x) => x.opcode == OpCodes.Ldfld && (x.operand as FieldInfo).Name == "slimeAppearanceApplicator") - 1;
            var newLabel = generator.DefineLabel();
            code[ind].labels.Add(newLabel);
            code.InsertRange(ind, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(Patch_SlotParent),"ShouldHandle")),
                new CodeInstruction(OpCodes.Brfalse_S,newLabel),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldfld,AccessTools.Field(typeof(AttachFashions),"slimeAppearanceApplicator")),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(Patch_SlotParent),"Handle2")),
                new CodeInstruction(OpCodes.Ret)
            });
            ind = code.FindIndex((x) => x.opcode == OpCodes.Ldstr && (string)x.operand == "Unhandled fashion slot");
            var labels = code[ind].labels;
            newLabel = generator.DefineLabel();
            code[ind].labels = new List<Label>() { newLabel };
            code.InsertRange(ind, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_1) { labels = labels },
                new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(Patch_SlotParent),"ShouldHandle")),
                new CodeInstruction(OpCodes.Brfalse_S,newLabel),
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Ldarg_1),
                new CodeInstruction(OpCodes.Ldloc_0),
                new CodeInstruction(OpCodes.Call,AccessTools.Method(typeof(Patch_SlotParent),"Handle")),
                new CodeInstruction(OpCodes.Ret)
            });
            return code;
        }
        static bool ShouldHandle(Fashion.Slot slot) {
            return Main.customSlotAttachments.ContainsKey(slot);
        }
        static Transform Handle(AttachFashions instance, Fashion.Slot slot, string attachmentPrefix) => instance.transform.Find(attachmentPrefix + Main.customSlotAttachments[slot].Item1);
        static Transform Handle2(SlimeAppearanceApplicator instance, Fashion.Slot slot) => instance._boneLookup[Main.customSlotAttachments[slot].Item2].transform;
    }

    [HarmonyPatch(typeof(AttachFashions), "Attach")]
    static class Patch_SlotOffset
    {
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var code = instructions.ToList();
            code[code.FindIndex((x) => x.opcode == OpCodes.Call && x.operand is MethodInfo && (x.operand as MethodInfo).Name == "GetParentOffset")].operand = AccessTools.Method(typeof(Patch_SlotOffset), "GetParentOffset");
            return code;
        }
        static Vector3 GetParentOffset(AttachFashions instance, Identifiable.Id fashion)
        {
            if (Main.customFashionOffset.TryGetValue(fashion, out var f) && f != null)
                return f(instance);
            return instance.GetParentOffset(fashion);
        }
    }

    [HarmonyPatch(typeof(RanchDirector),"NoteSelected")]
    static class Patch_RanchColorSelected
    {
        static void Postfix()
        {
            foreach (var r in Resources.FindObjectsOfTypeAll<MaterialColorCopier>())
                if (r.started)
                    r.UpdateColor();
        }
    }

    [HarmonyPatch(typeof(MessageDirector), "LoadBundle")]
    static class Patch_Update
    {
        static void Postfix(MessageDirector __instance, string path, ref ResourceBundle __result)
        {
            if (__result != null && path == "pedia")
            {
                var pedia = __result.dict;
                pedia.TryGetValue("m.gadget.desc.fashion_pod_clip_on", out var def);
                foreach (var t in Main.fashions)
                    if (t.Item2 != default && t.Item5 == null)
                        pedia["m.gadget.desc." + t.Item2.ToString().ToLowerInvariant()] = def;
            }
        }
    }
}