using HarmonyLib;
using STRINGS;
using KMod;
using PeterHan.PLib.UI;
using PeterHan.PLib.Core;

namespace Microcontroller {
	public class MicrocontrollerMod : UserMod2 {
        public override void OnLoad(Harmony harmony) {
            base.OnLoad(harmony);
			PUtil.InitLibrary(true);
        }
    }

	[HarmonyPatch]
	public class MicrocontrollerPatches {
		[HarmonyPatch(typeof(GeneratedBuildings))]
		[HarmonyPatch(nameof(GeneratedBuildings.LoadGeneratedBuildings))]
		public class GeneratedBuildings_LoadGeneratedBuildings_Patch {
			public static void Prefix() {
				foreach (var width in MicrocontrollerConfig.ImplementedWidths) {
					string microcontrollerUpperID = MicrocontrollerConfig.IDs[width].ToUpperInvariant();

					Strings.Add($"STRINGS.BUILDINGS.PREFABS.{microcontrollerUpperID}.NAME", UI.FormatAsLink(MicrocontrollerConfig.DisplayNames[width],  MicrocontrollerConfig.IDs[width]));
					Strings.Add($"STRINGS.BUILDINGS.PREFABS.{microcontrollerUpperID}.DESC", MicrocontrollerConfig.Description);
					Strings.Add($"STRINGS.BUILDINGS.PREFABS.{microcontrollerUpperID}.EFFECT", MicrocontrollerConfig.Effect);
					ModUtil.AddBuildingToPlanScreen("Automation", MicrocontrollerConfig.IDs[width], "uncategorized", null);
				}
			}

			[HarmonyPatch(typeof(Db))]
			[HarmonyPatch("Initialize")]
			public static class Db_Initialize_Patch {
				public static void Postfix() {
					foreach (var width in MicrocontrollerConfig.ImplementedWidths)
						Db.Get().Techs.Get(MicrocontrollerConfig.Techs[width]).unlockedItemIDs.Add(MicrocontrollerConfig.IDs[width]);
				}
			}
		}

		[HarmonyPatch(typeof(DetailsScreen), "OnPrefabInit")]
		public class DetailsScreen_OnPrefabInit_Patch {
			public static void Postfix() {
				MicrocontrollerScripting microcontrollerScripting = MicrocontrollerScripting.GetInstance(MicrocontrollerScripting.MAIN_EDITOR_INSTANCE_ID);
				PUIUtils.AddSideScreenContent<MicrocontrollerScriptingSideScreen>(microcontrollerScripting.GetUI().Build());
			}
		}
	}
}