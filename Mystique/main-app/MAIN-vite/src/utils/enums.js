const ModuleState = {
  none: "none",
  idle: "idle",
  busy: "busy",
  deviceBusy: "deviceBusy",
  offline: "offline",
  fault: "fault",
  disabled: "disabled",
};

const ModuleCategory = {
  none: "none",
  monitoring: "monitoring",
  directionFinding: "directionFinding",
  antennaControl: "antennaControl",
  control: "control",
  gps: "gps",
  compass: "compass",
  decoder: "decoder",
  sensor: "sensor",
  radioSuppressing: "radioSuppressing",
  switchArray: "switchArray",
  ioStorage: "ioStorage",
  swivel: "swivel",
  icb: "icb",
  recognizer: "recognizer",
  radar: "radar",
};

export { ModuleState, ModuleCategory };
