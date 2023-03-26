using KSerialization;
using Jint;
using System;
using System.Collections.Generic;

namespace Microcontroller {

	[SerializationConfig(MemberSerialization.OptIn)]
	public class Microcontroller : KMonoBehaviour, ISaveLoadable {
		private static readonly int MAXIMUM_SCRIPT_EXECUTION_TIME_MS = 100;

		[Serialize]
		public string script = "";

#pragma warning disable IDE0044
#pragma warning disable CS0649
		[MyCmpGet]
		private readonly LogicPorts logicPorts;
		[MyCmpReq]
		private Operational operational;
#pragma warning restore CS0649
#pragma warning restore IDE0044

		private Jint.Engine jsEngine = new Jint.Engine();
		private bool isScriptBroken = false;

		protected override void OnSpawn() {
			base.OnSpawn();
			if (this.script.Length == 0) {
				this.script = @"
/**
 * Runs on every Automation update.
 *
 * Gives 4 booleans for a each automation wire input.
 *
 * @parameters Array<boolean>
 * @return Array<boolean>
 */
function main([ input1, input2, input3, input4 ]) {
	const outputs = [ true, true, true, true ];
	return outputs;
}".Replace("\r", "");
			}

			this.CompileScript();
		}

		protected override void OnPrefabInit() {
			Subscribe((int) GameHashes.LogicEvent, LogicEventHandler);
			Subscribe((int) GameHashes.OperationalChanged, OperationalChangeHandler);
		}

		protected override void OnCleanUp() {
			base.OnCleanUp();
			Unsubscribe((int) GameHashes.LogicEvent, LogicEventHandler);
			Unsubscribe((int) GameHashes.OperationalChanged, OperationalChangeHandler);
		}

		private void OperationalChangeHandler(Object data) {
			this.ExecuteMain();
		}

		private void LogicEventHandler(Object data) {
			if (data != null && data is LogicValueChanged) {
				LogicValueChanged logicData = (LogicValueChanged) data;

				foreach (var port in this.logicPorts.inputPortInfo) {
					if (logicData.portID == port.id) {
						this.ExecuteMain();
						return;
					}
				}
			}
		}

		// private void UpdateVisualState(bool isOn) {
			// GetComponent<KBatchedAnimController>().Play(isOn ? "on_pst" : "off", KAnim.PlayMode.Loop);
		// }

		public void CompileScript() {
			this.jsEngine.Dispose();
			this.jsEngine = new Jint.Engine(options => {
				options.TimeoutInterval(TimeSpan.FromMilliseconds(MAXIMUM_SCRIPT_EXECUTION_TIME_MS));
			});

			try {
				this.jsEngine.Execute(this.script);
			} catch (Exception exception) {
				string message = exception is Jint.Runtime.JavaScriptException ? (exception as Jint.Runtime.JavaScriptException).GetJavaScriptErrorString() : exception.Message;
				Debug.Log(message);

				if (message == "The operation has timed out.")
					message += $" Maximum script execution time is {MAXIMUM_SCRIPT_EXECUTION_TIME_MS}ms.";

				this.SetScriptAsBroken(message);
				return;
			}

			this.SetScriptAsValid();
			this.ExecuteMain();
		}

		private void ExecuteMain() {
			if (this.isScriptBroken)
				return;

			if (!this.operational.IsOperational) {
				foreach (var port in this.logicPorts.outputPortInfo)
					this.logicPorts.SendSignal(port.id, 0);

				if (!this.operational.enabled) // TODO figure out how to show disabled message when it's disabled
					this.GetComponent<AttachedMicrocontrollerScripting>()?.microcontrollerScripting?.SetWarningInfo("Microcontroller is disabled");
				else this.GetComponent<AttachedMicrocontrollerScripting>()?.microcontrollerScripting?.SetWarningInfo("Microcontroller is not powered");

				return;
			}

			List<string> inputs = new List<string>();

			foreach (var port in this.logicPorts.inputPortInfo) {
				int portValue = this.logicPorts.GetInputValue(port.id);
				// LogicCircuitNetwork.IsBitActive(1, portValue);
				inputs.Add((portValue & 1) != 0 ? "true" : "false");
				inputs.Add((portValue & 2) != 0 ? "true" : "false");
				inputs.Add((portValue & 4) != 0 ? "true" : "false");
				inputs.Add((portValue & 8) != 0 ? "true" : "false");
			}

			string jsInputs = $"[ {String.Join(", ", inputs.ToArray())} ]";

			Jint.Native.JsValue result = null;
			try {
				result = this.jsEngine.Evaluate($"main instanceof Function && main({jsInputs})?.map(value => !!value);");
			} catch (Exception exception) {
				this.ScriptExceptionHandler(exception);
				return;
			}

			if (!result.IsArray()) {
				this.SetScriptAsBroken("return type of main() has to be an Array<boolean>. For example: return [ true ].");

				foreach (var port in this.logicPorts.outputPortInfo)
					this.logicPorts.SendSignal(port.id, -1); // -1 just for fun

				return;
			}

			int bit = 1;
			int signal = 0;
			int portIndex = 0;
			int index = 0;

			var resultArray = result.AsArray();
			foreach (var output in resultArray) {
				if (output.AsBoolean())
					signal |= bit;

				bit <<= 1;

				if (bit == 16 || (index + 1) == resultArray.Length) {
					this.logicPorts.SendSignal(this.logicPorts.outputPortInfo[portIndex].id, signal);
					portIndex++;

					if (portIndex >= this.logicPorts.outputPortInfo.Length)
						break;

					signal = 0;
					bit = 1;
				}
				index++;
			}
		}

		private void ScriptExceptionHandler(Exception exception) {
			string errorMessage = exception is Jint.Runtime.JavaScriptException ? (exception as Jint.Runtime.JavaScriptException).GetJavaScriptErrorString() : exception.Message;
			this.SetScriptAsBroken(errorMessage);
		}

		private void SetScriptAsBroken(string errorMessage) {
			this.isScriptBroken = true;

			foreach (var port in this.logicPorts.outputPortInfo)
				this.logicPorts.SendSignal(port.id, 0);

			this.GetComponent<AttachedMicrocontrollerScripting>()?.microcontrollerScripting?.SetErrorInfo(errorMessage);
		}

		private void SetScriptAsValid() {
			this.isScriptBroken = false;

			this.GetComponent<AttachedMicrocontrollerScripting>()?.microcontrollerScripting?.SetValidInfo("All Good");
		}
	}
}