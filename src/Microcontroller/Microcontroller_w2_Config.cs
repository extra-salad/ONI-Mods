using UnityEngine;

namespace Microcontroller {
	public class Microcontroller_w2_Config : IBuildingConfig {

		public override BuildingDef CreateBuildingDef() {
			return MicrocontrollerConfig.CreateBuildingDef(2);
		}

		public override void ConfigureBuildingTemplate(GameObject go, Tag prefab_tag) {
			BuildingConfigManager.Instance.IgnoreDefaultKComponent(typeof(RequiresFoundation), prefab_tag);
		}

		public override void DoPostConfigureComplete(GameObject go) {
			MicrocontrollerConfig.DoPostConfigureComplete(go);
		}

	}
}