using RimWorld;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using Verse.AI;

namespace Verse
{
	public class ThingDef : BuildableDef
	{
		public Type thingClass;

		public ThingCategory category;

		public TickerType tickerType;

		public int stackLimit = 1;

		public IntVec2 size = new IntVec2(1, 1);

		public bool destroyable = true;

		public bool rotatable = true;

		public bool smallVolume;

		public bool useHitPoints = true;

		public bool receivesSignals;

		public List<CompProperties> comps = new List<CompProperties>();

		public List<ThingCountClass> killedLeavings;

		public List<ThingCountClass> butcherProducts;

		public List<ThingCountClass> smeltProducts;

		public bool smeltable;

		public bool randomizeRotationOnSpawn;

		public List<DamageMultiplier> damageMultipliers;

		public bool isBodyPartOrImplant;

		public RecipeMakerProperties recipeMaker;

		public ThingDef minifiedDef;

		public bool isUnfinishedThing;

		public bool leaveResourcesWhenKilled;

		public ThingDef slagDef;

		public bool isFrame;

		public IntVec3 interactionCellOffset = IntVec3.Zero;

		public bool hasInteractionCell;

		public ThingDef filthLeaving;

		public bool forceDebugSpawnable;

		public bool intricate;

		public bool scatterableOnMapGen = true;

		public float deepCommonality;

		public int deepCountPerCell = 150;

		public float generateCommonality = 1f;

		public float generateAllowChance = 1f;

		private bool canOverlapZones = true;

		public FloatRange startingHpRange = FloatRange.One;

		[NoTranslate]
		public List<string> itemGeneratorTags;

		public bool alwaysFlee;

		public List<Tool> tools;

		public GraphicData graphicData;

		public DrawerType drawerType = DrawerType.RealtimeOnly;

		public bool drawOffscreen;

		public ColorGenerator colorGenerator;

		public float hideAtSnowDepth = 99999f;

		public bool drawDamagedOverlay = true;

		public bool castEdgeShadows;

		public float staticSunShadowHeight;

		public bool selectable;

		public bool neverMultiSelect;

		public bool isAutoAttackableMapObject;

		public bool hasTooltip;

		public List<Type> inspectorTabs;

		[Unsaved]
		public List<InspectTabBase> inspectorTabsResolved;

		public bool seeThroughFog;

		public bool drawGUIOverlay;

		public ResourceCountPriority resourceReadoutPriority;

		public bool resourceReadoutAlwaysShow;

		public bool drawPlaceWorkersWhileSelected;

		public ConceptDef storedConceptLearnOpportunity;

		public float iconDrawScale = -1f;

		public bool alwaysHaulable;

		public bool designateHaulable;

		public List<ThingCategoryDef> thingCategories;

		public bool mineable;

		public bool socialPropernessMatters;

		public bool stealable = true;

		public SoundDef soundDrop;

		public SoundDef soundPickup;

		public SoundDef soundInteract;

		public SoundDef soundImpactDefault;

		public bool saveCompressible;

		public bool isSaveable = true;

		public bool holdsRoof;

		public float fillPercent;

		public bool coversFloor;

		public bool neverOverlapFloors;

		public SurfaceType surfaceType;

		public bool blockPlants;

		public bool blockLight;

		public bool blockWind;

		[Unsaved]
		public bool affectsRegions;

		public Tradeability tradeability = Tradeability.Stockable;

		[NoTranslate]
		public List<string> tradeTags;

		public bool tradeNeverStack;

		public ColorGenerator colorGeneratorInTraderStock;

		public Type blueprintClass = typeof(Blueprint_Build);

		public GraphicData blueprintGraphicData;

		public TerrainDef naturalTerrain;

		public TerrainDef leaveTerrain;

		public List<RecipeDef> recipes;

		private List<VerbProperties> verbs;

		public float equippedAngleOffset;

		public EquipmentType equipmentType;

		public TechLevel techLevel;

		[NoTranslate]
		public List<string> weaponTags;

		[NoTranslate]
		public List<string> techHediffsTags;

		public bool canBeSpawningInventory = true;

		public bool destroyOnDrop;

		public List<StatModifier> equippedStatOffsets;

		public BuildableDef entityDefToBuild;

		public ThingDef projectileWhenLoaded;

		public IngestibleProperties ingestible;

		public FilthProperties filth;

		public GasProperties gas;

		public BuildingProperties building;

		public RaceProperties race;

		public ApparelProperties apparel;

		public MoteProperties mote;

		public PlantProperties plant;

		public ProjectileProperties projectile;

