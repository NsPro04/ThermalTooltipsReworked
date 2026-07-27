using HarmonyLib;
using UnityEngine;
using Database;

using System;
using System.IO;
using System.Reflection;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using STRINGS;

using TemperatureUnit = GameUtil.TemperatureUnit;

// ATTENTION!
// Yeah, it's a massive sheet of code with LOTS of duplication
// Please, stay safe - don't read this, lol

using TTRS = ThermalTooltipsReworked.ThermalTooltipsReworkedStrings;

namespace ThermalTooltipsReworked {
	public sealed class ThermalTooltipsReworkedMain : KMod.UserMod2 {
		public override void OnLoad(Harmony harmony) {
			base.OnLoad(harmony);

			{
				var __OS = AccessTools.Method(typeof(MainMenu), "OnSpawn");
				var __OSPostfix = AccessTools.Method(typeof(ThermalTooltipsReworkedMain), "OnSpawnPostfix");

				harmony.Patch(__OS, null, new HarmonyMethod(__OSPostfix));
				if (false) OnSpawnPostfix();
			}

			{
				var __I = AccessTools.Method(typeof(Localization), "Initialize");
				var __IPostfix = AccessTools.Method(typeof(ThermalTooltipsReworkedMain), "InitializePostfix");

				harmony.Patch(__I, null, new HarmonyMethod(__IPostfix));
				if (false) InitializePostfix();
			}

			{
				var __UHE = AccessTools.Method(typeof(SelectToolHoverTextCard), "UpdateHoverElements");
				var __UHEPrefix = AccessTools.Method(typeof(ThermalTooltipsReworkedMain), "UpdateHoverElementsPrefix");

				harmony.Patch(__UHE, new HarmonyMethod(__UHEPrefix));
				if (false) UpdateHoverElementsPrefix(null, null);
			}

			{
				var __SE = AccessTools.Method(typeof(MaterialSelector), "SetEffects");
				var __SEPrefix = AccessTools.Method(typeof(ThermalTooltipsReworkedMain), "SetEffectsPrefix");

				harmony.Patch(__SE, new HarmonyMethod(__SEPrefix));
				if (false) SetEffectsPrefix(null, null);
			}
		}

		private static void OnSpawnPostfix() {
			KMod.Manager modManager = Global.Instance.modManager;

			bool restartRequested = false;

			foreach (KMod.Mod mod in modManager.mods) {
				if (!mod.IsActive()) continue;

				if (mod.staticID == "PeterHan.ThermalTooltips"         || /*1983504552*/
					mod.staticID == "MaterialSelectionProperties"      || /*2600818338*/
					mod.staticID == "multiTemps"                       || /*3506650482*/
					mod.staticID == "ahc.HighPrecisionTemperature"     || /*3022469420*/
					mod.staticID == "ahc.TruthfulThermalConductivity"  || /*3022436400*/
					mod.staticID == "DOLj.TemperaturE-05"              || /*2729922307*/
					mod.staticID == "RP.Temperatur-0E5"                || /*3562130565*/
					(mod.label.id == "1737903327" || mod.label.title == "DisplayAllTemps")                   || /*1737903327*/
					(mod.label.id == "1878592057" || mod.label.title == "CustomTemperatureOverlay")          || /*1878592057*/
					(mod.label.id == "3647713426" || mod.label.title == "High Precision Temperature FIXED")  || /*3647713426*/
					(mod.label.id == "3636600859" || mod.label.title == "Controlled Overlay")                || /*3636600859*/
					(mod.label.id == "2563149160" || mod.label.title == "HeatMap")                           || /*2563149160*/
					false)
				{
					restartRequested = true;
					modManager.EnableMod(mod.label, false, null);
					modManager.events.Add(new KMod.Event {
						event_type = KMod.EventType.RestartRequested,
						mod = mod.label
					});
				}
			}

			if (restartRequested) {
				modManager.RestartDialog(UI.FRONTEND.MOD_DIALOGS.MODS_SCREEN_CHANGES.TITLE, UI.FRONTEND.MOD_DIALOGS.MODS_SCREEN_CHANGES.MESSAGE, null, true, null);
			}
		}

		private static void InitializePostfix() {
			Type TTRSType = typeof(TTRS);
			Localization.RegisterForTranslation(TTRSType);
			do {
				string code = Localization.GetLocale()?.Code;
				if (code.IsNullOrWhiteSpace()) break;

				string path = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "translations", Localization.GetLocale().Code + ".po");
				if (!File.Exists(path)) break;

				Localization.OverloadStrings(Localization.LoadStringsFile(path, false));
			} while (false);
			LocString.CreateLocStringKeys(TTRSType, null);
		}

