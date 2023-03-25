using System.Collections.Generic;
using STRINGS;
using TUNING;
using UnityEngine;
using BUILDINGS = TUNING.BUILDINGS;

namespace Microcontroller {
	public class Microcontroller_w8_Config : IBuildingConfig {

		public override BuildingDef CreateBuildingDef() {
			return MicrocontrollerConfig.CreateBuildingDef(8);
		}

		public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag) {
			BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
		}

		public override void DoPostConfigureComplete(GameObject go) {
			MicrocontrollerConfig.DoPostConfigureComplete(go);
		}

	}
}