		public StuffProperties stuffProps;

		public SkyfallerProperties skyfaller;

		public const int SmallUnitPerVolume = 10;

		public const float SmallVolumePerUnit = 0.1f;

		private List<RecipeDef> allRecipesCached;

		private static List<VerbProperties> EmptyVerbPropertiesList = new List<VerbProperties>();

		private Dictionary<ThingDef, Thing> concreteExamplesInt;

		public bool EverHaulable
		{
			get
			{
				return this.alwaysHaulable || this.designateHaulable;
			}
		}

		public bool EverStoreable
		{
			get
			{
				return !this.thingCategories.NullOrEmpty<ThingCategoryDef>();
			}
		}

		public float VolumePerUnit
		{
			get
			{
				return this.smallVolume ? 0.1f : 1f;
			}
		}

		public override Color IconDrawColor
		{
			get
			{
				return this.graphicData.color;
			}
		}

		public override IntVec2 Size
		{
			get
			{
				return this.size;
			}
		}

		public bool DiscardOnDestroyed
		{
			get
			{
				return this.race == null;
			}
		}

		public int BaseMaxHitPoints
		{
			get
			{
				return Mathf.RoundToInt(this.GetStatValueAbstract(StatDefOf.MaxHitPoints, null));
			}
		}

		public float BaseFlammability
		{
			get
			{
				return this.GetStatValueAbstract(StatDefOf.Flammability, null);
			}
		}

		public float BaseMarketValue
		{
			get
			{
				return this.GetStatValueAbstract(StatDefOf.MarketValue, null);
			}
			set
			{
				this.SetStatBaseValue(StatDefOf.MarketValue, value);
			}
		}

		public float BaseMass
		{
			get
			{
				return this.GetStatValueAbstract(StatDefOf.Mass, null);
			}
		}

		public bool PlayerAcquirable
		{
			get
			{
				return !this.destroyOnDrop;
			}
		}