		private static bool UpdateHoverElementsPrefix(SelectToolHoverTextCard __instance, List<KSelectable> hoverObjects) {
			int cell = Grid.PosToCell(Camera.main.ScreenToWorldPoint(KInputManager.GetMousePos()));

			if (hoverObjects == null ||
				__instance.iconDash == null ||
				OverlayScreen.Instance == null ||
				OverlayScreen.Instance.GetMode() != OverlayModes.Temperature.ID ||
				Game.Instance.temperatureOverlayMode == Game.TemperatureOverlayModes.AdaptiveTemperature ||
				Game.Instance.temperatureOverlayMode == Game.TemperatureOverlayModes.HeatFlow ||
				Grid.IsValidCell(cell) == false ||
				Grid.IsVisible(cell) == false ||
				Grid.WorldIdx[cell] != ClusterManager.Instance.activeWorldId
				)
				return true;

			__instance.recentNumberOfDisplayedSelectables = 0;
			__instance.currentSelectedSelectableIndex = 0;

			HoverTextDrawer hoverTextDrawer = HoverTextScreen.Instance.BeginDrawing();
			if (BICCompatibility.IsEnabled) BICCompatibility.InterceptHoverDrawer.IsInterceptMode = false;

			FieldInfo __currentPos = typeof(HoverTextDrawer).GetField("currentPos", BindingFlags.Instance | BindingFlags.NonPublic);

			TextStyleSetting TitleStyle = ScriptableObject.CreateInstance<TextStyleSetting>();
			{
				TitleStyle.sdfFont   = __instance.Styles_Title.Standard.sdfFont;
				TitleStyle.fontSize  = __instance.Styles_Title.Standard.fontSize;
				TitleStyle.textColor = __instance.Styles_Title.Standard.textColor;
				TitleStyle.style     = __instance.Styles_Title.Standard.style;
				TitleStyle.enableWordWrapping = false;
			}

			TextStyleSetting BodyStyle = ScriptableObject.CreateInstance<TextStyleSetting>();
			{
				BodyStyle.sdfFont   = __instance.Styles_BodyText.Standard.sdfFont;
				BodyStyle.fontSize  = __instance.Styles_BodyText.Standard.fontSize;
				BodyStyle.textColor = __instance.Styles_BodyText.Standard.textColor;
				BodyStyle.style     = __instance.Styles_BodyText.Standard.style;
				BodyStyle.enableWordWrapping = false;
			}

			TextStyleSetting PropertyStyle = ScriptableObject.CreateInstance<TextStyleSetting>();
			{
				PropertyStyle.sdfFont   = __instance.Styles_Values.Property.Standard.sdfFont;
				PropertyStyle.fontSize  = __instance.Styles_Values.Property.Standard.fontSize;
				PropertyStyle.textColor = __instance.Styles_Values.Property.Standard.textColor;
				PropertyStyle.style     = __instance.Styles_Values.Property.Standard.style;
				PropertyStyle.enableWordWrapping = false;
			}

			if (!BICCompatibility.MaxTempWidth.HasValue())
				BICCompatibility.MaxTempWidth.Init(hoverTextDrawer, "°C: 0.000000000", BodyStyle);

			Sprite icon_dash = __instance.iconDash;
			Sprite icon_warning  = __instance.iconWarning;
			Sprite state_temp_down = Assets.GetSprite("crew_state_temp_down");
			Sprite state_temp_up   = Assets.GetSprite("crew_state_temp_up");

			List<KSelectable> FilteredHoverObjects = new List<KSelectable>();

			for (int i = 0; i < hoverObjects.Count; ++i) {
				KSelectable hoverObject = hoverObjects[i];

				if (ICellSelectionProxy.IsSelectionProxy(hoverObject.gameObject)) continue;

				if (hoverObject == null) continue;

				{
					int __maskOverlay = (int)typeof(SelectToolHoverTextCard).GetField("maskOverlay", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);

					if ((hoverObject.gameObject.layer & __maskOverlay) != 0) continue;
				}

				if (hoverObject.GetComponent<KPrefabID>() == null ||
					hoverObject.GetComponent<PrimaryElement>() == null) continue;

				FilteredHoverObjects.Add(hoverObject);
			}

			for (int i = 0; i < FilteredHoverObjects.Count; ++i) { // buildings
				KSelectable hoverObject = FilteredHoverObjects[i];
				//BICCompatibility.ExportSelectToolData.curSelectable = hoverObject; // BIC

				bool selected = SelectTool.Instance.selected == hoverObject;

				Building objectBuilding             = hoverObject.GetComponent<Building>();
				PrimaryElement objectPrimaryElement = hoverObject.GetComponent<PrimaryElement>();

				Element element = objectPrimaryElement.Element;

				hoverTextDrawer.BeginShadowBar(selected);

				{ // title
					string title = GameUtil.GetUnitFormattedName(hoverObject.gameObject, true);

					if (element != null && element.nameUpperCase != null) {
						title = StringFormatter.Replace(StringFormatter.Replace(UI.TOOLS.GENERIC.BUILDING_HOVER_NAME_FMT, "{Name}", title), "{Element}", element.nameUpperCase);
					}

					//BICCompatibility.ExportSelectToolData.GetSelectInfo_Patch.ExportGO(BICCompatibility.ConverterManager.title); // BIC
					hoverTextDrawer.DrawText(title, TitleStyle);
				}

				{ // statuses
					StatusItem.StatusItemOverlays statusItemOverlays = StatusItem.GetStatusItemOverlayBySimViewMode(OverlayScreen.Instance.GetMode());

					Func<StatusItemGroup.Entry, bool> IsStatusItemWarning = (StatusItemGroup.Entry status_item) => (status_item.item.notificationType == NotificationType.Bad || status_item.item.notificationType == NotificationType.BadMinor || status_item.item.notificationType == NotificationType.DuplicantThreatening);
					Func<StatusItem, bool> ShowStatusItemInCurrentOverlay = (StatusItem status) => (((uint)status.status_overlays & (uint)statusItemOverlays) == (uint)statusItemOverlays);

					for (int step = 1; step <= 2; ++step) {
						foreach (StatusItemGroup.Entry status_item in hoverObject.GetStatusItemGroup()) {
							if (!ShowStatusItemInCurrentOverlay(status_item.item)) continue;

							{
								MiscStatusItems MiscStatusItems = Db.Get().MiscStatusItems;

								if (status_item.item.Id == MiscStatusItems.ElementalTemperature.Id ||
									status_item.item.Id == MiscStatusItems.ElementalMass.Id ||
									status_item.item.Id == MiscStatusItems.OreMass.Id ||
									status_item.item.Id == MiscStatusItems.OreTemp.Id ||
									status_item.item.Id == MiscStatusItems.BackwallMass.Id ||
									status_item.item.Id == MiscStatusItems.BackwallTemperature.Id)
									continue;
							}

							if ((step == 1) && (status_item.category != null && status_item.category.Id == "Main") ||
								(step == 2) && (status_item.category == null || status_item.category.Id != "Main")) {
								hoverTextDrawer.NewLine();
								{
									Sprite icon = (status_item.item.sprite != null) ? status_item.item.sprite.sprite : icon_warning;
									Color color = (IsStatusItemWarning(status_item) ? __instance.HoverTextStyleSettings[1].textColor : BodyStyle.textColor);
									hoverTextDrawer.DrawIcon(icon, color);
								}
								{
									TextStyleSetting style = (IsStatusItemWarning(status_item) ? __instance.HoverTextStyleSettings[1] : BodyStyle);

									//BICCompatibility.ExportSelectToolData.GetSelectInfo_Patch.Export(status_item.item.Id, status_item.data); // BIC
									hoverTextDrawer.DrawText(status_item.GetName(), style);
								}
							}
						}
					}
				}

				float mass = 0;

				{ // mass
					mass = objectPrimaryElement.Mass;

					hoverTextDrawer.NewLine();
					hoverTextDrawer.DrawIcon(icon_dash);

					float mass2 = float.NaN;

					do {
						if (objectBuilding == null) break;

						SimCellOccupier objectSimCellOccupier = hoverObject.GetComponent<SimCellOccupier>();

						if (objectSimCellOccupier == null) {
							mass2 = objectBuilding.Def.MassForTemperatureModification;
							break;
						}

						if (objectBuilding.Def.UseStructureTemperature && objectSimCellOccupier.doReplaceElement) {
							mass2 = objectBuilding.Def.MassForTemperatureModification + mass;
						}
					} while (false);

					if (float.IsNaN(mass2)) {
						hoverTextDrawer.DrawText(string.Format(UI.ELEMENTAL.MASS.NAME, GameUtil.GetFormattedMass(mass)), BodyStyle);
					} else {
						hoverTextDrawer.DrawText(string.Format(UI.ELEMENTAL.MASS.NAME, string.Format("{0} ({1})", GameUtil.GetFormattedMass(mass), GameUtil.GetFormattedMass(mass2))), BodyStyle);
						mass = mass2;
					}
				}

				float temperature = 0;

				{ // temperature
					temperature = objectPrimaryElement.Temperature;

					hoverTextDrawer.NewLine();
					hoverTextDrawer.DrawIcon(icon_dash);

					float __currentPosX = ((Vector2)__currentPos.GetValue(hoverTextDrawer)).x;

					List<TemperatureUnit> sortedUnits = Utils.GetTemperatureUnits(all: true);

					for (int idx = 0; idx <= sortedUnits.Count; ++idx) {
						__currentPos.SetValue(hoverTextDrawer, new Vector2(__currentPosX + BICCompatibility.MaxTempWidth.Get() * idx, ((Vector2)__currentPos.GetValue(hoverTextDrawer)).y));

						if (idx == sortedUnits.Count) {
							int icon_size = BICCompatibility.Options.CardSize.ShouldOverride ? -BICCompatibility.Options.CardSize.IconSizeChange : 0;
							hoverTextDrawer.DrawIcon(icon_dash, icon_size); // indent (dirty hack)
							break;
						}

						TemperatureUnit unit = sortedUnits[idx];

						hoverTextDrawer.DrawText(string.Format("{0}: {1}",
							Utils.GetTemperatureUnitSuffix(unit), GameUtil.GetTemperatureConvertedFromKelvin(temperature, unit).ToPrecisionString()
						), BodyStyle);
					}
				}

				{ // thermal conductivity
					float tc = element.thermalConductivity;

					if (objectBuilding != null) {
						float building_tc = objectBuilding.Def.ThermalConductivity;

						if (objectBuilding.GetComponent<Insulator>() != null) {
							float tc_modifier = 1.53787e-05f; // (1/255)^2
							int idk = (int)(building_tc * 255.0);
							tc *= idk * idk * tc_modifier;
						} else {
							tc *= building_tc;
						}
					}

					hoverTextDrawer.NewLine();
					hoverTextDrawer.DrawIcon(icon_dash);
					{
						float final_tc = GameUtil.GetDisplayThermalConductivity(tc);

						string final_tc_str = Math.Abs(final_tc) > 0.001 ? final_tc.ToPrecisionString() : final_tc.ToString("0.00e+0", CultureInfo.InvariantCulture);

						hoverTextDrawer.DrawText(string.Format("{0} (DTU/(m*s))/{1}", string.Format(UI.ELEMENTAL.THERMALCONDUCTIVITY.NAME, final_tc_str), Utils.GetTemperatureUnitSuffix()), BodyStyle);
					}
				}

				float shc = 0;

				{ // specific heat capacity
					shc = element.specificHeatCapacity;

					hoverTextDrawer.NewLine();
					hoverTextDrawer.DrawIcon(icon_dash);
					hoverTextDrawer.DrawText(string.Format("{0} (DTU/g)/{1}", string.Format(UI.ELEMENTAL.SHC.NAME, GameUtil.GetDisplaySHC(shc).ToPrecisionString()), Utils.GetTemperatureUnitSuffix()), BodyStyle);
				}

				{ // thermal mass
					hoverTextDrawer.NewLine();
					hoverTextDrawer.DrawIcon(icon_dash);
					hoverTextDrawer.DrawText(string.Format("{0} (kDTU)/{1}", string.Format(TTRS.UI.THERMALTOOLTIPS.THERMAL_MASS, (mass * GameUtil.GetDisplaySHC(shc)).ToPrecisionString()), Utils.GetTemperatureUnitSuffix()), BodyStyle);
				}

				{ // heat energy
					hoverTextDrawer.NewLine();
					hoverTextDrawer.DrawIcon(icon_dash);
					hoverTextDrawer.DrawText(string.Format("{0} kDTU", string.Format(TTRS.UI.THERMALTOOLTIPS.HEAT_ENERGY, (mass * shc * temperature).ToPrecisionString())), BodyStyle);
				}

				{ // high temp transition
					Element highPrimaryElement = element.highTempTransition;

					if (highPrimaryElement != null) {
						Element highSecondaryElement = ElementLoader.FindElementByHash(element.highTempTransitionOreID);

						hoverTextDrawer.NewLine();
						hoverTextDrawer.DrawIcon(icon_dash);
						hoverTextDrawer.DrawIcon(state_temp_up, 22);
						hoverTextDrawer.DrawText(Utils.GetTemperatures(element.highTemp + 3f, Utils.GetTemperatureUnits(), format: "0.##"), BodyStyle);

						{
							Tuple<Sprite, Color> UISprite = Def.GetUISprite(highPrimaryElement);

							hoverTextDrawer.DrawIcon(UISprite.first, UISprite.second, 22);

							hoverTextDrawer.DrawText(highPrimaryElement.name, BodyStyle);
						}

						if (highSecondaryElement != null) {
							Tuple<Sprite, Color> UISprite = Def.GetUISprite(highSecondaryElement);

							hoverTextDrawer.DrawIcon(UISprite.first, UISprite.second, 22);
							hoverTextDrawer.DrawText(highSecondaryElement.name, BodyStyle);
						}
					}
				}

				{ // low temp transition
					Element lowPrimaryElement = element.lowTempTransition;

					if (lowPrimaryElement != null) {
						Element lowSecondaryElement = ElementLoader.FindElementByHash(element.lowTempTransitionOreID);

						hoverTextDrawer.NewLine();
						hoverTextDrawer.DrawIcon(icon_dash);
						hoverTextDrawer.DrawIcon(state_temp_down, 22);
						hoverTextDrawer.DrawText(Utils.GetTemperatures(element.lowTemp - 3f, Utils.GetTemperatureUnits(), format: "0.##"), BodyStyle);

						{
							Tuple<Sprite, Color> UISprite = Def.GetUISprite(lowPrimaryElement);

							hoverTextDrawer.DrawIcon(UISprite.first, UISprite.second, 22);

							hoverTextDrawer.DrawText(lowPrimaryElement.name, BodyStyle);
						}

						if (lowSecondaryElement != null) {
							Tuple<Sprite, Color> UISprite = Def.GetUISprite(lowSecondaryElement);

							hoverTextDrawer.DrawIcon(UISprite.first, UISprite.second, 22);
							hoverTextDrawer.DrawText(lowSecondaryElement.name, BodyStyle);
						}
					}
				}

				hoverTextDrawer.EndShadowBar();
			}

			do { // cell
				if (Grid.DupePassable[cell] && Grid.Solid[cell]) break;

				Element element = Grid.Element[cell];

				if (element.IsSolid && FilteredHoverObjects.Any((KSelectable hoverObject) => {
					BuildingComplete objectBuilding = hoverObject.GetComponent<BuildingComplete>();
					return (objectBuilding != null && objectBuilding.Def.IsFoundation);
				})) break;

				bool selected = false;
				{
					CellSelectionObject cellSelectionObject = SelectTool.Instance.selected?.GetComponent<CellSelectionObject>();
					selected = cellSelectionObject != null && cellSelectionObject.mouseCell == cellSelectionObject.alternateSelectionObject.mouseCell;
				}

				hoverTextDrawer.BeginShadowBar(selected);

				{ // title
					hoverTextDrawer.DrawText(element.nameUpperCase, TitleStyle);
				}

				{ // disease
					int diseaseCount = Grid.DiseaseCount[cell];

					if (diseaseCount > 0) {
						hoverTextDrawer.NewLine();
						hoverTextDrawer.DrawIcon(icon_dash);
						hoverTextDrawer.DrawText(GameUtil.GetFormattedDisease(Grid.DiseaseIdx[cell], diseaseCount, color: true), PropertyStyle);
					}
				}

				{ // material category tag
					if (!element.IsVacuum && !BICCompatibility.Options.HideElementCategories) { // BIC
						hoverTextDrawer.NewLine();
						hoverTextDrawer.DrawIcon(icon_dash);
						hoverTextDrawer.DrawText(element.GetMaterialCategoryTag().ProperName(), BodyStyle);
					}
				}

				{ // space exposure
					if (CellSelectionObject.IsExposedToSpace(cell)) {
						hoverTextDrawer.NewLine();
						hoverTextDrawer.DrawIcon(icon_dash);
						hoverTextDrawer.DrawText(MISC.STATUSITEMS.SPACE.NAME, BodyStyle);
					}
				}

				{ // buried object
					if (Game.Instance.GetComponent<EntombedItemVisualizer>().IsEntombedItem(cell)) {
						hoverTextDrawer.NewLine();
						hoverTextDrawer.DrawIcon(icon_dash);
						hoverTextDrawer.DrawText(MISC.STATUSITEMS.BURIEDITEM.NAME, BodyStyle);
					}
				}

				do { // emitting
					SimHashes elementSublimateId = element.sublimateId;

					if (elementSublimateId == 0) break;

					if (!element.IsSolid) {
						if (!element.IsLiquid) break;

						int cellAbove = Grid.CellAbove(cell);

						if (!Grid.IsValidCell(cellAbove)) break;

						Element elementAbove = Grid.Element[cellAbove];

						if (!elementAbove.IsGas && !elementAbove.IsVacuum) break;
					}

					string element2Name = GameUtil.GetElementNameByElementHash(elementSublimateId);

					{
						float flowMass = Grid.AccumulatedFlow[cell] / 3f;

						hoverTextDrawer.NewLine();
						hoverTextDrawer.DrawIcon(icon_dash);

						hoverTextDrawer.DrawText(BUILDING.STATUSITEMS.EMITTINGGASAVG.NAME
							.Replace("{Element}", element2Name)
							.Replace("{FlowRate}", GameUtil.GetFormattedMass(flowMass, GameUtil.TimeSlice.PerSecond)),
							BodyStyle);
					}

					{
						GameUtil.IsEmissionBlocked(cell, out var all_not_gaseous, out var all_over_pressure);

						if (!all_not_gaseous && !all_over_pressure) break;

						string element1Name = element.tag.ProperName();

						hoverTextDrawer.NewLine();
						hoverTextDrawer.DrawIcon(icon_dash);

						hoverTextDrawer.DrawText((all_not_gaseous ? MISC.STATUSITEMS.SUBLIMATIONBLOCKED.NAME : MISC.STATUSITEMS.SUBLIMATIONOVERPRESSURE.NAME)
							.Replace("{Element}", element1Name)
							.Replace("{SubElement}", element2Name), BodyStyle);
					}
				} while (false);

				{ // bubbles
					if (BubbleManager.instance != null) {
						ListPool<BubbleManager.CellBubbleInfo, SelectToolHoverTextCard>.PooledList bubbles = ListPool<BubbleManager.CellBubbleInfo, SelectToolHoverTextCard>.Allocate();

						BubbleManager.instance.GetBubblesInCell(cell, bubbles);

						foreach (BubbleManager.CellBubbleInfo bubble in bubbles) {
							Element bubbleElement = ElementLoader.FindElementByHash(bubble.element);

							hoverTextDrawer.NewLine();
							hoverTextDrawer.DrawIcon(icon_dash);

							hoverTextDrawer.DrawText(string.Format("{0} {1}: {2}",
								bubbleElement.name,
								UI.TOOLS.GENERIC.BUBBLE_LABEL,
								GameUtil.GetFormattedMass(bubble.totalMass),

								Utils.GetTemperatures(element.highTemp, Utils.GetTemperatureUnits())
							), BodyStyle);
						}

						bubbles.Recycle();
					}
				}

				float mass = 0;

				{ // mass
					mass = Grid.Mass[cell];

					hoverTextDrawer.NewLine();
					hoverTextDrawer.DrawIcon(icon_dash);

					hoverTextDrawer.DrawText(string.Format("{0} {1}", string.Format(UI.ELEMENTAL.MASS.NAME, GameUtil.GetFormattedMass(mass)), GameUtil.GetBreathableString(element, mass)), BodyStyle);
				}

				float temperature = 0;

				{ // temperature
					temperature = Grid.Temperature[cell];

					hoverTextDrawer.NewLine();
					hoverTextDrawer.DrawIcon(icon_dash);

					float __currentPosX = ((Vector2)__currentPos.GetValue(hoverTextDrawer)).x;

					List<TemperatureUnit> sortedUnits = Utils.GetTemperatureUnits(all: true);

					for (int idx = 0; idx <= sortedUnits.Count; ++idx) {
						__currentPos.SetValue(hoverTextDrawer, new Vector2(__currentPosX + BICCompatibility.MaxTempWidth.Get() * idx, ((Vector2)__currentPos.GetValue(hoverTextDrawer)).y));

						if (idx == sortedUnits.Count) {
							int icon_size = BICCompatibility.Options.CardSize.ShouldOverride ? -BICCompatibility.Options.CardSize.IconSizeChange : 0;
							hoverTextDrawer.DrawIcon(icon_dash, icon_size); // indent (dirty hack)
							break;
						}

						TemperatureUnit unit = sortedUnits[idx];

						hoverTextDrawer.DrawText(string.Format("{0}: {1}",
							Utils.GetTemperatureUnitSuffix(unit), GameUtil.GetTemperatureConvertedFromKelvin(temperature, unit).ToPrecisionString()
						), BodyStyle);
					}
				}

				{ // thermal conductivity
					float tc = element.thermalConductivity;

					hoverTextDrawer.NewLine();
					hoverTextDrawer.DrawIcon(icon_dash);
					{
						float final_tc = GameUtil.GetDisplayThermalConductivity(tc);

						string final_tc_str = Math.Abs(final_tc) > 0.001 ? final_tc.ToPrecisionString() : final_tc.ToString("0.00e+0", CultureInfo.InvariantCulture);

						hoverTextDrawer.DrawText(string.Format("{0} (DTU/(m*s))/{1}", string.Format(UI.ELEMENTAL.THERMALCONDUCTIVITY.NAME, final_tc_str), Utils.GetTemperatureUnitSuffix()), BodyStyle);
					}
				}

				float shc = 0;

				{ // specific heat capacity
					shc = element.specificHeatCapacity;

					hoverTextDrawer.NewLine();
					hoverTextDrawer.DrawIcon(icon_dash);
					hoverTextDrawer.DrawText(string.Format("{0} (DTU/g)/{1}", string.Format(UI.ELEMENTAL.SHC.NAME, GameUtil.GetDisplaySHC(shc).ToPrecisionString()), Utils.GetTemperatureUnitSuffix()), BodyStyle);
				}

				{ // thermal mass
					hoverTextDrawer.NewLine();
					hoverTextDrawer.DrawIcon(icon_dash);
					hoverTextDrawer.DrawText(string.Format("{0} (kDTU)/{1}", string.Format(TTRS.UI.THERMALTOOLTIPS.THERMAL_MASS, (mass * GameUtil.GetDisplaySHC(shc)).ToPrecisionString()), Utils.GetTemperatureUnitSuffix()), BodyStyle);
				}

				{ // heat energy
					hoverTextDrawer.NewLine();
					hoverTextDrawer.DrawIcon(icon_dash);
					hoverTextDrawer.DrawText(string.Format("{0} kDTU", string.Format(TTRS.UI.THERMALTOOLTIPS.HEAT_ENERGY, (mass * shc * temperature).ToPrecisionString())), BodyStyle);
				}

				{ // high temp transition
					Element highPrimaryElement = element.highTempTransition;

					if (highPrimaryElement != null) {
						Element highSecondaryElement = ElementLoader.FindElementByHash(element.highTempTransitionOreID);

						hoverTextDrawer.NewLine();
						hoverTextDrawer.DrawIcon(icon_dash);
						hoverTextDrawer.DrawIcon(state_temp_up, 22);
						hoverTextDrawer.DrawText(Utils.GetTemperatures(element.highTemp + 3f, Utils.GetTemperatureUnits(), format: "0.##"), BodyStyle);

						{
							Tuple<Sprite, Color> UISprite = Def.GetUISprite(highPrimaryElement);

							hoverTextDrawer.DrawIcon(UISprite.first, UISprite.second, 22);

							hoverTextDrawer.DrawText(highPrimaryElement.name, BodyStyle);
						}

						if (highSecondaryElement != null) {
							Tuple<Sprite, Color> UISprite = Def.GetUISprite(highSecondaryElement);

							hoverTextDrawer.DrawIcon(UISprite.first, UISprite.second, 22);
							hoverTextDrawer.DrawText(highSecondaryElement.name, BodyStyle);
						}
					}
				}

				{ // low temp transition
					Element lowPrimaryElement = element.lowTempTransition;

					if (lowPrimaryElement != null) {
						Element lowSecondaryElement = ElementLoader.FindElementByHash(element.lowTempTransitionOreID);

						hoverTextDrawer.NewLine();
						hoverTextDrawer.DrawIcon(icon_dash);
						hoverTextDrawer.DrawIcon(state_temp_down, 22);
						hoverTextDrawer.DrawText(Utils.GetTemperatures(element.lowTemp - 3f, Utils.GetTemperatureUnits(), format: "0.##"), BodyStyle);

						{
							Tuple<Sprite, Color> UISprite = Def.GetUISprite(lowPrimaryElement);

							hoverTextDrawer.DrawIcon(UISprite.first, UISprite.second, 22);

							hoverTextDrawer.DrawText(lowPrimaryElement.name, BodyStyle);
						}

						if (lowSecondaryElement != null) {
							Tuple<Sprite, Color> UISprite = Def.GetUISprite(lowSecondaryElement);

							hoverTextDrawer.DrawIcon(UISprite.first, UISprite.second, 22);
							hoverTextDrawer.DrawText(lowSecondaryElement.name, BodyStyle);
						}
					}
				}

				hoverTextDrawer.EndShadowBar();
			} while (false);

			if (BackwallManager.HasBackwall(cell)) { // backwall
				bool selected =
					BackwallSelectionObject.Instance != null &&
					SelectTool.Instance.selected != null &&
					SelectTool.Instance.selected.GetComponent<BackwallSelectionObject>() != null &&
					BackwallSelectionObject.Instance.SelectedCell == cell;

				hoverTextDrawer.BeginShadowBar(selected);

				var backwall = BackwallManager.At(cell);
				Element backwallElement = backwall.Element;

				{ // title
					hoverTextDrawer.DrawText(string.Format("{0} {1}", backwallElement.nameUpperCase, UI.TOOLS.GENERIC.NATURAL_BACKWALL_LABEL), TitleStyle);
				}

				float mass = 0;

				{ // mass
					mass = backwall.Mass;

					hoverTextDrawer.NewLine();
					hoverTextDrawer.DrawIcon(icon_dash);

					hoverTextDrawer.DrawText(string.Format(UI.ELEMENTAL.MASS.NAME, GameUtil.GetFormattedMass(mass)), BodyStyle);
				}

				float temperature = 0;

				{ // temperature
					temperature = backwall.Temperature;

					hoverTextDrawer.NewLine();
					hoverTextDrawer.DrawIcon(icon_dash);

					float __currentPosX = ((Vector2)__currentPos.GetValue(hoverTextDrawer)).x;

					List<TemperatureUnit> sortedUnits = Utils.GetTemperatureUnits(all: true);

					for (int idx = 0; idx <= sortedUnits.Count; ++idx) {
						__currentPos.SetValue(hoverTextDrawer, new Vector2(__currentPosX + BICCompatibility.MaxTempWidth.Get() * idx, ((Vector2)__currentPos.GetValue(hoverTextDrawer)).y));

						if (idx == sortedUnits.Count) {
							int icon_size = BICCompatibility.Options.CardSize.ShouldOverride ? -BICCompatibility.Options.CardSize.IconSizeChange : 0;
							hoverTextDrawer.DrawIcon(icon_dash, icon_size); // indent (dirty hack)
							break;
						}

						TemperatureUnit unit = sortedUnits[idx];

						hoverTextDrawer.DrawText(string.Format("{0}: {1}",
							Utils.GetTemperatureUnitSuffix(unit), GameUtil.GetTemperatureConvertedFromKelvin(temperature, unit).ToPrecisionString()
						), BodyStyle);
					}
				}

				{ // thermal conductivity
					float tc = backwallElement.thermalConductivity;

					hoverTextDrawer.NewLine();
					hoverTextDrawer.DrawIcon(icon_dash);
					{
						float final_tc = GameUtil.GetDisplayThermalConductivity(tc);

						string final_tc_str = Math.Abs(final_tc) > 0.001 ? final_tc.ToPrecisionString() : final_tc.ToString("0.00e+0", CultureInfo.InvariantCulture);

						hoverTextDrawer.DrawText(string.Format("{0} (DTU/(m*s))/{1}", string.Format(UI.ELEMENTAL.THERMALCONDUCTIVITY.NAME, final_tc_str), Utils.GetTemperatureUnitSuffix()), BodyStyle);
					}
				}

				float shc = 0;

				{ // specific heat capacity
					shc = backwallElement.specificHeatCapacity;

					hoverTextDrawer.NewLine();
					hoverTextDrawer.DrawIcon(icon_dash);
					hoverTextDrawer.DrawText(string.Format("{0} (DTU/g)/{1}", string.Format(UI.ELEMENTAL.SHC.NAME, GameUtil.GetDisplaySHC(shc).ToPrecisionString()), Utils.GetTemperatureUnitSuffix()), BodyStyle);
				}

				{ // thermal mass
					hoverTextDrawer.NewLine();
					hoverTextDrawer.DrawIcon(icon_dash);
					hoverTextDrawer.DrawText(string.Format("{0} (kDTU)/{1}", string.Format(TTRS.UI.THERMALTOOLTIPS.THERMAL_MASS, (mass * GameUtil.GetDisplaySHC(shc)).ToPrecisionString()), Utils.GetTemperatureUnitSuffix()), BodyStyle);
				}

				{ // heat energy
					hoverTextDrawer.NewLine();
					hoverTextDrawer.DrawIcon(icon_dash);
					hoverTextDrawer.DrawText(string.Format("{0} kDTU", string.Format(TTRS.UI.THERMALTOOLTIPS.HEAT_ENERGY, (mass * shc * temperature).ToPrecisionString())), BodyStyle);
				}

				{ // high temp transition
					Element highPrimaryElement = backwallElement.highTempTransition;

					if (highPrimaryElement != null) {
						Element highSecondaryElement = ElementLoader.FindElementByHash(backwallElement.highTempTransitionOreID);

						hoverTextDrawer.NewLine();
						hoverTextDrawer.DrawIcon(icon_dash);
						hoverTextDrawer.DrawIcon(state_temp_up, 22);
						hoverTextDrawer.DrawText(Utils.GetTemperatures(backwallElement.highTemp + 3f, Utils.GetTemperatureUnits(), format: "0.##"), BodyStyle);

						{
							Tuple<Sprite, Color> UISprite = Def.GetUISprite(highPrimaryElement);

							hoverTextDrawer.DrawIcon(UISprite.first, UISprite.second, 22);

							hoverTextDrawer.DrawText(highPrimaryElement.name, BodyStyle);
						}

						if (highSecondaryElement != null) {
							Tuple<Sprite, Color> UISprite = Def.GetUISprite(highSecondaryElement);

							hoverTextDrawer.DrawIcon(UISprite.first, UISprite.second, 22);
							hoverTextDrawer.DrawText(highSecondaryElement.name, BodyStyle);
						}
					}
				}

				{ // low temp transition
					Element lowPrimaryElement = backwallElement.lowTempTransition;

					if (lowPrimaryElement != null) {
						Element lowSecondaryElement = ElementLoader.FindElementByHash(backwallElement.lowTempTransitionOreID);

						hoverTextDrawer.NewLine();
						hoverTextDrawer.DrawIcon(icon_dash);
						hoverTextDrawer.DrawIcon(state_temp_down, 22);
						hoverTextDrawer.DrawText(Utils.GetTemperatures(backwallElement.lowTemp - 3f, Utils.GetTemperatureUnits(), format: "0.##"), BodyStyle);

						{
							Tuple<Sprite, Color> UISprite = Def.GetUISprite(lowPrimaryElement);

							hoverTextDrawer.DrawIcon(UISprite.first, UISprite.second, 22);

							hoverTextDrawer.DrawText(lowPrimaryElement.name, BodyStyle);
						}

						if (lowSecondaryElement != null) {
							Tuple<Sprite, Color> UISprite = Def.GetUISprite(lowSecondaryElement);

							hoverTextDrawer.DrawIcon(UISprite.first, UISprite.second, 22);
							hoverTextDrawer.DrawText(lowSecondaryElement.name, BodyStyle);
						}
					}
				}

				hoverTextDrawer.EndShadowBar();
			}

			FilteredHoverObjects.Clear();

			hoverTextDrawer.EndDrawing();

			return false;
		}

