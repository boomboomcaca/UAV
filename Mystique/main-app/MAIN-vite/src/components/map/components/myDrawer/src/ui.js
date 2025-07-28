import xtend from "xtend";
import * as Constants from "./constants";

const classTypes = ["mode", "feature", "mouse"];
export default function (ctx) {
  const buttonElements = {};
  let activeButton = null;

  let currentMapClasses = {
    mode: null, // e.g. mode-direct_select
    feature: null, // e.g. feature-vertex
    mouse: null, // e.g. mouse-move
  };

  let nextMapClasses = {
    mode: null,
    feature: null,
    mouse: null,
  };

  function clearMapClasses() {
    queueMapClasses({ mode: null, feature: null, mouse: null });
    updateMapClasses();
  }

  function queueMapClasses(options) {
    nextMapClasses = xtend(nextMapClasses, options);
  }

  function updateMapClasses() {
    if (!ctx.container) return;

    const classesToRemove = [];
    const classesToAdd = [];

    classTypes.forEach((type) => {
      if (nextMapClasses[type] === currentMapClasses[type]) return;

      classesToRemove.push(`${type}-${currentMapClasses[type]}`);
      if (nextMapClasses[type] !== null) {
        classesToAdd.push(`${type}-${nextMapClasses[type]}`);
      }
    });

    if (classesToRemove.length > 0) {
      ctx.container.classList.remove(...classesToRemove);
    }

    if (classesToAdd.length > 0) {
      ctx.container.classList.add(...classesToAdd);
    }

    currentMapClasses = xtend(currentMapClasses, nextMapClasses);
  }

  function createControlButton(id, options = {}) {
    const button = document.createElement("button");
    button.className = `${Constants.classes.CONTROL_BUTTON} ${options.className}`;
    button.setAttribute("title", options.title);
    const label = document.createElement("span");
    label.innerHTML = options.label || "";
    label.className = `${Constants.classes.CONTROL_BUTTON_LABEL}`;
    button.appendChild(label);
    options.container.appendChild(button);

    button.addEventListener(
      "click",
      (e) => {
        e.preventDefault();
        e.stopPropagation();

        const clickedButton = e.target;
        if (clickedButton === activeButton) {
          deactivateButtons();
          options.onDeactivate();
          return;
        }

        setActiveButton(id);
        options.onActivate();
      },
      true
    );

    return button;
  }

  function deactivateButtons() {
    if (!activeButton) return;
    activeButton.classList.remove(Constants.classes.ACTIVE_BUTTON);
    activeButton = null;
  }

  function setActiveButton(id) {
    deactivateButtons();

    const button = buttonElements[id];
    if (!button) return;

    if (button && id !== "trash") {
      button.classList.add(Constants.classes.ACTIVE_BUTTON);
      activeButton = button;
    }
  }

  function addButtons() {
    const controls = ctx.options.controls;
    const controlGroup = document.createElement("div");
    controlGroup.className = `${Constants.classes.CONTROL_GROUP} ${Constants.classes.CONTROL_BASE}`;

    if (!controls) return controlGroup;

    if (controls[Constants.types.LINE]) {
      buttonElements[Constants.types.LINE] = createControlButton(
        Constants.types.LINE,
        {
          container: controlGroup,
          className: Constants.classes.CONTROL_BUTTON_LINE,
          title: `测距 ${ctx.options.keybindings ? "(l)" : ""}`,
          label: controls[Constants.types.LINE], // '测距',
          onActivate: () =>
            ctx.events.changeMode(Constants.modes.DRAW_LINE_STRING),
          onDeactivate: () => ctx.events.trash(),
        }
      );
    }

    if (controls[Constants.types.POLYGON]) {
      buttonElements[Constants.types.POLYGON] = createControlButton(
        Constants.types.POLYGON,
        {
          container: controlGroup,
          className: Constants.classes.CONTROL_BUTTON_POLYGON,
          title: `面积 ${ctx.options.keybindings ? "(p)" : ""}`,
          label: controls[Constants.types.POLYGON],
          onActivate: () => ctx.events.changeMode(Constants.modes.DRAW_POLYGON),
          onDeactivate: () => ctx.events.trash(),
        }
      );
    }

    if (controls[Constants.types.POINT]) {
      buttonElements[Constants.types.POINT] = createControlButton(
        Constants.types.POINT,
        {
          container: controlGroup,
          className: Constants.classes.CONTROL_BUTTON_POINT,
          title: `标点 ${ctx.options.keybindings ? "(m)" : ""}`,
          label: controls[Constants.types.POINT],
          onActivate: () => ctx.events.changeMode(Constants.modes.DRAW_POINT),
          onDeactivate: () => ctx.events.trash(),
        }
      );
    }

    // 魔改 添加量角器
    if (controls.measuring_angle) {
      buttonElements.uncombine_features = createControlButton(
        "measuringAngleFeatures",
        {
          container: controlGroup,
          className: Constants.classes.CONTROL_BUTTON_MEASUREING_ANGLE,
          title: "角度",
          label: controls.measuring_angle,
          onActivate: () => ctx.events.changeMode(Constants.modes.DRAW_ANGLE),
          onDeactivate: () => ctx.events.trash(),
        }
      );
    }

    if (controls.trash) {
      buttonElements.trash = createControlButton("trash", {
        container: controlGroup,
        className: Constants.classes.CONTROL_BUTTON_TRASH,
        title: "Delete",
        label: "删除",
        onActivate: () => {
          ctx.events.trash();
        },
      });
    }

    if (controls.combine_features) {
      buttonElements.combine_features = createControlButton("combineFeatures", {
        container: controlGroup,
        className: Constants.classes.CONTROL_BUTTON_COMBINE_FEATURES,
        title: "Combine",
        onActivate: () => {
          ctx.events.combineFeatures();
        },
      });
    }

    if (controls.uncombine_features) {
      buttonElements.uncombine_features = createControlButton(
        "uncombineFeatures",
        {
          container: controlGroup,
          className: Constants.classes.CONTROL_BUTTON_UNCOMBINE_FEATURES,
          title: "Uncombine",
          onActivate: () => {
            ctx.events.uncombineFeatures();
          },
        }
      );
    }

    return controlGroup;
  }

  function removeButtons() {
    Object.keys(buttonElements).forEach((buttonId) => {
      const button = buttonElements[buttonId];
      if (button.parentNode) {
        button.parentNode.removeChild(button);
      }
      delete buttonElements[buttonId];
    });
  }

  return {
    setActiveButton,
    queueMapClasses,
    updateMapClasses,
    clearMapClasses,
    addButtons,
    removeButtons,
  };
}
