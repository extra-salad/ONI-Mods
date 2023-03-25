using UnityEngine;

namespace Microcontroller {
	public class Microcontroller_w1_Config : IBuildingConfig {

		public override BuildingDef CreateBuildingDef() {
			return MicrocontrollerConfig.CreateBuildingDef(1);
		}

		public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag) {
			BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
		}

		public override void DoPostConfigureComplete(GameObject go) {
			MicrocontrollerConfig.DoPostConfigureComplete(go);
		}

	}
}