		private static bool SetEffectsPrefix(MaterialSelector __instance, Tag element) {
			if (__instance.MaterialDescriptionPane == null) return true;

			Element selectedElement = ElementLoader.GetElement(element);

			if (selectedElement == null) return true;

			__instance.MaterialEffectsPane.gameObject.SetActive(value: true);

			List<Descriptor> descriptors = GameUtil.GetMaterialDescriptors(element);

			do {
				if (descriptors.Count == 0) break;

				Descriptor header = default;
				header.SetupDescriptor(ELEMENTS.MATERIAL_MODIFIERS.EFFECTS_HEADER, ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.EFFECTS_HEADER);

				descriptors.Insert(0, header);
			} while (false);

			do {
				Recipe __activeRecipe = (Recipe)typeof(MaterialSelector).GetField("activeRecipe", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(__instance);

				if (__activeRecipe == null || __activeRecipe.Ingredients.Count == 0) break;

				GameObject prefab = Assets.GetPrefab(__activeRecipe.Result);

				BuildingDef buildingDef = prefab?.GetComponent<Building>()?.Def;

				PrimaryElement buildingPrimaryElement = prefab.GetComponent<PrimaryElement>();

				if (buildingDef == null || buildingPrimaryElement == null) break;

				{
					string[] tags = __activeRecipe.Ingredients[0].tag.ToString().Split('&');

					if (!tags.Contains(selectedElement.tag.Name) && !selectedElement.oreTags.Any((Tag tag) => tags.Contains(tag.Name))) break;
				}

				{ // header
					Descriptor header = default;
					header.SetupDescriptor("<b>Advanced Stats:</b>", ELEMENTS.MATERIAL_MODIFIERS.TOOLTIP.EFFECTS_HEADER);

					descriptors.Add(header);
				}

				float mass = 0;

				{ // mass
					mass = buildingPrimaryElement.Mass;

					float mass2 = float.NaN;

					do {
						SimCellOccupier objectSimCellOccupier = prefab.GetComponent<SimCellOccupier>();

						if (objectSimCellOccupier == null) {
							mass2 = buildingDef.MassForTemperatureModification;
							break;
						}

						if (buildingDef.UseStructureTemperature && objectSimCellOccupier.doReplaceElement) {
							mass2 = buildingDef.MassForTemperatureModification + mass;
						}
					} while (false);

					if (!float.IsNaN(mass2)) {
						mass = mass2;
					}
				}

				{ // thermal conductivity
					float tc = selectedElement.thermalConductivity;

					{
						float building_tc = buildingDef.ThermalConductivity;

						if (prefab.GetComponent<Insulator>() != null) {
							float tc_modifier = 1.53787e-05f; // (1/255)^2
							int idk = (int)(building_tc * 255.0);
							tc *= idk * idk * tc_modifier;
						} else {
							tc *= building_tc;
						}
					}

					Descriptor row = default;

					{
						float final_tc = GameUtil.GetDisplayThermalConductivity(tc);

						string final_tc_str = Math.Abs(final_tc) > 0.001 ? final_tc.ToPrecisionString() : final_tc.ToString("0.00e+0", CultureInfo.InvariantCulture);

						string temperatureUnitSuffix = Utils.GetTemperatureUnitSuffix();

						string tc_formatted = string.Format("{0} (DTU/(m*s))/{1}", final_tc_str, temperatureUnitSuffix);

						row.SetupDescriptor(
							string.Format("<link=\"HEAT\">{0}</link>: {1}", string.Format(UI.ELEMENTAL.THERMALCONDUCTIVITY.NAME, "").Trim().Replace(":", ""), final_tc_str),
							UI.ELEMENTAL.THERMALCONDUCTIVITY.TOOLTIP.Replace("{THERMAL_CONDUCTIVITY}", tc_formatted).Replace("{TEMPERATURE_UNIT}", temperatureUnitSuffix)
						);
					}

					row.IncreaseIndent();

					descriptors.Add(row);
				}

				float shc = 0;

				{ // specific heat capacity
					shc = selectedElement.specificHeatCapacity;

					Descriptor row = default;

					{
						string shc_str = GameUtil.GetDisplaySHC(shc).ToPrecisionString();

						string temperatureUnitSuffix = Utils.GetTemperatureUnitSuffix();

						string shc_formatted = string.Format("{0} (DTU/g)/{1}", shc_str, temperatureUnitSuffix);

						row.SetupDescriptor(
							string.Format("<link=\"HEAT\">{0}</link>: {1}", string.Format(UI.ELEMENTAL.SHC.NAME, "").Trim().Replace(":", ""), shc_str),
							UI.ELEMENTAL.SHC.TOOLTIP.Replace("{SPECIFIC_HEAT_CAPACITY}", shc_formatted).Replace("{TEMPERATURE_UNIT}", temperatureUnitSuffix)
						);
					}

					row.IncreaseIndent();

					descriptors.Add(row);
				}

				{ // thermal mass
					Descriptor row = default;

					{
						string tm_str = (mass * GameUtil.GetDisplaySHC(shc)).ToPrecisionString();

						string temperatureUnitSuffix = Utils.GetTemperatureUnitSuffix();

						string tm_formatted = string.Format("{0} (kDTU)/{1}", tm_str, temperatureUnitSuffix);

						row.SetupDescriptor(
							string.Format("<link=\"HEAT\">{0}</link>: {1}", string.Format(TTRS.UI.THERMALTOOLTIPS.THERMAL_MASS, "").Trim().Replace(":", ""), tm_str),
							string.Format("{0}", tm_formatted)
						);
					}

					row.IncreaseIndent();

					descriptors.Add(row);
				}

				do { // melting point
					Element highPrimaryElement = selectedElement.highTempTransition;

					if (highPrimaryElement == null) break;

					Descriptor row = default;

					{
						string mp_formatted1 = Utils.GetTemperatures(selectedElement.highTemp + 3f, Utils.GetTemperatureUnits(current: true, kelvin: false), brackets: false, format: "0.##");
						string mp_formatted2 = Utils.GetTemperatures(selectedElement.highTemp + 3f, Utils.GetTemperatureUnits(all: true), format: "0.##");

						row.SetupDescriptor(
							string.Format("<link=\"HEAT\">{0}</link>: {1}", string.Format(UI.ELEMENTAL.MELTINGPOINT.NAME, "").Trim().Replace(":", ""), mp_formatted1),
							string.Format(UI.ELEMENTAL.MELTINGPOINT.TOOLTIP, mp_formatted2)
						);
					}

					row.IncreaseIndent();

					descriptors.Add(row);
				} while (false);
			} while (false);

			__instance.MaterialEffectsPane.SetDescriptors(descriptors);

			return false;
		}
	}
}