		public bool EverTransmitsPower
		{
			get
			{
				for (int i = 0; i < this.comps.Count; i++)
				{
					CompProperties_Power compProperties_Power = this.comps[i] as CompProperties_Power;
					if (compProperties_Power != null && compProperties_Power.transmitsPower)
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool Minifiable
		{
			get
			{
				return this.minifiedDef != null;
			}
		}

		public bool HasThingIDNumber
		{
			get
			{
				return this.category != ThingCategory.Mote;
			}
		}

		public List<RecipeDef> AllRecipes
		{
			get
			{
				if (this.allRecipesCached == null)
				{
					this.allRecipesCached = new List<RecipeDef>();
					if (this.recipes != null)
					{
						for (int i = 0; i < this.recipes.Count; i++)
						{
							this.allRecipesCached.Add(this.recipes[i]);
						}
					}
					List<RecipeDef> allDefsListForReading = DefDatabase<RecipeDef>.AllDefsListForReading;
					for (int j = 0; j < allDefsListForReading.Count; j++)
					{
						if (allDefsListForReading[j].recipeUsers != null && allDefsListForReading[j].recipeUsers.Contains(this))
						{
							this.allRecipesCached.Add(allDefsListForReading[j]);
						}
					}
				}
				return this.allRecipesCached;
			}
		}

		public bool ConnectToPower
		{
			get
			{
				if (this.EverTransmitsPower)
				{
					return false;
				}
				for (int i = 0; i < this.comps.Count; i++)
				{
					if (this.comps[i].compClass == typeof(CompPowerBattery))
					{
						return true;
					}
					if (this.comps[i].compClass == typeof(CompPowerTrader))
					{
						return true;
					}
				}
				return false;
			}
		}

		public bool CoexistsWithFloors
		{
			get
			{
				return !this.neverOverlapFloors && !this.coversFloor;
			}
		}

		public FillCategory Fillage
		{
			get
			{
				if (this.fillPercent < 0.01f)
				{
					return FillCategory.None;
				}
				if (this.fillPercent > 0.99f)
				{
					return FillCategory.Full;
				}
				return FillCategory.Partial;
			}
		}

		public bool MakeFog
		{
			get
			{
				return this.Fillage == FillCategory.Full;
			}
		}

		public bool CanOverlapZones
		{
			get
			{
				if (this.building != null && this.building.SupportsPlants)
				{
					return false;
				}
				if (this.passability == Traversability.Impassable && this.category != ThingCategory.Plant)
				{
					return false;
				}
				if (this.surfaceType >= SurfaceType.Item)
				{
					return false;
				}
				if (typeof(ISlotGroupParent).IsAssignableFrom(this.thingClass))
				{
					return false;
				}
				if (!this.canOverlapZones)
				{
					return false;
				}
				if (this.IsBlueprint || this.IsFrame)
				{
					ThingDef thingDef = this.entityDefToBuild as ThingDef;
					if (thingDef != null)
					{
						return thingDef.CanOverlapZones;
					}
				}
				return true;
			}
		}

		public bool CountAsResource
		{
			get
			{
				return this.resourceReadoutPriority != ResourceCountPriority.Uncounted;
			}
		}

		public bool BlockPlanting
		{
			get
			{
				return (this.building == null || !this.building.SupportsPlants) && (this.blockPlants || this.category == ThingCategory.Plant || this.Fillage > FillCategory.None || this.IsEdifice());
			}
		}

		public List<VerbProperties> Verbs
		{
			get
			{
				if (this.verbs != null)
				{
					return this.verbs;
				}
				return ThingDef.EmptyVerbPropertiesList;
			}
		}

		public bool CanHaveFaction
		{
			get
			{
				if (this.IsBlueprint || this.IsFrame)
				{
					return true;
				}
				ThingCategory thingCategory = this.category;
				return thingCategory == ThingCategory.Pawn || thingCategory == ThingCategory.Building;
			}
		}

		public bool Claimable
		{
			get
			{
				return this.building != null && this.building.claimable;
			}
		}

		public ThingCategoryDef FirstThingCategory
		{
			get
			{
				if (this.thingCategories.NullOrEmpty<ThingCategoryDef>())
				{
					return null;
				}
				return this.thingCategories[0];
			}
		}

		public float MedicineTendXpGainFactor
		{
			get
			{
				return Mathf.Clamp(this.GetStatValueAbstract(StatDefOf.MedicalPotency, null) * 0.7f, 0.5f, 1f);
			}
		}

		public bool CanEverDeteriorate
		{
			get
			{
				return this.useHitPoints && (this.category == ThingCategory.Item || this == ThingDefOf.BurnedTree);
			}
		}

		public bool AffectsRegions
		{
			get
			{
				return this.passability == Traversability.Impassable || this.IsDoor;
			}
		}

		public bool AffectsReachability
		{
			get
			{
				return this.AffectsRegions || (this.passability == Traversability.Impassable || this.IsDoor) || TouchPathEndModeUtility.MakesOccupiedCellsAlwaysReachableDiagonally(this);
			}
		}

		public bool IsApparel
		{
			get
			{
				return this.apparel != null;
			}
		}

		public bool IsBed
		{
			get
			{
				return typeof(Building_Bed).IsAssignableFrom(this.thingClass);
			}
		}

		public bool IsCorpse
		{
			get
			{
				return typeof(Corpse).IsAssignableFrom(this.thingClass);
			}
		}

		public bool IsFrame
		{
			get
			{
				return this.isFrame;
			}
		}

		public bool IsBlueprint
		{
			get
			{
				return this.entityDefToBuild != null && this.category == ThingCategory.Ethereal;
			}
		}

		public bool IsStuff
		{
			get
			{
				return this.stuffProps != null;
			}
		}

		public bool IsMedicine
		{
			get
			{
				return this.statBases.StatListContains(StatDefOf.MedicalPotency);
			}
		}

		public bool IsDoor
		{
			get
			{
				return typeof(Building_Door).IsAssignableFrom(this.thingClass);
			}
		}

		public bool IsFilth
		{
			get
			{
				return this.filth != null;
			}
		}

		public bool IsIngestible
		{
			get
			{
				return this.ingestible != null;
			}
		}

		public bool IsNutritionGivingIngestible
		{
			get
			{
				return this.IsIngestible && this.ingestible.nutrition > 0f;
			}
		}

		public bool IsWeapon
		{
			get
			{
				return this.category == ThingCategory.Item && (!this.verbs.NullOrEmpty<VerbProperties>() || !this.tools.NullOrEmpty<Tool>());
			}
		}

		public bool IsCommsConsole
		{
			get
			{
				return typeof(Building_CommsConsole).IsAssignableFrom(this.thingClass);
			}
		}

		public bool IsOrbitalTradeBeacon
		{
			get
			{
				return typeof(Building_OrbitalTradeBeacon).IsAssignableFrom(this.thingClass);
			}
		}

		public bool IsFoodDispenser
		{
			get
			{
				return typeof(Building_NutrientPasteDispenser).IsAssignableFrom(this.thingClass);
			}
		}

		public bool IsDrug
		{
			get
			{
				return this.ingestible != null && this.ingestible.drugCategory != DrugCategory.None;
			}
		}

		public bool IsPleasureDrug
		{
			get
			{
				return this.IsDrug && this.ingestible.joy > 0f;
			}
		}

		public bool IsNonMedicalDrug
		{
			get
			{
				return this.IsDrug && this.ingestible.drugCategory != DrugCategory.Medical;
			}
		}

		public bool IsTable
		{
			get
			{
				return this.surfaceType == SurfaceType.Eat && this.HasComp(typeof(CompGatherSpot));
			}
		}

		public bool IsWorkTable
		{
			get
			{
				return typeof(Building_WorkTable).IsAssignableFrom(this.thingClass);
			}
		}

		public bool IsShell
		{
			get
			{
				return this.projectileWhenLoaded != null;
			}
		}

		public bool IsArt
		{
			get
			{
				return this.IsWithinCategory(ThingCategoryDefOf.Art);
			}
		}

		public bool IsAddictiveDrug
		{
			get
			{
				CompProperties_Drug compProperties = this.GetCompProperties<CompProperties_Drug>();
				return compProperties != null && compProperties.addictiveness > 0f;
			}
		}

		public bool IsMeat
		{
			get
			{
				return this.category == ThingCategory.Item && this.thingCategories != null && this.thingCategories.Contains(ThingCategoryDefOf.MeatRaw);
			}
		}

		public bool IsLeather
		{
			get
			{
				return this.category == ThingCategory.Item && this.thingCategories != null && this.thingCategories.Contains(ThingCategoryDefOf.Leathers);
			}
		}

		public bool IsRangedWeapon
		{
			get
			{
				if (!this.IsWeapon)
				{
					return false;
				}
				if (!this.verbs.NullOrEmpty<VerbProperties>())
				{
					for (int i = 0; i < this.verbs.Count; i++)
					{
						if (!this.verbs[i].MeleeRange)
						{
							return true;
						}
					}
				}
				return false;
			}
		}

		public bool IsMeleeWeapon
		{
			get
			{
				return this.IsWeapon && !this.IsRangedWeapon;
			}
		}

		public bool IsBuildingArtificial
		{
			get
			{
				return (this.category == ThingCategory.Building || this.IsFrame) && (this.building == null || (!this.building.isNaturalRock && !this.building.isResourceRock));
			}
		}

		public string LabelAsStuff
		{
			get
			{
				if (!this.stuffProps.stuffAdjective.NullOrEmpty())
				{
					return this.stuffProps.stuffAdjective;
				}
				return this.label;
			}
		}

		public Thing GetConcreteExample(ThingDef stuff = null)
		{
			if (this.concreteExamplesInt == null)
			{
				this.concreteExamplesInt = new Dictionary<ThingDef, Thing>();
			}
			if (stuff == null)
			{
				stuff = ThingDefOf.Steel;
			}
			if (!this.concreteExamplesInt.ContainsKey(stuff))
			{
				if (this.race == null)
				{
					this.concreteExamplesInt[stuff] = ThingMaker.MakeThing(this, (!base.MadeFromStuff) ? null : stuff);
				}
				else
				{
					this.concreteExamplesInt[stuff] = PawnGenerator.GeneratePawn((from pkd in DefDatabase<PawnKindDef>.AllDefsListForReading
					where pkd.race == this
					select pkd).FirstOrDefault<PawnKindDef>(), null);
				}
			}
			return this.concreteExamplesInt[stuff];
		}

		public List<Verb> GetConcreteExampleVerbs(Def def, ThingDef stuff = null)
		{
			List<Verb> result = null;
			ThingDef thingDef = def as ThingDef;
			if (thingDef != null)
			{
				Thing concreteExample = thingDef.GetConcreteExample(stuff);
				if (concreteExample is Pawn)
				{
					result = (concreteExample as Pawn).verbTracker.AllVerbs;
				}
				else if (concreteExample is ThingWithComps)
				{
					result = (concreteExample as ThingWithComps).GetComp<CompEquippable>().AllVerbs;
				}
			}
			HediffDef hediffDef = def as HediffDef;
			if (hediffDef != null)
			{
				Hediff concreteExample2 = hediffDef.ConcreteExample;
				result = concreteExample2.TryGetComp<HediffComp_VerbGiver>().VerbTracker.AllVerbs;
			}
			return result;
		}

		public CompProperties CompDefFor<T>() where T : ThingComp
		{
			return this.comps.FirstOrDefault((CompProperties c) => c.compClass == typeof(T));
		}

		public CompProperties CompDefForAssignableFrom<T>() where T : ThingComp
		{
			return this.comps.FirstOrDefault((CompProperties c) => typeof(T).IsAssignableFrom(c.compClass));
		}

		public bool HasComp(Type compType)
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				if (this.comps[i].compClass == compType)
				{
					return true;
				}
			}
			return false;
		}

		public T GetCompProperties<T>() where T : CompProperties
		{
			for (int i = 0; i < this.comps.Count; i++)
			{
				T t = this.comps[i] as T;
				if (t != null)
				{
					return t;
				}
			}
			return (T)((object)null);
		}

		public override void PostLoad()
		{
			if (this.graphicData != null)
			{
				LongEventHandler.ExecuteWhenFinished(delegate
				{
					if (this.graphicData.shaderType == ShaderType.None)
					{
						this.graphicData.shaderType = ShaderType.Cutout;
					}
					this.graphic = this.graphicData.Graphic;
				});
			}
			if (this.verbs != null && this.verbs.Count == 1)
			{
				this.verbs[0].label = this.label;
			}
			base.PostLoad();
			if (this.category == ThingCategory.Building && this.building == null)
			{
				this.building = new BuildingProperties();
			}
			if (this.building != null)
			{
				this.building.PostLoadSpecial(this);
			}
			if (this.plant != null)
			{
				this.plant.PostLoadSpecial(this);
			}
		}

		public override void ResolveReferences()
		{
			base.ResolveReferences();
			if (this.building != null)
			{
				this.building.ResolveReferencesSpecial();
			}
			if (this.graphicData != null)
			{
				this.graphicData.ResolveReferencesSpecial();
			}
			if (this.race != null)
			{
				this.race.ResolveReferencesSpecial();
			}
			if (this.soundImpactDefault == null)
			{
				this.soundImpactDefault = SoundDefOf.BulletImpactGround;
			}
			if (this.soundDrop == null)
			{
				this.soundDrop = SoundDefOf.Standard_Drop;
			}
			if (this.soundPickup == null)
			{
				this.soundPickup = SoundDefOf.Standard_Pickup;
			}
			if (this.soundInteract == null)
			{
				this.soundPickup = SoundDefOf.Standard_Pickup;
			}
			if (this.inspectorTabs != null && this.inspectorTabs.Any<Type>())
			{
				this.inspectorTabsResolved = new List<InspectTabBase>();
				for (int i = 0; i < this.inspectorTabs.Count; i++)
				{
					try
					{
						this.inspectorTabsResolved.Add(InspectTabManager.GetSharedInstance(this.inspectorTabs[i]));
					}
					catch (Exception ex)
					{
						Log.Error(string.Concat(new object[]
						{
							"Could not instantiate inspector tab of type ",
							this.inspectorTabs[i],
							": ",
							ex
						}));
					}
				}
			}
			if (this.comps != null)
			{
				for (int j = 0; j < this.comps.Count; j++)
				{
					this.comps[j].ResolveReferences(this);
				}
			}
		}

		[DebuggerHidden]
		public override IEnumerable<string> ConfigErrors()
		{
			ThingDef.<ConfigErrors>c__Iterator0 <ConfigErrors>c__Iterator = new ThingDef.<ConfigErrors>c__Iterator0();
			<ConfigErrors>c__Iterator.$this = this;
			ThingDef.<ConfigErrors>c__Iterator0 expr_0E = <ConfigErrors>c__Iterator;
			expr_0E.$PC = -2;
			return expr_0E;
		}

		public static ThingDef Named(string defName)
		{
			return DefDatabase<ThingDef>.GetNamed(defName, true);
		}

		public bool IsWithinCategory(ThingCategoryDef category)
		{
			if (this.thingCategories == null)
			{
				return false;
			}
			for (int i = 0; i < this.thingCategories.Count; i++)
			{
				if (this.thingCategories[i] == category || this.thingCategories[i].Parents.Contains(category))
				{
					return true;
				}
			}
			return false;
		}

		[DebuggerHidden]
		public override IEnumerable<StatDrawEntry> SpecialDisplayStats()
		{
			ThingDef.<SpecialDisplayStats>c__Iterator1 <SpecialDisplayStats>c__Iterator = new ThingDef.<SpecialDisplayStats>c__Iterator1();
			<SpecialDisplayStats>c__Iterator.$this = this;
			ThingDef.<SpecialDisplayStats>c__Iterator1 expr_0E = <SpecialDisplayStats>c__Iterator;
			expr_0E.$PC = -2;
			return expr_0E;
		}
	}
}
