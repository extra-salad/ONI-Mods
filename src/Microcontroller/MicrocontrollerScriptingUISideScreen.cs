using UnityEngine;

namespace Microcontroller {

	public class MicrocontrollerScriptingSideScreen : SideScreenContent {

		protected override void OnPrefabInit() {
			base.OnPrefabInit();
			base.titleKey = "Microcontroller";
			base.activateOnSpawn = true;
		}

		public override bool IsValidForTarget(GameObject target) {
			return target.GetComponent<Microcontroller>() != null;
		}

		public override void SetTarget(GameObject target) {
			if (target == null)
				return;

			Microcontroller microcontrollerGO = target.GetComponent<Microcontroller>();
			if (microcontrollerGO == null)
				return;

			MicrocontrollerScripting editor = MicrocontrollerScripting.GetInstance(MicrocontrollerScripting.MAIN_EDITOR_INSTANCE_ID);

			if (editor == null)
				return;

			editor.SetCurrrentMicrocontroller(microcontrollerGO);
		}

	}
}