namespace ThermalTooltipsReworked {
	public static class Utils {
		public static List<TemperatureUnit> GetTemperatureUnits(bool current = true, bool kelvin = true, bool all = false) {
			List<TemperatureUnit> sortedUnits = new List<TemperatureUnit>();

			TemperatureUnit currentUnit = GameUtil.temperatureUnit;

			if (all) {
				sortedUnits.Add(TemperatureUnit.Celsius);
				sortedUnits.Add(TemperatureUnit.Fahrenheit);

				sortedUnits.Remove(currentUnit);
				sortedUnits.Insert(0, currentUnit);

				if (currentUnit != TemperatureUnit.Kelvin)
					sortedUnits.Insert(1, TemperatureUnit.Kelvin);
			} else {
				if (current)
					sortedUnits.Add(currentUnit);

				if (kelvin)
					sortedUnits.Add(TemperatureUnit.Kelvin);
			}

			return sortedUnits;
		}

		public static string GetTemperatureUnitSuffix(TemperatureUnit unit) => (
			unit switch {
				TemperatureUnit.Celsius    => UI.UNITSUFFIXES.TEMPERATURE.CELSIUS.ToString().TrimStart(),
				TemperatureUnit.Fahrenheit => UI.UNITSUFFIXES.TEMPERATURE.FAHRENHEIT.ToString().TrimStart(),
				TemperatureUnit.Kelvin     => UI.UNITSUFFIXES.TEMPERATURE.KELVIN.ToString().TrimStart(),
				_ => throw new InvalidOperationException("Unknown unit"),
			}
		);

