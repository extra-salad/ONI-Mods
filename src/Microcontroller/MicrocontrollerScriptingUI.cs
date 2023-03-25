using UnityEngine;
using PeterHan.PLib.UI;
using System.Collections.Generic;
using System;
using UnityEngine.UI;


namespace Microcontroller {

	public class MicrocontrollerScripting {
		internal static readonly RectOffset OUTER_MARGIN = new RectOffset(6, 10, 6, 10);

		private TMPro.TMP_InputField scriptTextArea;
		private GameObject errorLabel;

		private Microcontroller currentMicrocontroller;

		private MicrocontrollerScripting() {

		}

		private string lastTextValue = "";

		public PContainer GetUI() {
			return new PPanel("MicrocontrollerScriptingSideScreen") {
				Direction = PanelDirection.Vertical,
				Margin = OUTER_MARGIN,
				// Alignment = TextAnchor.MiddleCenter,
				Spacing = 4,
				BackColor = PUITuning.Colors.BackgroundLight,
				FlexSize = Vector2.one,
			}
			// .AddChild(
			// 	new PSpacer() {
			// 		PreferredSize = new Vector2(500, 1)
			// 	}
			// )
			.AddChild(
				// new PScrollPane("Scripting Scroll Pane") {
				// 	ScrollHorizontal = true,
				// 	ScrollVertical = true,
				// 	FlexSize = Vector2.one,

				// 	Child =
					new PTextArea("Scripting TextArea") {
						LineCount = 15,
						Text = "",
						MinWidth = 500,
						OnTextChanged = (go, text) => {
							text = text.Replace("\r", "");
							this.lastTextValue = text;
							Debug.Log("[ TextArea ] [ OnTextChanged ] " + (go != null ? "OK" : "null") + " [ Text ] " + text);
							if (this.currentMicrocontroller == null || text == null)
								return;
							this.currentMicrocontroller.script = text;
						}
					}.AddOnRealize(go => {
						this.scriptTextArea = go.GetComponentInChildren<TMPro.TMP_InputField>();
						this.scriptTextArea.onFocusSelectAll = false;
						this.RefreshTextArea();
					})
				// }
			).AddChild(
				new PPanel("Scripting Panel") {
					Direction = PanelDirection.Vertical,
					Spacing = 4,
					FlexSize = Vector2.one,
				}.AddChild(
					new PButton("Complile Button") {
						Text = "Compile",
						OnClick = (go) => this.currentMicrocontroller?.CompileScript()
					}
				).AddChild(
					new PLabel("Info Label") {
						FlexSize = Vector2.one,
						Text = "HELLO",
					}.AddOnRealize(go => this.errorLabel = go)
				)
			).SetKleiBlueColor();
		}

		public void SetCurrrentMicrocontroller(Microcontroller microcontroller) {
			if (this.currentMicrocontroller)
				UnityEngine.MonoBehaviour.Destroy(this.currentMicrocontroller.GetComponent<AttachedMicrocontrollerScripting>());

			this.currentMicrocontroller = microcontroller;

			if (this.currentMicrocontroller) {
				this.currentMicrocontroller.FindOrAddComponent<AttachedMicrocontrollerScripting>().microcontrollerScripting = this;
				this.currentMicrocontroller.CompileScript();
			}

			this.RefreshTextArea();
		}

		public void RefreshTextArea() {
			if (this.scriptTextArea == null)
				return;

			string newText = (this.currentMicrocontroller?.script == null) ? "" : this.currentMicrocontroller.script;

			this.SetText(newText);
		}

		private void SetText(string text) {
			this.scriptTextArea.text = text;
		}

		public void SetValidInfo(string validText) {
			this.SetInfo(validText, Color.green);
		}

		public void SetErrorInfo(string errorText) {
			this.SetInfo(errorText, Color.red);
		}

		public void SetInfo(string infoText, Color color) {
			if (color == null)
				color = Color.white;

			var errorTextComponent = this.errorLabel.GetComponentInChildren<TMPro.TextMeshProUGUI>();
			if (errorTextComponent == null)
				return;

			errorTextComponent.color = color;
			errorTextComponent.SetText(infoText);
		}

		public static int MAIN_EDITOR_INSTANCE_ID = 69;

		static Dictionary<int, MicrocontrollerScripting> ScriptingInstances = new Dictionary<int, MicrocontrollerScripting>();
		public static MicrocontrollerScripting GetInstance(int index = 0) {
			if (!MicrocontrollerScripting.ScriptingInstances.ContainsKey(index))
				MicrocontrollerScripting.ScriptingInstances[index] = new MicrocontrollerScripting();

			return MicrocontrollerScripting.ScriptingInstances[index];
		}
	}

	public class AttachedMicrocontrollerScripting : KMonoBehaviour {
		public MicrocontrollerScripting microcontrollerScripting = null;
	}
}