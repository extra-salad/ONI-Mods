using System;
using System.Collections.Generic;
using TUNING;
using UnityEngine;

namespace Microcontroller {
	public class MicrocontrollerConfig {
		public static readonly int[] ImplementedWidths = { 1, 2, 4, 8 };
		public static readonly string[] IDs = {
			null,
			"Microcontroller_w1",
			"Microcontroller_w2",
			null,
			"Microcontroller_w4",
			null, null, null,
			"Microcontroller_w8",
		};

		public static readonly string[] DisplayNames = {
			null,
			"Small Microcontroller",
			"Medium Microcontroller",
			null,
			"Large Microcontroller",
			null, null, null,
			"Huge Microcontroller",
		};

		public static readonly string[] Techs = {
			null,
			"LogicCircuits",
			"DupeTrafficControl",
			null,
			"DupeTrafficControl",
			null, null, null,
			"Multiplexing",
		};

		public static string Effect = "Allows loading in JavaScript code for easier automation.";
		public static string Description = "Useful for those who hate logic gates.";

		private static readonly float[] ConstructionMassesForMetal = {
			0f,
			BUILDINGS.CONSTRUCTION_MASS_KG.TIER2[0],
			BUILDINGS.CONSTRUCTION_MASS_KG.TIER3[0],
			0f,
			BUILDINGS.CONSTRUCTION_MASS_KG.TIER4[0],
			0f, 0f, 0f,
			BUILDINGS.CONSTRUCTION_MASS_KG.TIER5[0],
		};

		private static readonly float[] ConstructionMassesForPlastic = {
			0f,
			BUILDINGS.CONSTRUCTION_MASS_KG.TIER0[0],
			BUILDINGS.CONSTRUCTION_MASS_KG.TIER0[0],
			0f,
			BUILDINGS.CONSTRUCTION_MASS_KG.TIER1[0],
			0f, 0f, 0f,
			BUILDINGS.CONSTRUCTION_MASS_KG.TIER2[0],
		};

		private static readonly float[] EnergyConsumptions = {
			0f,
			25f,
			75f,
			0f,
			150f,
			0f, 0f, 0f,
			450f,
		};

		private static readonly float[] SelfHeatKilowattValues = {
			0f,
			2f,
			4f,
			0f,
			6f,
			0f, 0f, 0f,
			8f,
		};

		private static readonly float[] ExhaustKilowattValues = {
			0f,
			0.5f,
			1f,
			0f,
			2f,
			0f, 0f, 0f,
			4f,
		};

		private static string PortInOn = "Incoming Signals";
		private static string PortOutOn = "Outgoing Signals";

		public static BuildingDef CreateBuildingDef(int width) {
			var buildingDef = BuildingTemplates.CreateBuildingDef(
				id: IDs[width],
				width: width,
				height: 2,
				anim: $"microcontroller_w{width}_kanim",
				hitpoints: BUILDINGS.HITPOINTS.TIER1,
				construction_time: BUILDINGS.CONSTRUCTION_TIME_SECONDS.TIER2,
				construction_mass: new float[2] { ConstructionMassesForMetal[width], ConstructionMassesForPlastic[width] },
				construction_materials: new string[2] { "Gold", MATERIALS.PLASTIC },
				melting_point: BUILDINGS.MELTING_POINT_KELVIN.TIER1,
				build_location_rule: BuildLocationRule.Anywhere,
				decor: DECOR.NONE,
				noise: NOISE_POLLUTION.NONE
			);

			ApplyBuildingDefProperties(buildingDef, width);

			GeneratedBuildings.RegisterWithOverlay(OverlayModes.Logic.HighlightItemIDs, IDs[width]);

			return buildingDef;
		}

		private static void ApplyBuildingDefProperties(BuildingDef buildingDef, int width) {
			buildingDef.Overheatable = true;
			buildingDef.OverheatTemperature = 373.15f - 50f; // -50 for gold
			buildingDef.Floodable = true;
			buildingDef.Entombable = false;
			buildingDef.ViewMode = OverlayModes.Logic.ID;
			buildingDef.AudioCategory = "Metal";
			buildingDef.SceneLayer = Grid.SceneLayer.LogicGates;
			buildingDef.PermittedRotations = PermittedRotations.R360;
			buildingDef.RequiresPowerInput = true;
			buildingDef.EnergyConsumptionWhenActive = EnergyConsumptions[width];
			buildingDef.SelfHeatKilowattsWhenActive = SelfHeatKilowattValues[width];
			buildingDef.ExhaustKilowattsWhenActive = ExhaustKilowattValues[width];
			buildingDef.PowerInputOffset = new CellOffset(0, 0);
			buildingDef.LogicInputPorts = MicrocontrollerConfig.GenerateLogicInputPorts(width);
			buildingDef.LogicOutputPorts = MicrocontrollerConfig.GenerateLogicOutputPorts(width);
		}

		private static List<LogicPorts.Port> GenerateLogicInputPorts(int numberOfPorts) {
			int portCellOffset = (((int) Math.Ceiling(numberOfPorts / 2f)) - 1) * -1;
			var inputPorts = new List<LogicPorts.Port>();
			for (int i = 0; i < numberOfPorts; i++) {
				inputPorts.Add(LogicPorts.Port.RibbonInputPort(
					(HashedString) $"MC_W{numberOfPorts}_I{i}", new CellOffset(portCellOffset + i, 1),
					STRINGS.UI.LOGIC_PORTS.INPUT_PORTS, PortInOn, null, false, true
				));
			}

			return inputPorts;
		}

		private static List<LogicPorts.Port> GenerateLogicOutputPorts(int numberOfPorts) {
			int portCellOffset = (((int) Math.Ceiling(numberOfPorts / 2f)) - 1) * -1;
			var outputPorts = new List<LogicPorts.Port>();
			for (int i = 0; i < numberOfPorts; i++) {
				outputPorts.Add(LogicPorts.Port.RibbonOutputPort(
					(HashedString) $"MC_W{numberOfPorts}_O{i}", new CellOffset(portCellOffset + i, 0),
					STRINGS.UI.LOGIC_PORTS.OUTPUT_PORTS, PortOutOn, null, false, true
				));
			}

			return outputPorts;
		}

		public static void DoPostConfigureComplete(GameObject go) {
			go.AddOrGetDef<PoweredController.Def>();
			go.AddOrGet<Microcontroller>();
		}
	}
}