		public static string GetTemperatureUnitSuffix() => (GetTemperatureUnitSuffix(GameUtil.temperatureUnit));

		public static string GetTemperatures(float temperature, List<TemperatureUnit> units = null, bool brackets = true, string separator = " | ", string format = null) {
			if (units == null || units.Count == 0) return "";

			StringBuilder sb = new StringBuilder();

			if (brackets) sb.Append("(");

			for (int idx = 0; idx < units.Count; ++idx) {
				if (idx != 0) sb.Append(separator);

				TemperatureUnit unit = units[idx];

				sb.Append(string.Format("{0} {1}",
					(format != null && unit != TemperatureUnit.Kelvin) ?
						GameUtil.GetTemperatureConvertedFromKelvin(temperature, unit).ToPrecisionString(format) :
						GameUtil.GetTemperatureConvertedFromKelvin(temperature, unit).ToPrecisionString(),
					GetTemperatureUnitSuffix(unit)
				));
			}

			if (brackets) sb.Append(")");

			return sb.ToString();
		}
	}
}

namespace ThermalTooltipsReworked {
	internal static class FloatExtensions {
		public static string ToPrecisionString(this float value, string format = "0.#########", float max = 1e+28f) => (
			((decimal)(
				Math.Min(Math.Abs(value), max) *
				Math.Sign(value) *
				Convert.ToInt32(Math.Abs(value) >= 0.000000001f) // 0.000000001f // 0.000000000000000001f
			)).ToString(format, CultureInfo.InvariantCulture)    // 0.#########  // 0.##################
		);
	}

	internal static class HoverTextDrawerExtensions {
		private static readonly Type HoverTextDrawerType = typeof(HoverTextDrawer);

		private static readonly FieldInfo textWidgets = HoverTextDrawerType.GetField("textWidgets", BindingFlags.Instance | BindingFlags.NonPublic);
		private static readonly Type textWidgetsType = textWidgets.FieldType;

		private static readonly FieldInfo prefab = textWidgetsType.GetField("prefab", BindingFlags.Instance | BindingFlags.NonPublic);

		private static readonly FieldInfo root = textWidgetsType.GetField("root", BindingFlags.Instance | BindingFlags.NonPublic);

		public static Vector2 CalcTextSize(this HoverTextDrawer drawer, string text, TextStyleSetting style, bool enableBICCompatibility = true) {
			var __textWidgets = textWidgets.GetValue(drawer);

			var __prefab = prefab.GetValue(__textWidgets);
			var __root = root.GetValue(__textWidgets);

			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)__prefab, ((RectTransform)__root).gameObject.transform, worldPositionStays: false);
			gameObject.SetActive(value: true);

			LocText locText = gameObject.GetComponent<LocText>();

			{
				locText.textStyleSetting = style;
				SetTextStyleSetting.ApplyStyle(locText, locText.textStyleSetting);

				if (enableBICCompatibility && BICCompatibility.Options.CardSize.ShouldOverride)
					locText.fontSize += BICCompatibility.Options.CardSize.FontSizeChange;

				locText.text = text;

				locText.KForceUpdateDirty();
			}

			Vector2 size = locText.GetRenderedValues();

			UnityEngine.Object.Destroy(gameObject);

			return size;
		}
	}
}

namespace ThermalTooltipsReworked {
	public static class ThermalTooltipsReworkedStrings {
		public static class UI {
			public static class THERMALTOOLTIPS {
				public static LocString THERMAL_MASS = "Thermal Mass: {0}";
				public static LocString HEAT_ENERGY = "Heat Energy: {0}";
			}
		}
	}
}

namespace ThermalTooltipsReworked {
	public static class BICCompatibility {
		private static bool? _IsEnabled = null;
		public static bool IsEnabled {
			get {
				if (!_IsEnabled.HasValue) {
					_IsEnabled = Global.Instance.modManager.mods.Any((KMod.Mod mod) => (mod.IsActive() && mod.staticID == "Aze.BetterInfoCards"));
				}

				return _IsEnabled.Value;
			}
		}

		public static class Options {
			private static Type OptionsType {
				get => (Type.GetType("BetterInfoCards.Options, BetterInfoCards", true));
			}

			private static bool? _HideElementCategories = null;
			public static bool HideElementCategories {
				get {
					if (!_HideElementCategories.HasValue) {
						try {
							_HideElementCategories = (bool)Options.OptionsType
								.GetProperty("HideElementCategories", BindingFlags.Instance | BindingFlags.Public)
								.GetValue(Options.__Options);
						} catch {
							_HideElementCategories = false;
						}
					}

					return _HideElementCategories.Value;
				}
			}

			private static object __Options {
				get => (
					Type.GetType("AzeLib.BaseOptions`1, BetterInfoCards", true)
						.MakeGenericType(Options.OptionsType)
						.GetProperty("Opts", BindingFlags.Static | BindingFlags.Public)
						.GetValue(null)
				);
			}

			public static class CardSize {
				private static Type CardSizeType {
					get => (Type.GetType("BetterInfoCards.Options+CardSize, BetterInfoCards", true));
				}

				private static bool? _ShouldOverride = null;
				public static bool ShouldOverride {
					get {
						if (!_ShouldOverride.HasValue) {
							try {
								_ShouldOverride = (bool)Options.CardSize.CardSizeType
									.GetProperty("ShouldOverride", BindingFlags.Instance | BindingFlags.Public)
									.GetValue(Options.CardSize.__CardSize);
							} catch {
								_ShouldOverride = false;
							}
						}

						return _ShouldOverride.Value;
					}
				}

				private static int? _FontSizeChange = null;
				public static int FontSizeChange {
					get {
						if (!_FontSizeChange.HasValue) {
							try {
								_FontSizeChange = (int)Options.CardSize.CardSizeType
									.GetProperty("FontSizeChange", BindingFlags.Instance | BindingFlags.Public)
									.GetValue(Options.CardSize.__CardSize);
							} catch {
								_FontSizeChange = 0;
							}
						}

						return _FontSizeChange.Value;
					}
				}

				private static int? _IconSizeChange = null;
				public static int IconSizeChange {
					get {
						if (!_IconSizeChange.HasValue) {
							try {
								_IconSizeChange = (int)Options.CardSize.CardSizeType
									.GetProperty("IconSizeChange", BindingFlags.Instance | BindingFlags.Public)
									.GetValue(Options.CardSize.__CardSize);
							} catch {
								_IconSizeChange = 0;
							}
						}

						return _IconSizeChange.Value;
					}
				}

				private static object __CardSize {
					get => (
						Options.OptionsType
							.GetProperty("InfoCardSize", BindingFlags.Instance | BindingFlags.Public)
							.GetValue(Options.__Options)
					);
				}
			}
		}

		public static class ExportSelectToolData {
			private static Type ExportSelectToolDataType {
				get => (Type.GetType("BetterInfoCards.ExportSelectToolData, BetterInfoCards", true));
			}

			private static FieldInfo _curSelectable = null;
			public static KSelectable curSelectable {
				set {
					if (_curSelectable == null) {
						_curSelectable = ExportSelectToolData.ExportSelectToolDataType.GetField("curSelectable", BindingFlags.Static | BindingFlags.NonPublic);
					}

					_curSelectable.SetValue(null, value);
				}
			}

			public static class GetSelectInfo_Patch {
				private static MethodInfo _ExportGO = null;
				private static MethodInfo _Export = null;

				public static void ExportGO(string name) {
					if (_ExportGO == null) {
						_ExportGO = ExportSelectToolData.GetSelectInfo_Patch.GetSelectInfo_PatchType.GetMethod("ExportGO", BindingFlags.Static | BindingFlags.NonPublic);
					}

					_ExportGO.Invoke(null, new object[] { name });
				}

				public static void Export(string name, object data) {
					if (_Export == null) {
						_Export = ExportSelectToolData.GetSelectInfo_Patch.GetSelectInfo_PatchType.GetMethod("Export", BindingFlags.Static | BindingFlags.NonPublic);
					}

					_Export.Invoke(null, new object[] { name, data });
				}

				private static Type GetSelectInfo_PatchType {
					get => (Type.GetType("BetterInfoCards.ExportSelectToolData+GetSelectInfo_Patch, BetterInfoCards", true));
				}
			}
		}

		public static class InterceptHoverDrawer {
			private static Type InterceptHoverDrawerType {
				get => (Type.GetType("BetterInfoCards.InterceptHoverDrawer, BetterInfoCards", true));
			}

			private static PropertyInfo _IsInterceptMode = null;
			public static bool IsInterceptMode {
				get {
					if (_IsInterceptMode == null) {
						_IsInterceptMode = InterceptHoverDrawer.InterceptHoverDrawerType.GetProperty("IsInterceptMode", BindingFlags.Static | BindingFlags.Public);
					}

					return (bool)_IsInterceptMode.GetValue(null);
				}
				set {
					if (_IsInterceptMode == null) {
						_IsInterceptMode = InterceptHoverDrawer.InterceptHoverDrawerType.GetProperty("IsInterceptMode", BindingFlags.Static | BindingFlags.Public);
					}

					_IsInterceptMode.SetValue(null, value);
				}
			}
		}

		public static class MaxTempWidth {
			private static float? width = null;

			public static void Init(HoverTextDrawer drawer, string text, TextStyleSetting style, float roundUpTo = 5f, bool enableBICCompatibility = true) {
				width = MathF.Ceiling(drawer.CalcTextSize(text, style, enableBICCompatibility).x / roundUpTo) * roundUpTo;
			}

			public static bool HasValue() => (width.HasValue);

			public static float Get() => (width.Value);
		}

		public static class ConverterManager {
			public const string title = "Title";
			public const string germs = "Germs";
			public const string temp  = "Temp";
		}
	}
}

// TODO: Display buried objects?
// TODO: Display pipes and rails content? Buildings?